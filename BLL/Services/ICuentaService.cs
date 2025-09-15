using System.Collections.Generic;
using BE;

namespace BLL.Contracts
{
    public interface ICuentaService
    {
        Cuenta GetById(int id);
        List<Cuenta> GetAll();
        List<Cuenta> GetByCliente(int clienteId);
        int Crear(Cuenta c);
        bool Actualizar(Cuenta c);
        bool Eliminar(Cuenta c);
        List<Cuenta> Buscar(string cliente = null, string tipo = null, string estado = null);
        string GenerarNumeroCuenta(int clienteId);
        string GenerarCBU();
        string GenerarAlias();
    }
}
