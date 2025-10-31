using System;
using System.Collections.Generic;
using System.Linq;
using BLL.Contracts;
using BE;
using DAL;

namespace BLL.Services
{
    public class RolService : IRolService
    {
        private readonly FamiliaDao _familias;
        private readonly PatenteDao _patentes;
        private readonly UsuarioDao _usuarios;

        public RolService()
        {
            _familias = FamiliaDao.GetInstance();
            _patentes = PatenteDao.GetInstance();
            _usuarios = UsuarioDao.GetInstance();
        }

        public interface IRolService
        {
            IEnumerable<BE.Permiso> ListarRoles();
            BE.Familia GetFamilia(int id);
            int CrearFamilia(BE.Familia f);
            bool ActualizarFamilia(BE.Familia f);

            // Patentes
            List<BE.Patente> GetPatentes();
            List<BE.Patente> GetPatentesDeFamilia(int idFamilia);
            bool SetPatentesDeFamilia(int idFamilia, IEnumerable<int> idsPatentes);

            // Familias por usuario
            List<BE.Familia> GetFamiliasUsuario(int idUsuario);
            bool SetFamiliasDeUsuario(int idUsuario, IEnumerable<int> familiasIds);

            // Patentes directas por usuario
            List<BE.Permiso> GetPatentesDeUsuario(int idUsuario);
            bool SetPatentesDeUsuario(int idUsuario, IEnumerable<int> idsPatentes);

            // ➕ Falta en el contrato (ya existe en RolService)
            bool EliminarFamilia(int idFamilia);
        }


        // ---------- Familias / Roles ----------
        public IEnumerable<Permiso> ListarRoles()
        {
            var list = _familias.GetAll() ?? new List<Familia>();
            return list.Cast<Permiso>();
        }

        public Familia GetFamilia(int id) => _familias.GetById(id);

        public int CrearFamilia(Familia f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            // el DAO devuelve el nuevo Id
            return _familias.Add(f);
        }

        public bool ActualizarFamilia(Familia f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            // el DAO devuelve filas afectadas (int)
            return _familias.Update(f);
        }

        public bool EliminarFamilia(int idFamilia)
        {
            return _familias.Delete(new Familia { Id = idFamilia }) > 0;
        }

        // ---------- Patentes ----------
        public List<Patente> GetPatentes()
        {
            var hs = _patentes.GetAll() ?? new HashSet<Permiso>();
            return hs.OfType<Patente>().OrderBy(p => p.Name).ToList();
        }

        public List<Patente> GetPatentesDeFamilia(int idFamilia)
        {
            var hs = _patentes.GetPatentesFamilia(idFamilia) ?? new HashSet<Permiso>();
            return hs.OfType<Patente>().OrderBy(p => p.Name).ToList();
        }

        public bool SetPatentesDeFamilia(int idFamilia, IEnumerable<int> idsPatentes)
        {
            idsPatentes = idsPatentes ?? Enumerable.Empty<int>();

            // Ejecutamos la acción (el método del DAO no devuelve nada)
            _familias.SetPatentes(idFamilia, idsPatentes);

            // Consideramos que si no lanza excepción, se ejecutó correctamente
            return true;
        }


        // ---------- Familias por usuario ----------
        public List<Familia> GetFamiliasUsuario(int idUsuario)
        {
            return _familias.GetFamiliasUsuario(idUsuario) ?? new List<Familia>();
        }

        public bool SetFamiliasDeUsuario(int idUsuario, IEnumerable<int> familiasIds)
        {
            // Evita null
            familiasIds = familiasIds ?? Enumerable.Empty<int>();

            // Ejecuta la asignación en el DAO
            _usuarios.SetUsuarioFamilias(idUsuario, familiasIds);

            // Si no lanza excepción, consideramos éxito
            return true;
        }
        public List<BE.Permiso> GetPatentesDeUsuario(int idUsuario)
        {
            var hs = _patentes.GetPatentesUsuario(idUsuario) ?? new HashSet<BE.Permiso>();
            return hs.ToList();
        }

        public bool SetPatentesDeUsuario(int idUsuario, IEnumerable<int> idsPatentes)
        {
            // Fix: Correct the namespace for UsuarioDao
            return DAL.UsuarioDao.GetInstance()   // o PatenteDao si lo preferís allí
                .SetPatentesDeUsuario(idUsuario, idsPatentes?.ToList() ?? new List<int>());
        }
    }
}
