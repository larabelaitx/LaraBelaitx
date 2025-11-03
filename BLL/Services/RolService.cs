using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

public class RolService : IRolService
{
    private readonly FamiliaDao _familiaDao = FamiliaDao.GetInstance();
    private readonly PatenteDao _patenteDao = PatenteDao.GetInstance();
    private readonly UsuarioDao _usuarioDao = UsuarioDao.GetInstance();

    // ===== Roles (Familias) =====
    public IEnumerable<Familia> ListarRoles() => _familiaDao.GetAll();
    public Familia GetFamilia(int id) => _familiaDao.GetById(id);
    public int CrearFamilia(Familia f) => _familiaDao.Add(f);
    public bool ActualizarFamilia(Familia f) => _familiaDao.Update(f);
    public bool EliminarFamilia(int idFamilia)
    {
        var fam = _familiaDao.GetById(idFamilia);
        if (fam == null) return false;
        return _familiaDao.Delete(fam) > 0;
    }

    // ===== Patentes =====
    public List<Patente> GetPatentes()
        => _patenteDao.GetAll().OfType<Patente>().ToList();

    public List<Patente> GetPatentesDeFamilia(int idFamilia)
        => _patenteDao.GetPatentesFamilia(idFamilia).OfType<Patente>().ToList();

    public bool SetPatentesDeFamilia(int idFamilia, IEnumerable<int> idsPatentes)
    {
        _familiaDao.SetPatentesDeFamilia(idFamilia, idsPatentes ?? Enumerable.Empty<int>());
        return true;
    }

    // ===== Usuario–Familia =====
    public List<Familia> GetFamiliasUsuario(int idUsuario)
        => _familiaDao.GetFamiliasUsuario(idUsuario);

    public bool SetFamiliasDeUsuario(int idUsuario, IEnumerable<int> familiasIds)
        => _usuarioDao.SetUsuarioFamilias(idUsuario, (familiasIds ?? Enumerable.Empty<int>()).Distinct().ToList());

    // ===== Usuario–Patente =====
    public List<Patente> GetPatentesDirectasDeUsuario(int idUsuario)
        => _patenteDao.GetPatentesUsuario(idUsuario).OfType<Patente>().ToList();

    public List<Patente> GetPatentesEfectivasDeUsuario(int idUsuario)
    {
        var directas = GetPatentesDirectasDeUsuario(idUsuario);
        var heredadas = (GetFamiliasUsuario(idUsuario) ?? new List<Familia>())
                        .SelectMany(f => GetPatentesDeFamilia(f.Id))
                        .ToList();

        return directas.Concat(heredadas)
                       .GroupBy(p => p.Id)
                       .Select(g => g.First())
                       .ToList();
    }

    public bool SetPatentesDeUsuario(int idUsuario, IEnumerable<int> idsPatentes)
        => _usuarioDao.SetPatentesDeUsuario(
                idUsuario, (idsPatentes ?? Enumerable.Empty<int>()).Distinct().ToList());

    public List<Patente> GetPatentesDeUsuario(int idUsuario)
    => GetPatentesEfectivasDeUsuario(idUsuario);
}
