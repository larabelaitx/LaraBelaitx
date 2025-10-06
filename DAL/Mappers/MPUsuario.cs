using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DAL.Mappers
{
    internal class MPUsuario
    {
        #region Singleton
        private static MPUsuario _instance;
        public static MPUsuario GetInstance()
        {
            if (_instance == null) _instance = new MPUsuario();
            return _instance;
        }
        private MPUsuario() { }
        #endregion

        public BE.Usuario MapUser(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];

            var u = new BE.Usuario
            {
                Id = Get<int>(r, "IdUsuario"),
                Name = Get<string>(r, "Nombre"),
                LastName = Get<string>(r, "Apellido"),
                Email = Get<string>(r, "Mail"),
                UserName = Get<string>(r, "Usuario"),
                PasswordHash = Get<string>(r, "PasswordHash"),
                PasswordSalt = Get<string>(r, "PasswordSalt"),
                PasswordIterations = Get<int>(r, "PasswordIterations", 0),
                Tries = Get<int>(r, "NroIntentos", 0),
                Documento = Get<string>(r, "Documento"),
                EstadoUsuarioId = Get<int>(r, "IdEstado"),
                IdiomaId = Get<int?>(r, "IdIdioma"),
                IdiomaNombre = "—",
                Permisos = new List<BE.Permiso>()
            };

            if (u.IdiomaId.HasValue)
            {
                var idioma = DAL.IdiomaDao.GetInstance().GetById(u.IdiomaId.Value);
                u.IdiomaNombre = idioma?.Name ?? "—";
            }

            var permisosUsuario = DAL.PatenteDao.GetInstance().GetPatentesUsuario(u.Id);
            var familiasUsuario = DAL.FamiliaDao.GetInstance().GetFamiliasUsuario(u.Id);

            foreach (var fam in familiasUsuario)
                foreach (var perm in fam.Permisos)
                    if (!permisosUsuario.Any(p => p.Id == perm.Id))
                        permisosUsuario.Add(perm);

            foreach (var p in permisosUsuario) u.Permisos.Add(p);

            return u;
        }

        public List<BE.Usuario> Map(DataTable dt)
        {
            var list = new List<BE.Usuario>();
            if (dt == null || dt.Rows.Count == 0) return list;

            foreach (DataRow r in dt.Rows)
            {
                var u = new BE.Usuario
                {
                    Id = Get<int>(r, "IdUsuario"),
                    Name = Get<string>(r, "Nombre"),
                    LastName = Get<string>(r, "Apellido"),
                    Email = Get<string>(r, "Mail"),
                    UserName = Get<string>(r, "Usuario"),
                    PasswordHash = Get<string>(r, "PasswordHash"),
                    PasswordSalt = Get<string>(r, "PasswordSalt"),
                    PasswordIterations = Get<int>(r, "PasswordIterations", 0),
                    Tries = Get<int>(r, "NroIntentos", 0),
                    Documento = Get<string>(r, "Documento"),
                    EstadoUsuarioId = Get<int>(r, "IdEstado"),
                    IdiomaId = Get<int?>(r, "IdIdioma"),
                    IdiomaNombre = "—",
                    Permisos = new List<BE.Permiso>()
                };

                if (u.IdiomaId.HasValue)
                {
                    var idioma = DAL.IdiomaDao.GetInstance().GetById(u.IdiomaId.Value);
                    u.IdiomaNombre = idioma?.Name ?? "—";
                }
                var permisosUsuario = DAL.PatenteDao.GetInstance().GetPatentesUsuario(u.Id);
                var familiasUsuario = DAL.FamiliaDao.GetInstance().GetFamiliasUsuario(u.Id);

                foreach (var fam in familiasUsuario)
                    foreach (var perm in fam.Permisos)
                        if (!permisosUsuario.Any(p => p.Id == perm.Id))
                            permisosUsuario.Add(perm);

                foreach (var p in permisosUsuario) u.Permisos.Add(p);

                list.Add(u);
            }

            return list;
        }
        private static bool Has(DataRow r, string c) => r.Table.Columns.Contains(c);
        private static T Get<T>(DataRow r, string c, T def = default)
        {
            if (!Has(r, c) || r[c] == DBNull.Value) return def;
            return (T)Convert.ChangeType(r[c], typeof(T));
        }
    }
}
