using System.Collections.Generic;
using BE;
using DAL;
using BLL.Contracts;
using Svc = global::Services;

namespace BLL.Services
{
    public class RolService : IRolService
    {
        private readonly FamiliaDao _familias = FamiliaDao.GetInstance();
        private readonly PatenteDao _patentes = PatenteDao.GetInstance();
        public List<Familia> GetFamilias() => _familias.GetAll();
        public HashSet<Permiso> GetPatentes() => _patentes.GetAll();
        public bool CrearFamilia(Familia f) => _familias.Add(f);
        public bool ActualizarFamilia(Familia f) => _familias.Update(f);
        public bool EliminarFamilia(Familia f) => _familias.Delete(f);
        public bool AgregarPatenteAFamilia(Familia f, Permiso p)
        {
            var dvh = new DVH { dvh = Svc.DV.GetDV($"{f.Id}|{p.Id}") };
            return _familias.AddUpdatePatente(f, p, dvh);
        }
        public bool QuitarPatenteAFamilia(Familia f, Permiso p)
            => _familias.DelPatente(f, p);
        public List<Familia> GetFamiliasUsuario(int idUsuario)
            => _familias.GetFamiliasUsuario(idUsuario);
        public List<Familia> ListarRoles() => GetFamilias();

    }

}
