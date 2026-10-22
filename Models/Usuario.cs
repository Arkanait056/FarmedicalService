namespace FarmedicalService.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string NombreUsuario { get; set; } = "";
        public string Rol { get; set; } = "";
        public bool Activo { get; set; } = true;
    }
}