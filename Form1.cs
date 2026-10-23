using FarmedicalService.Database;
using FarmedicalService.Forms;
using FarmedicalService.Models;
using FarmedicalService.UI;

namespace FarmedicalService
{
    public partial class Form1 : Form
    {
        private readonly Panel _contenido = new();
        private readonly Dictionary<string, NavItem> _nav = new();
        private Form? _paginaActual;

        public Form1()
        {
            InitializeComponent();
            InicializarVentanaPrincipal();
        }


        //  Estructura: barra lateral + área de contenido

        private void InicializarVentanaPrincipal()
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = "FarmedicalService";
            WindowState = FormWindowState.Maximized;
            MinimumSize = Ui.Sz(1000, 680);
            BackColor = Theme.Background;
            Font = Theme.Body;
            DoubleBuffered = true;

            var rol = Program.UsuarioActual?.Rol ?? "";

            _contenido.Dock = DockStyle.Fill;
            _contenido.BackColor = Theme.Background;

            // El panel de relleno se agrega primero; el lateral después (así se acopla a la izquierda)
            Controls.Add(_contenido);
            Controls.Add(ConstruirBarraLateral(rol));

            MostrarDashboard();
        }

        private Panel ConstruirBarraLateral(string rol)
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = Ui.S(256),
                BackColor = Theme.Sidebar
            };

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Theme.Sidebar
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // marca
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // navegación
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // separador
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // usuario
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // cerrar sesión

            root.Controls.Add(ConstruirMarca(), 0, 0);
            root.Controls.Add(ConstruirNavegacion(rol), 0, 1);

            var separador = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 1,
                BackColor = Theme.SidebarHover,
                Margin = new Padding(Ui.S(18), 0, Ui.S(18), 0)
            };
            root.Controls.Add(separador, 0, 2);
            root.Controls.Add(ConstruirUsuario(), 0, 3);

            var logout = new NavItem
            {
                Text = "Cerrar sesión",
                Icon = "logout",
                Dock = DockStyle.Fill,
                Height = Ui.S(48),
                Margin = new Padding(0, 0, 0, Ui.S(12))
            };
            logout.Click += (s, e) => CerrarSesion();
            root.Controls.Add(logout, 0, 4);

            sidebar.Controls.Add(root);
            return sidebar;
        }

        private Control ConstruirMarca()
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Sidebar,
                Padding = Ui.P(22, 28, 14, 24)
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var logo = new LogoMark
            {
                Size = Ui.Sz(44, 44),
                Margin = new Padding(0, 0, Ui.S(12), 0),
                Anchor = AnchorStyles.Left
            };
            var nombre = Ui.Lbl("FarmedicalService", Theme.Brand, Color.White);
            nombre.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            var sub = Ui.Lbl("Gestión de farmacia", Theme.Small, Theme.SidebarText);
            sub.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            t.Controls.Add(logo, 0, 0);
            t.SetRowSpan(logo, 2);
            t.Controls.Add(nombre, 1, 0);
            t.Controls.Add(sub, 1, 1);
            return t;
        }

        private Control ConstruirNavegacion(string rol)
        {
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Theme.Sidebar,
                Padding = Ui.P(0, 4, 0, 4)
            };

            CrearNav(flow, "Inicio", "home", null);
            CrearNav(flow, "Facturación", "receipt", () => new FormFacturacion());

            if (rol != "Cajero")
            {
                CrearNav(flow, "Inventario", "box", () => new FormInventario());
                CrearNav(flow, "Vencimientos", "warning", () => new FormVencimiento());
            }

            if (rol != "Cajero" && rol != "Supervisor")
            {
                CrearNav(flow, "Reportes", "chart", () => new FormReportes());
                CrearNav(flow, "Usuarios", "users", () => new FormUsuarios());
            }

            return flow;
        }

        private void CrearNav(FlowLayoutPanel host, string texto, string icono, Func<Form>? crearPagina)
        {
            var item = new NavItem
            {
                Text = texto,
                Icon = icono,
                Width = Ui.S(256),
                Height = Ui.S(48),
                Margin = new Padding(0, 0, 0, Ui.S(2))
            };
            item.Click += (s, e) =>
            {
                if (crearPagina == null) MostrarDashboard();
                else AbrirPagina(crearPagina(), texto);
            };
            host.Controls.Add(item);
            _nav[texto] = item;
        }

        private Control ConstruirUsuario()
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Sidebar,
                Padding = Ui.P(22, 16, 14, 10)
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var avatar = new IconBadge
            {
                Icon = "user",
                Tint = Color.White,
                Fill = Theme.SidebarHover,
                Circle = true,
                Size = Ui.Sz(40, 40),
                Margin = new Padding(0, 0, Ui.S(12), 0),
                Anchor = AnchorStyles.Left
            };
            var nombre = Ui.Lbl(Program.UsuarioActual?.Nombre ?? "", Theme.BodyBold, Color.White);
            nombre.AutoSize = false;
            nombre.AutoEllipsis = true;
            nombre.Dock = DockStyle.Fill;
            nombre.Height = Ui.S(20);
            nombre.TextAlign = ContentAlignment.BottomLeft;
            var rol = Ui.Lbl(Program.UsuarioActual?.Rol ?? "", Theme.Small, Theme.SidebarText);
            rol.AutoSize = false;
            rol.Dock = DockStyle.Fill;
            rol.Height = Ui.S(18);
            rol.TextAlign = ContentAlignment.TopLeft;

            t.Controls.Add(avatar, 0, 0);
            t.SetRowSpan(avatar, 2);
            t.Controls.Add(nombre, 1, 0);
            t.Controls.Add(rol, 1, 1);
            return t;
        }


        //  Navegación entre páginas

        private void ActivarNav(string clave)
        {
            foreach (var par in _nav)
                par.Value.Active = par.Key == clave;
        }

        private void LimpiarContenido()
        {
            foreach (var c in _contenido.Controls.Cast<Control>().ToList())
            {
                _contenido.Controls.Remove(c);
                c.Dispose();
            }
        }

        private void CerrarPaginaActual()
        {
            var pagina = _paginaActual;
            _paginaActual = null;
            if (pagina == null || pagina.IsDisposed) return;
            pagina.Close();
        }

        private void AbrirPagina(Form pagina, string clave)
        {
            CerrarPaginaActual();
            LimpiarContenido();

            _paginaActual = pagina;
            pagina.TopLevel = false;
            pagina.FormBorderStyle = FormBorderStyle.None;
            pagina.Dock = DockStyle.Fill;

            // Si la propia página se cierra (p. ej. al terminar una venta), volvemos al inicio
            pagina.FormClosed += (s, e) =>
            {
                if (ReferenceEquals(pagina, _paginaActual) && !IsDisposed)
                    BeginInvoke((Action)MostrarDashboard);
            };

            _contenido.Controls.Add(pagina);
            ActivarNav(clave);
            pagina.Show();
        }

        private void MostrarDashboard()
        {
            CerrarPaginaActual();
            LimpiarContenido();
            _contenido.Controls.Add(ConstruirDashboard());
            ActivarNav("Inicio");
        }

       
        //  Panel principal: información real del negocio
        private Control ConstruirDashboard()
        {
            var rol = Program.UsuarioActual?.Rol ?? "";
            bool esCajero = rol == "Cajero";
            bool esSupervisor = rol == "Supervisor";

            var productos = new List<Producto>();
            var lotes = new List<Lote>();
            var ultimas = new List<Factura>();
            var porDia = new Dictionary<DateTime, decimal>();
            decimal ventasHoy = 0, ventasAyer = 0;
            int facturasHoy = 0, facturasAyer = 0;

            try
            {
                productos = new ProductoRepository().ObtenerTodos();
                lotes = new LoteRepository().ObtenerProximosAVencer(30);

                if (!esSupervisor)
                {
                    var facturas = new FacturaRepository();
                    var ayer = DateTime.Today.AddDays(-1);
                    ventasHoy = facturas.ObtenerTotalPorFecha(DateTime.Today);
                    ventasAyer = facturas.ObtenerTotalPorFecha(ayer);
                    facturasHoy = facturas.ObtenerFacturasHoy().Count(f => !f.Anulada);
                    facturasAyer = facturas.ObtenerFacturasPorFecha(ayer.ToString("yyyy-MM-dd")).Count(f => !f.Anulada);
                    porDia = facturas.ObtenerVentasPorDia(DateTime.Today.AddDays(-6), DateTime.Today);
                    ultimas = facturas.ObtenerUltimas(6);
                }
            }
            catch (Exception ex)
            {
                // El panel debe abrir aunque una consulta falle; las secciones quedan vacías.
                System.Diagnostics.Debug.WriteLine($"Panel principal: {ex.Message}");
            }

            var bajos = productos
                .Where(p => p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.StockActual)
                .ThenBy(p => p.Nombre)
                .ToList();

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Background,
                Padding = Ui.P(36, 30, 36, 24),
                AutoScroll = true
            };

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                BackColor = Theme.Background
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var saludo = Ui.Lbl(Saludo(), Theme.Title, Theme.Text);
            var fecha = Ui.Lbl(Ui.FechaLarga(DateTime.Now), Theme.Body, Theme.TextMuted);
            fecha.Margin = new Padding(Ui.S(2), Ui.S(2), 0, Ui.S(24));
            AgregarFila(root, saludo);
            AgregarFila(root, fecha);

            // Indicadores
            if (!esCajero)
            {
                var tarjetas = new List<KpiCard>();

                if (!esSupervisor)
                {
                    var ventas = new KpiCard("Ventas de hoy", Ui.Money(ventasHoy), "", "cash", Theme.Neutral, Theme.NeutralSoft);
                    if (ventasAyer > 0)
                    {
                        var pct = (ventasHoy - ventasAyer) / ventasAyer * 100m;
                        ventas.SetHint($"{pct.ToString("+0;-0;0")} % vs. ayer", pct >= 0 ? Theme.SuccessDark : Theme.DangerDark);
                    }
                    else
                    {
                        ventas.SetHint("Ayer no hubo ventas");
                    }
                    tarjetas.Add(ventas);

                    tarjetas.Add(new KpiCard("Facturas de hoy", facturasHoy.ToString(), $"{facturasAyer} ayer",
                        "receipt", Theme.Neutral, Theme.NeutralSoft));
                }

                bool hayBajos = bajos.Count > 0;
                var porReponer = new KpiCard("Por reponer", bajos.Count.ToString(),
                    hayBajos ? "Productos con stock bajo o agotado" : "Inventario al día",
                    "box",
                    hayBajos ? Theme.Danger : Theme.Neutral,
                    hayBajos ? Theme.DangerSoft : Theme.NeutralSoft);
                porReponer.MakeClickable(() => AbrirPagina(new FormInventario(soloStockBajo: true), "Inventario"));
                tarjetas.Add(porReponer);

                bool hayVencen = lotes.Count > 0;
                var porVencer = new KpiCard("Próximos a vencer", lotes.Count.ToString(),
                    hayVencen ? "Lotes vencidos o que vencen en 30 días" : "Ningún lote en riesgo",
                    "clock",
                    hayVencen ? Theme.Warning : Theme.Neutral,
                    hayVencen ? Theme.WarningSoft : Theme.NeutralSoft);
                porVencer.MakeClickable(() => AbrirPagina(new FormVencimiento(), "Vencimientos"));
                tarjetas.Add(porVencer);

                AgregarFila(root, FilaDeTarjetas(tarjetas.Cast<Control>().ToList(), 116, null));
            }

            // Ventas y últimas facturas (cajero y administrador)
            if (!esSupervisor)
            {
                Control izquierda;
                if (esCajero)
                    izquierda = TarjetaNuevaVenta();
                else
                    izquierda = TarjetaGrafica(porDia);

                var filasFacturas = ultimas
                    .Select(f => (
                        Izq: $"{f.NumeroFactura}  {f.Fecha:dd/MM HH:mm}" + (f.Anulada ? "  (anulada)" : ""),
                        Der: Ui.Money(f.Total),
                        ColorDer: f.Anulada ? Theme.TextMuted : Theme.Text))
                    .ToList();
                var derecha = ListaCard("Últimas facturas", filasFacturas, "Todavía no hay facturas emitidas");

                AgregarFila(root, FilaDeTarjetas(new List<Control> { izquierda, derecha }, 330, new[] { 58f, 42f }));
            }

            // Alertas con nombre y fecha (administrador y supervisor)
            if (!esCajero)
            {
                var filasBajos = bajos.Take(6)
                    .Select(p => (
                        Izq: p.Nombre,
                        Der: p.StockActual == 0 ? "Agotado" : $"{p.StockActual} uds.",
                        ColorDer: p.StockActual == 0 ? Theme.DangerDark : Theme.WarningDark))
                    .ToList();

                var filasLotes = lotes.Take(6)
                    .Select(l => (
                        Izq: $"{l.ProductoNombre}" + (string.IsNullOrWhiteSpace(l.NumeroLote) ? "" : $"  (lote {l.NumeroLote})"),
                        Der: l.Vencido ? "Vencido" : l.DiasParaVencer == 0 ? "Vence hoy" : $"{l.FechaVencimiento:dd/MM/yyyy}",
                        ColorDer: l.DiasParaVencer <= 0 ? Theme.DangerDark : Theme.WarningDark))
                    .ToList();

                AgregarFila(root, FilaDeTarjetas(new List<Control>
                {
                    ListaCard("Por reponer", filasBajos, "Todo el inventario está sobre el mínimo"),
                    ListaCard("Próximos a vencer", filasLotes, "Ningún lote vence en los próximos 30 días")
                }, 330, null));
            }

            panel.Controls.Add(root);
            return panel;
        }

        private static void AgregarFila(TableLayoutPanel t, Control c)
        {
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(c, 0, t.RowStyles.Count - 1);
        }

        ///Fila de tarjetas con el mismo alto y separación uniforme.
        private static Control FilaDeTarjetas(List<Control> tarjetas, int alto, float[]? pesos)
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = tarjetas.Count,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Background,
                Margin = new Padding(0, 0, 0, Ui.S(16))
            };
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            for (int i = 0; i < tarjetas.Count; i++)
            {
                float peso = pesos != null ? pesos[i] : 100f / tarjetas.Count;
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, peso));
                var card = tarjetas[i];
                card.Height = Ui.S(alto);
                card.Dock = DockStyle.Fill;
                card.Margin = new Padding(0, 0, i == tarjetas.Count - 1 ? 0 : Ui.S(16), 0);
                grid.Controls.Add(card, i, 0);
            }
            return grid;
        }

        private static Card TarjetaGrafica(Dictionary<DateTime, decimal> porDia)
        {
            var card = new Card { Radius = 14, Padding = Ui.P(22, 18, 22, 14) };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var titulo = Ui.Lbl("Ventas de los últimos 7 días", Theme.Heading, Theme.Text);
            titulo.Margin = new Padding(0, 0, 0, Ui.S(6));

            var datos = new List<(string Label, decimal Value)>();
            for (int i = 6; i >= 0; i--)
            {
                var dia = DateTime.Today.AddDays(-i);
                porDia.TryGetValue(dia, out var total);
                datos.Add((dia.ToString("ddd d", Ui.Cultura).Replace(".", ""), total));
            }

            var grafica = new BarChart { Dock = DockStyle.Fill, Data = datos };
            layout.Controls.Add(titulo, 0, 0);
            layout.Controls.Add(grafica, 0, 1);
            card.Controls.Add(layout);
            return card;
        }

        private Card TarjetaNuevaVenta()
        {
            var card = new Card { Radius = 14, Padding = Ui.P(28) };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 3; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var titulo = Ui.Lbl("Listo para vender", Theme.Heading, Theme.Text);
            titulo.Margin = new Padding(0, 0, 0, Ui.S(6));
            var texto = Ui.Lbl("Registra una venta, cobra y entrega el recibo.", Theme.Body, Theme.TextMuted);
            texto.Margin = new Padding(0, 0, 0, Ui.S(22));

            var boton = new AppButton("Nueva venta", ButtonKind.Primary);
            boton.Click += (s, e) => AbrirPagina(new FormFacturacion(), "Facturación");

            layout.Controls.Add(titulo, 0, 0);
            layout.Controls.Add(texto, 0, 1);
            layout.Controls.Add(boton, 0, 2);
            card.Controls.Add(layout);
            return card;
        }

        private static Card ListaCard(string titulo, List<(string Izq, string Der, Color ColorDer)> filas, string vacio)
        {
            var card = new Card { Radius = 14, Padding = Ui.P(22, 18, 22, 14) };
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Theme.Surface
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            int fila = 0;
            var lblTitulo = Ui.Lbl(titulo, Theme.Heading, Theme.Text);
            lblTitulo.Margin = new Padding(0, 0, 0, Ui.S(8));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(lblTitulo, 0, fila);
            t.SetColumnSpan(lblTitulo, 2);
            fila++;

            if (filas.Count == 0)
            {
                var lblVacio = Ui.Lbl(vacio, Theme.Body, Theme.TextMuted);
                lblVacio.Margin = new Padding(0, Ui.S(10), 0, 0);
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                t.Controls.Add(lblVacio, 0, fila);
                t.SetColumnSpan(lblVacio, 2);
            }

            foreach (var f in filas)
            {
                var izq = new Label
                {
                    Text = f.Izq,
                    Font = Theme.Body,
                    ForeColor = Theme.Text,
                    AutoSize = false,
                    AutoEllipsis = true,
                    Dock = DockStyle.Fill,
                    Height = Ui.S(34),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    Margin = Padding.Empty
                };
                var der = Ui.Lbl(f.Der, Theme.Num, f.ColorDer);
                der.Anchor = AnchorStyles.Right;
                der.Margin = new Padding(Ui.S(12), 0, 0, 0);

                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                t.Controls.Add(izq, 0, fila);
                t.Controls.Add(der, 1, fila);
                fila++;

                var linea = new Panel { Dock = DockStyle.Fill, Height = 1, BackColor = Color.FromArgb(238, 241, 245), Margin = Padding.Empty };
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                t.Controls.Add(linea, 0, fila);
                t.SetColumnSpan(linea, 2);
                fila++;
            }

            card.Controls.Add(t);
            return card;
        }

        private static string Saludo()
        {
            var hora = DateTime.Now.Hour;
            var momento = hora < 12 ? "Buenos días" : hora < 19 ? "Buenas tardes" : "Buenas noches";
            var nombre = (Program.UsuarioActual?.Nombre ?? "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            return string.IsNullOrEmpty(nombre) ? momento : $"{momento}, {nombre}";
        }


        private void CerrarSesion()
        {
            var confirm = MessageBox.Show(
                "¿Seguro que deseas cerrar sesión?",
                "Cerrar sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                Program.UsuarioActual = null;
                Application.Restart();
            }
        }
    }
}
