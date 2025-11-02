using System;
using System.Windows.Forms;
using Services;  // por AppConn / ConfigStringConn

namespace UI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Asegurar cadena de conexión antes de lanzar el Login
            while (true)
            {
                if (AppConn.TryTest(out _)) break;

                using (var cfg = new ConfigStringConn())
                {
                    if (cfg.ShowDialog() != DialogResult.OK)
                        return; // usuario canceló la configuración
                }
            }

            // Toda la navegación queda encapsulada en Login
            Application.Run(new Login());
        }
    }
}
