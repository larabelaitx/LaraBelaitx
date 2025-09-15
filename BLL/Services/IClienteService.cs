using System.Collections.Generic;
using BE;

namespace BLL.Contracts
{
    public interface IClienteService
    {
        List<Cliente> Buscar(string nomApe = null, string doc = null, string estadoCivil = null, string situacionFiscal = null, bool? pep = null);
        Cliente GetById(int id);
        List<Cliente> GetAll();
        int Crear(Cliente c);
        bool Actualizar(Cliente c);
        bool Eliminar(Cliente c);
    }
}
