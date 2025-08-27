using System.Collections.Generic;
using BE;

namespace BLL.Services
{
    public interface IClienteService
    {
        List<Cliente> ObtenerTodos();
        List<Cliente> Buscar(string nombre, string apellido, string documento);
        Cliente GetById(int id);

        int Crear(Cliente c);
        bool Actualizar(Cliente c);
        bool Eliminar(Cliente c);
      //OJO CON EL REACTIVARRRRR
    }
}
