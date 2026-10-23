using System.Runtime.InteropServices;

namespace FarmedicalService.UI
{
    
    /// Base de los cuadros de diálogo: cabecera propia (sin barra de título nativa), contenido con scroll y barra de botones al pie (acciones secundarias a la izquierda, principales a la derecha).

    public abstract class DialogForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 2;

        protected readonly Panel Content;
        private readonly FlowLayoutPanel _footerLeft;
        private readonly FlowLayoutPanel _footerRight;

        protected DialogForm(string title, int width, int height)
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = title;
            Font = Theme.Body;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Theme.Border;          // el marco de 1 px
            Padding = new Padding(1);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            int cabecera = Ui.S(58);
            int maxHeight = (Screen.PrimaryScreen?.WorkingArea.Height ?? 768) - Ui.S(60);
            ClientSize = new Size(Ui.S(width), Math.Min(Ui.S(height) + cabecera, maxHeight));

            var shell = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };

            // Cabecera
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = cabecera,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(Ui.S(28), 0, Ui.S(14), 0),
                BackColor = Theme.Surface
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var lblTitulo = Ui.Lbl(title, Theme.Heading, Theme.Text);
            lblTitulo.AutoSize = false;
            lblTitulo.AutoEllipsis = true;
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;

            var cerrar = new CloseButton { Anchor = AnchorStyles.None };
            cerrar.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            header.Controls.Add(lblTitulo, 0, 0);
            header.Controls.Add(cerrar, 1, 0);

            MouseEventHandler arrastrar = (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            };
            header.MouseDown += arrastrar;
            lblTitulo.MouseDown += arrastrar;

            var lineaSuperior = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Theme.Border };
            var lineaInferior = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Theme.Border };

            // Pie
            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 1,
                Padding = Ui.P(24, 14, 24, 14),
                BackColor = Theme.SurfaceAlt
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _footerLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.SurfaceAlt,
                Margin = Padding.Empty
            };
            _footerRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.SurfaceAlt,
                Margin = Padding.Empty
            };
            footer.Controls.Add(_footerLeft, 0, 0);
            footer.Controls.Add(_footerRight, 1, 0);

            Content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = Ui.P(28, 24, 28, 12),
                BackColor = Theme.Surface,
                AutoScroll = true
            };

            // Orden de acoplamiento: lo último agregado queda más cerca del borde
            shell.Controls.Add(Content);
            shell.Controls.Add(lineaSuperior);
            shell.Controls.Add(lineaInferior);
            shell.Controls.Add(header);
            shell.Controls.Add(footer);
            Controls.Add(shell);
        }

        ///Botones de acción a la derecha: el primero agregado queda en el extremo derecho.
        protected void AddFooterButton(Control b)
        {
            b.Margin = new Padding(Ui.S(10), 0, 0, 0);
            _footerRight.Controls.Add(b);
        }

        ///Acciones secundarias a la izquierda del pie
        protected void AddFooterLeft(Control b)
        {
            b.Margin = new Padding(0, 0, Ui.S(10), 0);
            _footerLeft.Controls.Add(b);
        }
    }
}
