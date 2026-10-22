using FarmedicalService.Database;
using FarmedicalService.Models;
using FarmedicalService.UI;

namespace FarmedicalService.Forms
{
    public class FormProducto : DialogForm
    {
        private TextBox txtNombre = null!;
        private TextBox txtCodigoBarras = null!;
        private TextBox txtPrecioCompra = null!;
        private TextBox txtPrecioVenta = null!;
        private TextBox txtStockMinimo = null!;
        private TextBox txtItbis = null!;
        private TextBox txtCantidadLote = null!;
        private DateTimePicker dtpVencimiento = null!;
        private TextBox txtNumeroLote = null!;
        private AppCombo cmbUsoTerapeutico = null!;
        private AppCombo cmbViaAdministracion = null!;
        private TextBox txtFabricante = null!;

        private readonly ProductoRepository _productoRepo = new();
        private readonly CategoriaRepository _categoriaRepo = new();
        private readonly FabricanteRepository _fabricanteRepo = new();
        private readonly Producto? _productoEditar;
        private List<Categoria> _categorias = new();

        public FormProducto(Producto? producto)
            : base(producto == null ? "Nuevo producto" : "Editar producto", 680, producto == null ? 700 : 520)
        {
            _productoEditar = producto;
            InicializarComponentes();
            CargarCombos();
            if (_productoEditar != null)
                CargarDatosEdicion();
        }

        private void InicializarComponentes()
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                BackColor = Theme.Surface
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            int fila = 0;

            void Seccion(string texto)
            {
                var l = Ui.Section(texto);
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                grid.Controls.Add(l, 0, fila);
                grid.SetColumnSpan(l, 2);
                fila++;
            }

            // Un campo ocupa una columna (o las dos si ancho = true)
            void Campo(Control campo, int columna, bool ancho = false)
            {
                if (columna == 0 && !ancho) campo.Margin = new Padding(0, 0, Ui.S(16), Ui.S(12));
                else campo.Margin = new Padding(0, 0, 0, Ui.S(12));
                if (columna == 0) grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                grid.Controls.Add(campo, columna, fila);
                if (ancho) grid.SetColumnSpan(campo, 2);
                if (columna == 1 || ancho) fila++;
            }

            txtNombre = Ui.Txt("Ej. Acetaminofén 500 mg");
            txtCodigoBarras = Ui.Txt("Opcional");
            cmbUsoTerapeutico = Ui.Combo();
            cmbViaAdministracion = Ui.Combo();
            txtFabricante = Ui.Txt("Escribe o elige un fabricante");
            txtFabricante.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtFabricante.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtFabricante.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            txtPrecioCompra = Ui.Txt("0.00");
            txtPrecioVenta = Ui.Txt("0.00");
            txtStockMinimo = Ui.Txt("0");
            txtItbis = Ui.Txt("Vacío si no aplica");

            Seccion("Información del medicamento");
            Campo(Ui.Field("Nombre del medicamento", txtNombre, fill: true), 0, ancho: true);
            Campo(Ui.Field("Código de barras", txtCodigoBarras, fill: true), 0);
            Campo(Ui.Field("Fabricante (o escribe uno nuevo)", txtFabricante, fill: true), 1);
            Campo(Ui.Field("Acción terapéutica", cmbUsoTerapeutico, fill: true), 0);
            Campo(Ui.Field("Vía de administración", cmbViaAdministracion, fill: true), 1);

            Seccion("Precios y alertas");
            Campo(Ui.Field("Precio de compra (RD$)", txtPrecioCompra, fill: true), 0);
            Campo(Ui.Field("Precio de venta (RD$)", txtPrecioVenta, fill: true), 1);
            Campo(Ui.Field("Stock mínimo de alerta", txtStockMinimo, fill: true), 0);
            Campo(Ui.Field("ITBIS (%)", txtItbis, fill: true), 1);

            if (_productoEditar == null)
            {
                txtCantidadLote = Ui.Txt("0");
                txtNumeroLote = Ui.Txt("Opcional");
                dtpVencimiento = Ui.Fecha();

                Seccion("Lote inicial");
                Campo(Ui.Field("Cantidad en este lote", txtCantidadLote, fill: true), 0);
                Campo(Ui.Field("Número de lote", txtNumeroLote, fill: true), 1);
                Campo(Ui.Field("Fecha de vencimiento", dtpVencimiento, fill: true), 0);
            }

            Content.Controls.Add(grid);

            var btnGuardar = new AppButton("Guardar", ButtonKind.Primary);
            btnGuardar.Click += BtnGuardar_Click;
            var btnCancelar = new AppButton("Cancelar", ButtonKind.Secondary) { DialogResult = DialogResult.Cancel };
            AddFooterButton(btnGuardar);
            AddFooterButton(btnCancelar);
            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private void CargarCombos()
        {
            _categorias = _categoriaRepo.ObtenerTodas();

            var usos = _categorias.Select(c => c.UsoTerapeutico).Distinct().OrderBy(u => u).ToList();
            cmbUsoTerapeutico.Items.Add("Seleccionar...");
            foreach (var uso in usos)
                cmbUsoTerapeutico.Items.Add(uso);
            cmbUsoTerapeutico.SelectedIndex = 0;

            var vias = _categorias.Select(c => c.FormaAdministracion).Distinct().OrderBy(v => v).ToList();
            cmbViaAdministracion.Items.Add("Seleccionar...");
            foreach (var via in vias)
                cmbViaAdministracion.Items.Add(via);
            cmbViaAdministracion.SelectedIndex = 0;

            foreach (var f in _fabricanteRepo.ObtenerTodos())
                txtFabricante.AutoCompleteCustomSource.Add(f.Nombre);
        }

        private void CargarDatosEdicion()
        {
            txtNombre.Text = _productoEditar!.Nombre;
            txtCodigoBarras.Text = _productoEditar.CodigoBarras;
            txtPrecioCompra.Text = _productoEditar.PrecioCompra.ToString();
            txtPrecioVenta.Text = _productoEditar.PrecioVenta.ToString();
            txtStockMinimo.Text = _productoEditar.StockMinimo.ToString();
            txtItbis.Text = _productoEditar!.Itbis == 0 ? "" : _productoEditar.Itbis.ToString();

            var categoria = _categorias.FirstOrDefault(c => c.Id == _productoEditar.CategoriaId);
            if (categoria != null)
            {
                cmbUsoTerapeutico.SelectedItem = categoria.UsoTerapeutico;
                cmbViaAdministracion.SelectedItem = categoria.FormaAdministracion;
            }

            txtFabricante.Text = _productoEditar.FabricanteNombre;
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbUsoTerapeutico.SelectedIndex == 0 || cmbViaAdministracion.SelectedIndex == 0)
            {
                MessageBox.Show("Selecciona la acción terapéutica y la vía de administración.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecioCompra.Text, out decimal precioCompra) ||
                !decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta))
            {
                MessageBox.Show("Los precios deben ser números válidos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtStockMinimo.Text, out int stockMinimo))
            {
                MessageBox.Show("El stock mínimo debe ser un número entero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal itbis = 0;
            if (!string.IsNullOrWhiteSpace(txtItbis.Text))
            {
                if (!decimal.TryParse(txtItbis.Text, out itbis) || itbis < 0 || itbis > 100)
                {
                    MessageBox.Show("El ITBIS debe ser un porcentaje entre 0 y 100.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var usoSeleccionado = cmbUsoTerapeutico.SelectedItem?.ToString() ?? "";
            var viaSeleccionada = cmbViaAdministracion.SelectedItem?.ToString() ?? "";
            var categoria = _categorias.FirstOrDefault(c =>
                c.UsoTerapeutico == usoSeleccionado &&
                c.FormaAdministracion == viaSeleccionada);

            if (categoria == null)
            {
                var nuevaCategoria = new Categoria
                {
                    Nombre = $"{usoSeleccionado} - {viaSeleccionada}",
                    UsoTerapeutico = usoSeleccionado,
                    FormaAdministracion = viaSeleccionada
                };
                _categoriaRepo.Insertar(nuevaCategoria);
                _categorias = _categoriaRepo.ObtenerTodas();
                categoria = _categorias.FirstOrDefault(c =>
                    c.UsoTerapeutico == usoSeleccionado &&
                    c.FormaAdministracion == viaSeleccionada);
            }

            var nombreFabricante = txtFabricante.Text.Trim();
            int fabricanteId = 0;

            if (!string.IsNullOrWhiteSpace(nombreFabricante))
            {
                var fabricantes = _fabricanteRepo.ObtenerTodos();
                var fabricanteExistente = fabricantes.FirstOrDefault(f =>
                    f.Nombre.Equals(nombreFabricante, StringComparison.OrdinalIgnoreCase));

                if (fabricanteExistente != null)
                {
                    fabricanteId = fabricanteExistente.Id;
                }
                else
                {
                    var nuevoFabricante = new Fabricante
                    {
                        Nombre = nombreFabricante,
                        Tipo = "Marca"
                    };
                    _fabricanteRepo.Insertar(nuevoFabricante);
                    fabricanteId = _fabricanteRepo.ObtenerTodos()
                        .First(f => f.Nombre.Equals(nombreFabricante, StringComparison.OrdinalIgnoreCase)).Id;
                }
            }

            var producto = new Producto
            {
                Id = _productoEditar?.Id ?? 0,
                Nombre = txtNombre.Text.Trim(),
                CodigoBarras = txtCodigoBarras.Text.Trim(),
                PrecioCompra = precioCompra,
                PrecioVenta = precioVenta,
                StockMinimo = stockMinimo,
                Itbis = itbis,
                CategoriaId = categoria?.Id ?? 0,
                FabricanteId = fabricanteId
            };

            if (_productoEditar == null)
            {
                if (!int.TryParse(txtCantidadLote.Text, out int cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("La cantidad del lote debe ser mayor a 0.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var lote = new Lote
                {
                    Cantidad = cantidad,
                    NumeroLote = txtNumeroLote.Text.Trim(),
                    FechaVencimiento = dtpVencimiento.Value,
                    FechaEntrada = DateTime.Today
                };

                _productoRepo.Insertar(producto, lote);
                MessageBox.Show("Producto registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _productoRepo.Actualizar(producto);
                MessageBox.Show("Producto actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
