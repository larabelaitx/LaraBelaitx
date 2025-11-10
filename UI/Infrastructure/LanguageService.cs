using System;
using System.Globalization;
using System.Threading;

namespace UI.Infrastructure
{
    public static class LanguageService
    {
        public static event Action<CultureInfo> LanguageChanged;

        // culture: "es" | "en" (o "es-AR" si querés formatos argentinos)
        public static void SetUICulture(string culture)
        {
            var ci = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;
            LanguageChanged?.Invoke(ci);
        }
    }
}
