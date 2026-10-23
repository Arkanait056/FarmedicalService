using FarmedicalService.Database;
using FarmedicalService.Models;
using FarmedicalService.UI;

namespace FarmedicalService.Forms
{
    public class FormAgregarLote : DialogForm
    {
        private TextBox txtCantidad = null!;
        private TextBox txtNumeroLote = null!;
        private DateTimePicker dtpVencimiento = null!;
        private TextBox txtMotivo = null!;
        private AppButton btnGuardar = null!;
        private RadioButton rbAgregar = null!;
        private RadioButton rbQuitar = null!;
        private TableLayoutPanel campoNumeroLote = null!;
        private TableLayoutPanel campoVencimiento = null!;
        private TableLayoutPanel campoMotivo = null!;
        private DataGridView dgvLotes = null!;

        private readonly LoteRepository _loteRepo = new();
        private readonly int _productoId;

        /// Verdadero si se agregó o descontó stock mientras el diálogo estuvo abierto.
        public bool HuboCambios { get; private set; }

        public FormAgregarLote(int productoId, string productoNombre)
            : base($"Gestión de lotes — {productoNombre}", 660, 640)
        {
            _productoId = productoId;
            InicializarComponentes();
            CargarLotes();
            ActualizarModo();
        }

        private void InicializarComponentes()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            for (int i = 0; i < 5; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Modo: agregar o quitar
            rbAgregar = new RadioButton
            {
                Text = "Agregar stock (nueva entrada)",
                Font = Theme.BodyBold,
                ForeColor = Theme.SuccessDark,
                AutoSize = true,
                Checked = true,
                Margin = new Padding(0, 0, Ui.S(28), 0)
            };
            rbQuitar = new RadioButton
            {
                Text = "Quitar stock (descarte)",
                Font = Theme.BodyBold,
                ForeColor = Theme.DangerDark,
                AutoSize = true,
                Margin = Padding.Empty
            };
            rbAgregar.CheckedChanged += (s, e) => ActualizarModo();
            rbQuitar.CheckedChanged += (s, e) => ActualizarModo();

            var modo = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                BackColor = Theme.SurfaceAlt,
                Padding = Ui.P(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, Ui.S(16))
            };
            modo.Controls.Add(rbAgregar);
            modo.Controls.Add(rbQuitar);
            layout.Controls.Add(modo, 0, 0);
            layout.SetColumnSpan(modo, 2);

            // Campos
            txtCantidad = Ui.Txt("0");
            txtNumeroLote = Ui.Txt("Opcional");
            dtpVencimiento = Ui.Fecha();
            txtMotivo = Ui.Txt("Opcional");

            var campoCantidad = Ui.Field("Cantidad", txtCantidad, fill: true);
            campoNumeroLote = Ui.Field("Número de lote (opcional)", txtNumeroLote, fill: true);
            campoVencimiento = Ui.Field("Fecha de vencimiento", dtpVencimiento, fill: true);
            campoMotivo = Ui.Field("Motivo del descarte o ajuste (opcional)", txtMotivo, fill: true);

            layout.Controls.Add(campoCantidad, 0, 1);
            layout.Controls.Add(campoNumeroLote, 1, 1);
            layout.Controls.Add(campoVencimiento, 0, 2);
            layout.SetColumnSpan(campoVencimiento, 2);
            layout.Controls.Add(campoMotivo, 0, 3);
            layout.SetColumnSpan(campoMotivo, 2);

            // Lotes actuales
            var titulo = Ui.Lbl("Lotes actuales (selecciona uno para quitar stock)", Theme.SmallBold, Theme.TextMuted);
            titulo.Margin = new Padding(0, Ui.S(4), 0, Ui.S(8));

            dgvLotes = new DataGridView { Dock = DockStyle.Fill };
            Ui.StyleGrid(dgvLotes);
            var cardLotes = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 10,
                Height = Ui.S(210),
                Padding = Ui.P(6),
                Margin = Padding.Empty
            };
            cardLotes.Controls.Add(dgvLotes);

            var bloqueLotes = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Surface
            };
            bloqueLotes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            bloqueLotes.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bloqueLotes.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bloqueLotes.Controls.Add(titulo, 0, 0);
            bloqueLotes.Controls.Add(cardLotes, 0, 1);
            layout.Controls.Add(bloqueLotes, 0, 4);
            layout.SetColumnSpan(bloqueLotes, 2);

            Content.Controls.Add(layout);

            btnGuardar = new AppButton("Agregar lote", ButtonKind.Success);
            btnGuardar.Click += BtnGuardar_Click;
            var btnCerrar = new AppButton("Cerrar", ButtonKind.Secondary) { DialogResult = DialogResult.Cancel };
            AddFooterButton(btnGuardar);
            AddFooterButton(btnCerrar);
            CancelButton = btnCerrar;
        }

        private void ActualizarModo()
        {
            bool esAgregar = rbAgregar.Checked;

            campoNumeroLote.Visible = esAgregar;
            campoVencimiento.Visible = esAgregar;
            campoMotivo.Visible = !esAgregar;

            btnGuardar.Kind = esAgregar ? ButtonKind.Success : ButtonKind.Danger;
            btnGuardar.Text = esAgregar ? "Agregar lote" : "Quitar stock";
            btnGuardar.Invalidate();
        }

        private void CargarLotes()
        {
            dgvLotes.Columns.Clear();
            Ui.AddColumn(dgvLotes, "Id", "ID", visible: false);
            Ui.AddColumn(dgvLotes, "NumeroLote", "# Lote", 110);
            Ui.AddColumn(dgvLotes, "Cantidad", "Cantidad", 80, right: true);
            Ui.AddColumn(dgvLotes, "Vencimiento", "Vence", 110);
            Ui.AddColumn(dgvLotes, "Estado", "Estado", 90);

            dgvLotes.Rows.Clear();

            foreach (var l in _loteRepo.ObtenerPorProducto(_productoId))
            {
                string estado = l.Vencido ? "VENCIDO" : l.ProximoAVencer ? "Próximo" : "OK";
                var fila = dgvLotes.Rows.Add(
                    l.Id,
                    l.NumeroLote == "" ? "Sin número" : l.NumeroLote,
                    l.Cantidad,
                    l.FechaVencimiento.ToString("dd/MM/yyyy"),
                    estado
                );
                if (l.Vencido)
                {
                    dgvLotes.Rows[fila].DefaultCellStyle.BackColor = Color.FromArgb(255, 241, 241);
                    dgvLotes.Rows[fila].DefaultCellStyle.ForeColor = Theme.DangerDark;
                }
                else if (l.ProximoAVencer)
                {
                    dgvLotes.Rows[fila].DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 232);
                    dgvLotes.Rows[fila].DefaultCellStyle.ForeColor = Theme.WarningDark;
                }
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (rbAgregar.Checked)
            {
                var lote = new Lote
                {
                    ProductoId = _productoId,
                    NumeroLote = txtNumeroLote.Text.Trim(),
                    Cantidad = cantidad,
                    FechaVencimiento = dtpVencimiento.Value,
                    FechaEntrada = DateTime.Today
                };
                _loteRepo.AgregarLote(lote);
                MessageBox.Show("Lote agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (dgvLotes.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Selecciona el lote del que deseas quitar stock.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var loteId = Convert.ToInt32(dgvLotes.SelectedRows[0].Cells["Id"].Value);
                var cantidadLote = Convert.ToInt32(dgvLotes.SelectedRows[0].Cells["Cantidad"].Value);

                if (cantidad > cantidadLote)
                {
                    MessageBox.Show($"Solo hay {cantidadLote} unidades en ese lote.", "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var motivo = string.IsNullOrWhiteSpace(txtMotivo.Text) ? "Descarte o ajuste de inventario" : txtMotivo.Text.Trim();
                _loteRepo.RetirarLote(loteId, _productoId, cantidad, motivo, Program.UsuarioActual!.Id);
                MessageBox.Show("Stock descontado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            CargarLotes();
            txtCantidad.Clear();
            HuboCambios = true;
        }
    }
}
