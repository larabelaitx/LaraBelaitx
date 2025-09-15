using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BE;
using DAL.Mappers;
using BLL.Contracts;
using Svc = global::Services;

namespace BLL.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ClienteDao _dao = ClienteDao.GetInstance();

        public List<Cliente> Buscar(string nomApe = null, string doc = null, string estadoCivil = null, string situacionFiscal = null, bool? pep = null)
            => _dao.Buscar(nomApe, doc, estadoCivil, situacionFiscal, pep);

        public Cliente GetById(int id) => _dao.GetById(id);
        public List<Cliente> GetAll() => _dao.GetAll();
        public List<Cliente> ObtenerTodos() => GetAll();

        public int Crear(Cliente c)
        {
            ValidarCliente(c, isUpdate: false);

            var dvh = new DVH { dvh = Svc.DV.GetDV($"{c.Nombre}|{c.Apellido}|{c.DocumentoIdentidad}") };
            int id = _dao.Add(c, dvh);

            BLL.Bitacora.Info(null, $"Alta de cliente #{id} - {c.Nombre} {c.Apellido} ({c.DocumentoIdentidad})");

            return id;
        }

        public bool Actualizar(Cliente c)
        {
            ValidarCliente(c, isUpdate: true);

            var dvh = new DVH { dvh = Svc.DV.GetDV($"{c.IdCliente}|{c.DocumentoIdentidad}") };
            bool ok = _dao.Update(c, dvh);

            if (ok) BLL.Bitacora.Info(null, $"Modificación de cliente #{c.IdCliente} - {c.Nombre} {c.Apellido}");

            return ok;
        }

        public bool Eliminar(Cliente c)
        {
            bool ok = _dao.Delete(c);
            if (ok) BLL.Bitacora.Warn(null, $"Baja de cliente #{c.IdCliente} - {c.Nombre} {c.Apellido}");
            return ok;
        }

        private void ValidarCliente(Cliente c, bool isUpdate)
        {
            if (string.IsNullOrWhiteSpace(c.Nombre) || string.IsNullOrWhiteSpace(c.Apellido))
                throw new System.Exception("Nombre y Apellido son obligatorios.");

            if (!string.IsNullOrWhiteSpace(c.DocumentoIdentidad))
            {
                var posibles = _dao.Buscar(documento: c.DocumentoIdentidad);
                bool hayOtro = posibles.Any(x => x.IdCliente != c.IdCliente);
                if (hayOtro)
                    throw new System.Exception("Ya existe un cliente con ese Documento.");
            }

            if (!string.IsNullOrWhiteSpace(c.CorreoElectronico))
            {
                if (!Regex.IsMatch(c.CorreoElectronico, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    throw new System.Exception("El email no tiene un formato válido.");
            }

            if (!string.IsNullOrWhiteSpace(c.CorreoElectronico))
            {
                var porMail = _dao.GetAll().Where(x =>
                    !string.IsNullOrWhiteSpace(x.CorreoElectronico) &&
                    x.CorreoElectronico.Trim().ToLower() == c.CorreoElectronico.Trim().ToLower() &&
                    x.IdCliente != c.IdCliente);

                if (porMail.Any())
                    throw new System.Exception("Ya existe un cliente con ese Email.");
            }
        }
    }
}
