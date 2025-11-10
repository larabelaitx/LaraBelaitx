// BLL/Services/RolService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;
using BLL.Contracts;

namespace BLL.Services
{
    public sealed class RolService : IRolService
    {
        private readonly FamiliaDao _familiaDao = FamiliaDao.GetInstance();
        private readonly PatenteDao _patenteDao = PatenteDao.GetInstance();
        private readonly UsuarioDao _usuarioDao = UsuarioDao.GetInstance();

        private const string P_USU_PAT_EDITAR = "USUARIOS_PATENTES_EDITAR";
        private const string P_FAMILIA_LISTAR = "FAMILIA_LISTAR";
        private const string P_FAMILIA_ALTA = "FAMILIA_ALTA";
        private const string P_FAMILIA_EDITAR = "FAMILIA_EDITAR";
        private const string P_FAMILIA_ELIMINAR = "FAMILIA_ELIMINAR";

        private static string Norm(string s) =>
            (s ?? string.Empty).Trim().Replace('-', '_').Replace(' ', '_').ToUpperInvariant();

        // ===== Familias =====
        public IEnumerable<Familia> ListarRoles() => _familiaDao.GetAll();
        public Familia GetFamilia(int id) => _familiaDao.GetById(id);
        public int CrearFamilia(Familia f) => _familiaDao.Add(f);
        public bool ActualizarFamilia(Familia f) => _familiaDao.Update(f);

        public bool EliminarFamilia(int idFamilia)
        {
            var fam = _familiaDao.GetById(idFamilia);
            // Delete ahora es (Familia, DVH) y retorna bool
            return fam != null && _familiaDao.Delete(fam, null);
        }

        // ===== Patentes =====
        public List<Patente> GetPatentes() => _patenteDao.GetAll().OfType<Patente>().ToList();
        public List<Patente> GetPatentesDeFamilia(int idFamilia)
            => _patenteDao.GetPatentesFamilia(idFamilia).OfType<Patente>().ToList();

        public bool SetPatentesDeFamilia(int idFamilia, IEnumerable<int> idsPatentes)
        {
            _familiaDao.SetPatentesDeFamilia(idFamilia, (idsPatentes ?? Enumerable.Empty<int>()).Distinct());
            return true;
        }

        // ===== Usuario–Familia/Patente =====
        public List<Familia> GetFamiliasUsuario(int idUsuario) => _familiaDao.GetFamiliasUsuario(idUsuario);
        public bool SetFamiliasDeUsuario(int idUsuario, IEnumerable<int> familiasIds)
            => _usuarioDao.SetUsuarioFamilias(idUsuario, (familiasIds ?? Enumerable.Empty<int>()).Distinct().ToList());

        public List<Patente> GetPatentesDirectasDeUsuario(int idUsuario)
            => _patenteDao.GetPatentesUsuario(idUsuario).OfType<Patente>().ToList();

        public List<Patente> GetPatentesEfectivasDeUsuario(int idUsuario)
        {
            var directas = GetPatentesDirectasDeUsuario(idUsuario);
            var heredadas = (GetFamiliasUsuario(idUsuario) ?? new List<Familia>())
                            .SelectMany(f => GetPatentesDeFamilia(f.Id) ?? new List<Patente>())
                            .ToList();

            return directas.Concat(heredadas).GroupBy(p => p.Id).Select(g => g.First()).ToList();
        }

        public bool SetPatentesDeUsuario(int idUsuario, IEnumerable<int> idsPatentes)
        {
            var lista = (idsPatentes ?? Enumerable.Empty<int>()).Distinct().ToList();
            return _usuarioDao.SetPatentesDeUsuario(idUsuario, lista);
        }

        public List<Patente> GetPatentesDeUsuario(int idUsuario) => GetPatentesEfectivasDeUsuario(idUsuario);

        // ===== Chequeos =====
        private static readonly string[] FAM_ABM_SET = new[]
        {
            "FAMILIA_ALTA","FAMILIA_EDITAR","FAMILIA_ELIMINAR","FAMILIA_ASIGNARPATENTES","FAMILIA_LISTAR"
        };
        private static bool IsFamAbm(string code)
        {
            var c = Norm(code);
            return c == "FAMILIAS_ABM" || c == "FAMILIA_ABM";
        }
        private bool HasFamAbm(int idUsuario) => TieneAlguna(idUsuario, FAM_ABM_SET);

        public bool TienePatente(int idUsuario, string codigoPatente)
        {
            if (string.IsNullOrWhiteSpace(codigoPatente)) return false;
            if (IsFamAbm(codigoPatente)) return HasFamAbm(idUsuario);

            var set = new HashSet<string>(
                GetPatentesEfectivasDeUsuario(idUsuario).Select(p => Norm(p.Name)),
                StringComparer.Ordinal);

            return set.Contains(Norm(codigoPatente));
        }

        public void ThrowIfNotAllowed(int idUsuario, string codigoPatente)
        {
            if (IsFamAbm(codigoPatente))
            {
                if (!HasFamAbm(idUsuario))
                    throw new UnauthorizedAccessException("No tenés permisos de ABM de Familias.");
                return;
            }
            if (!TienePatente(idUsuario, codigoPatente))
                throw new UnauthorizedAccessException($"El usuario no tiene permiso: {codigoPatente}");
        }

        public bool TieneAlguna(int idUsuario, params string[] codigos)
        {
            if (codigos == null || codigos.Length == 0) return false;
            var set = new HashSet<string>(
                GetPatentesEfectivasDeUsuario(idUsuario).Select(p => Norm(p?.Name)),
                StringComparer.Ordinal);
            return codigos.Any(c => !string.IsNullOrWhiteSpace(c) && set.Contains(Norm(c)));
        }

        public bool TieneTodas(int idUsuario, params string[] codigos)
        {
            if (codigos == null || codigos.Length == 0) return true;
            var set = new HashSet<string>(
                GetPatentesEfectivasDeUsuario(idUsuario).Select(p => Norm(p?.Name)),
                StringComparer.Ordinal);
            return codigos.All(c => !string.IsNullOrWhiteSpace(c) && set.Contains(Norm(c)));
        }

        // ===== Wrappers secure opcionales =====
        public bool SetPatentesDeUsuarioSecure(int requesterUserId, int targetUserId, IEnumerable<int> idsPatentes)
        {
            ThrowIfNotAllowed(requesterUserId, P_USU_PAT_EDITAR);
            var lista = (idsPatentes ?? Enumerable.Empty<int>()).Distinct().ToList();
            return _usuarioDao.SetPatentesDeUsuario(targetUserId, lista);
        }
        public bool SetPatentesDeFamiliaSecure(int requesterUserId, int idFamilia, IEnumerable<int> idsPatentes)
        {
            ThrowIfNotAllowed(requesterUserId, P_FAMILIA_EDITAR);
            _familiaDao.SetPatentesDeFamilia(idFamilia, (idsPatentes ?? Enumerable.Empty<int>()).Distinct());
            return true;
        }
        public bool SetFamiliasDeUsuarioSecure(int requesterUserId, int targetUserId, IEnumerable<int> familiasIds)
        {
            ThrowIfNotAllowed(requesterUserId, P_FAMILIA_EDITAR);
            return _usuarioDao.SetUsuarioFamilias(targetUserId, (familiasIds ?? Enumerable.Empty<int>()).Distinct().ToList());
        }
        public bool EliminarFamiliaSecure(int requesterUserId, int idFamilia)
        {
            ThrowIfNotAllowed(requesterUserId, P_FAMILIA_ELIMINAR);
            var fam = _familiaDao.GetById(idFamilia);
            // idem: pasar DVH (null) y usar bool
            return fam != null && _familiaDao.Delete(fam, null);
        }
        public int CrearFamiliaSecure(int requesterUserId, Familia f)
        {
            ThrowIfNotAllowed(requesterUserId, P_FAMILIA_ALTA);
            return _familiaDao.Add(f);
        }
        public bool ActualizarFamiliaSecure(int requesterUserId, Familia f)
        {
            ThrowIfNotAllowed(requesterUserId, P_FAMILIA_EDITAR);
            return _familiaDao.Update(f);
        }
    }
}
