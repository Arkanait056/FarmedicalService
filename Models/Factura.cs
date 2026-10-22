namespace FarmedicalService.Models
{
    public class Factura
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; } = "";
        public DateTime Fecha { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Itbis { get; set; }
        public decimal Total { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal Cambio { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string Medicamentos { get; set; } = "";
        public string Estado { get; set; } = "Emitida";
        public string MetodoPago { get; set; } = "Efectivo";
        public string Cliente { get; set; } = "";
        public string Ncf { get; set; } = "";
        public string MotivoAnulacion { get; set; } = "";
        public bool Anulada => Estado == "Anulada";
        public List<DetalleFactura> Detalles { get; set; } = new();
    }
}