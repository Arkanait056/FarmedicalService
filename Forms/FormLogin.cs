using FarmedicalService.Database;
using FarmedicalService.UI;

namespace FarmedicalService.Forms
{
    public class FormLogin : Form
    {
        private TextBox txtUsuario = null!;
        private TextBox txtContrasena = null!;
        private AppButton btnEntrar = null!;
        private Label lblError = null!;

        public FormLogin()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = "FarmedicalService — Iniciar sesión";
            Font = Theme.Body;
            ClientSize = Ui.Sz(860, 520);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Theme.Surface;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Surface
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(ConstruirPanelMarca(), 0, 0);
            root.Controls.Add(ConstruirPanelFormulario(), 1, 0);
            Controls.Add(root);

            AcceptButton = btnEntrar;
            Shown += (s, e) => txtUsuario.Focus();
        }

        private Control ConstruirPanelMarca()
        {
            var fondo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                BackColor = Theme.Sidebar,
                Padding = Ui.P(40),
                Margin = Padding.Empty
            };
            fondo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            fondo.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var pila = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None,
                BackColor = Theme.Sidebar,
                Margin = Padding.Empty
            };
            pila.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var logo = new LogoMark
            {
                Size = Ui.Sz(72, 72),
                Anchor = AnchorStyles.None,
                Margin = new Padding(0, 0, 0, Ui.S(22))
            };
            var titulo = Ui.Lbl("FarmedicalService", new Font("Segoe UI Semibold", 20f), Color.White);
            titulo.Anchor = AnchorStyles.None;
            var subtitulo = Ui.Lbl("Gestión de farmacia", new Font("Segoe UI", 11f), Theme.SidebarText);
            subtitulo.Anchor = AnchorStyles.None;
            subtitulo.Margin = new Padding(0, Ui.S(6), 0, 0);

            AgregarFila(pila, logo);
            AgregarFila(pila, titulo);
            AgregarFila(pila, subtitulo);

            fondo.Controls.Add(pila, 0, 0);
            return fondo;
        }

        private static void AgregarFila(TableLayoutPanel t, Control c)
        {
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(c, 0, t.RowStyles.Count - 1);
        }

        private Control ConstruirPanelFormulario()
        {
            var fondo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                BackColor = Theme.Surface,
                Padding = Ui.P(40),
                Margin = Padding.Empty
            };
            fondo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            fondo.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var pila = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None,
                BackColor = Theme.Surface,
                Margin = Padding.Empty
            };
            pila.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var titulo = Ui.Lbl("Iniciar sesión", Theme.Title, Theme.Text);
            var subtitulo = Ui.Lbl("Ingresa tus credenciales para continuar", Theme.Body, Theme.TextMuted);
            subtitulo.Margin = new Padding(Ui.S(2), Ui.S(4), 0, Ui.S(28));

            txtUsuario = Ui.Txt("Nombre de usuario");
            txtContrasena = Ui.Txt("Contraseña", password: true);
            var campoUsuario = Ui.Field("Usuario", txtUsuario, 340);
            var campoClave = Ui.Field("Contraseña", txtContrasena, 340);
            if (campoClave.Controls.OfType<FieldFrame>().FirstOrDefault() is FieldFrame marcoClave)
                marcoClave.PasswordToggle = true;

            btnEntrar = new AppButton("Entrar", ButtonKind.Primary)
            {
                AutoSize = false,
                Size = Ui.Sz(340, 48),
                Margin = new Padding(0, Ui.S(6), 0, 0)
            };
            btnEntrar.Click += BtnEntrar_Click;

            // El mensaje de error aparece debajo del formulario
            lblError = Ui.Lbl("", Theme.Small, Theme.DangerDark);
            lblError.MaximumSize = new Size(Ui.S(340), 0);
            lblError.Margin = new Padding(0, Ui.S(14), 0, 0);
            lblError.Visible = false;

            AgregarFila(pila, titulo);
            AgregarFila(pila, subtitulo);
            AgregarFila(pila, campoUsuario);
            AgregarFila(pila, campoClave);
            AgregarFila(pila, btnEntrar);
            AgregarFila(pila, lblError);

            fondo.Controls.Add(pila, 0, 0);
            return fondo;
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.Visible = true;
        }

        private void BtnEntrar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                MostrarError("Completa el usuario y la contraseña.");
                return;
            }

            var repo = new UsuarioRepository();
            var usuario = repo.Login(txtUsuario.Text.Trim(), txtContrasena.Text);

            if (usuario != null)
            {
                Program.UsuarioActual = usuario;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MostrarError("Usuario o contraseña incorrectos, o la cuenta está desactivada.");
                txtContrasena.Clear();
                txtContrasena.Focus();
            }
        }
    }
}
