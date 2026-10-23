using Microsoft.Data.Sqlite;
using FarmedicalService.Models;

namespace FarmedicalService.Database
{
    
    public class CategoriaRepository
    {
        public List<Categoria> ObtenerTodas()
        {
            
            var lista = new List<Categoria>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = "SELECT Id, Nombre, UsoTerapeutico, FormaAdministracion FROM Categoria ORDER BY Nombre";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Categoria
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    UsoTerapeutico = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    FormaAdministracion = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }
            return lista;
        }

        public void Insertar(Categoria categoria)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql =  @"INSERT INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion)
                        VALUES (@Nombre, @UsoTerapeutico, @FormaAdministracion)";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nombre", categoria.Nombre);
            cmd.Parameters.AddWithValue("@UsoTerapeutico", categoria.UsoTerapeutico);
            cmd.Parameters.AddWithValue("@FormaAdministracion", categoria.FormaAdministracion);
            cmd.ExecuteNonQuery();
        }
    }
}