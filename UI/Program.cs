using System;
using System.Windows.Forms;
using Services;              // AppConn / ConfigStringConn
using UI.Infrastructure;     // LanguageService

namespace UI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1) Config de cadena de conexión
            while (true)
            {
                if (AppConn.TryTest(out _)) break;
                using (var cfg = new ConfigStringConn())
                {
                    if (cfg.ShowDialog() != DialogResult.OK) return;
                }
            }

            // 2) Idioma UI
            LanguageService.SetUICulture("es");

            // 3) Arranca directamente el Login (él abre el Menú por su cuenta)
            Application.Run(new Login());
        }
    }
}
