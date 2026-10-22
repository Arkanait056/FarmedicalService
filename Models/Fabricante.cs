namespace FarmedicalService.Models
{
    public class Fabricante
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public override string ToString() => Nombre;
    }
}