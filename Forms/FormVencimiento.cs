using FarmedicalService.Database;
using FarmedicalService.Models;
using FarmedicalService.UI;

namespace FarmedicalService.Forms
{
    public class FormVencimiento : PageForm
    {
        private DataGridView dgvVencimientos = null!;
        private Label lblResumen = null!;
        private Label lblVacio = null!;
        private AppCombo cmbDias = null!;
        private bool _cargando = true;
        private List<Lote> _lotes = new();
        private readonly LoteRepository _loteRepo = new();

        public FormVencimiento() : base("Vencimientos", "Control de lotes vencidos y próximos a vencer")
        {
            InicializarComponentes();
            _cargando = false;
            CargarDatos(DiasSeleccionados());
        }

        private void InicializarComponentes()
        {
            var btnRetirar = new AppButton("Retirar lote", ButtonKind.DangerOutline);
            btnRetirar.Click += (s, e) => RetirarLoteSeleccionado();
            AddAction(btnRetirar);

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

            // Filtro
            cmbDias = Ui.Combo();
            cmbDias.Items.AddRange(new object[] { "30 días", "60 días", "90 días", "Todos los lotes" });
            cmbDias.SelectedIndex = 0;
            cmbDias.SelectedIndexChanged += (s, e) => { if (!_cargando) CargarDatos(DiasSeleccionados()); };

            var filtros = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                BackColor = Theme.Surface
            };
            filtros.Controls.Add(Ui.Field("Mostrar lotes que vencen en los próximos", cmbDias, 220));

            var toolbar = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 10,
                Height = Ui.S(92),
                Padding = Ui.P(20, 14, 20, 0),
                Margin = new Padding(0, 0, 0, Ui.S(14))
            };
            toolbar.Controls.Add(filtros);

            // Resumen
            lblResumen = new Label
            {
                Dock = DockStyle.Fill,
                Height = Ui.S(46),
                AutoSize = false,
                Font = Theme.BodyBold,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = Ui.P(18, 0, 18, 0),
                Margin = new Padding(0, 0, 0, Ui.S(14))
            };

            // Tabla
            dgvVencimientos = new DataGridView { Dock = DockStyle.Fill };
            Ui.StyleGrid(dgvVencimientos);
            dgvVencimientos.CellPainting += DgvVencimientos_CellPainting;
            lblVacio = Ui.EmptyLabel(dgvVencimientos, "No hay lotes en este período.");

            var gridCard = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 10,
                Padding = Ui.P(6),
                Margin = Padding.Empty
            };
            gridCard.Controls.Add(dgvVencimientos);

            root.Controls.Add(toolbar, 0, 0);
            root.Controls.Add(lblResumen, 0, 1);
            root.Controls.Add(gridCard, 0, 2);
            Body.Controls.Add(root);
        }

        private int DiasSeleccionados() => cmbDias.SelectedIndex switch
        {
            0 => 30,
            1 => 60,
            2 => 90,
            _ => 3650
        };

        private static string EstadoDe(Lote l)
        {
            if (l.DiasParaVencer < 0) return "Vencido";
            if (l.DiasParaVencer == 0) return "Vence hoy";
            if (l.DiasParaVencer <= 30) return "Por vencer";
            return "Vigente";
        }

        private static string DiasTexto(Lote l)
        {
            int d = l.DiasParaVencer;
            if (d < 0) return d == -1 ? "Hace 1 día" : $"Hace {-d} días";
            if (d == 0) return "Hoy";
            return d == 1 ? "1 día" : $"{d} días";
        }

        private void CargarDatos(int dias)
        {
            // El repositorio ya los entrega ordenados por fecha; se reafirma por días restantes
            _lotes = _loteRepo.ObtenerProximosAVencer(dias).OrderBy(l => l.DiasParaVencer).ToList();

            dgvVencimientos.Columns.Clear();
            Ui.AddColumn(dgvVencimientos, "Id", "ID", visible: false);
            Ui.AddColumn(dgvVencimientos, "Medicamento", "Medicamento", 220);
            Ui.AddColumn(dgvVencimientos, "NumeroLote", "Número de lote", 120);
            Ui.AddColumn(dgvVencimientos, "Cantidad", "Cantidad", 80, right: true);
            Ui.AddColumn(dgvVencimientos, "Vencimiento", "Fecha de vencimiento", 130, right: true);
            Ui.AddColumn(dgvVencimientos, "Dias", "Días restantes", 120, right: true);
            Ui.AddColumn(dgvVencimientos, "Estado", "Estado", 120);

            dgvVencimientos.Rows.Clear();

            foreach (var l in _lotes)
            {
                dgvVencimientos.Rows.Add(
                    l.Id,
                    l.ProductoNombre,
                    string.IsNullOrWhiteSpace(l.NumeroLote) ? "Sin número" : l.NumeroLote,
                    l.Cantidad,
                    l.FechaVencimiento.ToString("dd/MM/yyyy"),
                    DiasTexto(l),
                    EstadoDe(l));
            }

            lblVacio.Visible = dgvVencimientos.Rows.Count == 0;
            lblVacio.BringToFront();

            int vencidos = _lotes.Count(l => l.DiasParaVencer < 0);
            int hoy = _lotes.Count(l => l.DiasParaVencer == 0);
            int porVencer = _lotes.Count(l => l.DiasParaVencer > 0 && l.DiasParaVencer <= 30);
            var periodo = dias >= 3650 ? "en el inventario" : $"en los próximos {dias} días";

            if (vencidos + hoy > 0)
            {
                lblResumen.Text = $"Alerta: {vencidos} lote(s) vencido(s), {hoy} que vence(n) hoy y {porVencer} por vencer";
                lblResumen.ForeColor = Theme.DangerDark;
                lblResumen.BackColor = Theme.DangerSoft;
            }
            else if (porVencer > 0)
            {
                lblResumen.Text = $"Atención: {porVencer} lote(s) por vencer {periodo}";
                lblResumen.ForeColor = Theme.WarningDark;
                lblResumen.BackColor = Theme.WarningSoft;
            }
            else
            {
                lblResumen.Text = $"Todo en orden: ningún lote vence {periodo}";
                lblResumen.ForeColor = Theme.SuccessDark;
                lblResumen.BackColor = Theme.SuccessSoft;
            }
        }

        private void DgvVencimientos_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvVencimientos.Columns[e.ColumnIndex].Name != "Estado") return;

            switch (e.Value?.ToString())
            {
                case "Vencido":
                case "Vence hoy":
                    Ui.PintarInsignia(e, e.Value!.ToString()!, Theme.DangerSoft, Theme.DangerDark);
                    break;
                case "Por vencer":
                    Ui.PintarInsignia(e, "Por vencer", Theme.WarningSoft, Theme.WarningDark);
                    break;
                case "Vigente":
                    Ui.PintarInsignia(e, "Vigente", Theme.SuccessSoft, Theme.SuccessDark);
                    break;
            }
        }

        private void RetirarLoteSeleccionado()
        {
            if (dgvVencimientos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un lote primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var loteId = Convert.ToInt32(dgvVencimientos.SelectedRows[0].Cells["Id"].Value);
            var lote = _lotes.FirstOrDefault(l => l.Id == loteId);
            if (lote == null) return;

            var motivo = InputDialog.Ask(
                "Retirar lote",
                $"Se descontarán {lote.Cantidad} unidades de {lote.ProductoNombre} (lote {lote.NumeroLote}) del inventario.\nMotivo del retiro:",
                password: false,
                textoBoton: "Retirar lote");

            if (motivo == null) return;  

            if (string.IsNullOrWhiteSpace(motivo))
                motivo = lote.Vencido ? "Descarte por vencimiento" : "Retiro de lote";

            _loteRepo.RetirarLote(lote.Id, lote.ProductoId, lote.Cantidad, motivo.Trim(), Program.UsuarioActual!.Id);
            CargarDatos(DiasSeleccionados());
        }
    }
}
