using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPUsuario
    {
        #region Singleton
        private static MPUsuario _instance;

        public static MPUsuario GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MPUsuario();
            }

            return _instance;
        }
        #endregion

        public BE.Usuario MapUser(DataTable dt)
        {
            try
            {
                BE.Usuario usuario = new BE.Usuario()
                {
                    Id = dt.Rows[0].Field<int>("IdUsuario"),
                    Name = dt.Rows[0].Field<string>("Nombre"),
                    LastName = dt.Rows[0].Field<string>("Apellido"),
                    Email = dt.Rows[0].Field<string>("Mail"),
                    UserName = dt.Rows[0].Field<string>("Usuario"),
                    Password = dt.Rows[0].Field<string>("Password"),
                    Tries = dt.Rows[0].Field<int>("NroIntentos"),
                    Permisos = new List<Permiso>(),
                    Language = IdiomaDao.GetInstance().GetById(dt.Rows[0].Field<int>("IdIdioma")),
                    Enabled = EstadoDao.GetInstance().GetById(dt.Rows[0].Field<int>("IdEstado"))
                };

                HashSet<BE.Permiso> permisosUsuario = new HashSet<BE.Permiso>();
                List<BE.Familia> familiasUsuario = new List<BE.Familia>();

                permisosUsuario = PatenteDao.GetInstance().GetPatentesUsuario(usuario.Id);
                familiasUsuario = FamiliaDao.GetInstance().GetFamiliasUsuario(usuario.Id);

                if (familiasUsuario.Count() >= 1)
                {
                    foreach (Familia familia in familiasUsuario)
                    {
                        foreach (Permiso permiso in familia.Patentes)
                        {
                            if (!permisosUsuario.Any(x => x.Id == permiso.Id))
                                permisosUsuario.Add(permiso);
                        }
                    }
                }
                if (permisosUsuario.Count() >= 1)
                {
                    foreach (Permiso permiso in permisosUsuario)
                    {
                        usuario.Permisos.Add(permiso);
                    }
                }

                return usuario;

            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public List<BE.Usuario> Map(DataTable dt)
        {
            try
            {
                List<BE.Usuario> usuarios = new List<BE.Usuario>();
                foreach (DataRow dr in dt.Rows)
                {
                    BE.Usuario usuario = (new BE.Usuario()
                    {
                        Id = dr.Field<int>("IdUsuario"),
                        Name = dr.Field<string>("Nombre"),
                        UserName = dr.Field<string>("Usuario"),
                        LastName = dr.Field<string>("Apellido"),
                        Email = dr.Field<string>("Mail"),
                        Password = dr.Field<string>("Password"),
                        Permisos = new List<Permiso>(),
                        Tries = dr.Field<int>("NroIntentos"),
                        Language = IdiomaDao.GetInstance().GetById(dr.Field<int>("IdIdioma")),
                        Enabled = EstadoDao.GetInstance().GetById(dr.Field<int>("IdEstado"))
                    }); ;

                    HashSet<BE.Permiso> permisosUsuario = new HashSet<BE.Permiso>();
                    List<BE.Familia> familiasUsuario = new List<BE.Familia>();

                    permisosUsuario = PatenteDao.GetInstance().GetPatentesUsuario(usuario.Id);
                    familiasUsuario = FamiliaDao.GetInstance().GetFamiliasUsuario(usuario.Id);

                    if (familiasUsuario.Count() >= 1)
                    {
                        foreach (Familia familia in familiasUsuario)
                        {
                            foreach (Permiso permiso in familia.Patentes)
                            {
                                if (!permisosUsuario.Any(x => x.Id == permiso.Id))
                                    permisosUsuario.Add(permiso);
                            }
                        }
                    }
                    if (permisosUsuario.Count() >= 1)
                    {
                        foreach (Permiso permiso in permisosUsuario)
                        {
                            usuario.Permisos.Add(permiso);
                        }
                    }
                    usuarios.Add(usuario);

                }
                ;


                return usuarios;

            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
