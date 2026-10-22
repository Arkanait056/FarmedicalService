using FarmedicalService.Database;
using FarmedicalService.Models;
using FarmedicalService.UI;
using Microsoft.Data.Sqlite;

namespace FarmedicalService.Forms
{
    public class FormUsuarios : PageForm
    {
        private static readonly string[] Roles = { "Cajero", "Supervisor", "Administrador" };

        private DataGridView dgvUsuarios = null!;
        private TextBox txtNombre = null!;
        private TextBox txtUsuario = null!;
        private TextBox txtPassword = null!;
        private AppCombo cmbRol = null!;
        private AppButton btnEstado = null!;
        private List<Usuario> _usuarios = new();

        private readonly UsuarioRepository _repo = new();

        public FormUsuarios() : base("Usuarios", "Cuentas, roles y acceso al sistema")
        {
            InicializarComponentes();
            CargarUsuarios();
        }

        private void InicializarComponentes()
        {
            var btnEliminar = new AppButton("Eliminar", ButtonKind.DangerOutline);
            btnEstado = new AppButton("Desactivar", ButtonKind.Secondary);
            var btnRol = new AppButton("Cambiar rol", ButtonKind.Secondary);
            var btnCambiarPassword = new AppButton("Cambiar contraseña", ButtonKind.Secondary);
            btnEliminar.Click += BtnEliminar_Click;
            btnEstado.Click += BtnEstado_Click;
            btnRol.Click += BtnRol_Click;
            btnCambiarPassword.Click += BtnCambiarPassword_Click;
            AddAction(btnEliminar);
            AddAction(btnEstado);
            AddAction(btnRol);
            AddAction(btnCambiarPassword);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Background
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Formulario de alta
            txtNombre = Ui.Txt("Nombre y apellido");
            txtUsuario = Ui.Txt("Nombre de usuario");
            txtPassword = Ui.Txt("Mín. 8 caracteres", password: true);
            cmbRol = Ui.Combo();
            cmbRol.Items.AddRange(Roles);
            cmbRol.SelectedIndex = 0;

            var btnAgregar = new AppButton("Agregar usuario", ButtonKind.Primary, compact: true);
            btnAgregar.Click += BtnAgregar_Click;

            var campoClave = Ui.Field("Contraseña", txtPassword, 180);
            if (campoClave.Controls.OfType<FieldFrame>().FirstOrDefault() is FieldFrame marco)
                marco.PasswordToggle = true;

            var filaAlta = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Theme.Surface
            };
            filaAlta.Controls.Add(Ui.Field("Nombre completo", txtNombre, 210));
            filaAlta.Controls.Add(Ui.Field("Usuario", txtUsuario, 150));
            filaAlta.Controls.Add(campoClave);
            filaAlta.Controls.Add(Ui.Field("Rol", cmbRol, 140));
            filaAlta.Controls.Add(Ui.Field(" ", btnAgregar));

            var nota = Ui.Lbl("La contraseña debe tener al menos 8 caracteres y combinar letras y números.", Theme.Small, Theme.TextMuted);
            nota.Margin = new Padding(Ui.S(2), 0, 0, Ui.S(8));

            var contenidoAlta = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Surface
            };
            contenidoAlta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            contenidoAlta.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            contenidoAlta.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contenidoAlta.Controls.Add(filaAlta, 0, 0);
            contenidoAlta.Controls.Add(nota, 0, 1);

            var cardAlta = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 10,
                Height = Ui.S(116),
                Padding = Ui.P(20, 14, 20, 4),
                Margin = new Padding(0, 0, 0, Ui.S(14))
            };
            cardAlta.Controls.Add(contenidoAlta);

            // Tabla
            dgvUsuarios = new DataGridView { Dock = DockStyle.Fill };
            Ui.StyleGrid(dgvUsuarios);
            dgvUsuarios.CellPainting += DgvUsuarios_CellPainting;
            dgvUsuarios.SelectionChanged += (s, e) => ActualizarBotonEstado();

            var cardTabla = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 10,
                Padding = Ui.P(6),
                Margin = Padding.Empty
            };
            cardTabla.Controls.Add(dgvUsuarios);

            root.Controls.Add(cardAlta, 0, 0);
            root.Controls.Add(cardTabla, 0, 1);
            Body.Controls.Add(root);
        }

        private void CargarUsuarios()
        {
            _usuarios = _repo.ObtenerTodos();

            dgvUsuarios.Columns.Clear();
            Ui.AddColumn(dgvUsuarios, "Id", "ID", visible: false);
            Ui.AddColumn(dgvUsuarios, "Nombre", "Nombre completo", 200);
            Ui.AddColumn(dgvUsuarios, "Usuario", "Usuario", 140);
            Ui.AddColumn(dgvUsuarios, "Rol", "Rol", 120);
            Ui.AddColumn(dgvUsuarios, "Estado", "Estado", 110);

            dgvUsuarios.Rows.Clear();
            foreach (var u in _usuarios)
                dgvUsuarios.Rows.Add(u.Id, u.Nombre, u.NombreUsuario, u.Rol, u.Activo ? "Activo" : "Inactivo");

            ActualizarBotonEstado();
        }

        private void DgvUsuarios_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvUsuarios.Columns[e.ColumnIndex].Name != "Estado") return;

            var estado = e.Value?.ToString() ?? "";
            if (estado == "Activo")
                Ui.PintarInsignia(e, estado, Theme.SuccessSoft, Theme.SuccessDark);
            else
                Ui.PintarInsignia(e, estado, Theme.NeutralSoft, Theme.Neutral);
        }

        private Usuario? Seleccionado()
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return null;
            var id = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["Id"].Value);
            return _usuarios.FirstOrDefault(u => u.Id == id);
        }

        private void ActualizarBotonEstado()
        {
            var u = Seleccionado();
            btnEstado.Text = u != null && !u.Activo ? "Activar" : "Desactivar";
        }

        private Usuario? RequerirSeleccion()
        {
            var u = Seleccionado();
            if (u == null)
                MessageBox.Show("Selecciona un usuario primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return u;
        }

        private void BtnAgregar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var error = PasswordHasher.ValidarReglas(txtPassword.Text);
            if (error != null)
            {
                MessageBox.Show(error, "Contraseña no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var usuario = new Usuario
            {
                Nombre = txtNombre.Text.Trim(),
                NombreUsuario = txtUsuario.Text.Trim(),
                Rol = cmbRol.SelectedItem?.ToString() ?? "Cajero"
            };

            try
            {
                _repo.Insertar(usuario, txtPassword.Text);
            }
            catch (SqliteException)
            {
                MessageBox.Show("Ya existe un usuario con ese nombre de usuario.", "Usuario duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtNombre.Clear();
            txtUsuario.Clear();
            txtPassword.Clear();
            cmbRol.SelectedIndex = 0;
            CargarUsuarios();
            MessageBox.Show("Usuario creado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            var u = RequerirSeleccion();
            if (u == null) return;

            if (u.Id == Program.UsuarioActual?.Id)
            {
                MessageBox.Show("No puedes eliminar tu propia cuenta.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Eliminar al usuario {u.Nombre}?\n\nSi ya emitió facturas, es mejor desactivarlo para conservar el historial.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            try
            {
                if (!_repo.Eliminar(u.Id))
                {
                    MessageBox.Show("No puedes eliminar al administrador principal.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (SqliteException)
            {
                MessageBox.Show("Este usuario tiene facturas registradas y no se puede eliminar. Desactívalo en su lugar.",
                    "No se puede eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CargarUsuarios();
        }

        private void BtnEstado_Click(object? sender, EventArgs e)
        {
            var u = RequerirSeleccion();
            if (u == null) return;

            bool activar = !u.Activo;
            if (!activar && u.Id == Program.UsuarioActual?.Id)
            {
                MessageBox.Show("No puedes desactivar tu propia cuenta.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_repo.CambiarEstado(u.Id, activar))
            {
                MessageBox.Show("No puedes desactivar al administrador principal.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            CargarUsuarios();
        }

        private void BtnRol_Click(object? sender, EventArgs e)
        {
            var u = RequerirSeleccion();
            if (u == null) return;

            if (u.Id == Program.UsuarioActual?.Id)
            {
                MessageBox.Show("No puedes cambiar tu propio rol.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rol = ChoiceDialog.Ask("Cambiar rol", $"Rol de {u.Nombre}:", Roles, u.Rol);
            if (rol == null || rol == u.Rol) return;

            if (!_repo.CambiarRol(u.Id, rol))
            {
                MessageBox.Show("No puedes cambiar el rol del administrador principal.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            CargarUsuarios();
        }

        private void BtnCambiarPassword_Click(object? sender, EventArgs e)
        {
            var u = RequerirSeleccion();
            if (u == null) return;

            var nuevaPass = InputDialog.Ask("Cambiar contraseña",
                $"Nueva contraseña para {u.Nombre}\n(mínimo 8 caracteres, con letras y números):",
                password: true, textoBoton: "Guardar");

            if (string.IsNullOrWhiteSpace(nuevaPass)) return;

            var error = PasswordHasher.ValidarReglas(nuevaPass);
            if (error != null)
            {
                MessageBox.Show(error, "Contraseña no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _repo.CambiarContrasena(u.Id, nuevaPass);
            MessageBox.Show("Contraseña actualizada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
