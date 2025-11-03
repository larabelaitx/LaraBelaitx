using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace UI
{
    public static class F1Help
    {
        /// <summary>
        /// Abre {Base}\Help\{lang}\{topic}.pdf al presionar F1
        /// </summary>
        public static void Wire(Form form, string topic, Func<string> getLang)
        {
            form.KeyPreview = true;

            form.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.F1) return;

                var lang = getLang?.Invoke() ?? "es-AR";
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var pdfPath = Path.Combine(baseDir, "Help", lang, topic + ".pdf");

                if (File.Exists(pdfPath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        KryptonMessageBox.Show(
                            form,
                            "No se pudo abrir la ayuda.\n\n" + ex.Message,
                            "Ayuda",
                            KryptonMessageBoxButtons.OK,
                            KryptonMessageBoxIcon.Error);
                    }
                }
                else
                {
                    KryptonMessageBox.Show(
                        form,
                        "No se encontró la ayuda: " + topic + "\n\nRuta buscada:\n" + pdfPath,
                        "Ayuda",
                        KryptonMessageBoxButtons.OK,
                        KryptonMessageBoxIcon.Information);
                }

                e.Handled = true;
            };
        }
    }
}
