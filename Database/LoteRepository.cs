using Microsoft.Data.Sqlite;
using FarmedicalService.Models;

namespace FarmedicalService.Database
{
    public class LoteRepository
    {
        public List<Lote> ObtenerProximosAVencer(int diasAlerta = 30)
        {
            var lotes = new List<Lote>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var fechaLimite = DateTime.Today.AddDays(diasAlerta).ToString("yyyy-MM-dd");

            var sql = @"
                SELECT l.Id, l.ProductoId, l.NumeroLote, l.Cantidad, 
                       l.FechaVencimiento, l.FechaEntrada, p.Nombre
                FROM Lote l
                INNER JOIN Producto p ON l.ProductoId = p.Id
                WHERE l.Cantidad > 0 AND l.FechaVencimiento <= @FechaLimite
                ORDER BY l.FechaVencimiento ASC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@FechaLimite", fechaLimite);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lotes.Add(new Lote
                {
                    Id = reader.GetInt32(0),
                    ProductoId = reader.GetInt32(1),
                    NumeroLote = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Cantidad = reader.GetInt32(3),
                    FechaVencimiento = DateTime.Parse(reader.GetString(4)),
                    FechaEntrada = DateTime.Parse(reader.GetString(5)),
                    ProductoNombre = reader.GetString(6)
                });
            }
            return lotes;
        }

        public void AgregarLote(Lote lote)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sqlLote = @"
                    INSERT INTO Lote (ProductoId, NumeroLote, Cantidad, FechaVencimiento, FechaEntrada)
                    VALUES (@ProductoId, @NumeroLote, @Cantidad, @FechaVencimiento, @FechaEntrada)";

                using var cmdLote = new SqliteCommand(sqlLote, connection, transaction);
                cmdLote.Parameters.AddWithValue("@ProductoId", lote.ProductoId);
                cmdLote.Parameters.AddWithValue("@NumeroLote", lote.NumeroLote);
                cmdLote.Parameters.AddWithValue("@Cantidad", lote.Cantidad);
                cmdLote.Parameters.AddWithValue("@FechaVencimiento", lote.FechaVencimiento.ToString("yyyy-MM-dd"));
                cmdLote.Parameters.AddWithValue("@FechaEntrada", lote.FechaEntrada.ToString("yyyy-MM-dd"));
                cmdLote.ExecuteNonQuery();

                var sqlStock = "UPDATE Producto SET StockActual = StockActual + @Cantidad WHERE Id = @ProductoId";
                using var cmdStock = new SqliteCommand(sqlStock, connection, transaction);
                cmdStock.Parameters.AddWithValue("@Cantidad", lote.Cantidad);
                cmdStock.Parameters.AddWithValue("@ProductoId", lote.ProductoId);
                cmdStock.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DescontarStock(int loteId, int productoId, int cantidad)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sqlLote = "UPDATE Lote SET Cantidad = Cantidad - @Cantidad WHERE Id = @LoteId";
                using var cmdLote = new SqliteCommand(sqlLote, connection, transaction);
                cmdLote.Parameters.AddWithValue("@Cantidad", cantidad);
                cmdLote.Parameters.AddWithValue("@LoteId", loteId);
                cmdLote.ExecuteNonQuery();

                var sqlProd = "UPDATE Producto SET StockActual = StockActual - @Cantidad WHERE Id = @ProductoId";
                using var cmdProd = new SqliteCommand(sqlProd, connection, transaction);
                cmdProd.Parameters.AddWithValue("@Cantidad", cantidad);
                cmdProd.Parameters.AddWithValue("@ProductoId", productoId);
                cmdProd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        ///Fecha de vencimiento más próxima entre los lotes con existencias, por producto.
        public Dictionary<int, DateTime> ObtenerVencimientosMasProximos()
        {
            var resultado = new Dictionary<int, DateTime>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                SELECT ProductoId, MIN(FechaVencimiento)
                FROM Lote
                WHERE Cantidad > 0
                GROUP BY ProductoId";

            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                resultado[reader.GetInt32(0)] = DateTime.Parse(reader.GetString(1));
            return resultado;
        }

        ///Descuenta stock de un lote (descarte, vencimiento, ajuste) y deja el movimiento registrado.
        public void RetirarLote(int loteId, int productoId, int cantidad, string motivo, int usuarioId)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using (var cmdLote = new SqliteCommand(
                    "UPDATE Lote SET Cantidad = Cantidad - @Cantidad WHERE Id = @LoteId", connection, transaction))
                {
                    cmdLote.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmdLote.Parameters.AddWithValue("@LoteId", loteId);
                    cmdLote.ExecuteNonQuery();
                }

                using (var cmdProd = new SqliteCommand(
                    "UPDATE Producto SET StockActual = StockActual - @Cantidad WHERE Id = @ProductoId", connection, transaction))
                {
                    cmdProd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmdProd.Parameters.AddWithValue("@ProductoId", productoId);
                    cmdProd.ExecuteNonQuery();
                }

                using (var cmdMov = new SqliteCommand(@"
                    INSERT INTO MovimientoInventario (ProductoId, LoteId, Tipo, Cantidad, Fecha, Motivo, UsuarioId)
                    VALUES (@ProductoId, @LoteId, 'Salida', @Cantidad, @Fecha, @Motivo, @UsuarioId)", connection, transaction))
                {
                    cmdMov.Parameters.AddWithValue("@ProductoId", productoId);
                    cmdMov.Parameters.AddWithValue("@LoteId", loteId);
                    cmdMov.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmdMov.Parameters.AddWithValue("@Fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdMov.Parameters.AddWithValue("@Motivo", motivo);
                    cmdMov.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    cmdMov.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Lote> ObtenerPorProducto(int productoId)
        {
            var lotes = new List<Lote>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                SELECT Id, ProductoId, NumeroLote, Cantidad, FechaVencimiento, FechaEntrada
                FROM Lote
                WHERE ProductoId = @ProductoId AND Cantidad > 0
                ORDER BY FechaVencimiento ASC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ProductoId", productoId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lotes.Add(new Lote
                {
                    Id = reader.GetInt32(0),
                    ProductoId = reader.GetInt32(1),
                    NumeroLote = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Cantidad = reader.GetInt32(3),
                    FechaVencimiento = DateTime.Parse(reader.GetString(4)),
                    FechaEntrada = DateTime.Parse(reader.GetString(5))
                });
            }
            return lotes;
        }
    }
}