using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace BLL.Services
{
    public interface IDVService
    {
        DVH CalcularDVHUsuario(Usuario u);
    }
}
