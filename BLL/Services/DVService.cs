using System.Text;
using BE;
using BLL.Services;

namespace BLL
{
    public class DVService : IDVService
    {
        public DVH CalcularDVHUsuario(Usuario u)
        {
            // ⚠️ REEMPLAZAR por tu cálculo real de DVH.
            // Armamos una cadena estable con los campos que afectan integridad:
            var raw = new StringBuilder();
            raw.Append(u.Id).Append("|")
               .Append(u.UserName ?? "").Append("|")
               .Append(u.Password ?? "").Append("|")
               .Append(u.Name ?? "").Append("|")
               .Append(u.LastName ?? "").Append("|")
               .Append(u.Email ?? "").Append("|")
               .Append(u.Enabled != null ? u.Enabled.Id.ToString() : "0").Append("|")
               .Append(u.Language != null ? u.Language.Id.ToString() : "0").Append("|")
               .Append(u.Tries);

            // Ejemplo tonto: checksum; reemplazalo por tu hash/algoritmo:
            // BLL/DVService.cs  (método que calcula el DVH)
            int sum = 0;
            foreach (var ch in raw.ToString())
                sum = (sum * 31 + ch) & 0x7fffffff;

            // ⬇⬇⬇  ANTES: new DVH { dvh = sum }   (ERROR: int→string)
            return new DVH { dvh = sum.ToString() };   // ✅ OK: string

        }
    }
}
