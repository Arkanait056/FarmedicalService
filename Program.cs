using FarmedicalService.Database;
using FarmedicalService.Models;
using FarmedicalService.Forms;

namespace FarmedicalService
{
    internal static class Program
    {
        public static Usuario? UsuarioActual { get; set; }

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            DatabaseManager.InitializeDatabase();

    
            var login = new FormLogin();
            if (login.ShowDialog() == DialogResult.OK)
            {

                Application.Run(new Form1());
            }
        }
    }
}