namespace FarmedicalService.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string UsoTerapeutico { get; set; } = "";
        public string FormaAdministracion { get; set; } = "";
        public override string ToString() => Nombre;
    }
}