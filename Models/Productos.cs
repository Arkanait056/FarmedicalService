namespace FarmedicalService.Models
{
    public class Producto
    {
        
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string CodigoBarras { get; set; } = "";
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int CategoriaId { get; set; }
        public int FabricanteId { get; set; }
        public decimal Itbis { get; set; }
        public string CategoriaNombre { get; set; } = "";
        public string FabricanteNombre { get; set; } = "";
        public string FabricanteTipo { get; set; } = "";
    }
}