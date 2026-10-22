using FarmedicalService.Database;
using FarmedicalService.Models;
using FarmedicalService.UI;

namespace FarmedicalService.Forms
{
    public class FormInventario : PageForm
    {
        private DataGridView dgvProductos = null!;
        private TextBox txtBuscar = null!;
        private AppCombo cmbCategoria = null!;
        private AppCombo cmbFabricante = null!;
        private AppButton btnPorReponer = null!;
        private Label lblVacio = null!;
        private bool _cargando;
        private bool _soloStockBajo;
        private int _totalBajos;
        private Dictionary<int, DateTime> _vencimientos = new();

        private readonly ProductoRepository _productoRepo = new();
        private readonly CategoriaRepository _categoriaRepo = new();
        private readonly FabricanteRepository _fabricanteRepo = new();
        private readonly LoteRepository _loteRepo = new();

        public FormInventario(bool soloStockBajo = false) : base("Inventario", "Productos, existencias y alertas de stock")
        {
            _soloStockBajo = soloStockBajo;
            InicializarComponentes();
            CargarDatos();
        }

        private void InicializarComponentes()
        {
            // Acciones en la cabecera: la destructiva es secundaria (borde rojo) y pide confirmación
            var btnEliminar = new AppButton("Eliminar", ButtonKind.DangerOutline);
            var btnLotes = new AppButton("Gestionar lotes", ButtonKind.Secondary);
            var btnEditar = new AppButton("Editar", ButtonKind.Secondary);
            var btnNuevo = new AppButton("Nuevo producto", ButtonKind.Primary);
            btnEliminar.Click += (s, e) => EliminarProducto();
            btnLotes.Click += (s, e) => AgregarLote();
            btnEditar.Click += (s, e) => EditarProducto();
            btnNuevo.Click += (s, e) => AbrirFormProducto(null);
            AddAction(btnEliminar);
            AddAction(btnLotes);
            AddAction(btnEditar);
            AddAction(btnNuevo);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Background
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Filtros
            txtBuscar = Ui.Txt("Nombre del medicamento");
            cmbCategoria = Ui.Combo();
            cmbFabricante = Ui.Combo();

            btnPorReponer = new AppButton("Por reponer", ButtonKind.DangerOutline, compact: true);
            btnPorReponer.Click += (s, e) =>
            {
                _soloStockBajo = !_soloStockBajo;
                ActualizarBotonPorReponer();
                FiltrarProductos();
            };
            var btnLimpiar = new AppButton("Limpiar", ButtonKind.Secondary, compact: true);
            btnLimpiar.Click += (s, e) => LimpiarFiltros();

            txtBuscar.TextChanged += (s, e) => { if (!_cargando) FiltrarProductos(); };
            cmbCategoria.SelectedIndexChanged += (s, e) => { if (!_cargando) FiltrarProductos(); };
            cmbFabricante.SelectedIndexChanged += (s, e) => { if (!_cargando) FiltrarProductos(); };

            var filtros = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Theme.Surface
            };
            filtros.Controls.Add(Ui.Field("Buscar", txtBuscar, 200));
            filtros.Controls.Add(Ui.Field("Categoría", cmbCategoria, 170));
            filtros.Controls.Add(Ui.Field("Fabricante", cmbFabricante, 170));
            filtros.Controls.Add(Ui.Field(" ", btnPorReponer));
            filtros.Controls.Add(Ui.Field(" ", btnLimpiar));

            var toolbar = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 10,
                Height = Ui.S(92),
                Padding = Ui.P(20, 14, 20, 0),
                Margin = new Padding(0, 0, 0, Ui.S(14))
            };
            toolbar.Controls.Add(filtros);

            // Tabla
            dgvProductos = new DataGridView { Dock = DockStyle.Fill };
            Ui.StyleGrid(dgvProductos);
            dgvProductos.CellPainting += DgvProductos_CellPainting;
            dgvProductos.CellFormatting += DgvProductos_CellFormatting;
            lblVacio = Ui.EmptyLabel(dgvProductos, "No hay productos que coincidan con los filtros.");

            var gridCard = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 10,
                Padding = Ui.P(6),
                Margin = Padding.Empty
            };
            gridCard.Controls.Add(dgvProductos);

            root.Controls.Add(toolbar, 0, 0);
            root.Controls.Add(gridCard, 0, 1);
            Body.Controls.Add(root);
        }

        private void ActualizarBotonPorReponer()
        {
            btnPorReponer.Text = _totalBajos > 0 ? $"Por reponer ({_totalBajos})" : "Por reponer";
            btnPorReponer.Kind = _soloStockBajo ? ButtonKind.Danger : ButtonKind.DangerOutline;
            btnPorReponer.Invalidate();
        }

        private void LimpiarFiltros()
        {
            _cargando = true;
            txtBuscar.Clear();
            cmbCategoria.SelectedIndex = 0;
            cmbFabricante.SelectedIndex = 0;
            _soloStockBajo = false;
            _cargando = false;
            ActualizarBotonPorReponer();
            FiltrarProductos();
        }

        private void CargarDatos()
        {
            _cargando = true;

            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add("Todas");
            var usos = _categoriaRepo.ObtenerTodas()
                .Select(c => c.UsoTerapeutico)
                .Distinct()
                .OrderBy(u => u)
                .ToList();
            foreach (var uso in usos)
                cmbCategoria.Items.Add(uso);
            cmbCategoria.SelectedIndex = 0;

            cmbFabricante.Items.Clear();
            cmbFabricante.Items.Add("Todos");
            foreach (var f in _fabricanteRepo.ObtenerTodos())
                cmbFabricante.Items.Add(f.Nombre);
            cmbFabricante.SelectedIndex = 0;

            _totalBajos = _productoRepo.ObtenerTodos().Count(p => p.StockActual <= p.StockMinimo);
            _vencimientos = _loteRepo.ObtenerVencimientosMasProximos();

            _cargando = false;
            ActualizarBotonPorReponer();
            FiltrarProductos();
        }

        private static string EstadoStock(Producto p)
        {
            if (p.StockActual <= 0) return "Agotado";
            if (p.StockActual <= p.StockMinimo) return "Bajo";
            return "";
        }

        private void CargarProductos(List<Producto> productos)
        {
            dgvProductos.Columns.Clear();
            Ui.AddColumn(dgvProductos, "Id", "ID", visible: false);
            Ui.AddColumn(dgvProductos, "Nombre", "Producto", 220);
            Ui.AddColumn(dgvProductos, "Categoria", "Categoría", 150);
            Ui.AddColumn(dgvProductos, "Fabricante", "Fabricante", 120);
            Ui.AddColumn(dgvProductos, "Itbis", "ITBIS", 65, right: true);
            Ui.AddColumn(dgvProductos, "PrecioVenta", "Precio de venta", 105, right: true);
            Ui.AddColumn(dgvProductos, "Stock", "Stock", 60, right: true);
            Ui.AddColumn(dgvProductos, "StockMin", "Mínimo", 65, right: true);
            Ui.AddColumn(dgvProductos, "Vence", "Próx. vencimiento", 120, right: true);
            Ui.AddColumn(dgvProductos, "Estado", "Estado", 95);

            dgvProductos.Rows.Clear();

            foreach (var p in productos)
            {
                DateTime? vence = _vencimientos.TryGetValue(p.Id, out var f) ? f : null;
                var fila = dgvProductos.Rows.Add(
                    p.Id, p.Nombre, p.CategoriaNombre, p.FabricanteNombre,
                    p.Itbis == 0 ? "No aplica" : $"{p.Itbis}%", Ui.Money(p.PrecioVenta),
                    p.StockActual, p.StockMinimo,
                    vence.HasValue ? vence.Value.ToString("dd/MM/yyyy") : "—",
                    EstadoStock(p));
                dgvProductos.Rows[fila].Tag = vence;
            }

            lblVacio.Visible = dgvProductos.Rows.Count == 0;
            lblVacio.BringToFront();
        }

        private void DgvProductos_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvProductos.Columns[e.ColumnIndex].Name != "Estado") return;

            var estado = e.Value?.ToString() ?? "";
            if (estado == "Agotado")
                Ui.PintarInsignia(e, estado, Theme.DangerSoft, Theme.DangerDark);
            else if (estado == "Bajo")
                Ui.PintarInsignia(e, estado, Theme.WarningSoft, Theme.WarningDark);
        }

        private void DgvProductos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle == null) return;
            if (dgvProductos.Columns[e.ColumnIndex].Name != "Vence") return;

            if (dgvProductos.Rows[e.RowIndex].Tag is DateTime vence)
            {
                var dias = (vence.Date - DateTime.Today).Days;
                if (dias < 0) e.CellStyle.ForeColor = Theme.DangerDark;
                else if (dias <= 30) e.CellStyle.ForeColor = Theme.WarningDark;
            }
        }

        private void FiltrarProductos()
        {
            var usoSeleccionado = cmbCategoria.SelectedIndex <= 0 ? "" : cmbCategoria.SelectedItem?.ToString() ?? "";
            var fabricanteSeleccionado = cmbFabricante.SelectedIndex <= 0 ? "" : cmbFabricante.SelectedItem?.ToString() ?? "";

            var productos = _productoRepo.BuscarPorFiltroUso(
                txtBuscar.Text.Trim(),
                usoSeleccionado,
                fabricanteSeleccionado);

            if (_soloStockBajo)
                productos = productos.Where(p => p.StockActual <= p.StockMinimo).ToList();

            CargarProductos(productos);
        }

        private void AbrirFormProducto(Producto? producto)
        {
            using var form = new FormProducto(producto);
            if (form.ShowDialog() == DialogResult.OK)
                CargarDatos();
        }

        private void EditarProducto()
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un producto primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Id"].Value);
            var producto = _productoRepo.ObtenerTodos().FirstOrDefault(p => p.Id == id);
            AbrirFormProducto(producto);
        }

        private void EliminarProducto()
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un producto primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value?.ToString() ?? "";
            var confirm = MessageBox.Show(
                $"¿Eliminar {nombre}?\n\nSe borrarán también sus lotes y su historial de ventas. Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm == DialogResult.Yes)
            {
                var id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Id"].Value);
                _productoRepo.Eliminar(id);
                CargarDatos();
            }
        }

        private void AgregarLote()
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un producto primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Id"].Value);
            var nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value?.ToString() ?? "";
            using var form = new FormAgregarLote(id, nombre);
            form.ShowDialog();
            if (form.HuboCambios)
                CargarDatos();
        }
    }
}
