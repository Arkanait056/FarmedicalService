namespace FarmedicalService.Models
{
    public class Lote
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NumeroLote { get; set; } = "";
        public int Cantidad { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaEntrada { get; set; }
        public string ProductoNombre { get; set; } = "";
        public int DiasParaVencer => (FechaVencimiento - DateTime.Today).Days;
        public bool ProximoAVencer => DiasParaVencer <= 30 && DiasParaVencer >= 0;
        public bool Vencido => DiasParaVencer < 0;
    }
}