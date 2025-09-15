using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BE;
using DAL.Mappers;
using BLL.Contracts;

namespace BLL.Services
{
    public class CuentaService : ICuentaService
    {
        private readonly CuentaDao _dao = CuentaDao.GetInstance();

        public Cuenta GetById(int id) => _dao.GetById(id);
        public List<Cuenta> GetAll() => _dao.GetAll();
        public List<Cuenta> GetByCliente(int clienteId) => _dao.GetByCliente(clienteId);

        public int Crear(Cuenta c) => _dao.Add(c);
        public bool Actualizar(Cuenta c) => _dao.Update(c);
        public bool Eliminar(Cuenta c) => _dao.Delete(c);

        public List<Cuenta> Buscar(string cliente = null, string tipo = null, string estado = null)
            => _dao.Buscar(cliente, tipo, estado);
        public string GenerarNumeroCuenta(int clienteId)
        {
            string baseNum = (clienteId.ToString().PadLeft(9, '0') + DateTime.UtcNow.Ticks.ToString().Substring(10, 2)).Substring(0, 11);
            int chk = Mod97(baseNum);
            return $"{baseNum}{chk:00}";
        }

        public string GenerarCBU()
        {
            var rnd = new Random();
            string banco = "285"; // cualquiera
            string suc = rnd.Next(1, 999).ToString().PadLeft(3, '0');
            string tipo = "0";
            string nro = rnd.Next(1_000_000, 9_999_999).ToString().PadLeft(13, '0');
            string baseNum = $"{banco}{suc}{tipo}{nro}";
            int chk = Mod97(baseNum);
            return $"{baseNum}{chk:00}";
        }
        public string GenerarAlias()
        {
            string[] w1 = { "pampa", "lago", "mate", "ceibo", "tigre", "norte", "sur", "andes" };
            string[] w2 = { "verde", "azul", "rojo", "gris", "blanco", "negro", "dorado", "claro" };
            var rnd = new Random();
            return $"{w1[rnd.Next(w1.Length)]}.{w2[rnd.Next(w2.Length)]}.{rnd.Next(100, 999)}";
        }
        private static int Mod97(string digits)
        {
            int acc = 0;
            foreach (char c in digits)
                acc = (acc * 10 + (c - '0')) % 97;
            return acc == 0 ? 97 : acc;
        }
    }
}
