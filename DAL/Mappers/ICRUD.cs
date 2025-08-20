using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    public interface ICRUD<T>
    {
        bool Add(T alta);
        List<T> GetAll();
        T GetById(int id);
        bool Update(T update);
        bool Delete(T delete);
    }
}
