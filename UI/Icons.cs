using System.Drawing.Drawing2D;

namespace FarmedicalService.UI
{
    /// <summary>
    /// Iconos vectoriales dibujados con GDI+ sobre una cuadrícula de 24x24.
    /// No dependen de fuentes ni de emojis, así que se ven igual en cualquier PC y a cualquier DPI.
    /// </summary>
    public static class Icons
    {
        private static PointF[] Pts(params float[] v)
        {
            var p = new PointF[v.Length / 2];
            for (int i = 0; i < p.Length; i++)
                p[i] = new PointF(v[i * 2], v[i * 2 + 1]);
            return p;
        }

        public static void Draw(Graphics g, string name, RectangleF bounds, Color color)
        {
            var state = g.Save();
            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TranslateTransform(bounds.X, bounds.Y);
                g.ScaleTransform(bounds.Width / 24f, bounds.Height / 24f);

                using var pen = new Pen(color, 1.8f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };
                using var brush = new SolidBrush(color);

                switch (name)
                {
                    case "home":
                        g.DrawLines(pen, Pts(3, 11, 12, 3.5f, 21, 11));
                        g.DrawLines(pen, Pts(5.5f, 9.5f, 5.5f, 20.5f, 18.5f, 20.5f, 18.5f, 9.5f));
                        g.DrawRectangle(pen, 10f, 14f, 4f, 6.5f);
                        break;

                    case "receipt":
                        g.DrawPolygon(pen, Pts(6, 3, 18, 3, 18, 21, 15, 19, 12, 21, 9, 19, 6, 21));
                        g.DrawLine(pen, 9f, 8f, 15f, 8f);
                        g.DrawLine(pen, 9f, 11.5f, 15f, 11.5f);
                        g.DrawLine(pen, 9f, 15f, 13f, 15f);
                        break;

                    case "box":
                        g.DrawPolygon(pen, Pts(12, 3, 20, 7.5f, 12, 12, 4, 7.5f));
                        g.DrawLines(pen, Pts(4, 7.5f, 4, 16.5f, 12, 21, 20, 16.5f, 20, 7.5f));
                        g.DrawLine(pen, 12f, 12f, 12f, 21f);
                        break;

                    case "warning":
                        g.DrawPolygon(pen, Pts(12, 3.5f, 22, 20.5f, 2, 20.5f));
                        g.DrawLine(pen, 12f, 9.5f, 12f, 14.5f);
                        g.FillEllipse(brush, 10.9f, 16.4f, 2.2f, 2.2f);
                        break;

                    case "chart":
                        g.DrawRectangle(pen, 4f, 12f, 4.5f, 8f);
                        g.DrawRectangle(pen, 9.75f, 5f, 4.5f, 15f);
                        g.DrawRectangle(pen, 15.5f, 9f, 4.5f, 11f);
                        break;

                    case "users":
                        g.DrawEllipse(pen, 4.5f, 4.5f, 7f, 7f);
                        g.DrawArc(pen, 1f, 14f, 14f, 12f, 180, 180);
                        g.DrawEllipse(pen, 14.5f, 5.5f, 6f, 6f);
                        g.DrawArc(pen, 15f, 15f, 8f, 10f, 180, 180);
                        break;

                    case "user":
                        g.DrawEllipse(pen, 8f, 4.5f, 8f, 8f);
                        g.DrawArc(pen, 4f, 15f, 16f, 14f, 180, 180);
                        break;

                    case "pill":
                    {
                        g.TranslateTransform(12f, 12f);
                        g.RotateTransform(-45f);
                        using var capsule = new GraphicsPath();
                        capsule.AddArc(-10f, -5f, 10f, 10f, 90, 180);
                        capsule.AddArc(0f, -5f, 10f, 10f, 270, 180);
                        capsule.CloseFigure();
                        using var half = new GraphicsPath();
                        half.AddArc(0f, -5f, 10f, 10f, 270, 180);
                        half.AddLine(5f, 5f, 0f, 5f);
                        half.AddLine(0f, 5f, 0f, -5f);
                        half.CloseFigure();
                        using var halfBrush = new SolidBrush(Color.FromArgb(90, color));
                        g.FillPath(halfBrush, half);
                        g.DrawPath(pen, capsule);
                        g.DrawLine(pen, 0f, -5f, 0f, 5f);
                        break;
                    }

                    case "logout":
                        g.DrawLines(pen, Pts(10, 4, 4, 4, 4, 20, 10, 20));
                        g.DrawLine(pen, 9f, 12f, 20f, 12f);
                        g.DrawLines(pen, Pts(16, 8, 20, 12, 16, 16));
                        break;

                    case "check":
                        g.DrawLines(pen, Pts(5, 12.5f, 10, 17.5f, 19, 7));
                        break;

                    case "cash":
                        g.DrawRectangle(pen, 2.5f, 6f, 19f, 12f);
                        g.DrawEllipse(pen, 9f, 9f, 6f, 6f);
                        g.FillEllipse(brush, 5f, 11.2f, 1.6f, 1.6f);
                        g.FillEllipse(brush, 17.4f, 11.2f, 1.6f, 1.6f);
                        break;

                    case "clock":
                        g.DrawEllipse(pen, 3f, 3f, 18f, 18f);
                        g.DrawLines(pen, Pts(12, 7, 12, 12, 15.5f, 14));
                        break;

                    case "trend":
                        g.DrawLines(pen, Pts(3, 17, 9, 11, 13, 15, 21, 6));
                        g.DrawLines(pen, Pts(15.5f, 6, 21, 6, 21, 11.5f));
                        break;

                    case "star":
                        g.DrawPolygon(pen, Pts(12, 3, 14.8f, 9, 21, 9.6f, 16.3f, 13.8f, 17.7f, 20, 12, 16.8f, 6.3f, 20, 7.7f, 13.8f, 3, 9.6f, 9.2f, 9));
                        break;

                    case "close":
                        g.DrawLine(pen, 6f, 6f, 18f, 18f);
                        g.DrawLine(pen, 18f, 6f, 6f, 18f);
                        break;

                    case "plus":
                        g.DrawLine(pen, 12f, 5f, 12f, 19f);
                        g.DrawLine(pen, 5f, 12f, 19f, 12f);
                        break;
                }
            }
            finally
            {
                g.Restore(state);
            }
        }
    }
}
