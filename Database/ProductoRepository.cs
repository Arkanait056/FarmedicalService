using Microsoft.Data.Sqlite;
using FarmedicalService.Models;
using System.ComponentModel.DataAnnotations;

namespace FarmedicalService.Database
{
    public class ProductoRepository
    {
        public List<Producto> ObtenerTodos()
        {
            var productos = new List<Producto>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                SELECT p.Id, p.Nombre, p.CodigoBarras, p.PrecioCompra, p.PrecioVenta,
                       p.StockActual, p.StockMinimo, p.Itbis, p.CategoriaId, p.FabricanteId,
                       c.Nombre as CategoriaNombre, f.Nombre as FabricanteNombre, f.Tipo as FabricanteTipo
                FROM Producto p
                LEFT JOIN Categoria c ON p.CategoriaId = c.Id
                LEFT JOIN Fabricante f ON p.FabricanteId = f.Id
                ORDER BY p.Nombre";
            using var command = new SqliteCommand(sql, connection);
            using var reader  = command.ExecuteReader();

            while (reader.Read())
            {
                productos.Add(new Producto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    CodigoBarras = reader.IsDBNull(2) ?"" : reader.GetString(2),
                    PrecioCompra = reader.GetDecimal(3),
                    PrecioVenta = reader.GetDecimal(4),
                    StockActual = reader.GetInt32(5),
                    StockMinimo = reader.GetInt32(6),
                    Itbis = reader.GetDecimal(7),
                    CategoriaId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    FabricanteId = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                    CategoriaNombre = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    FabricanteNombre = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    FabricanteTipo = reader.IsDBNull(12) ? "" : reader.GetString(12)
                });
            }
return productos;
        }

        public void Insertar(Producto p, Lote lote)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sqlProducto = @"
                    INSERT INTO Producto (Nombre, CodigoBarras, PrecioCompra, PrecioVenta, StockActual, StockMinimo, Itbis, CategoriaId, FabricanteId)
                    VALUES (@Nombre, @CodigoBarras, @PrecioCompra, @PrecioVenta, @StockActual, @StockMinimo, @Itbis, @CategoriaId, @FabricanteId);
                    SELECT last_insert_rowid();";

                using var cmd = new SqliteCommand(sqlProducto, connection, transaction);
                cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
                cmd.Parameters.AddWithValue("@CodigoBarras", p.CodigoBarras);
                cmd.Parameters.AddWithValue("@PrecioCompra", p.PrecioCompra);
                cmd.Parameters.AddWithValue("@PrecioVenta", p.PrecioVenta);
                cmd.Parameters.AddWithValue("@StockActual", lote.Cantidad);
                cmd.Parameters.AddWithValue("@StockMinimo", p.StockMinimo);
                cmd.Parameters.AddWithValue("@Itbis", p.Itbis);
                cmd.Parameters.AddWithValue("@CategoriaId", p.CategoriaId == 0 ? DBNull.Value : p.CategoriaId);
                cmd.Parameters.AddWithValue("@FabricanteId", p.FabricanteId == 0 ? DBNull.Value : p.FabricanteId);

                var productoId = Convert.ToInt32(cmd.ExecuteScalar());

                var sqlLote = @"
                    INSERT INTO Lote (ProductoId, NumeroLote, Cantidad, FechaVencimiento, FechaEntrada)
                    VALUES (@ProductoId, @NumeroLote, @Cantidad, @FechaVencimiento, @FechaEntrada)";

                using var cmdLote = new SqliteCommand(sqlLote, connection, transaction);
                cmdLote.Parameters.AddWithValue("@ProductoId", productoId);
                cmdLote.Parameters.AddWithValue("@NumeroLote", lote.NumeroLote);
                cmdLote.Parameters.AddWithValue("@Cantidad", lote.Cantidad);
                cmdLote.Parameters.AddWithValue("@FechaVencimiento", lote.FechaVencimiento.ToString("yyyy-MM-dd"));
                cmdLote.Parameters.AddWithValue("@FechaEntrada", DateTime.Today.ToString("yyyy-MM-dd"));
                cmdLote.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Actualizar(Producto p)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                UPDATE Producto SET Nombre=@Nombre, CodigoBarras=@CodigoBarras,
                PrecioCompra=@PrecioCompra, PrecioVenta=@PrecioVenta,
                StockMinimo=@StockMinimo, Itbis=@Itbis, CategoriaId=@CategoriaId, FabricanteId=@FabricanteId
                WHERE Id=@Id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@CodigoBarras", p.CodigoBarras);
            cmd.Parameters.AddWithValue("@PrecioCompra", p.PrecioCompra);
            cmd.Parameters.AddWithValue("@PrecioVenta", p.PrecioVenta);
            cmd.Parameters.AddWithValue("@StockMinimo", p.StockMinimo);
            cmd.Parameters.AddWithValue("@Itbis", p.Itbis);
            cmd.Parameters.AddWithValue("@CategoriaId", p.CategoriaId == 0 ? DBNull.Value : p.CategoriaId);
            cmd.Parameters.AddWithValue("@FabricanteId", p.FabricanteId == 0 ? DBNull.Value : p.FabricanteId);
            cmd.Parameters.AddWithValue("@Id", p.Id);
            cmd.ExecuteNonQuery();
        }
        public void Eliminar(int id)
{
    using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    try
    {

        var sqlDetalle = "DELETE FROM DetalleFactura WHERE ProductoId = @Id";
        using var cmdDetalle = new SqliteCommand(sqlDetalle, connection, transaction);
        cmdDetalle.Parameters.AddWithValue("Id",id);
        cmdDetalle.ExecuteNonQuery();

        var sqlMov = "DELETE FROM MovimientoInventario WHERE ProductoId = @Id";
        using var cmdMov = new SqliteCommand(sqlMov, connection, transaction);
        cmdMov.Parameters.AddWithValue("Id", id);
        cmdMov.ExecuteNonQuery();

        var sqlLotes = "DELETE FROM Lote WHERE ProductoId = @Id";
        using var cmdLotes = new SqliteCommand(sqlLotes, connection, transaction);
        cmdLotes.Parameters.AddWithValue("@Id", id);
        cmdLotes.ExecuteNonQuery();

        var sqlProducto = "DELETE FROM Producto WHERE Id = @Id";
        using var cmdProducto = new SqliteCommand(sqlProducto, connection, transaction);
        cmdProducto.Parameters.AddWithValue("@Id", id);
        cmdProducto.ExecuteNonQuery();

        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}

        public List<Producto> BuscarPorFiltro(string nombre = "", int categoriaId = 0, int fabricanteId = 0, string tipoFabricante = "")
        {
            var productos = new List<Producto>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                SELECT p.Id, p.Nombre, p.CodigoBarras, p.PrecioCompra, p.PrecioVenta,
                       p.StockActual, p.StockMinimo, p.Itbis, p.CategoriaId, p.FabricanteId,
                       c.Nombre, f.Nombre, f.Tipo
                FROM Producto p
                LEFT JOIN Categoria c ON p.CategoriaId = c.Id
                LEFT JOIN Fabricante f ON p.FabricanteId = f.Id
                WHERE 1=1";

            if (!string.IsNullOrEmpty(nombre)) sql += " AND p.Nombre LIKE @Nombre";
            if (categoriaId > 0) sql += " AND p.CategoriaId = @CategoriaId";
            if (fabricanteId > 0) sql += " AND p.FabricanteId = @FabricanteId";
            if (!string.IsNullOrEmpty(tipoFabricante)) sql += " AND f.Tipo = @Tipo";

            using var cmd = new SqliteCommand(sql, connection);
            if (!string.IsNullOrEmpty(nombre)) cmd.Parameters.AddWithValue("@Nombre", $"%{nombre}%");
            if (categoriaId > 0) cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);
            if (fabricanteId > 0) cmd.Parameters.AddWithValue("@FabricanteId", fabricanteId);
            if (!string.IsNullOrEmpty(tipoFabricante)) cmd.Parameters.AddWithValue("@FabricanteNombre", tipoFabricante);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                productos.Add(new Producto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    CodigoBarras = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    PrecioCompra = reader.GetDecimal(3),
                    PrecioVenta = reader.GetDecimal(4),
                    StockActual = reader.GetInt32(5),
                    StockMinimo = reader.GetInt32(6),
                    Itbis = reader.GetInt32(7),
                    CategoriaId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    FabricanteId = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                    CategoriaNombre = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    FabricanteNombre = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    FabricanteTipo = reader.IsDBNull(12) ? "" : reader.GetString(12)
                });
            }
            return productos;
        }
        public List<Producto> BuscarPorFiltroUso(string nombre = "", string usoTerapeutico = "", string FabricanteNombre = "")
        {
            var productos = new List<Producto>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = @"
                SELECT p.Id, p.Nombre, p.CodigoBarras, p.PrecioCompra, p.PrecioVenta,
                       p.StockActual, p.StockMinimo, p.Itbis, p.CategoriaId, p.FabricanteId,
                       c.Nombre, f.Nombre, f.Tipo
                FROM Producto p
                LEFT JOIN Categoria c ON p.CategoriaId = c.Id
                LEFT JOIN Fabricante f ON p.FabricanteId = f.Id
                WHERE 1=1";

            if (!string.IsNullOrEmpty(nombre)) sql += " AND p.Nombre LIKE @Nombre";
            if (!string.IsNullOrEmpty(usoTerapeutico)) sql += " AND c.UsoTerapeutico = @UsoTerapeutico";
            if (!string.IsNullOrEmpty(FabricanteNombre)) sql += " AND f.Nombre = @FabricanteNombre";

            using var cmd = new SqliteCommand(sql, connection);
            if (!string.IsNullOrEmpty(nombre)) cmd.Parameters.AddWithValue("@Nombre", $"%{nombre}%");
            if (!string.IsNullOrEmpty(usoTerapeutico)) cmd.Parameters.AddWithValue("@UsoTerapeutico", usoTerapeutico);
            if (!string.IsNullOrEmpty(FabricanteNombre)) cmd.Parameters.AddWithValue("@FabricanteNombre", FabricanteNombre);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                productos.Add(new Producto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    CodigoBarras = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    PrecioCompra = reader.GetDecimal(3),
                    PrecioVenta = reader.GetDecimal(4),
                    StockActual = reader.GetInt32(5),
                    StockMinimo = reader.GetInt32(6),
                    Itbis = reader.GetDecimal(7),
                    CategoriaId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    FabricanteId = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                    CategoriaNombre = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    FabricanteNombre = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    FabricanteTipo = reader.IsDBNull(12) ? "" : reader.GetString(12)
                });
            }
            return productos;
        }
    }
}