using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace BLL.Services
{
    public interface ICuentaService
    {
        // Lectura
        List<Cuenta> GetAll();
        List<Cuenta> GetByCliente(int clienteId);
        Cuenta GetById(int idCuenta);

        // Escritura
        int Crear(Cuenta c);
        bool Actualizar(Cuenta c);
        bool Eliminar(int idCuenta);

        // Reglas
        bool ExisteParaCliente(int clienteId);
        bool ExisteNumero(string numeroCuenta);
        bool ExisteCBU(string cbu);
        bool ExisteAlias(string alias);

        // Generadores
        string GenerarNumeroCuenta();
        string GenerarCBU();
        string GenerarAlias();
    }
}
