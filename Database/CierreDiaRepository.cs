using Microsoft.Data.Sqlite;
using FarmedicalService.Models;

namespace FarmedicalService.Database
{
    public class CierreDiaRepository
    {
        public void RegistrarCierre(CierreDia cierre)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                INSERT OR REPLACE INTO CierreDia 
                (Fecha, TotalVentas, NumeroFacturas, ProductoMasVendido, CantidadMasVendido, UsuarioId, HoraCierre, EfectivoContado, Diferencia)
                VALUES (@Fecha, @TotalVentas, @NumeroFacturas, @ProductoMasVendido, @CantidadMasVendido, @UsuarioId, @HoraCierre, @EfectivoContado, @Diferencia)";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Fecha", cierre.Fecha.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@TotalVentas", cierre.TotalVentas);
            cmd.Parameters.AddWithValue("@NumeroFacturas", cierre.NumeroFacturas);
            cmd.Parameters.AddWithValue("@ProductoMasVendido", cierre.ProductoMasVendido);
            cmd.Parameters.AddWithValue("@CantidadMasVendido", cierre.CantidadMasVendido);
            cmd.Parameters.AddWithValue("@UsuarioId", cierre.UsuarioId);
            cmd.Parameters.AddWithValue("@HoraCierre", cierre.HoraCierre);
            cmd.Parameters.AddWithValue("@EfectivoContado", cierre.EfectivoContado);
            cmd.Parameters.AddWithValue("@Diferencia", cierre.Diferencia);
            cmd.ExecuteNonQuery();
        }

        public bool ExisteCierreHoy()
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var hoy = DateTime.Today.ToString("yyyy-MM-dd");
            var sql = "SELECT COUNT(*) FROM CierreDia WHERE Fecha = @Fecha";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Fecha", hoy);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<CierreDia> ObtenerCierresMes(string mes = "")
        {
            var lista = new List<CierreDia>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            if (string.IsNullOrEmpty(mes))
                mes = DateTime.Today.ToString("yyyy-MM");

            var sql = @"
                SELECT c.Id, c.Fecha, c.TotalVentas, c.NumeroFacturas, 
                       c.ProductoMasVendido, c.CantidadMasVendido, c.HoraCierre, u.Nombre
                FROM CierreDia c
                INNER JOIN Usuario u ON c.UsuarioId = u.Id
                WHERE c.Fecha LIKE @Mes
                ORDER BY c.Fecha DESC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Mes", $"{mes}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new CierreDia
                {
                    Id = reader.GetInt32(0),
                    Fecha = DateTime.Parse(reader.GetString(1)),
                    TotalVentas = reader.GetDecimal(2),
                    NumeroFacturas = reader.GetInt32(3),
                    ProductoMasVendido = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    CantidadMasVendido = reader.GetInt32(5),
                    HoraCierre = reader.GetString(6),
                    UsuarioNombre = reader.GetString(7)
                });
            }
            return lista;
        }

        public decimal ObtenerTotalMes(string mes = "")
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            if (string.IsNullOrEmpty(mes))
                mes = DateTime.Today.ToString("yyyy-MM");

            var sql = @"
                SELECT COALESCE(SUM(TotalVentas), 0)
                FROM CierreDia
                WHERE Fecha LIKE @Mes";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Mes", $"{mes}%");
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }
    }
}