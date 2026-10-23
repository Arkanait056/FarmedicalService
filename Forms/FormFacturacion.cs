using FarmedicalService.Database;
using FarmedicalService.Models;
using FarmedicalService.UI;

namespace FarmedicalService.Forms
{
    public class FormFacturacion : PageForm
    {
        private TextBox txtBuscarProducto = null!;
        private DataGridView dgvProductos = null!;
        private DataGridView dgvDetalle = null!;
        private TextBox txtCantidad = null!;
        private AppButton btnAgregar = null!;
        private AppButton btnEliminarItem = null!;
        private AppButton btnCobrar = null!;
        private AppButton btnCancelar = null!;
        private Label lblSubtotal = null!;
        private Label lblItbis = null!;
        private Label lblTotal = null!;
        private Label lblNumeroFactura = null!;
        private Label lblStockInfo = null!;
        private Label lblDetalleVacio = null!;

        private readonly ProductoRepository _productoRepo = new();
        private readonly FacturaRepository _facturaRepo = new();
        private readonly LoteRepository _loteRepo = new();
        private readonly List<DetalleFactura> _detalles = new();
        private readonly Dictionary<int, string> _numerosLote = new();
        private List<Producto> _productos = new();

        public FormFacturacion() : base("Facturación", "Registra una nueva venta")
        {
            InicializarComponentes();
            CargarProductos();
            lblNumeroFactura.Text = $"Factura {_facturaRepo.GenerarNumeroFactura()}";
            ActualizarDetalle();
            Load += (s, e) => BeginInvoke((Action)(() => txtBuscarProducto.Focus()));
        }

        // Atajos de teclado: F2 busca, F9 cobra
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F2)
            {
                txtBuscarProducto.Focus();
                txtBuscarProducto.SelectAll();
                return true;
            }
            if (keyData == Keys.F9)
            {
                Cobrar();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static Label Chip(string texto, Color fondo, Color letra)
        {
            return new Label
            {
                Text = texto,
                Font = Theme.BodyBold,
                ForeColor = letra,
                BackColor = fondo,
                AutoSize = true,
                Padding = Ui.P(14, 8, 14, 8),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private void InicializarComponentes()
        {
            lblNumeroFactura = Chip("Factura", Theme.AccentSoft, Theme.AccentHover);
            AddAction(lblNumeroFactura);
            AddAction(Chip(DateTime.Now.ToString("dd/MM/yyyy HH:mm"), Theme.NeutralSoft, Theme.Neutral));

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Background
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(ConstruirPanelProductos(), 0, 0);
            root.Controls.Add(ConstruirPanelFactura(), 1, 0);
            Body.Controls.Add(root);
        }

        //  Izquierda: catálogo 
        private Control ConstruirPanelProductos()
        {
            var card = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 12,
                Padding = Ui.P(20),
                Margin = new Padding(0, 0, Ui.S(10), 0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var titulo = Ui.Lbl("Medicamentos", Theme.Heading, Theme.Text);
            titulo.Margin = new Padding(0, 0, 0, Ui.S(12));

            txtBuscarProducto = Ui.Txt("Buscar por nombre  (F2)");
            var marcoBusqueda = new FieldFrame(txtBuscarProducto)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, Ui.S(12))
            };
            txtBuscarProducto.TextChanged += (s, e) => FiltrarProductos();

            dgvProductos = new DataGridView { Dock = DockStyle.Fill };
            Ui.StyleGrid(dgvProductos);
            dgvProductos.DoubleClick += (s, e) => AgregarProducto();

            // Cantidad + botón agregar
            txtCantidad = Ui.Txt();
            txtCantidad.Font = new Font("Segoe UI", 12f);
            txtCantidad.Text = "1";
            txtCantidad.TextAlign = HorizontalAlignment.Center;
            txtCantidad.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    AgregarProducto();
                    e.SuppressKeyPress = true;
                }
            };

            btnAgregar = new AppButton("Agregar a factura (Enter)", ButtonKind.Secondary)
            {
                AutoSize = false,
                Height = Ui.S(40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 0, 0, Ui.S(12))
            };
            btnAgregar.Click += (s, e) => AgregarProducto();

            var fila = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Surface,
                Margin = new Padding(0, Ui.S(14), 0, 0)
            };
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            fila.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fila.Controls.Add(Ui.Field("Cantidad", txtCantidad, 90), 0, 0);
            fila.Controls.Add(btnAgregar, 1, 0);

            lblStockInfo = Ui.Lbl("", Theme.Small, Theme.TextMuted);
            lblStockInfo.MaximumSize = new Size(Ui.S(420), 0);

            layout.Controls.Add(titulo, 0, 0);
            layout.Controls.Add(marcoBusqueda, 0, 1);
            layout.Controls.Add(dgvProductos, 0, 2);
            layout.Controls.Add(fila, 0, 3);
            layout.Controls.Add(lblStockInfo, 0, 4);
            card.Controls.Add(layout);
            return card;
        }

        // Derecha: detalle y cobro 
        private Control ConstruirPanelFactura()
        {
            var card = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 12,
                Padding = Ui.P(20),
                Margin = new Padding(Ui.S(10), 0, 0, 0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Encabezado con botón "Quitar"
            var encabezado = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Surface,
                Margin = new Padding(0, 0, 0, Ui.S(12))
            };
            encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            encabezado.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var titulo = Ui.Lbl("Detalle de la factura", Theme.Heading, Theme.Text);
            titulo.Anchor = AnchorStyles.Left;
            btnEliminarItem = new AppButton("Quitar seleccionado", ButtonKind.Secondary, compact: true)
            {
                Anchor = AnchorStyles.Right
            };
            btnEliminarItem.Click += (s, e) => QuitarProducto();
            encabezado.Controls.Add(titulo, 0, 0);
            encabezado.Controls.Add(btnEliminarItem, 1, 0);

            dgvDetalle = new DataGridView { Dock = DockStyle.Fill };
            Ui.StyleGrid(dgvDetalle);
            lblDetalleVacio = Ui.EmptyLabel(dgvDetalle, "Agrega un medicamento para comenzar");

            // Totales (el total va en el color de marca)
            var totales = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.SurfaceAlt,
                Padding = Ui.P(20, 14, 20, 14),
                Margin = new Padding(0, Ui.S(14), 0, Ui.S(14))
            };
            totales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            totales.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < 3; i++)
                totales.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            lblSubtotal = Ui.Lbl(Ui.Money(0), Theme.Num, Theme.Text);
            lblItbis = Ui.Lbl(Ui.Money(0), Theme.Num, Theme.Text);
            lblTotal = Ui.Lbl(Ui.Money(0), Theme.NumSize(20f), Theme.Accent);
            lblSubtotal.Anchor = AnchorStyles.Right;
            lblItbis.Anchor = AnchorStyles.Right;
            lblTotal.Anchor = AnchorStyles.Right;

            var tSub = Ui.Lbl("Subtotal", Theme.Body, Theme.TextMuted);
            var tItb = Ui.Lbl("ITBIS", Theme.Body, Theme.TextMuted);
            var tTot = Ui.Lbl("Total a pagar", Theme.BodyBold, Theme.Text);
            tSub.Margin = new Padding(0, 0, 0, Ui.S(4));
            tItb.Margin = new Padding(0, 0, 0, Ui.S(8));
            lblSubtotal.Margin = new Padding(0, 0, 0, Ui.S(4));
            lblItbis.Margin = new Padding(0, 0, 0, Ui.S(8));
            tSub.Anchor = AnchorStyles.Left;
            tItb.Anchor = AnchorStyles.Left;
            tTot.Anchor = AnchorStyles.Left;

            totales.Controls.Add(tSub, 0, 0);
            totales.Controls.Add(lblSubtotal, 1, 0);
            totales.Controls.Add(tItb, 0, 1);
            totales.Controls.Add(lblItbis, 1, 1);
            totales.Controls.Add(tTot, 0, 2);
            totales.Controls.Add(lblTotal, 1, 2);

            // Botones de acción (el primero queda a la derecha)
            btnCobrar = new AppButton("Cobrar (F9)", ButtonKind.Primary)
            {
                AutoSize = false,
                Size = Ui.Sz(220, 54),
                Font = Theme.Heading
            };
            btnCobrar.Click += (s, e) => Cobrar();

            btnCancelar = new AppButton("Cancelar venta", ButtonKind.Secondary)
            {
                AutoSize = false,
                Size = Ui.Sz(170, 54),
                Margin = new Padding(0, 0, Ui.S(12), 0)
            };
            btnCancelar.Click += (s, e) => CancelarVenta();

            var acciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Surface
            };
            acciones.Controls.Add(btnCobrar);
            acciones.Controls.Add(btnCancelar);

            layout.Controls.Add(encabezado, 0, 0);
            layout.Controls.Add(dgvDetalle, 0, 1);
            layout.Controls.Add(totales, 0, 2);
            layout.Controls.Add(acciones, 0, 3);
            card.Controls.Add(layout);
            return card;
        }

        // ---------------- Lógica ----------------
        private void CargarProductos()
        {
            _productos = _productoRepo.ObtenerTodos();
            MostrarProductos(_productos);
        }

        private void FiltrarProductos()
        {
            var filtrados = _productos
                .Where(p => p.Nombre.Contains(txtBuscarProducto.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
            MostrarProductos(filtrados);
        }

        private void MostrarProductos(List<Producto> productos)
        {
            dgvProductos.Columns.Clear();
            Ui.AddColumn(dgvProductos, "Id", "ID", visible: false);
            Ui.AddColumn(dgvProductos, "Nombre", "Medicamento", 200);
            Ui.AddColumn(dgvProductos, "Precio", "Precio", 90, right: true);
            Ui.AddColumn(dgvProductos, "Stock", "Stock", 60, right: true);

            dgvProductos.Rows.Clear();

            foreach (var p in productos)
            {
                var fila = dgvProductos.Rows.Add(p.Id, p.Nombre, Ui.Money(p.PrecioVenta), p.StockActual);
                if (p.StockActual == 0)
                    dgvProductos.Rows[fila].DefaultCellStyle.ForeColor = Theme.DangerDark;
                else if (p.StockActual <= p.StockMinimo)
                    dgvProductos.Rows[fila].DefaultCellStyle.ForeColor = Theme.WarningDark;
            }
        }

        private int Reservado(int loteId) => _detalles.Where(d => d.LoteId == loteId).Sum(d => d.Cantidad);

        private void AgregarProducto()
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un medicamento.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Id"].Value);
            var producto = _productos.First(p => p.Id == id);

            var lotes = _loteRepo.ObtenerPorProducto(id);
            if (lotes.Count == 0)
            {
                MessageBox.Show($"{producto.Nombre} no tiene existencias.", "Sin stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Solo se vende de lotes vigentes, del que vence primero al que vence después
            var vigentes = lotes.Where(l => !l.Vencido).ToList();
            bool hayVencidos = vigentes.Count < lotes.Count;
            if (vigentes.Count == 0)
            {
                MessageBox.Show(
                    $"Todos los lotes de {producto.Nombre} están vencidos y no se pueden vender.\nRetíralos desde Vencimientos.",
                    "Lotes vencidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int disponible = vigentes.Sum(l => Math.Max(0, l.Cantidad - Reservado(l.Id)));
            if (cantidad > disponible)
            {
                var extra = hayVencidos ? "\n(Hay lotes vencidos que no se pueden vender.)" : "";
                MessageBox.Show(
                    disponible == 0
                        ? $"Ya agregaste todas las unidades disponibles de {producto.Nombre}.{extra}"
                        : $"Solo quedan {disponible} unidades disponibles de {producto.Nombre}.{extra}",
                    "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Reparte la cantidad entre lotes (el que vence primero sale primero)
            var plan = new List<(Lote Lote, int Cantidad)>();
            int restante = cantidad;
            foreach (var lote in vigentes)
            {
                int libre = lote.Cantidad - Reservado(lote.Id);
                if (libre <= 0) continue;
                int toma = Math.Min(libre, restante);
                plan.Add((lote, toma));
                restante -= toma;
                if (restante == 0) break;
            }

            var porVencer = plan.FirstOrDefault(p => p.Lote.ProximoAVencer);
            if (porVencer.Lote != null)
            {
                var loteAviso = porVencer.Lote;
                var cuando = loteAviso.DiasParaVencer == 0 ? "vence hoy" : $"vence en {loteAviso.DiasParaVencer} día(s)";
                var confirm = MessageBox.Show(
                    $"El lote {(string.IsNullOrWhiteSpace(loteAviso.NumeroLote) ? "sin número" : loteAviso.NumeroLote)} de {producto.Nombre} {cuando} ({loteAviso.FechaVencimiento:dd/MM/yyyy}).\n\n¿Agregarlo de todas formas?",
                    "Lote por vencer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;
            }

            foreach (var (lote, toma) in plan)
            {
                _numerosLote[lote.Id] = string.IsNullOrWhiteSpace(lote.NumeroLote) ? "—" : lote.NumeroLote;
                var existente = _detalles.FirstOrDefault(d => d.ProductoId == id && d.LoteId == lote.Id);
                if (existente != null)
                {
                    existente.Cantidad += toma;
                    existente.Subtotal = existente.Cantidad * existente.PrecioUnitario;
                }
                else
                {
                    _detalles.Add(new DetalleFactura
                    {
                        ProductoId = producto.Id,
                        LoteId = lote.Id,
                        Cantidad = toma,
                        PrecioUnitario = producto.PrecioVenta,
                        Subtotal = toma * producto.PrecioVenta,
                        ProductoNombre = producto.Nombre
                    });
                }
            }

            int quedan = disponible - cantidad;
            lblStockInfo.Text = $"{producto.Nombre}: quedan {quedan} unidades disponibles para vender.";
            lblStockInfo.ForeColor = quedan <= producto.StockMinimo ? Theme.WarningDark : Theme.TextMuted;

            ActualizarDetalle();
            txtCantidad.Text = "1";
            txtCantidad.SelectAll();
            txtCantidad.Focus();
        }

        private void QuitarProducto()
        {
            if (dgvDetalle.SelectedRows.Count == 0) return;
            var idx = dgvDetalle.SelectedRows[0].Index;
            if (idx < 0 || idx >= _detalles.Count) return;
            _detalles.RemoveAt(idx);
            ActualizarDetalle();
        }

        private decimal ItbisDe(DetalleFactura d)
        {
            var producto = _productos.FirstOrDefault(p => p.Id == d.ProductoId);
            return d.Subtotal * ((producto?.Itbis ?? 0) / 100);
        }

        private void ActualizarDetalle()
        {
            dgvDetalle.Columns.Clear();
            Ui.AddColumn(dgvDetalle, "Medicamento", "Medicamento", 190);
            Ui.AddColumn(dgvDetalle, "Lote", "Lote", 70);
            Ui.AddColumn(dgvDetalle, "Cantidad", "Cant.", 50, right: true);
            Ui.AddColumn(dgvDetalle, "Precio", "Precio unit.", 95, right: true);
            Ui.AddColumn(dgvDetalle, "ITBIS", "ITBIS", 60, right: true);
            Ui.AddColumn(dgvDetalle, "Subtotal", "Importe", 105, right: true);
            dgvDetalle.Rows.Clear();

            foreach (var d in _detalles)
            {
                var producto = _productos.FirstOrDefault(p => p.Id == d.ProductoId);
                var itbisPct = producto?.Itbis ?? 0;
                dgvDetalle.Rows.Add(
                    d.ProductoNombre,
                    _numerosLote.TryGetValue(d.LoteId, out var num) ? num : "—",
                    d.Cantidad,
                    Ui.Money(d.PrecioUnitario),
                    itbisPct > 0 ? $"{itbisPct}%" : "N/A",
                    Ui.Money(d.Subtotal + ItbisDe(d))
                );
            }

            var subtotal = _detalles.Sum(d => d.Subtotal);
            var itbisTotal = _detalles.Sum(d => ItbisDe(d));
            var total = subtotal + itbisTotal;

            lblSubtotal.Text = Ui.Money(subtotal);
            lblItbis.Text = Ui.Money(itbisTotal);
            lblTotal.Text = Ui.Money(total);

            lblDetalleVacio.Visible = _detalles.Count == 0;
            lblDetalleVacio.BringToFront();
            btnCobrar.Enabled = _detalles.Count > 0;
            btnEliminarItem.Enabled = _detalles.Count > 0;
        }

        private void CancelarVenta()
        {
            if (_detalles.Count > 0)
            {
                var confirm = MessageBox.Show(
                    "Hay productos en la factura. ¿Cancelar la venta?",
                    "Cancelar venta",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
            }
            this.Close();
        }

        private void Cobrar()
        {
            if (_detalles.Count == 0)
            {
                MessageBox.Show("Agrega al menos un producto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var subtotal = _detalles.Sum(d => d.Subtotal);
            var itbisTotal = _detalles.Sum(d => ItbisDe(d));
            var total = subtotal + itbisTotal;

            using var formCobro = new FormCobro(total);
            if (formCobro.ShowDialog() == DialogResult.OK)
            {
                var factura = new Factura
                {
                    NumeroFactura = _facturaRepo.GenerarNumeroFactura(),
                    Fecha = DateTime.Now,
                    Subtotal = subtotal,
                    Itbis = itbisTotal,
                    Total = total,
                    MontoPagado = formCobro.MontoPagado,
                    Cambio = formCobro.Cambio,
                    MetodoPago = formCobro.MetodoPago,
                    Cliente = formCobro.Cliente,
                    UsuarioId = Program.UsuarioActual!.Id,
                    UsuarioNombre = Program.UsuarioActual!.Nombre,
                    Detalles = _detalles
                };

                _facturaRepo.CrearFactura(factura);
                using var formImpresion = new FormImpresion(factura, _detalles);
                formImpresion.ShowDialog();
                this.Close();
            }
        }
    }
}
