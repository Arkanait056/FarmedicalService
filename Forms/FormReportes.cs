using FarmedicalService.Database;
using FarmedicalService.Models;
using FarmedicalService.UI;

namespace FarmedicalService.Forms
{
    public class FormReportes : PageForm
    {
        private KpiCard _kpiTotalHoy = null!;
        private KpiCard _kpiFacturas = null!;
        private KpiCard _kpiMasVendido = null!;
        private KpiCard _kpiMes = null!;
        private DataGridView dgvFacturas = null!;
        private DataGridView dgvMes = null!;
        private BarChart _grafica = null!;
        private AppButton btnCierreDia = null!;
        private readonly List<AppButton> _tabs = new();
        private readonly List<Control> _paneles = new();
        private int _tabActual;
        private ListBox lstMeses = null!;
        private Label lblTotalMesHistorico = null!;
        private Label lblVaciaHoy = null!;
        private Label lblVaciaMes = null!;
        private readonly Dictionary<int, Factura> _facturas = new();

        private readonly FacturaRepository _facturaRepo = new();
        private readonly CierreDiaRepository _cierreRepo = new();

        public FormReportes() : base("Reportes", "Ventas del día, del mes y cierre de caja")
        {
            InicializarComponentes();
            CargarDatos();
        }

        private void InicializarComponentes()
        {
            var btnAnular = new AppButton("Anular factura", ButtonKind.DangerOutline);
            btnAnular.Click += (s, e) => AnularFacturaSeleccionada();
            var btnExportar = new AppButton("Exportar a Excel", ButtonKind.Secondary);
            btnExportar.Click += (s, e) => Exportar();
            btnCierreDia = new AppButton("Cierre del día", ButtonKind.Primary);
            btnCierreDia.Click += BtnCierreDia_Click;
            AddAction(btnAnular);
            AddAction(btnExportar);
            AddAction(btnCierreDia);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Background
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            root.Controls.Add(ConstruirIndicadores(), 0, 0);
            root.Controls.Add(ConstruirPestanas(), 0, 1);

            var contenedor = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            _paneles.Add(ConstruirPanelHoy());
            _paneles.Add(ConstruirPanelMes());
            _paneles.Add(ConstruirPanelGrafica());
            foreach (var p in _paneles) contenedor.Controls.Add(p);
            root.Controls.Add(contenedor, 0, 2);

            Body.Controls.Add(root);
            MostrarPestana(0);
        }

        private Control ConstruirIndicadores()
        {
            _kpiTotalHoy = new KpiCard("Vendido hoy", Ui.Money(0), "Facturas no anuladas del día",
                "cash", Theme.Neutral, Theme.NeutralSoft);
            _kpiFacturas = new KpiCard("Facturas de hoy", "0", "Emitidas en el día",
                "receipt", Theme.Neutral, Theme.NeutralSoft);
            _kpiMasVendido = new KpiCard("Más vendido del mes", "Sin ventas", "Producto con más unidades",
                "star", Theme.Neutral, Theme.NeutralSoft, valueSize: 15f);
            _kpiMes = new KpiCard("Vendido este mes", Ui.Money(0), "Todas las facturas del mes, incluido hoy",
                "trend", Theme.Neutral, Theme.NeutralSoft);

            var cards = new[] { _kpiTotalHoy, _kpiFacturas, _kpiMasVendido, _kpiMes };
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = cards.Length,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Background,
                Margin = new Padding(0, 0, 0, Ui.S(18))
            };
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            for (int i = 0; i < cards.Length; i++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cards.Length));
                cards[i].Height = Ui.S(116);
                cards[i].Dock = DockStyle.Fill;
                cards[i].Margin = new Padding(0, 0, i == cards.Length - 1 ? 0 : Ui.S(16), 0);
                grid.Controls.Add(cards[i], i, 0);
            }
            return grid;
        }

        private Control ConstruirPestanas()
        {
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                BackColor = Theme.Background,
                Margin = new Padding(0, 0, 0, Ui.S(12))
            };

            var nombres = new[] { "Facturas de hoy", "Historial por mes", "Ventas por día" };
            for (int i = 0; i < nombres.Length; i++)
            {
                int indice = i;
                var tab = new AppButton(nombres[i], ButtonKind.Secondary, compact: true);
                tab.Margin = new Padding(0, 0, Ui.S(8), 0);
                tab.Click += (s, e) => MostrarPestana(indice);
                _tabs.Add(tab);
                flow.Controls.Add(tab);
            }
            return flow;
        }

        private void MostrarPestana(int indice)
        {
            _tabActual = indice;
            for (int i = 0; i < _paneles.Count; i++)
            {
                _paneles[i].Visible = i == indice;
                _tabs[i].Kind = i == indice ? ButtonKind.Primary : ButtonKind.Secondary;
                _tabs[i].Invalidate();
            }
        }

        private Control ConstruirPanelHoy()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            dgvFacturas = new DataGridView { Dock = DockStyle.Fill };
            Ui.StyleGrid(dgvFacturas);
            dgvFacturas.CellPainting += DgvFacturas_CellPainting;
            lblVaciaHoy = Ui.EmptyLabel(dgvFacturas, "Todavía no hay facturas hoy.");
            var card = new Card { Dock = DockStyle.Fill, Radius = 10, Padding = Ui.P(6) };
            card.Controls.Add(dgvFacturas);
            panel.Controls.Add(card);
            return panel;
        }

        private Control ConstruirPanelMes()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Background
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Ui.S(230)));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lstMeses = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Surface,
                DrawMode = DrawMode.OwnerDrawFixed,
                IntegralHeight = false,
                Font = Theme.Body
            };
            lstMeses.ItemHeight = TextRenderer.MeasureText("Ag", Theme.BodyBold).Height + Ui.S(18);

            var mesInicio = new DateTime(2026, 3, 1);
            var mesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            for (var mes = mesActual; mes >= mesInicio; mes = mes.AddMonths(-1))
                lstMeses.Items.Add(mes);

            lstMeses.DrawItem += LstMeses_DrawItem;
            lstMeses.SelectedIndexChanged += LstMeses_SelectedIndexChanged;

            var cardMeses = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 10,
                Padding = Ui.P(8),
                Margin = new Padding(0, 0, Ui.S(14), 0)
            };
            cardMeses.Controls.Add(lstMeses);

            var derecha = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Surface
            };
            derecha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            derecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            derecha.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblTotalMesHistorico = Ui.Lbl("", Theme.BodyBold, Theme.Text);
            lblTotalMesHistorico.Margin = new Padding(Ui.S(12), Ui.S(10), 0, Ui.S(10));

            dgvMes = new DataGridView { Dock = DockStyle.Fill };
            Ui.StyleGrid(dgvMes);
            dgvMes.CellPainting += DgvFacturas_CellPainting;
            lblVaciaMes = Ui.EmptyLabel(dgvMes, "No hay facturas en este mes.");

            derecha.Controls.Add(lblTotalMesHistorico, 0, 0);
            derecha.Controls.Add(dgvMes, 0, 1);

            var cardDetalle = new Card { Dock = DockStyle.Fill, Radius = 10, Padding = Ui.P(6) };
            cardDetalle.Controls.Add(derecha);

            layout.Controls.Add(cardMeses, 0, 0);
            layout.Controls.Add(cardDetalle, 1, 0);
            panel.Controls.Add(layout);
            return panel;
        }

        private Control ConstruirPanelGrafica()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            var card = new Card { Dock = DockStyle.Fill, Radius = 10, Padding = Ui.P(22, 18, 22, 14) };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var titulo = Ui.Lbl("Ventas de los últimos 14 días (RD$)", Theme.Heading, Theme.Text);
            titulo.Margin = new Padding(0, 0, 0, Ui.S(6));
            _grafica = new BarChart { Dock = DockStyle.Fill };

            layout.Controls.Add(titulo, 0, 0);
            layout.Controls.Add(_grafica, 0, 1);
            card.Controls.Add(layout);
            panel.Controls.Add(card);
            return panel;
        }

        private void LstMeses_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var fondo = new SolidBrush(seleccionado ? Theme.Selection : Theme.Surface))
                e.Graphics.FillRectangle(fondo, e.Bounds);

            var mes = (DateTime)lstMeses.Items[e.Index];
            var texto = mes.ToString("MMMM yyyy", Ui.Cultura);
            if (texto.Length > 0) texto = char.ToUpper(texto[0], Ui.Cultura) + texto.Substring(1);
            var area = Rectangle.Inflate(e.Bounds, -Ui.S(14), 0);
            TextRenderer.DrawText(e.Graphics, texto,
                seleccionado ? Theme.BodyBold : Theme.Body, area, Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        private void ConfigurarColumnasFacturas(DataGridView g)
        {
            g.Columns.Clear();
            Ui.AddColumn(g, "Id", "ID", visible: false);
            Ui.AddColumn(g, "NumeroFactura", "# Factura", 85);
            Ui.AddColumn(g, "Ncf", "NCF", 105);
            Ui.AddColumn(g, "Fecha", "Fecha y hora", 115);
            Ui.AddColumn(g, "Medicamentos", "Medicamentos vendidos", 210);
            Ui.AddColumn(g, "Total", "Total", 95, right: true);
            Ui.AddColumn(g, "Metodo", "Método", 90);
            Ui.AddColumn(g, "Cajero", "Cajero", 90);
            Ui.AddColumn(g, "Estado", "Estado", 90);
        }

        private void LlenarFacturas(DataGridView g, IEnumerable<Factura> facturas)
        {
            g.Rows.Clear();
            foreach (var f in facturas)
            {
                _facturas[f.Id] = f;
                var fila = g.Rows.Add(
                    f.Id,
                    f.NumeroFactura,
                    string.IsNullOrEmpty(f.Ncf) ? "—" : f.Ncf,
                    f.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    f.Medicamentos,
                    Ui.Money(f.Total),
                    f.MetodoPago,
                    f.UsuarioNombre,
                    f.Anulada ? "Anulada" : "");
                if (f.Anulada)
                    g.Rows[fila].DefaultCellStyle.ForeColor = Theme.TextMuted;
            }
        }

        private void DgvFacturas_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is not DataGridView g) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || g.Columns[e.ColumnIndex].Name != "Estado") return;
            if (e.Value?.ToString() == "Anulada")
                Ui.PintarInsignia(e, "Anulada", Theme.DangerSoft, Theme.DangerDark);
        }

        private void CargarDatos()
        {
            var facturasHoy = _facturaRepo.ObtenerFacturasHoy();
            var masVendido = _facturaRepo.ObtenerMasVendidoDelMes();

            _kpiTotalHoy.SetValue(Ui.Money(_facturaRepo.ObtenerTotalVendidoHoy()));
            _kpiFacturas.SetValue(facturasHoy.Count(f => !f.Anulada).ToString());
            _kpiMasVendido.SetValue(masVendido.Cantidad > 0
                ? $"{masVendido.Producto} ({masVendido.Cantidad})"
                : "Sin ventas");
            _kpiMes.SetValue(Ui.Money(_facturaRepo.ObtenerTotalMes()));

            ConfigurarColumnasFacturas(dgvFacturas);
            LlenarFacturas(dgvFacturas, facturasHoy);
            lblVaciaHoy.Visible = dgvFacturas.Rows.Count == 0;
            lblVaciaHoy.BringToFront();

            var cierreHecho = _cierreRepo.ExisteCierreHoy();
            btnCierreDia.Enabled = !cierreHecho;
            btnCierreDia.Text = cierreHecho ? "Cierre realizado" : "Cierre del día";

            CargarGrafica();

            if (lstMeses.Items.Count > 0)
            {
                if (lstMeses.SelectedIndex == 0) LstMeses_SelectedIndexChanged(null, EventArgs.Empty);
                else lstMeses.SelectedIndex = 0;
            }
        }

        private void CargarGrafica()
        {
            var desde = DateTime.Today.AddDays(-13);
            var porDia = _facturaRepo.ObtenerVentasPorDia(desde, DateTime.Today);
            var datos = new List<(string Label, decimal Value)>();
            for (int i = 0; i < 14; i++)
            {
                var dia = desde.AddDays(i);
                porDia.TryGetValue(dia, out var total);
                datos.Add((dia.ToString("dd/MM"), total));
            }
            _grafica.Data = datos;
            _grafica.Invalidate();
        }

        private void LstMeses_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstMeses.SelectedIndex < 0) return;

            var mesSeleccionado = (DateTime)lstMeses.Items[lstMeses.SelectedIndex];
            var facturasMes = _facturaRepo.ObtenerFacturasPorFecha(mesSeleccionado.ToString("yyyy-MM"));
            var vigentes = facturasMes.Where(f => !f.Anulada).ToList();

            var nombreMes = mesSeleccionado.ToString("MMMM yyyy", Ui.Cultura);
            lblTotalMesHistorico.Text =
                $"{nombreMes}: {Ui.Money(vigentes.Sum(f => f.Total))} en {vigentes.Count} factura(s)" +
                (facturasMes.Count > vigentes.Count ? $"  ({facturasMes.Count - vigentes.Count} anulada(s))" : "");

            ConfigurarColumnasFacturas(dgvMes);
            LlenarFacturas(dgvMes, facturasMes);
            lblVaciaMes.Visible = dgvMes.Rows.Count == 0;
            lblVaciaMes.BringToFront();
        }

        private void Exportar()
        {
            switch (_tabActual)
            {
                case 0: Exportador.GuardarCsv(dgvFacturas, "facturas_hoy"); break;
                case 1: Exportador.GuardarCsv(dgvMes, "facturas_mes"); break;
                default:
                    MessageBox.Show("Abre la pestaña \"Facturas de hoy\" o \"Historial por mes\" para exportar una tabla.",
                        "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void AnularFacturaSeleccionada()
        {
            var grid = _tabActual == 1 ? dgvMes : dgvFacturas;
            if (_tabActual == 2 || grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona la factura que deseas anular en \"Facturas de hoy\" o \"Historial por mes\".",
                    "Anular factura", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var id = Convert.ToInt32(grid.SelectedRows[0].Cells["Id"].Value);
            if (!_facturas.TryGetValue(id, out var factura)) return;

            if (factura.Anulada)
            {
                MessageBox.Show("Esta factura ya está anulada.", "Anular factura", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var motivo = InputDialog.Ask(
                "Anular factura",
                $"Se anulará la factura {factura.NumeroFactura} por {Ui.Money(factura.Total)} y las unidades volverán al inventario.\nMotivo de la anulación (obligatorio):",
                password: false,
                textoBoton: "Continuar");

            if (motivo == null) return;
            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show("Debes indicar el motivo de la anulación.", "Anular factura", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Anular definitivamente la factura {factura.NumeroFactura}?\nQuedará registrado quién la anuló y el motivo.",
                "Confirmar anulación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            if (_facturaRepo.Anular(factura.Id, motivo.Trim(), Program.UsuarioActual!.Id))
                CargarDatos();
            else
                MessageBox.Show("No se pudo anular la factura.", "Anular factura", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnCierreDia_Click(object? sender, EventArgs e)
        {
            var totalHoy = _facturaRepo.ObtenerTotalVendidoHoy();
            var facturasHoy = _facturaRepo.ObtenerFacturasHoy().Where(f => !f.Anulada).ToList();
            var masVendido = _facturaRepo.ObtenerMasVendidoDelMes();

            if (facturasHoy.Count == 0)
            {
                MessageBox.Show("No hay facturas registradas hoy.", "Sin ventas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var esperado = _facturaRepo.ObtenerEfectivoDelDia(DateTime.Today);
            var texto = InputDialog.Ask(
                "Cierre del día",
                $"Ventas del día: {Ui.Money(totalHoy)} en {facturasHoy.Count} factura(s).\nEfectivo esperado en caja: {Ui.Money(esperado)}.\nIngresa el efectivo contado:",
                password: false,
                textoBoton: "Continuar");

            if (texto == null) return;
            if (!decimal.TryParse(texto, out decimal contado) || contado < 0)
            {
                MessageBox.Show("Ingresa un monto válido.", "Cierre del día", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var diferencia = contado - esperado;
            var estado = diferencia == 0 ? "La caja cuadra." : diferencia > 0 ? "Hay un sobrante." : "Hay un faltante.";

            var confirm = MessageBox.Show(
                $"Efectivo contado: {Ui.Money(contado)}\nEfectivo esperado: {Ui.Money(esperado)}\nDiferencia: {Ui.Money(diferencia)}\n{estado}\n\n¿Confirmar el cierre del día?",
                "Cierre del día",
                MessageBoxButtons.YesNo,
                diferencia == 0 ? MessageBoxIcon.Question : MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var cierre = new CierreDia
            {
                Fecha = DateTime.Today,
                TotalVentas = totalHoy,
                NumeroFacturas = facturasHoy.Count,
                ProductoMasVendido = masVendido.Producto,
                CantidadMasVendido = masVendido.Cantidad,
                UsuarioId = Program.UsuarioActual!.Id,
                HoraCierre = DateTime.Now.ToString("HH:mm:ss"),
                EfectivoContado = contado,
                Diferencia = diferencia
            };

            _cierreRepo.RegistrarCierre(cierre);
            MessageBox.Show($"Cierre realizado.\nTotal: {Ui.Money(totalHoy)}\nDiferencia de caja: {Ui.Money(diferencia)}",
                "Cierre completado", MessageBoxButtons.OK, MessageBoxIcon.Information);

            CargarDatos();
        }
    }
}
