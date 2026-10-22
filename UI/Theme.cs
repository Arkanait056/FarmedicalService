namespace FarmedicalService.UI
{
    /// <summary>
    /// Paleta y tipografía. Un solo color de marca (Accent): botón principal, ítem activo y totales.
    /// Rojo y ámbar quedan reservados para alertas reales.
    /// </summary>
    public static class Theme
    {
        private static readonly string NumFace =
            FontFamily.Families.Any(f => f.Name == "Bahnschrift") ? "Bahnschrift" : "Segoe UI";

        // Marca
        public static readonly Color Sidebar = Color.FromArgb(22, 30, 40);
        public static readonly Color SidebarHover = Color.FromArgb(35, 46, 58);
        public static readonly Color SidebarText = Color.FromArgb(176, 189, 201);
        public static readonly Color Accent = Color.FromArgb(13, 122, 112);
        public static readonly Color AccentHover = Color.FromArgb(9, 98, 90);
        public static readonly Color AccentSoft = Color.FromArgb(222, 240, 237);

        // Superficies
        public static readonly Color Background = Color.FromArgb(243, 245, 247);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceAlt = Color.FromArgb(248, 249, 251);
        public static readonly Color Border = Color.FromArgb(222, 227, 233);
        public static readonly Color Selection = Color.FromArgb(230, 237, 247);

        // Texto
        public static readonly Color Text = Color.FromArgb(28, 36, 46);
        public static readonly Color TextMuted = Color.FromArgb(99, 112, 128);

        // Neutros para iconos y chips informativos
        public static readonly Color Neutral = Color.FromArgb(84, 98, 112);
        public static readonly Color NeutralSoft = Color.FromArgb(235, 239, 243);

        // Estados (solo para información que realmente lo es)
        public static readonly Color Success = Color.FromArgb(22, 150, 74);
        public static readonly Color SuccessSoft = Color.FromArgb(220, 245, 229);
        public static readonly Color SuccessDark = Color.FromArgb(18, 110, 56);

        public static readonly Color Danger = Color.FromArgb(206, 48, 48);
        public static readonly Color DangerHover = Color.FromArgb(172, 36, 36);
        public static readonly Color DangerSoft = Color.FromArgb(253, 232, 232);
        public static readonly Color DangerDark = Color.FromArgb(160, 30, 30);

        public static readonly Color Warning = Color.FromArgb(201, 112, 10);
        public static readonly Color WarningSoft = Color.FromArgb(254, 243, 214);
        public static readonly Color WarningDark = Color.FromArgb(140, 74, 4);

        // Tipografía: Segoe UI para texto, Segoe UI Semibold para títulos, Bahnschrift para cifras
        public static readonly Font Body = new("Segoe UI", 10f);
        public static readonly Font BodyBold = new("Segoe UI Semibold", 10f);
        public static readonly Font Small = new("Segoe UI", 9f);
        public static readonly Font SmallBold = new("Segoe UI Semibold", 9f);
        public static readonly Font Heading = new("Segoe UI Semibold", 13f);
        public static readonly Font Brand = new("Segoe UI Semibold", 12f);
        public static readonly Font Title = new("Segoe UI Semibold", 20f);

        public static readonly Font Num = new(NumFace, 10f);
        public static readonly Font NumBold = new(NumFace, 10f, FontStyle.Bold);
        public static readonly Font NumSmall = new(NumFace, 9f);

        public static Font NumSize(float size, FontStyle style = FontStyle.Bold) => new(NumFace, size, style);
    }
}
