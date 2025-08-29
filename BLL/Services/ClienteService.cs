
using System;
using System.Collections.Generic;
using BE;

namespace BLL.Services
{
    public class ClienteService : IClienteService
    {
        private readonly DAL.Mappers.ClienteDao _dao = DAL.Mappers.ClienteDao.GetInstance();

        public List<Cliente> ObtenerTodos()
            => _dao.GetAll();

        public List<Cliente> Buscar(string nombre, string apellido, string documento)
        {
            string nomApe = BuildNomApe(nombre, apellido);
            string doc = string.IsNullOrWhiteSpace(documento) ? null : documento.Trim();

            return _dao.Buscar(nomApe, doc, null, null, null);
        }

        public Cliente GetById(int id)
            => _dao.GetById(id);

        public int Crear(Cliente c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            return _dao.Add(c);
        }

        public bool Actualizar(Cliente c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            return _dao.Update(c);
        }

        public bool Eliminar(Cliente c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            return _dao.Delete(c);
        }

        private static string BuildNomApe(string nombre, string apellido)
        {
            nombre = (nombre ?? "").Trim();
            apellido = (apellido ?? "").Trim();
            if (string.IsNullOrEmpty(nombre) && string.IsNullOrEmpty(apellido)) return null;

           
            return string.IsNullOrEmpty(apellido) ? nombre : $"{nombre} {apellido}";
        }
    }
}
