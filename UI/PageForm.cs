namespace FarmedicalService.UI
{
    /// <summary>
    /// Base de las pantallas que se muestran dentro de la ventana principal:
    /// cabecera con título, subtítulo y acciones, más un área de contenido.
    /// </summary>
    public abstract class PageForm : Form
    {
        protected readonly Panel Body;
        private readonly FlowLayoutPanel _actions;

        protected PageForm(string title, string subtitle)
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = title;
            Font = Theme.Body;
            BackColor = Theme.Background;
            DoubleBuffered = true;

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 1,
                Padding = Ui.P(32, 26, 32, 12),
                BackColor = Theme.Background
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var titles = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Left,
                BackColor = Theme.Background,
                Margin = Padding.Empty
            };
            titles.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            titles.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            titles.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblTitle = Ui.Lbl(title, Theme.Title, Theme.Text);
            var lblSubtitle = Ui.Lbl(subtitle, Theme.Body, Theme.TextMuted);
            lblSubtitle.Margin = new Padding(Ui.S(2), Ui.S(2), 0, 0);
            titles.Controls.Add(lblTitle, 0, 0);
            titles.Controls.Add(lblSubtitle, 0, 1);

            _actions = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.Right,
                BackColor = Theme.Background,
                Margin = Padding.Empty
            };

            header.Controls.Add(titles, 0, 0);
            header.Controls.Add(_actions, 1, 0);

            Body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = Ui.P(32, 8, 32, 28),
                BackColor = Theme.Background
            };

            Controls.Add(Body);
            Controls.Add(header);
        }

        /// <summary>Agrega un botón a la esquina superior derecha de la pantalla.</summary>
        protected void AddAction(Control c)
        {
            c.Margin = new Padding(Ui.S(10), 0, 0, 0);
            _actions.Controls.Add(c);
        }
    }
}
