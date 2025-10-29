using System;
using System.Windows.Forms;
using Services;  

namespace UI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                if (AppConn.TryTest(out _)) break;  

                using (var cfg = new ConfigStringConn())
                {
                    var dr = cfg.ShowDialog();
                    if (dr != DialogResult.OK)
                        return; 
                }
            }

            Application.Run(new Login());
        }
    }
}
