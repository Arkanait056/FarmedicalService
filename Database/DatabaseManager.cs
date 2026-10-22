using Microsoft.Data.Sqlite;

namespace FarmedicalService.Database
{
    public class DatabaseManager
    {
        private static readonly string Dbpath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,"Farmedical.db");

        public static string ConnectionString => $"Data source= {Dbpath}";

        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            CrearTablas(connection);
            
        }
        private static void CrearTablas (SqliteConnection connection)
        {
            var sql = @"
             PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Usuario (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Usuario TEXT NOT NULL UNIQUE,
                Contrasena TEXT NOT NULL,
                Rol TEXT NOT NULL CHECK(Rol IN ('Administrador','Cajero','Supervisor'))
            );

            CREATE TABLE IF NOT EXISTS Categoria (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                UsoTerapeutico TEXT,
                FormaAdministracion TEXT
            );

            CREATE TABLE IF NOT EXISTS Fabricante (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Tipo TEXT NOT NULL CHECK(Tipo IN ('Marca','Generico'))
            );

            CREATE TABLE IF NOT EXISTS Producto (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                CodigoBarras TEXT,
                PrecioCompra REAL NOT NULL,
                PrecioVenta REAL NOT NULL,
                StockActual INTEGER NOT NULL DEFAULT 0,
                StockMinimo INTEGER NOT NULL DEFAULT 5,
                Itbis REAL NOT NULL DEFAULT 0,
                CategoriaId INTEGER,
                FabricanteId INTEGER,
                FOREIGN KEY (CategoriaId) REFERENCES Categoria(Id),
                FOREIGN KEY (FabricanteId) REFERENCES Fabricante(Id)
            );

            CREATE TABLE IF NOT EXISTS Lote (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductoId INTEGER NOT NULL,
                NumeroLote TEXT,
                Cantidad INTEGER NOT NULL,
                FechaVencimiento TEXT NOT NULL,
                FechaEntrada TEXT NOT NULL,
                FOREIGN KEY (ProductoId) REFERENCES Producto(Id)
            );


            CREATE TABLE IF NOT EXISTS Factura (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NumeroFactura TEXT NOT NULL UNIQUE,
                Fecha TEXT NOT NULL,
                Subtotal REAL NOT NULL,
                Itbis REAL NOT NULL,
                Total REAL NOT NULL,
                MontoPagado REAL NOT NULL,
                Cambio REAL NOT NULL,
                UsuarioId INTEGER NOT NULL,
                FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
            );

            CREATE TABLE IF NOT EXISTS DetalleFactura (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FacturaId INTEGER NOT NULL,
                ProductoId INTEGER NOT NULL,
                LoteId INTEGER NOT NULL,
                Cantidad INTEGER NOT NULL,
                PrecioUnitario REAL NOT NULL,
                Subtotal REAL NOT NULL,
                FOREIGN KEY (FacturaId) REFERENCES Factura(Id),
                FOREIGN KEY (ProductoId) REFERENCES Producto(Id),
                FOREIGN KEY (LoteId) REFERENCES Lote(Id)
            );

            CREATE TABLE IF NOT EXISTS MovimientoInventario (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductoId INTEGER NOT NULL,
                LoteId INTEGER,
                Tipo TEXT NOT NULL CHECK(Tipo IN ('Entrada','Salida','Ajuste')),
                Cantidad INTEGER NOT NULL,
                Fecha TEXT NOT NULL,
                Motivo TEXT,
                UsuarioId INTEGER NOT NULL,
                FOREIGN KEY (ProductoId) REFERENCES Producto(Id),
                FOREIGN KEY (LoteId) REFERENCES Lote(Id),
                FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
            );
            CREATE TABLE IF NOT EXISTS CierreDia (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Fecha TEXT NOT NULL UNIQUE,
                TotalVentas REAL NOT NULL,
                NumeroFacturas INTEGER NOT NULL,
                ProductoMasVendido TEXT,
                CantidadMasVendido INTEGER,
                UsuarioId INTEGER NOT NULL,
                HoraCierre TEXT NOT NULL
            );
            
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Analgésico', 'Analgésico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antipirético', 'Antipirético', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiinflamatorio', 'Antiinflamatorio', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antibiótico', 'Antibiótico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiviral', 'Antiviral', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antifúngico', 'Antifúngico', 'Tópica (cutánea)');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiparasitario', 'Antiparasitario', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antihistamínico', 'Antihistamínico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antialérgico', 'Antialérgico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antihipertensivo', 'Antihipertensivo', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antianginoso', 'Antianginoso', 'Sublingual');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiarrítmico', 'Antiarrítmico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Vasodilatador', 'Vasodilatador', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Vasoconstrictor', 'Vasoconstrictor', 'Intranasal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Inotrópico', 'Inotrópico', 'Intravenosa');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Diurético', 'Diurético', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anticoagulante', 'Anticoagulante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiagregante plaquetario', 'Antiagregante plaquetario', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Trombolítico', 'Trombolítico', 'Intravenosa');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Hipolipemiante', 'Hipolipemiante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Broncodilatador', 'Broncodilatador', 'Inhalatoria');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antitusivo', 'Antitusivo', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Expectorante', 'Expectorante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Mucolítico', 'Mucolítico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Descongestionante', 'Descongestionante', 'Intranasal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiácido', 'Antiácido', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Protector gástrico', 'Protector gástrico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Laxante', 'Laxante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antidiarreico', 'Antidiarreico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiemético', 'Antiemético', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Procinético', 'Procinético', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antidepresivo', 'Antidepresivo', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Ansiolítico', 'Ansiolítico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antipsicótico', 'Antipsicótico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anticonvulsivo', 'Anticonvulsivo', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Sedante', 'Sedante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Hipnótico', 'Hipnótico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Estimulante', 'Estimulante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiparkinsoniano', 'Antiparkinsoniano', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antidiabético', 'Antidiabético', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Hipoglucemiante', 'Hipoglucemiante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Hormonal', 'Hormonal', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antitiroideo', 'Antitiroideo', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Corticoide', 'Corticoide', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anticonceptivo', 'Anticonceptivo', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Inmunosupresor', 'Inmunosupresor', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Inmunoestimulante', 'Inmunoestimulante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antineoplásico', 'Antineoplásico', 'Intravenosa');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Relajante muscular', 'Relajante muscular', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anestésico', 'Anestésico', 'Intravenosa');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiséptico', 'Antiséptico', 'Tópica (cutánea)');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Desinfectante', 'Desinfectante', 'Tópica (cutánea)');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Hematopoyético', 'Hematopoyético', 'Subcutánea');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Analgésico - Intramuscular', 'Analgésico', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Analgésico - Intravenosa', 'Analgésico', 'Intravenosa');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Analgésico - Rectal', 'Analgésico', 'Rectal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Analgésico - Transdérmica', 'Analgésico', 'Transdérmica');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antibiótico - Intramuscular', 'Antibiótico', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antibiótico - Intravenosa', 'Antibiótico', 'Intravenosa');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antibiótico - Tópica (cutánea)', 'Antibiótico', 'Tópica (cutánea)');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antibiótico - Oftálmica', 'Antibiótico', 'Oftálmica');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiinflamatorio - Intramuscular', 'Antiinflamatorio', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiinflamatorio - Tópica (cutánea)', 'Antiinflamatorio', 'Tópica (cutánea)');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiinflamatorio - Rectal', 'Antiinflamatorio', 'Rectal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Corticoide - Inhalatoria', 'Corticoide', 'Inhalatoria');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Corticoide - Tópica (cutánea)', 'Corticoide', 'Tópica (cutánea)');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Corticoide - Intramuscular', 'Corticoide', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antihistamínico - Intramuscular', 'Antihistamínico', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antihistamínico - Tópica (cutánea)', 'Antihistamínico', 'Tópica (cutánea)');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antifúngico - Oral', 'Antifúngico', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antifúngico - Vaginal', 'Antifúngico', 'Vaginal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anticonceptivo - Intramuscular', 'Anticonceptivo', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anticonceptivo - Transdérmica', 'Anticonceptivo', 'Transdérmica');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anticonceptivo - Vaginal', 'Anticonceptivo', 'Vaginal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Hormonal - Intramuscular', 'Hormonal', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Hormonal - Transdérmica', 'Hormonal', 'Transdérmica');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiemético - Intramuscular', 'Antiemético', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiemético - Rectal', 'Antiemético', 'Rectal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Laxante - Rectal', 'Laxante', 'Rectal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Antiséptico - Bucal', 'Antiséptico', 'Bucal');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Oftálmico - Oftálmica', 'Oftálmico', 'Oftálmica');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Ótico - Ótica', 'Ótico', 'Ótica');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Descongestionante - Oral', 'Descongestionante', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Vasoconstrictor - Oral', 'Vasoconstrictor', 'Oral');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Relajante muscular - Intramuscular', 'Relajante muscular', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anestésico - Intramuscular', 'Anestésico', 'Intramuscular');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anestésico - Epidural', 'Anestésico', 'Epidural');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Diurético - Intravenosa', 'Diurético', 'Intravenosa');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anticoagulante - Subcutánea', 'Anticoagulante', 'Subcutánea');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Anticoagulante - Intravenosa', 'Anticoagulante', 'Intravenosa');
            INSERT OR IGNORE INTO Categoria (Nombre, UsoTerapeutico, FormaAdministracion) VALUES ('Otros - Otros', 'Otros', 'Otros');

            INSERT OR IGNORE INTO USUARIO (Nombre, Usuario, Contrasena, Rol)
            VALUES ('Administrador', 'admin', 'admin123', 'Administrador');
            ";

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();

            Migrar(connection);
        }

        /// <summary>Agrega columnas nuevas a bases de datos creadas con versiones anteriores.</summary>
        private static void Migrar(SqliteConnection connection)
        {
            AsegurarColumna(connection, "Usuario", "Activo", "INTEGER NOT NULL DEFAULT 1");
            AsegurarColumna(connection, "Factura", "Estado", "TEXT NOT NULL DEFAULT 'Emitida'");
            AsegurarColumna(connection, "Factura", "MotivoAnulacion", "TEXT");
            AsegurarColumna(connection, "Factura", "AnuladaPor", "INTEGER");
            AsegurarColumna(connection, "Factura", "FechaAnulacion", "TEXT");
            AsegurarColumna(connection, "Factura", "MetodoPago", "TEXT NOT NULL DEFAULT 'Efectivo'");
            AsegurarColumna(connection, "Factura", "Cliente", "TEXT");
            AsegurarColumna(connection, "Factura", "Ncf", "TEXT");
            AsegurarColumna(connection, "CierreDia", "EfectivoContado", "REAL");
            AsegurarColumna(connection, "CierreDia", "Diferencia", "REAL");
        }

        private static void AsegurarColumna(SqliteConnection connection, string tabla, string columna, string definicion)
        {
            bool existe = false;
            using (var check = new SqliteCommand($"PRAGMA table_info({tabla})", connection))
            using (var reader = check.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(reader.GetString(1), columna, StringComparison.OrdinalIgnoreCase))
                    {
                        existe = true;
                        break;
                    }
                }
            }

            if (existe) return;

            using var alter = new SqliteCommand($"ALTER TABLE {tabla} ADD COLUMN {columna} {definicion}", connection);
            alter.ExecuteNonQuery();
        }
    }
}