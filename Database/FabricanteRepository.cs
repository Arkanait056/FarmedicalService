using Microsoft.Data.Sqlite;
using FarmedicalService.Models;
using SQLitePCL;

namespace FarmedicalService.Database
{
    public class FabricanteRepository
    {
         public List<Fabricante> ObtenerTodos()
        {
            var lista = new List<Fabricante>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = "SELECT Id, Nombre, Tipo FROM Fabricante ORDER BY Nombre";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                
                lista.Add(new Fabricante
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Tipo = reader.GetString(2)
                });

            }
            return lista;
        }


        public void Insertar(Fabricante fabricante)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = "INSERT INTO Fabricante (Nombre, Tipo) VALUES (@Nombre, @Tipo)";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nombre", fabricante.Nombre);
            cmd.Parameters.AddWithValue("@Tipo", fabricante.Tipo);
            cmd.ExecuteNonQuery();
        }
        
    }
}