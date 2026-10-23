using Microsoft.Data.Sqlite;
using FarmedicalService.Models;

namespace FarmedicalService.Database
{
    public class FacturaRepository
    {
        private const string SelectFactura = @"
            SELECT f.Id, f.NumeroFactura, f.Fecha, f.Subtotal, f.Itbis, f.Total,
                   f.MontoPagado, f.Cambio, f.UsuarioId, u.Nombre,
                   (SELECT GROUP_CONCAT(p.Nombre || ' x' || df.Cantidad, ', ')
                    FROM DetalleFactura df
                    JOIN Producto p ON df.ProductoId = p.Id
                    WHERE df.FacturaId = f.Id) AS Medicamentos,
                   COALESCE(f.Estado, 'Emitida'), COALESCE(f.MetodoPago, 'Efectivo'),
                   COALESCE(f.Cliente, ''), COALESCE(f.Ncf, ''), COALESCE(f.MotivoAnulacion, '')
            FROM Factura f
            INNER JOIN Usuario u ON f.UsuarioId = u.Id ";

        private const string NoAnulada = "COALESCE(Estado, 'Emitida') <> 'Anulada'";

        private static Factura LeerFactura(SqliteDataReader reader)
        {
            return new Factura
            {
                Id = reader.GetInt32(0),
                NumeroFactura = reader.GetString(1),
                Fecha = DateTime.Parse(reader.GetString(2)),
                Subtotal = reader.GetDecimal(3),
                Itbis = reader.GetDecimal(4),
                Total = reader.GetDecimal(5),
                MontoPagado = reader.GetDecimal(6),
                Cambio = reader.GetDecimal(7),
                UsuarioId = reader.GetInt32(8),
                UsuarioNombre = reader.GetString(9),
                Medicamentos = reader.IsDBNull(10) ? "" : reader.GetString(10),
                Estado = reader.GetString(11),
                MetodoPago = reader.GetString(12),
                Cliente = reader.GetString(13),
                Ncf = reader.GetString(14),
                MotivoAnulacion = reader.GetString(15)
            };
        }

        public int CrearFactura(Factura factura)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Comprobante fiscal: serie + secuencial de 8 dígitos
                using (var cmdNcf = new SqliteCommand(
                    "SELECT COUNT(*) FROM Factura WHERE Ncf IS NOT NULL AND Ncf <> ''", connection, transaction))
                {
                    var emitidos = Convert.ToInt32(cmdNcf.ExecuteScalar());
                    factura.Ncf = $"{FarmaciaConfig.SerieNcf}{(FarmaciaConfig.NcfSecuencialInicial + emitidos):D8}";
                }

                var sqlFactura = @"
                    INSERT INTO Factura (NumeroFactura, Fecha, Subtotal, Itbis, Total, MontoPagado, Cambio, UsuarioId,
                                         MetodoPago, Cliente, Ncf)
                    VALUES (@NumeroFactura, @Fecha, @Subtotal, @Itbis, @Total, @MontoPagado, @Cambio, @UsuarioId,
                            @MetodoPago, @Cliente, @Ncf);
                    SELECT last_insert_rowid();";

                using var cmdFactura = new SqliteCommand(sqlFactura, connection, transaction);
                cmdFactura.Parameters.AddWithValue("@NumeroFactura", factura.NumeroFactura);
                cmdFactura.Parameters.AddWithValue("@Fecha", factura.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                cmdFactura.Parameters.AddWithValue("@Subtotal", factura.Subtotal);
                cmdFactura.Parameters.AddWithValue("@Itbis", factura.Itbis);
                cmdFactura.Parameters.AddWithValue("@Total", factura.Total);
                cmdFactura.Parameters.AddWithValue("@MontoPagado", factura.MontoPagado);
                cmdFactura.Parameters.AddWithValue("@Cambio", factura.Cambio);
                cmdFactura.Parameters.AddWithValue("@UsuarioId", factura.UsuarioId);
                cmdFactura.Parameters.AddWithValue("@MetodoPago", factura.MetodoPago);
                cmdFactura.Parameters.AddWithValue("@Cliente", factura.Cliente);
                cmdFactura.Parameters.AddWithValue("@Ncf", factura.Ncf);

                var facturaId = Convert.ToInt32(cmdFactura.ExecuteScalar());

                foreach (var detalle in factura.Detalles)
                {
                    var sqlDetalle = @"
                        INSERT INTO DetalleFactura (FacturaId, ProductoId, LoteId, Cantidad, PrecioUnitario, Subtotal)
                        VALUES (@FacturaId, @ProductoId, @LoteId, @Cantidad, @PrecioUnitario, @Subtotal)";

                    using var cmdDetalle = new SqliteCommand(sqlDetalle, connection, transaction);
                    cmdDetalle.Parameters.AddWithValue("@FacturaId", facturaId);
                    cmdDetalle.Parameters.AddWithValue("@ProductoId", detalle.ProductoId);
                    cmdDetalle.Parameters.AddWithValue("@LoteId", detalle.LoteId);
                    cmdDetalle.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                    cmdDetalle.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
                    cmdDetalle.Parameters.AddWithValue("@Subtotal", detalle.Subtotal);
                    cmdDetalle.ExecuteNonQuery();

                    var sqlLote = "UPDATE Lote SET Cantidad = Cantidad - @Cantidad WHERE Id = @LoteId";
                    using var cmdLote = new SqliteCommand(sqlLote, connection, transaction);
                    cmdLote.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                    cmdLote.Parameters.AddWithValue("@LoteId", detalle.LoteId);
                    cmdLote.ExecuteNonQuery();

                    var sqlProd = "UPDATE Producto SET StockActual = StockActual - @Cantidad WHERE Id = @ProductoId";
                    using var cmdProd = new SqliteCommand(sqlProd, connection, transaction);
                    cmdProd.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                    cmdProd.Parameters.AddWithValue("@ProductoId", detalle.ProductoId);
                    cmdProd.ExecuteNonQuery();
                }

                transaction.Commit();
                return facturaId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public string GenerarNumeroFactura()
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = "SELECT COUNT(*) FROM Factura";
            using var cmd = new SqliteCommand(sql, connection);
            var total = Convert.ToInt32(cmd.ExecuteScalar());

            return $"FAR-{(total + 1):D6}";
        }

        public List<Factura> ObtenerFacturasPorFecha(string filtroFecha)
        {
            var facturas = new List<Factura>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = SelectFactura + "WHERE f.Fecha LIKE @Filtro ORDER BY f.Fecha DESC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Filtro", $"{filtroFecha}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                facturas.Add(LeerFactura(reader));
            return facturas;
        }

        public List<Factura> ObtenerFacturasHoy()
        {
            return ObtenerFacturasPorFecha(DateTime.Today.ToString("yyyy-MM-dd"));
        }

        public List<Factura> ObtenerUltimas(int cantidad)
        {
            var facturas = new List<Factura>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = SelectFactura + "ORDER BY f.Fecha DESC LIMIT @N";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@N", cantidad);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                facturas.Add(LeerFactura(reader));
            return facturas;
        }

        public (string Producto, int Cantidad) ObtenerMasVendidoDelMes()
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var mesActual = DateTime.Today.ToString("yyyy-MM");

            var sql = @"
                SELECT p.Nombre, SUM(df.Cantidad) as TotalVendido
                FROM DetalleFactura df
                INNER JOIN Producto p ON df.ProductoId = p.Id
                INNER JOIN Factura f ON df.FacturaId = f.Id
                WHERE f.Fecha LIKE @Mes AND COALESCE(f.Estado, 'Emitida') <> 'Anulada'
                GROUP BY df.ProductoId
                ORDER BY TotalVendido DESC
                LIMIT 1";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Mes", $"{mesActual}%");

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return (reader.GetString(0), reader.GetInt32(1));

            return ("Sin ventas este mes", 0);
        }

        private decimal SumarTotal(string patronFecha, bool soloEfectivo = false)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = $@"
                SELECT COALESCE(SUM(Total), 0)
                FROM Factura
                WHERE Fecha LIKE @Patron AND {NoAnulada}"
                + (soloEfectivo ? " AND COALESCE(MetodoPago, 'Efectivo') = 'Efectivo'" : "");

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Patron", $"{patronFecha}%");
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public decimal ObtenerTotalPorFecha(DateTime fecha) => SumarTotal(fecha.ToString("yyyy-MM-dd"));

        public decimal ObtenerTotalVendidoHoy() => ObtenerTotalPorFecha(DateTime.Today);

        public decimal ObtenerTotalMes() => SumarTotal(DateTime.Today.ToString("yyyy-MM"));

        ///Ventas cobradas en efectivo ese día (base para el cuadre de caja).
        public decimal ObtenerEfectivoDelDia(DateTime fecha) => SumarTotal(fecha.ToString("yyyy-MM-dd"), soloEfectivo: true);

        /// Total vendido por día en el rango (solo días con ventas).
        public Dictionary<DateTime, decimal> ObtenerVentasPorDia(DateTime desde, DateTime hasta)
        {
            var resultado = new Dictionary<DateTime, decimal>();
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();

            var sql = $@"
                SELECT substr(Fecha, 1, 10) AS Dia, COALESCE(SUM(Total), 0)
                FROM Factura
                WHERE substr(Fecha, 1, 10) BETWEEN @Desde AND @Hasta AND {NoAnulada}
                GROUP BY Dia";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Desde", desde.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Hasta", hasta.ToString("yyyy-MM-dd"));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                resultado[DateTime.Parse(reader.GetString(0)).Date] = reader.GetDecimal(1);
            return resultado;
        }

        /// Anula una factura: devuelve las unidades a su lote y al stock, y deja constancia de quién y por qué.
        public bool Anular(int facturaId, string motivo, int usuarioId)
        {
            using var connection = new SqliteConnection(DatabaseManager.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string estado;
                string numero;
                using (var cmdEstado = new SqliteCommand(
                    "SELECT COALESCE(Estado, 'Emitida'), NumeroFactura FROM Factura WHERE Id = @Id", connection, transaction))
                {
                    cmdEstado.Parameters.AddWithValue("@Id", facturaId);
                    using var reader = cmdEstado.ExecuteReader();
                    if (!reader.Read())
                    {
                        transaction.Rollback();
                        return false;
                    }
                    estado = reader.GetString(0);
                    numero = reader.GetString(1);
                }

                if (estado == "Anulada")
                {
                    transaction.Rollback();
                    return false;
                }

                var detalles = new List<(int ProductoId, int LoteId, int Cantidad)>();
                using (var cmdDetalles = new SqliteCommand(
                    "SELECT ProductoId, LoteId, Cantidad FROM DetalleFactura WHERE FacturaId = @Id", connection, transaction))
                {
                    cmdDetalles.Parameters.AddWithValue("@Id", facturaId);
                    using var reader = cmdDetalles.ExecuteReader();
                    while (reader.Read())
                        detalles.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)));
                }

                foreach (var d in detalles)
                {
                    using (var cmdLote = new SqliteCommand(
                        "UPDATE Lote SET Cantidad = Cantidad + @Cantidad WHERE Id = @LoteId", connection, transaction))
                    {
                        cmdLote.Parameters.AddWithValue("@Cantidad", d.Cantidad);
                        cmdLote.Parameters.AddWithValue("@LoteId", d.LoteId);
                        cmdLote.ExecuteNonQuery();
                    }

                    using (var cmdProd = new SqliteCommand(
                        "UPDATE Producto SET StockActual = StockActual + @Cantidad WHERE Id = @ProductoId", connection, transaction))
                    {
                        cmdProd.Parameters.AddWithValue("@Cantidad", d.Cantidad);
                        cmdProd.Parameters.AddWithValue("@ProductoId", d.ProductoId);
                        cmdProd.ExecuteNonQuery();
                    }

                    using (var cmdMov = new SqliteCommand(@"
                        INSERT INTO MovimientoInventario (ProductoId, LoteId, Tipo, Cantidad, Fecha, Motivo, UsuarioId)
                        VALUES (@ProductoId, @LoteId, 'Entrada', @Cantidad, @Fecha, @Motivo, @UsuarioId)", connection, transaction))
                    {
                        cmdMov.Parameters.AddWithValue("@ProductoId", d.ProductoId);
                        cmdMov.Parameters.AddWithValue("@LoteId", d.LoteId);
                        cmdMov.Parameters.AddWithValue("@Cantidad", d.Cantidad);
                        cmdMov.Parameters.AddWithValue("@Fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmdMov.Parameters.AddWithValue("@Motivo", $"Anulación de factura {numero}");
                        cmdMov.Parameters.AddWithValue("@UsuarioId", usuarioId);
                        cmdMov.ExecuteNonQuery();
                    }
                }

                using (var cmdAnular = new SqliteCommand(@"
                    UPDATE Factura
                    SET Estado = 'Anulada', MotivoAnulacion = @Motivo, AnuladaPor = @Usuario, FechaAnulacion = @Fecha
                    WHERE Id = @Id", connection, transaction))
                {
                    cmdAnular.Parameters.AddWithValue("@Motivo", motivo);
                    cmdAnular.Parameters.AddWithValue("@Usuario", usuarioId);
                    cmdAnular.Parameters.AddWithValue("@Fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdAnular.Parameters.AddWithValue("@Id", facturaId);
                    cmdAnular.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
