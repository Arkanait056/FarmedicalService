using FarmedicalService.UI;

namespace FarmedicalService.Forms
{
    public class FormCobro : DialogForm
    {
        private TextBox txtMontoPagado = null!;
        private TextBox txtCliente = null!;
        private AppCombo cmbMetodo = null!;
        private Label lblCambio = null!;
        private Label lblCambioTitulo = null!;
        private AppButton btnConfirmar = null!;

        public decimal MontoPagado { get; private set; }
        public decimal Cambio { get; private set; }
        public string MetodoPago { get; private set; } = "Efectivo";
        public string Cliente { get; private set; } = "";
        private readonly decimal _total;

        public FormCobro(decimal total) : base("Cobro", 480, 570)
        {
            _total = total;
            InicializarComponentes();
        }

        private bool EsEfectivo => cmbMetodo.SelectedItem?.ToString() == "Efectivo";

        private void InicializarComponentes()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // Total a cobrar
            var cardTotal = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 12,
                BackColor = Theme.AccentSoft,
                BorderColor = Theme.AccentSoft,
                Padding = Ui.P(20, 14, 20, 14),
                Height = Ui.S(96),
                Margin = new Padding(0, 0, 0, Ui.S(22))
            };
            var totalLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.AccentSoft
            };
            totalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            totalLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            totalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            totalLayout.Controls.Add(Ui.Lbl("Total a cobrar", Theme.SmallBold, Theme.AccentHover), 0, 0);
            var lblTotal = new Label
            {
                Text = Ui.Money(_total),
                Font = Theme.NumSize(24f),
                ForeColor = Theme.AccentHover,
                AutoSize = false,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
            totalLayout.Controls.Add(lblTotal, 0, 1);
            cardTotal.Controls.Add(totalLayout);

            // Método de pago y cliente
            cmbMetodo = Ui.Combo();
            cmbMetodo.Items.AddRange(new object[] { "Efectivo", "Tarjeta", "Transferencia" });
            cmbMetodo.SelectedIndex = 0;
            cmbMetodo.SelectedIndexChanged += (s, e) => AplicarMetodo();

            txtCliente = Ui.Txt("Nombre o RNC (opcional)");

            var fila1 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Surface
            };
            fila1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            fila1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            fila1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fila1.Controls.Add(Ui.Field("Método de pago", cmbMetodo, fill: true), 0, 0);
            var campoCliente = Ui.Field("Cliente", txtCliente, fill: true);
            campoCliente.Margin = new Padding(0, 0, 0, Ui.S(12));
            fila1.Controls.Add(campoCliente, 1, 0);

            // Monto recibido
            txtMontoPagado = Ui.Txt("0.00");
            txtMontoPagado.Font = Theme.NumSize(14f, FontStyle.Regular);
            txtMontoPagado.TextAlign = HorizontalAlignment.Right;
            txtMontoPagado.TextChanged += (s, e) => CalcularCambio();
            var campoMonto = Ui.Field("Monto recibido (RD$)", txtMontoPagado, fill: true);
            campoMonto.Margin = new Padding(0, 0, 0, Ui.S(8));

            // Cambio
            var cardCambio = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 12,
                BackColor = Theme.SurfaceAlt,
                Padding = Ui.P(20, 12, 20, 12),
                Height = Ui.S(70)
            };
            var cambioLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.SurfaceAlt
            };
            cambioLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            cambioLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cambioLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            lblCambioTitulo = Ui.Lbl("Cambio a devolver", Theme.BodyBold, Theme.TextMuted);
            lblCambioTitulo.Anchor = AnchorStyles.Left;
            lblCambio = Ui.Lbl(Ui.Money(0), Theme.NumSize(16f), Theme.Text);
            lblCambio.Anchor = AnchorStyles.Right;
            cambioLayout.Controls.Add(lblCambioTitulo, 0, 0);
            cambioLayout.Controls.Add(lblCambio, 1, 0);
            cardCambio.Controls.Add(cambioLayout);

            for (int i = 0; i < 4; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(cardTotal, 0, 0);
            layout.Controls.Add(fila1, 0, 1);
            layout.Controls.Add(campoMonto, 0, 2);
            layout.Controls.Add(cardCambio, 0, 3);
            Content.Controls.Add(layout);

            // Pie: acción auxiliar a la izquierda, confirmar a la derecha
            var btnExacto = new AppButton("Monto exacto", ButtonKind.Secondary);
            btnExacto.Click += (s, e) =>
            {
                if (!EsEfectivo) return;
                txtMontoPagado.Text = _total.ToString("0.00");
                txtMontoPagado.Focus();
            };
            AddFooterLeft(btnExacto);

            btnConfirmar = new AppButton("Confirmar cobro", ButtonKind.Primary);
            btnConfirmar.Click += BtnConfirmar_Click;
            var btnCancelar = new AppButton("Cancelar", ButtonKind.Secondary) { DialogResult = DialogResult.Cancel };
            AddFooterButton(btnConfirmar);
            AddFooterButton(btnCancelar);

            AcceptButton = btnConfirmar;
            CancelButton = btnCancelar;
            Shown += (s, e) => txtMontoPagado.Focus();
        }

        private void AplicarMetodo()
        {
            bool efectivo = EsEfectivo;
            txtMontoPagado.Enabled = efectivo;
            if (!efectivo)
            {
                txtMontoPagado.Text = _total.ToString("0.00");
                lblCambioTitulo.Text = "Pago exacto";
            }
            else
            {
                txtMontoPagado.Clear();
                lblCambioTitulo.Text = "Cambio a devolver";
                txtMontoPagado.Focus();
            }
            CalcularCambio();
        }

        private void CalcularCambio()
        {
            if (decimal.TryParse(txtMontoPagado.Text, out decimal monto))
            {
                var cambio = monto - _total;
                lblCambio.Text = Ui.Money(Math.Max(cambio, 0));
                lblCambio.ForeColor = cambio >= 0 ? Theme.Text : Theme.DangerDark;
                if (cambio < 0)
                    lblCambio.Text = $"Faltan {Ui.Money(-cambio)}";
            }
            else
            {
                lblCambio.Text = Ui.Money(0);
                lblCambio.ForeColor = Theme.Text;
            }
        }

        private void BtnConfirmar_Click(object? sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMontoPagado.Text, out decimal monto))
            {
                MessageBox.Show("Ingresa un monto válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (monto < _total)
            {
                MessageBox.Show("El monto recibido es menor al total.", "Monto insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MontoPagado = monto;
            Cambio = EsEfectivo ? monto - _total : 0;
            MetodoPago = cmbMetodo.SelectedItem?.ToString() ?? "Efectivo";
            Cliente = txtCliente.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
