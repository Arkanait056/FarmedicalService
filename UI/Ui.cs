using System.Globalization;
using System.Reflection;

namespace FarmedicalService.UI
{
    /// Utilidades de interfaz: escalado por DPI, fábricas de controles y estilo de tablas. Todas las medidas se escriben en "píxeles a 96 DPI" y se convierten con <see cref="S"/>.
    public static class Ui
    {
        private static float _scale;

        public static float Scale
        {
            get
            {
                if (_scale <= 0f)
                {
                    using var g = Graphics.FromHwnd(IntPtr.Zero);
                    _scale = g.DpiX / 96f;
                }
                return _scale;
            }
        }

        public static int S(int v) => (int)Math.Round(v * Scale);
        public static Padding P(int all) => new Padding(S(all));
        public static Padding P(int l, int t, int r, int b) => new Padding(S(l), S(t), S(r), S(b));
        public static Size Sz(int w, int h) => new Size(S(w), S(h));

        public static string Money(decimal v) => $"RD$ {v:N2}";

        private static CultureInfo? _cultura;

        ///Cultura en español (República Dominicana) para fechas y nombres de días.
        public static CultureInfo Cultura
        {
            get
            {
                if (_cultura == null)
                {
                    try { _cultura = new CultureInfo("es-DO"); }
                    catch (CultureNotFoundException) { _cultura = CultureInfo.CurrentCulture; }
                }
                return _cultura;
            }
        }

        public static string FechaLarga(DateTime fecha)
        {
            var texto = fecha.ToString("dddd, d 'de' MMMM 'de' yyyy", Cultura);
            return texto.Length > 0 ? char.ToUpper(texto[0], Cultura) + texto.Substring(1) : texto;
        }

        // Fábricas de controles 

        public static Label Lbl(string text, Font? font = null, Color? color = null)
        {
            return new Label
            {
                Text = text,
                Font = font ?? Theme.Body,
                ForeColor = color ?? Theme.Text,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
        }

        public static TextBox Txt(string placeholder = "", bool password = false)
        {
            return new TextBox
            {
                Font = Theme.Body,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = placeholder,
                UseSystemPasswordChar = password
            };
        }

        public static AppCombo Combo() => new AppCombo();

        /// Envuelve cajas de texto y selectores de fecha en un marco redondeado; los demás controles pasan tal cual.
        public static Control Wrap(Control c)
        {
            return c is TextBox || c is DateTimePicker ? new FieldFrame(c) : c;
        }

        public static DateTimePicker Fecha()
        {
            return new DateTimePicker
            {
                Font = Theme.Body,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                MinDate = DateTime.Today
            };
        }

        ///Título de sección dentro de un formulario.
        public static Label Section(string text)
        {
            var l = Lbl(text, Theme.BodyBold, Theme.Accent);
            l.Margin = new Padding(0, S(6), 0, S(10));
            return l;
        }

        /// Campo con etiqueta arriba y control abajo. Con <paramref name="fill"/> el control ocupa todo el ancho disponible; si no, usa el ancho indicado (en px a 96 DPI).
 
        public static TableLayoutPanel Field(string label, Control input, int width = 0, bool fill = false)
        {
            var t = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, S(16), S(12)),
                Padding = Padding.Empty
            };
            t.ColumnStyles.Add(new ColumnStyle(fill ? SizeType.Percent : SizeType.AutoSize, 100f));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var l = Lbl(label, Theme.SmallBold, Theme.TextMuted);
            l.Margin = new Padding(0, 0, 0, S(5));
            t.Controls.Add(l, 0, 0);

            var host = Wrap(input);
            host.Margin = Padding.Empty;
            if (fill)
            {
                host.Dock = DockStyle.Fill;
                t.Dock = DockStyle.Fill;
            }
            else if (width > 0)
            {
                host.Width = S(width);
            }
            t.Controls.Add(host, 0, 1);
            return t;
        }

        // Tablas 

        public static void StyleGrid(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.BackgroundColor = Theme.Surface;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Color.FromArgb(234, 240, 237);
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.RowHeadersVisible = false;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.ReadOnly = true;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            g.Font = Theme.Body;

            var head = g.ColumnHeadersDefaultCellStyle;
            head.BackColor = Theme.SurfaceAlt;
            head.ForeColor = Theme.TextMuted;
            head.Font = Theme.SmallBold;
            head.SelectionBackColor = Theme.SurfaceAlt;
            head.SelectionForeColor = Theme.TextMuted;
            head.Padding = P(10, 9, 10, 9);
            head.Alignment = DataGridViewContentAlignment.MiddleLeft;
            head.WrapMode = DataGridViewTriState.False;

            var cell = g.DefaultCellStyle;
            cell.BackColor = Theme.Surface;
            cell.ForeColor = Theme.Text;
            cell.SelectionBackColor = Theme.Selection;
            cell.SelectionForeColor = Theme.Text;
            cell.Padding = P(10, 8, 10, 8);
            cell.WrapMode = DataGridViewTriState.False;

            // Evita el parpadeo al redibujar
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(g, true);
        }

        /// Mensaje centrado para tablas sin resultados. Muéstralo con Visible = (filas == 0).
        public static Label EmptyLabel(DataGridView g, string text)
        {
            var l = new Label
            {
                Text = text,
                Font = Theme.Body,
                ForeColor = Theme.TextMuted,
                BackColor = Theme.Surface,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            g.Controls.Add(l);

            void Ajustar()
            {
                int arriba = S(46);
                l.SetBounds(0, arriba, g.ClientSize.Width, Math.Max(0, g.ClientSize.Height - arriba));
            }

            g.Resize += (s, e) => Ajustar();
            Ajustar();
            return l;
        }

        /// Dibuja una celda con una insignia redondeada (por ejemplo "Bajo", "Vencido").
        public static void PintarInsignia(DataGridViewCellPaintingEventArgs e, string texto, Color fondo, Color letra)
        {
            e.Paint(e.CellBounds, e.PaintParts & ~DataGridViewPaintParts.ContentForeground);
            e.Handled = true;
            if (string.IsNullOrEmpty(texto) || e.Graphics == null) return;

            var g = e.Graphics;
            var tam = TextRenderer.MeasureText(texto, Theme.SmallBold, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            int ancho = tam.Width + S(20);
            int alto = S(24);
            var rect = new Rectangle(e.CellBounds.X + S(10), e.CellBounds.Y + (e.CellBounds.Height - alto) / 2, ancho, alto);

            var modo = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = Gfx.Round(rect, alto / 2f))
            using (var brush = new SolidBrush(fondo))
                g.FillPath(brush, path);
            g.SmoothingMode = modo;

            TextRenderer.DrawText(g, texto, Theme.SmallBold, rect, letra,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        public static void AddColumn(DataGridView g, string name, string header,
            float weight = 100f, bool right = false, bool visible = true)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = S(60),
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Visible = visible
            };
            g.Columns.Add(col);
            if (right)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.DefaultCellStyle.Font = Theme.Num;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }
}
