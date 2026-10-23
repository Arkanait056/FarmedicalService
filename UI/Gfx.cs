using System.Drawing.Drawing2D;

namespace FarmedicalService.UI
{
    internal static class Gfx
    {
        ///Rectángulo con esquinas redondeadas.
        public static GraphicsPath Round(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
            if (d <= 1f)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        ///Primer color opaco hacia arriba en la jerarquía (para pintar esquinas sin "halo").
        public static Color EffectiveBack(Control? c)
        {
            while (c != null)
            {
                if (c.BackColor.A == 255) return c.BackColor;
                c = c.Parent;
            }
            return Theme.Background;
        }
    }
}
