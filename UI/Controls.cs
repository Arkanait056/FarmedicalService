using System.Drawing.Drawing2D;

namespace FarmedicalService.UI
{
    ///Panel blanco con borde fino y esquinas redondeadas.
    public class Card : Panel
    {
        public int Radius { get; set; } = 12;
        public Color BorderColor { get; set; } = Theme.Border;

        public Card()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Theme.Surface;
            Padding = Ui.P(16);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Gfx.EffectiveBack(Parent));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using var path = Gfx.Round(rect, Radius * Ui.Scale);
            using var fill = new SolidBrush(BackColor);
            using var pen = new Pen(BorderColor);
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }

        ///Hace que toda la tarjeta (y sus hijos) responda al clic, con borde de énfasis al pasar el mouse.
        public void MakeClickable(Action onClick)
        {
            Wire(this, onClick);
        }

        private void Wire(Control c, Action onClick)
        {
            c.Cursor = Cursors.Hand;
            c.Click += (s, e) => onClick();
            c.MouseEnter += (s, e) => SetHover(true);
            c.MouseLeave += (s, e) => SetHover(ClientRectangle.Contains(PointToClient(Cursor.Position)));
            foreach (Control child in c.Controls)
                Wire(child, onClick);
        }

        private void SetHover(bool hover)
        {
            var color = hover ? Theme.Accent : Theme.Border;
            if (BorderColor == color) return;
            BorderColor = color;
            Invalidate();
        }
    }

    ///Icono vectorial sobre un fondo suave (cuadrado redondeado o círculo).
    public class IconBadge : Control
    {
        public string Icon { get; set; } = "";
        public Color Tint { get; set; } = Theme.Neutral;
        public Color Fill { get; set; } = Theme.NeutralSoft;
        public bool Circle { get; set; }

        public IconBadge()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = Ui.Sz(48, 48);
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Gfx.EffectiveBack(Parent));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new RectangleF(0, 0, Width - 1, Height - 1);
            using (var fill = new SolidBrush(Fill))
            {
                if (Circle)
                {
                    g.FillEllipse(fill, rect);
                }
                else
                {
                    using var path = Gfx.Round(rect, Width * 0.28f);
                    g.FillPath(fill, path);
                }
            }
            float inset = Width * 0.26f;
            Icons.Draw(g, Icon, new RectangleF(inset, inset, Width - 2 * inset, Height - 2 * inset), Tint);
        }
    }

    ///Logotipo simple con las iniciales de la marca.
    public class LogoMark : Control
    {
        public string Initials { get; set; } = "FS";

        public LogoMark()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = Ui.Sz(44, 44);
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Gfx.EffectiveBack(Parent));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new RectangleF(0, 0, Width - 1, Height - 1);
            using (var path = Gfx.Round(rect, Width * 0.26f))
            using (var fill = new SolidBrush(Theme.Accent))
                g.FillPath(fill, path);

            using var font = new Font("Segoe UI Semibold", Height * 0.36f, FontStyle.Regular, GraphicsUnit.Pixel);
            TextRenderer.DrawText(g, Initials, font, new Rectangle(0, 0, Width, Height), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }

    ///Elemento del menú lateral.
    public class NavItem : Control
    {
        private bool _hover;
        private bool _active;

        public string Icon { get; set; } = "";

        public bool Active
        {
            get => _active;
            set { _active = value; Invalidate(); }
        }

        public NavItem()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 10.5f);
            TabStop = false;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Theme.Sidebar);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = Ui.Scale;

            var r = new RectangleF(10 * s, 2 * s, Width - 20 * s, Height - 4 * s);
            if (_active || _hover)
            {
                using var path = Gfx.Round(r, 9 * s);
                using var bg = new SolidBrush(_active ? Theme.Accent : Theme.SidebarHover);
                g.FillPath(bg, path);
            }

            var fore = _active ? Color.White : Theme.SidebarText;
            float iconSize = 20 * s;
            Icons.Draw(g, Icon, new RectangleF(r.X + 16 * s, (Height - iconSize) / 2f, iconSize, iconSize), fore);

            var textRect = new Rectangle((int)(r.X + 52 * s), 0, (int)(r.Width - 58 * s), Height);
            TextRenderer.DrawText(g, Text, Font, textRect, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }

    ///Botón de cierre (X) para diálogos sin barra de título nativa.
    public class CloseButton : Control
    {
        private bool _hover;

        public CloseButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = Ui.Sz(36, 36);
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Gfx.EffectiveBack(Parent));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_hover)
            {
                using var path = Gfx.Round(new RectangleF(0, 0, Width - 1, Height - 1), 8 * Ui.Scale);
                using var bg = new SolidBrush(Theme.NeutralSoft);
                g.FillPath(bg, path);
            }
            float pad = Width * 0.28f;
            Icons.Draw(g, "close", new RectangleF(pad, pad, Width - 2 * pad, Height - 2 * pad), Theme.TextMuted);
        }
    }

    public enum ButtonKind { Primary, Success, Danger, DangerOutline, Warning, Info, Secondary }

    ///Botón plano con esquinas redondeadas, estados hover/pressed y tamaño automático según el texto.
    public class AppButton : Button
    {
        private bool _hover;
        private bool _down;

        public ButtonKind Kind { get; set; }
        public bool Compact { get; set; }

        public AppButton(string text, ButtonKind kind = ButtonKind.Primary, bool compact = false)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Text = text;
            Kind = kind;
            Compact = compact;
            Font = Theme.BodyBold;
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Margin = Padding.Empty;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var text = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            int padX = Ui.S(Compact ? 18 : 26);
            int padY = Ui.S(Compact ? 8 : 11);
            int w = Math.Max(text.Width + padX * 2, Ui.S(Compact ? 84 : 108));
            int h = Math.Max(text.Height + padY * 2, Ui.S(Compact ? 40 : 44));
            return new Size(w, h);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }

        private (Color Back, Color Fore, Color? Border) Colores()
        {
            if (!Enabled)
                return (Color.FromArgb(230, 234, 238), Color.FromArgb(146, 156, 168), null);

            bool activo = _down || _hover;
            return Kind switch
            {
                ButtonKind.Success => (activo ? Color.FromArgb(17, 124, 60) : Theme.Success, Color.White, null),
                ButtonKind.Danger => (activo ? Theme.DangerHover : Theme.Danger, Color.White, null),
                ButtonKind.DangerOutline => (activo ? Theme.DangerSoft : Theme.Surface, Theme.Danger, Color.FromArgb(232, 160, 160)),
                ButtonKind.Warning => (activo ? Color.FromArgb(166, 90, 6) : Theme.Warning, Color.White, null),
                ButtonKind.Info => (activo ? Color.FromArgb(29, 78, 160) : Color.FromArgb(37, 99, 200), Color.White, null),
                ButtonKind.Secondary => (activo ? Color.FromArgb(238, 241, 245) : Theme.Surface, Theme.Text, Theme.Border),
                _ => (activo ? Theme.AccentHover : Theme.Accent, Color.White, null)
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Gfx.EffectiveBack(Parent));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var (back, fore, borde) = Colores();
            var rect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using var path = Gfx.Round(rect, 8 * Ui.Scale);
            using (var fill = new SolidBrush(back))
                g.FillPath(fill, path);

            if (borde.HasValue)
            {
                using var pen = new Pen(borde.Value);
                g.DrawPath(pen, path);
            }

            if (Focused && ShowFocusCues)
            {
                using var ring = new Pen(Color.FromArgb(150, Theme.Accent), 2f);
                g.DrawPath(ring, path);
            }

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }

    /// Marco redondeado para cajas de texto (y selectores de fecha): borde suave, altura uniforme y borde de marca al enfocar. Opcionalmente muestra "Mostrar/Ocultar" para contraseñas.

    public sealed class FieldFrame : Panel
    {
        private readonly Control _inner;
        private bool _focus;

        public Control Inner => _inner;

        private bool _passwordToggle;

        public bool PasswordToggle
        {
            get => _passwordToggle;
            set
            {
                _passwordToggle = value;
                Reposicionar();
                Invalidate();
            }
        }

        public FieldFrame(Control inner)
        {
            _inner = inner;
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Theme.Surface;
            Height = Ui.S(40);

            if (inner is TextBox tb)
            {
                tb.BorderStyle = BorderStyle.None;
                tb.BackColor = Theme.Surface;
            }

            Controls.Add(inner);
            inner.Enter += (s, e) => { _focus = true; Invalidate(); };
            inner.Leave += (s, e) => { _focus = false; Invalidate(); };
            Click += (s, e) => inner.Focus();
            Reposicionar();
        }

        private void Reposicionar()
        {
            if (_inner is DateTimePicker)
            {
                // El borde nativo queda recortado fuera del marco
                _inner.SetBounds(-Ui.S(2), -Ui.S(2), Width + Ui.S(4), Height + Ui.S(4));
                return;
            }

            int pad = Ui.S(12);
            int derecha = pad + (PasswordToggle ? Ui.S(66) : 0);
            int alto = _inner.Height;
            _inner.SetBounds(pad, Math.Max(0, (Height - alto) / 2), Math.Max(Ui.S(20), Width - pad - derecha), alto);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Reposicionar();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Gfx.EffectiveBack(Parent));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using var path = Gfx.Round(rect, 8 * Ui.Scale);
            using var fill = new SolidBrush(_inner.Enabled ? Theme.Surface : Theme.SurfaceAlt);
            using var pen = new Pen(_focus ? Theme.Accent : Theme.Border, _focus ? 2f : 1f);
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }

        private bool ClaveVisible => _inner is TextBox t && !t.UseSystemPasswordChar;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!PasswordToggle) return;
            var area = new Rectangle(Width - Ui.S(74), 0, Ui.S(66), Height);
            TextRenderer.DrawText(e.Graphics, ClaveVisible ? "Ocultar" : "Mostrar", Theme.SmallBold, area, Theme.Accent,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Cursor = PasswordToggle && e.X > Width - Ui.S(76) ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (PasswordToggle && e.X > Width - Ui.S(76) && _inner is TextBox t)
            {
                t.UseSystemPasswordChar = !t.UseSystemPasswordChar;
                Invalidate();
                t.Focus();
            }
        }
    }

    /// <summary>Selector desplegable con el mismo estilo que las cajas de texto (solo lista, sin escribir).</summary>
    public class AppCombo : ComboBox
    {
        private bool _hover;

        public AppCombo()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            FlatStyle = FlatStyle.Flat;
            Font = Theme.Body;
            IntegralHeight = false;
            ItemHeight = Ui.S(34);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnSelectedIndexChanged(EventArgs e) { Invalidate(); base.OnSelectedIndexChanged(e); }
        protected override void OnDropDownClosed(EventArgs e) { Invalidate(); base.OnDropDownClosed(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Gfx.EffectiveBack(Parent));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using var path = Gfx.Round(rect, 8 * Ui.Scale);
            using (var fill = new SolidBrush(Enabled ? Theme.Surface : Theme.SurfaceAlt))
                g.FillPath(fill, path);

            bool enfocado = Focused || DroppedDown;
            var colorBorde = enfocado ? Theme.Accent : _hover ? Color.FromArgb(178, 188, 200) : Theme.Border;
            using (var pen = new Pen(colorBorde, enfocado ? 2f : 1f))
                g.DrawPath(pen, path);

            var texto = SelectedItem != null ? GetItemText(SelectedItem) : "";
            var area = new Rectangle(Ui.S(12), 0, Width - Ui.S(42), Height);
            TextRenderer.DrawText(g, texto, Font, area, Enabled ? Theme.Text : Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            float cx = Width - Ui.S(20);
            float cy = Height / 2f;
            float s = Ui.S(4);
            using var chevron = new Pen(Theme.TextMuted, 1.8f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            g.DrawLines(chevron, new[]
            {
                new PointF(cx - s, cy - s / 2f),
                new PointF(cx, cy + s / 2f),
                new PointF(cx + s, cy - s / 2f)
            });
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var fondo = new SolidBrush(seleccionado ? Theme.Selection : Theme.Surface))
                e.Graphics.FillRectangle(fondo, e.Bounds);

            var area = new Rectangle(e.Bounds.X + Ui.S(12), e.Bounds.Y, e.Bounds.Width - Ui.S(16), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, area, Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }

    /// <summary>Tarjeta de indicador (KPI): título, cifra grande, nota e icono. Puede ser clicable.</summary>
    public sealed class KpiCard : Card
    {
        private readonly Label _value;
        private readonly Label _hint;

        public KpiCard(string title, string value, string hint, string icon, Color tint, Color soft, float valueSize = 22f)
        {
            Radius = 14;
            Padding = Ui.P(22, 18, 22, 18);

            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Theme.Surface
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblTitle = Ui.Lbl(title, Theme.SmallBold, Theme.TextMuted);

            _value = new Label
            {
                Text = value,
                Font = Theme.NumSize(valueSize),
                ForeColor = Theme.Text,
                AutoSize = false,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                Height = Ui.S(valueSize >= 20f ? 46 : 36),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };

            _hint = Ui.Lbl(hint, Theme.Small, Theme.TextMuted);

            var badge = new IconBadge
            {
                Icon = icon,
                Tint = tint,
                Fill = soft,
                Size = Ui.Sz(48, 48),
                Anchor = AnchorStyles.None,
                Margin = new Padding(Ui.S(14), 0, 0, 0)
            };

            t.Controls.Add(lblTitle, 0, 0);
            t.Controls.Add(_value, 0, 1);
            t.Controls.Add(_hint, 0, 2);
            t.Controls.Add(badge, 1, 0);
            t.SetRowSpan(badge, 3);
            Controls.Add(t);
        }

        public void SetValue(string value) => _value.Text = value;

        public void SetHint(string hint, Color? color = null)
        {
            _hint.Text = hint;
            _hint.ForeColor = color ?? Theme.TextMuted;
        }
    }

    /// <summary>Gráfica de barras simple (una serie) para ventas por día.</summary>
    public sealed class BarChart : Control
    {
        public List<(string Label, decimal Value)> Data { get; set; } = new();
        public string EmptyText { get; set; } = "Aún no hay ventas en este período";

        public BarChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            TabStop = false;
        }

        private static string Corto(decimal v) => v >= 1000m ? $"{v / 1000m:0.#}k" : $"{v:0}";

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Gfx.EffectiveBack(Parent));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = Ui.Scale;

            if (Data.Count == 0 || Data.All(d => d.Value <= 0m))
            {
                TextRenderer.DrawText(g, EmptyText, Theme.Body, ClientRectangle, Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                return;
            }

            var area = new RectangleF(4 * s, 26 * s, Width - 8 * s, Height - 26 * s - 30 * s);
            decimal max = Data.Max(d => d.Value);
            int n = Data.Count;
            float slot = area.Width / n;
            float barW = Math.Min(slot * 0.58f, 46 * s);

            using (var basePen = new Pen(Theme.Border))
                g.DrawLine(basePen, area.Left, area.Bottom, area.Right, area.Bottom);

            for (int i = 0; i < n; i++)
            {
                var (label, value) = Data[i];
                float x = area.X + slot * i + (slot - barW) / 2f;
                float h = max > 0m ? (float)(value / max) * area.Height : 0f;
                bool ultima = i == n - 1;

                if (h > 0.5f)
                {
                    var rect = new RectangleF(x, area.Bottom - h, barW, h);
                    using var path = Gfx.Round(rect, Math.Min(6 * s, barW / 2f));
                    using var fill = new SolidBrush(ultima ? Theme.Accent : Color.FromArgb(182, 205, 202));
                    g.FillPath(fill, path);

                    var valRect = new Rectangle((int)(area.X + slot * i), (int)(area.Bottom - h - 22 * s), (int)slot, (int)(20 * s));
                    TextRenderer.DrawText(g, Corto(value), Theme.NumSmall, valRect, Theme.Text,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }

                var labRect = new Rectangle((int)(area.X + slot * i), (int)(area.Bottom + 6 * s), (int)slot, (int)(20 * s));
                TextRenderer.DrawText(g, label, Theme.Small, labRect, ultima ? Theme.Text : Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
                    TextFormatFlags.NoPrefix);
            }
        }
    }
}
