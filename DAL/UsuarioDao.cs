using BE;
using DAL.Mappers;
using Services;
using Servicios.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;             
using Services = Servicios;     


namespace DAL
{
    public class UsuarioDao : ICRUD<BE.Usuario>
    {

        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Services.Security.Crypto.Decript(Services.Helpers.FileHelper.GetInstance(configFilePath).ReadFile());

        #region Singleton
        private static UsuarioDao _instance;
        public static UsuarioDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new UsuarioDao();
            }

            return _instance;
        }
        #endregion

        #region CRUD USUARIO
        public BE.Usuario GetById(int idUsuario)
        {
            string SelectId = "SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido, Mail, Password, NroIntentos FROM Usuario WHERE IdUsuario = @idUsuario";
            List<SqlParameter> parameter = new List<SqlParameter>();
            parameter.Add(new SqlParameter("@idUsuario", idUsuario));

            return Mappers.MPUsuario.GetInstance().MapUser(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectId, parameter));
        }
        public BE.Usuario GetByUserName(string username)
        {
            string SelectId = "SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido, Mail, Password, NroIntentos FROM Usuario WHERE Usuario = @user";
            List<SqlParameter> parameter = new List<SqlParameter>();
            parameter.Add(new SqlParameter("@user", username));

            return Mappers.MPUsuario.GetInstance().MapUser(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectId, parameter));
        }
        public bool Add(Usuario usuario, DVH dvh)
        {
            bool returnValue = false;
            string queryInsert = "INSERT INTO Usuario (IdEstado, IdIdioma, Usuario, Password, Nombre, Apellido, Mail, NroIntentos, DVH) VALUES (@idEstado, @IdIdioma, @Usuario, @Password, @Nombre, @Apellido, @Mail, @NroIntentos, @DVH)";
            string checkPrev = "SELECT COUNT(*) FROM Usuario WHERE Usuario = @username OR Mail= @email";
            try
            {
                if ((int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(checkPrev, new List<SqlParameter> { new SqlParameter("@username", usuario.UserName), new SqlParameter("@email", usuario.Email) }) > 0)
                {
                    throw new Exception(message: "Email o Usuario ya registrado");
                }
                if (usuario != null)
                {

                    List<SqlParameter> sqlParams = new List<SqlParameter>();
                    sqlParams.Add(new SqlParameter("@idEstado", usuario.Enabled.Id));
                    sqlParams.Add(new SqlParameter("@IdIdioma", usuario.Language.Id));
                    sqlParams.Add(new SqlParameter("@Usuario", usuario.UserName));
                    sqlParams.Add(new SqlParameter("@Password", usuario.Password));
                    sqlParams.Add(new SqlParameter("@Nombre", usuario.Name));
                    sqlParams.Add(new SqlParameter("@Apellido", usuario.LastName));
                    sqlParams.Add(new SqlParameter("@Mail", usuario.Email));
                    sqlParams.Add(new SqlParameter("@NroIntentos", usuario.Tries));
                    sqlParams.Add(new SqlParameter("@DVH", dvh.dvh));
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryInsert, sqlParams) > 0)
                    {
                        returnValue = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "Usuario",
                            dvv = DVVDao.GetInstance().CalculateDVV("Usuario"),
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);
                    }
                }
            }
            catch (Exception e)
            {

                throw e;
            }
            return returnValue;
        }
        public bool Delete(Usuario usuario, BE.DVH dvh)
        {
            bool result = false;
            string delUsuario = "UPDATE Usuario SET IdEstado = 3, DVH =  @dvh WHERE IdUsuario = @idUsuario";
            List<SqlParameter> sqlParams = new List<SqlParameter>() { new SqlParameter("@idUsuario", usuario.Id), new SqlParameter("@dvh", dvh.dvh) };

            try
            {
                HashSet<Permiso> patentesUsuario = PatenteDao.GetInstance().GetPatentesUsuario(usuario.Id);

                foreach (BE.Permiso patente in patentesUsuario)
                {
                    if (!PatenteDao.GetInstance().CheckPatenteAsing(patente.Id))
                    {
                        throw new Exception(message: string.Format("No se puede eliminar el Usuario, ya que quedaría la patente {0} sin asignar", patente.Name));
                    }
                }

                if (SqlHelpers.GetInstance(_connString).ExecuteQuery(delUsuario, sqlParams) > 0)
                {
                    result = true;
                    DVV dvv = new DVV()
                    {
                        tabla = "Usuario",
                        dvv = DVVDao.GetInstance().CalculateDVV("Usuario"),
                    };
                    DVVDao.GetInstance().AddUpdateDVV(dvv);
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return result;
        }
        public List<Usuario> GetAll()
        {
            string selectAll = "SELECT idUsuario, IdEstado, IdIdioma, Usuario, Password, Nombre, Apellido, Mail, NroIntentos FROM Usuario";
            return Mappers.MPUsuario.GetInstance().Map(SqlHelpers.GetInstance(_connString).GetDataTable(selectAll));
        }
        public List<Usuario> GetAllActive()
        {
            string selectAll = "SELECT idUsuario, IdEstado, IdIdioma, Usuario, Password, Nombre, Apellido, Mail, NroIntentos FROM Usuario WHERE IdEstado != 3";
            return Mappers.MPUsuario.GetInstance().Map(SqlHelpers.GetInstance(_connString).GetDataTable(selectAll));
        }
        public bool Update(Usuario usuario, BE.DVH dvh)
        {
            bool returnValue = false;
            string queryUpdate = "Update Usuario SET Usuario = @Usuario, Nombre = @Nombre, Password = @Password, Apellido = @Apellido, Mail = @Mail, IdEstado = @IdEstado, NroIntentos = @NroIntentos, IdIdioma = @IdIdioma, DVH = @DVH FROM Usuario WHERE IdUsuario = @IdUsuario";
            try
            {
                if (usuario != null)
                {
                    List<SqlParameter> sqlParams = new List<SqlParameter>();
                    sqlParams.Add(new SqlParameter("@IdUsuario", usuario.Id));
                    sqlParams.Add(new SqlParameter("@IdEstado", usuario.Enabled.Id));
                    sqlParams.Add(new SqlParameter("@IdIdioma", usuario.Language.Id));
                    sqlParams.Add(new SqlParameter("@Usuario", usuario.UserName));
                    sqlParams.Add(new SqlParameter("@Password", usuario.Password));
                    sqlParams.Add(new SqlParameter("@Nombre", usuario.Name));
                    sqlParams.Add(new SqlParameter("@Apellido", usuario.LastName));
                    sqlParams.Add(new SqlParameter("@Mail", usuario.Email));
                    sqlParams.Add(new SqlParameter("@NroIntentos", usuario.Tries));
                    sqlParams.Add(new SqlParameter("@DVH", dvh.dvh));
                    if (SqlHelpers.GetInstance(_connString).ExecuteQuery(queryUpdate, sqlParams) > 0)
                    {
                        returnValue = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "Usuario",
                            dvv = DVVDao.GetInstance().CalculateDVV("Usuario"),
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);
                    }
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return returnValue;
        }

        #endregion

        public bool Login(string user, string password)
        {
            bool result = false;
            string selectQuery = "SELECT COUNT(*) FROM Usuario WHERE Usuario = @user AND Password = @password";
            try
            {
                List<SqlParameter> sqlParams = new List<SqlParameter>()
                {
                    new SqlParameter("@user", user),
                    new SqlParameter("@password", password)
                };
                if ((int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(selectQuery, sqlParams) > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return result;
        }

        #region UsuarioFamilia
        public bool AddUsuarioFamilia(Usuario usuario, Familia familia, DVH dvh)
        {
            bool result = false;
            try
            {
                string queryCheckExistence = @"SELECT COUNT(*) FROM UsuarioFamilia as US WHERE IdFamilia = @idFamilia AND IdUsuario = @IdUsuario";
                string insertQuery = "INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia, DVH) VALUES (@idUsuario, @idFamilia, @dvh)";
                string queryUpdate = "UPDATE UsuarioFamilia SET DVH = @dvh WHERE IdFamilia = @idFamilia AND IdUsuario = @idUsuario";
                List<SqlParameter> sqlParams = new List<SqlParameter>()
                {
                    new SqlParameter("@idUsuario", usuario.Id),
                    new SqlParameter("@idFamilia", familia.Id),
                    new SqlParameter("@dvh", dvh.dvh)
                };
                int count = (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(queryCheckExistence, new List<SqlParameter> { new SqlParameter("@idFamilia", familia.Id), new SqlParameter("@idUsuario", usuario.Id) });
                if (count > 0)
                {
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryUpdate, sqlParams) > 0)
                    {
                        result = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "UsuarioFamilia",
                            dvv = DVVDao.GetInstance().CalculateDVV("UsuarioFamilia"),
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);
                    }
                }
                else
                {

                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(insertQuery, sqlParams) > 0)
                    {
                        result = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "UsuarioFamilia",
                            dvv = DVVDao.GetInstance().CalculateDVV("UsuarioFamilia"),
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);
                    }
                }
            }
            catch (Exception e)
            {

                throw e;
            }


            return result;
        }
        public bool DeleteUsuarioFamilia(int idFamilia)
        {
            bool result = false;
            string queryDelUsuarioFamilia = "DELETE FROM UsuarioFamilia WHERE IdFamilia = @IdFamilia";
            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@IdFamilia", idFamilia)
            };
            try
            {
                if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryDelUsuarioFamilia, sqlParams) > 0)
                {
                    result = true;
                    DVV dvv = new DVV()
                    {
                        tabla = "UsuarioFamilia",
                        dvv = DVVDao.GetInstance().CalculateDVV("UsuarioFamilia"),
                    };
                    DVVDao.GetInstance().AddUpdateDVV(dvv);
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return result;
        }
        public bool DeleteUsuarioFamilia(int idUsuario, int idFamilia)
        {
            bool result = false;
            string queryDelUsuarioFamilia = "DELETE FROM UsuarioFamilia WHERE IdFamilia = @IdFamilia AND IdUsuario = @IdUsuario";
            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@IdFamilia", idFamilia),
                new SqlParameter("@IdUsuario", idUsuario)
            };
            try
            {
                HashSet<Permiso> patentesFamilia = PatenteDao.GetInstance().GetPatentesFamilia(idFamilia);

                foreach (BE.Permiso patente in patentesFamilia)
                {
                    if (!PatenteDao.GetInstance().CheckPatenteAsing(patente.Id))
                    {
                        throw new Exception(message: string.Format("No se puede eliminar el Usuario de la Familia, ya que quedaría la patente {0} sin asignar", patente.Name));
                    }
                }
                if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryDelUsuarioFamilia, sqlParams) > 0)
                {
                    result = true;
                    DVV dvv = new DVV()
                    {
                        tabla = "UsuarioFamilia",
                        dvv = DVVDao.GetInstance().CalculateDVV("UsuarioFamilia"),
                    };
                    DVVDao.GetInstance().AddUpdateDVV(dvv);
                }
            }
            catch (Exception e)
            {

                throw e;
            }
            return result;
        }
        #endregion

        #region UsuarioPatente
        public bool AddUsuarioPatente(BE.Usuario usuario, BE.Permiso patente, BE.DVH dvh)
        {
            bool result = false;
            try
            {
                string queryCheckExistence = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdPatente = @idPatente AND IdUsuario = @IdUsuario";
                string insertQuery = "INSERT INTO UsuarioPatente (IdUsuario, IdPatente, DVH) VALUES (@idUsuario, @idPatente, @dvh)";
                string queryUpdate = "UPDATE UsuarioPatente SET DVH = @dvh WHERE IdPatente = @idPatente AND IdUsuario = @idUsuario";
                List<SqlParameter> sqlParams = new List<SqlParameter>()
                {
                    new SqlParameter("@idUsuario", usuario.Id),
                    new SqlParameter("@idPatente", patente.Id),
                    new SqlParameter("@dvh", dvh.dvh)
                };
                int count = (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(queryCheckExistence, new List<SqlParameter> { new SqlParameter("@idPatente", patente.Id), new SqlParameter("@idUsuario", usuario.Id) });
                if (count > 0)
                {
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryUpdate, sqlParams) > 0)
                    {
                        result = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "UsuarioPatente",
                            dvv = DVVDao.GetInstance().CalculateDVV("UsuarioPatente"),
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);

                    }

                }
                else
                {
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(insertQuery, sqlParams) > 0)
                    {
                        result = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "UsuarioPatente",
                            dvv = DVVDao.GetInstance().CalculateDVV("UsuarioPatente"),
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);

                    }
                }


            }
            catch (Exception e)
            {

                throw e;
            }


            return result;
        }
        public bool DeleteUsuarioPatente(BE.Usuario usuario, BE.Permiso patente)
        {
            bool result = false;
            string queryDelUsuarioPatente = "DELETE FROM UsuarioPatente WHERE IdUsuario = @IdUsuario AND IdPatente = @IdPatente";
            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", usuario.Id),
                new SqlParameter("@IdPatente", patente.Id)
            };
            try
            {
                if (!PatenteDao.GetInstance().CheckPatenteAsing(patente.Id))
                {
                    throw new Exception(message: "No se puede eliminar la Patente del Usuario, ya que quedaría sin asignar");
                }
                if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryDelUsuarioPatente, sqlParams) > 0)
                {
                    result = true;
                    DVV dvv = new DVV()
                    {
                        tabla = "UsuarioPatente",
                        dvv = DVVDao.GetInstance().CalculateDVV("UsuarioPatente"),
                    };
                    DVVDao.GetInstance().AddUpdateDVV(dvv);
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return result;
        }
        #endregion


        #region NOT IMPLEMENTED
        public bool Add(Usuario alta)
        {
            throw new NotImplementedException();
        }

        public bool Update(Usuario update)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Usuario alta)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
