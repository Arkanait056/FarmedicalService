namespace FarmedicalService.Models
{
    public class CierreDia
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal TotalVentas { get; set; }
        public int NumeroFacturas { get; set; }
        public string ProductoMasVendido { get; set; } = "";
        public int CantidadMasVendido { get; set; }
        public int UsuarioId { get; set; }
        public string HoraCierre { get; set; } = "";
        public string UsuarioNombre { get; set; } = "";
        public decimal EfectivoContado { get; set; }
        public decimal Diferencia { get; set; }
    }
}