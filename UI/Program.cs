// Program.cs (ejemplo)
using System;
using System.Windows.Forms;
using BLL;


static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Servicios
        var dvSvc = new DVService(); // o tu implementación real
        var usrSvc = UsuarioService.CreateWithSingletons(dvSvc);
        var rolSvc = RolService.CreateWithSingletons();

        // UI
        Application.Run(new UI.MainUsuarios(usrSvc, rolSvc));
    }
}
