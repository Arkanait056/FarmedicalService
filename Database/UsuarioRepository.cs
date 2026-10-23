using Microsoft.Data.Sqlite;
using FarmedicalService.Models;

namespace FarmedicalService.Database
{
    public class UsuarioRepository
    {
        public Usuario? Login(string nombreUsuario, string contrasena)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                SELECT Id, Nombre, Usuario, Rol, Contrasena
                FROM Usuario
                WHERE Usuario = @Usuario AND COALESCE(Activo, 1) = 1";

            Usuario? usuario = null;
            string almacenada = "";

            using (var cmd = new SqliteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Usuario", nombreUsuario);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    usuario = new Usuario
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        NombreUsuario = reader.GetString(2),
                        Rol = reader.GetString(3),
                        Activo = true
                    };
                    almacenada = reader.GetString(4);
                }
            }

            if (usuario == null || !PasswordHasher.Verificar(contrasena, almacenada))
                return null;

            // Migra contraseñas antiguas (texto plano) a hash
            if (!PasswordHasher.EstaCifrada(almacenada))
                CambiarContrasena(usuario.Id, contrasena);

            return usuario;
        }

        public List<Usuario> ObtenerTodos()
        {
            var usuarios = new List<Usuario>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = "SELECT Id, Nombre, Usuario, Rol, COALESCE(Activo, 1) FROM Usuario ORDER BY Nombre";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                usuarios.Add(new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    NombreUsuario = reader.GetString(2),
                    Rol = reader.GetString(3),
                    Activo = reader.GetInt32(4) == 1
                });
            }
            return usuarios;
        }

        public void Insertar(Usuario usuario, string contrasena)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                INSERT INTO Usuario (Nombre, Usuario, Contrasena, Rol)
                VALUES (@Nombre, @Usuario, @Password, @Rol)";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            cmd.Parameters.AddWithValue("@Usuario", usuario.NombreUsuario);
            cmd.Parameters.AddWithValue("@Password", PasswordHasher.Hash(contrasena));
            cmd.Parameters.AddWithValue("@Rol", usuario.Rol);
            cmd.ExecuteNonQuery();
        }

        private static bool EsAdminPrincipal(SqliteConnection connection, int id)
        {
            using var cmd = new SqliteCommand("SELECT Usuario FROM Usuario WHERE Id = @Id", connection);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteScalar()?.ToString() == "admin";
        }

        public bool Eliminar(int id)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            if (EsAdminPrincipal(connection, id))
                return false;

            using var cmd = new SqliteCommand("DELETE FROM Usuario WHERE Id = @Id", connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return true;
        }

        public void CambiarContrasena(int id, string nuevaContrasena)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = "UPDATE Usuario SET Contrasena = @Password WHERE Id = @Id";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Password", PasswordHasher.Hash(nuevaContrasena));
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        ///Cambia el rol. El administrador principal no se puede modificar.
        public bool CambiarRol(int id, string rol)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            if (EsAdminPrincipal(connection, id))
                return false;

            using var cmd = new SqliteCommand("UPDATE Usuario SET Rol = @Rol WHERE Id = @Id", connection);
            cmd.Parameters.AddWithValue("@Rol", rol);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return true;
        }

        ///Activa o desactiva una cuenta. El administrador principal no se puede desactivar.
        public bool CambiarEstado(int id, bool activo)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            if (!activo && EsAdminPrincipal(connection, id))
                return false;

            using var cmd = new SqliteCommand("UPDATE Usuario SET Activo = @Activo WHERE Id = @Id", connection);
            cmd.Parameters.AddWithValue("@Activo", activo ? 1 : 0);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return true;
        }
    }
}
