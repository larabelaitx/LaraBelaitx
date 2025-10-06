using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Mappers;
using Services;
using BE;

namespace DAL
{
    public class FamiliaDao : ICRUD<BE.Familia>
    {

        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Crypto.Decript(FileHelper.GetInstance(configFilePath).ReadFile());

        #region Singleton
        private static FamiliaDao _instance;
        public static FamiliaDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FamiliaDao();
            }

            return _instance;
        }
        #endregion

        #region CRUD Familia
        public bool Add(Familia familia)
        {
            bool returnValue = false;
            string queryInsert = "INSERT INTO Familia (Nombre, Descripcion) VALUES (@Nombre, @Descripcion)";
            try
            {
                if (familia != null)
                {
                    List<SqlParameter> sqlParams = new List<SqlParameter>
                    {
                        new SqlParameter("@Nombre", familia.Name),
                        new SqlParameter("@Descripcion", familia.Descripcion)
                    };
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryInsert, sqlParams) > 0)
                    {
                        returnValue = true;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return returnValue;
        }
        public bool Delete(Familia familia)
        {
            const string sqlDelFamilia = "DELETE FROM Familia WHERE IdFamilia = @IdFamilia";
            var sqlParams = new List<SqlParameter> { new SqlParameter("@IdFamilia", familia.Id) };

            try
            {
                List<BE.Usuario> usuariosFamilia = GetUsuariosFamilias(familia.Id);
                HashSet<BE.Permiso> patentesFamilia = PatenteDao.GetInstance().GetPatentesFamilia(familia.Id);

                foreach (var patente in patentesFamilia)
                {
                    if (!PatenteDao.GetInstance().CheckPatenteAsing(patente.Id))
                        throw new Exception("No se puede eliminar la familia: quedaría patente sin asignar.");
                }
                if (usuariosFamilia.Count > 0)
                {
                    UsuarioDao.GetInstance().DeleteUsuarioFamilia(familia.Id);
                    DVVDao.GetInstance().AddUpdateDVV(new DVV
                    {
                        tabla = "UsuarioFamilia",
                        dvv = DVVDao.GetInstance().CalculateDVV("UsuarioFamilia")
                    });
                }
                if (patentesFamilia.Count > 0)
                {
                    PatenteDao.GetInstance().DeletePatenteFamilia(familia.Id);
                    DVVDao.GetInstance().AddUpdateDVV(new DVV
                    {
                        tabla = "FamiliaPatente",
                        dvv = DVVDao.GetInstance().CalculateDVV("FamiliaPatente")
                    });
                }
                Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(sqlDelFamilia, sqlParams);

                DVVDao.GetInstance().AddUpdateDVV(new DVV
                {
                    tabla = "Familia",
                    dvv = DVVDao.GetInstance().CalculateDVV("Familia")
                });

                return true;
            }
            catch
            {
                throw;
            }
        }
        public List<Familia> GetAll()
        {
            string SelectAll = "SELECT IdFamilia, Nombre, Descripcion FROM Familia";
            return Mappers.MPFamilia.GetInstance().MapFamilias(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectAll));
        }
        public Familia GetById(int IdFamilia)
        {
            string SelectId = "SELECT IdFamilia, Nombre, Descripcion FROM Familia WHERE IdFamilia = @IdFamilia";
            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@IdFamilia", IdFamilia)
            };

            return Mappers.MPFamilia.GetInstance().Map(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectId, sqlParams));
        }
        public bool Update(Familia familia)
        {
            bool returnValue = false;
            string queryUpdate = "UPDATE Familia SET Nombre = @Nombre, Descripcion = @Descripcion WHERE IdFamilia = @IdFamilia";
            try
            {
                if (familia != null)
                {
                    List<SqlParameter> sqlParameters = new List<SqlParameter>()
                    {
                        new SqlParameter("@IdFamilia",familia.Id),
                        new SqlParameter("@Nombre", familia.Name),
                        new SqlParameter("@Descripcion", familia.Descripcion)
                    };
                    if (SqlHelpers.GetInstance(_connString).ExecuteQuery(queryUpdate, sqlParameters) > 0)
                    {
                        returnValue = true;
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

        #region GestPermisos
        public bool AddUpdatePatente(Familia familia, Permiso patente, DVH dvh)
        {
            bool result = false;
            string queryCheckExistence = "SELECT COUNT(*) FROM FamiliaPatente WHERE IdFamilia = @idFamilia AND IdPatente = @idPatente";
            string queryInsert = "INSERT INTO FamiliaPatente (IdFamilia,IdPatente,DVH) VALUES (@idFamilia, @idPatente, @dvh)";
            string queryUpdate = "UPDATE FamiliaPatente SET DVH = @dvh WHERE IdFamilia = @idFamilia AND IdPatente = @idPatente";
            try
            {

                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@idFamilia", familia.Id),
                    new SqlParameter("@idPatente", patente.Id),
                    new SqlParameter("@dvh", dvh.dvh)
                };
                int count = (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(queryCheckExistence, new List<SqlParameter> { new SqlParameter("@idFamilia", familia.Id), new SqlParameter("@idPatente", patente.Id) });
                if (count > 0)
                {
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryUpdate, sqlParams) > 0)
                    {
                        result = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "FamiliaPatente",
                            dvv = DVVDao.GetInstance().CalculateDVV("FamiliaPatente")
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);
                    }
                }
                else
                {
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryInsert, sqlParams) > 0)
                    {
                        result = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "FamiliaPatente",
                            dvv = DVVDao.GetInstance().CalculateDVV("FamiliaPatente")
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
        public bool DelPatente(Familia familia, Permiso patente)
        {
            bool result = false;

            if (!PatenteDao.GetInstance().CheckPatenteAsing(patente.Id))
            {
                throw new Exception(message: "No se puede eliminar la patente de la Familia, quedaria sin asignar");
            }
            else
            {
                string queryDelUsuarioFamilia = "DELETE FROM FamiliaPatente WHERE IdFamilia = @IdFamilia and IdPatente = @IdPatente";
                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@IdFamilia", familia.Id),
                    new SqlParameter("@IdPatente", patente.Id)
                };
                try
                {
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryDelUsuarioFamilia, sqlParams) > 0)
                    {
                        result = true;
                        DVV dvv = new DVV()
                        {
                            tabla = "FamiliaPatente",
                            dvv = DVVDao.GetInstance().CalculateDVV("FamiliaPatente")
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);
                    }
                }
                catch (Exception e)
                {

                    throw e;
                }
            }

            return result;
        }
        public List<Familia> GetFamiliasUsuario(int IdUsuario)
        {
            string SelectJoin = "SELECT F.IdFamilia, Nombre, Descripcion FROM Familia F INNER JOIN UsuarioFamilia UF ON F.IdFamilia = UF.IdFamilia WHERE UF.IdUsuario = @IdUsuario";
            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", IdUsuario)
            };
            return Mappers.MPFamilia.GetInstance().MapFamilias(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectJoin, sqlParams));
        }
        public List<Usuario> GetUsuariosFamilias(int IdFamilia)
        {
            string SelectJoin = "SELECT U.idUsuario, U.IdEstado, U.IdIdioma, U.Usuario, U.Password, U.Nombre, U.Apellido, U.Mail, U.NroIntentos FROM Usuario as U INNER JOIN UsuarioFamilia UF ON U.IdUsuario = UF.IdUsuario WHERE UF.IdFamilia = @IdFamilia";
            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@IdFamilia", IdFamilia)
            };
            return Mappers.MPUsuario.GetInstance().Map(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectJoin, sqlParams));
        }
        #endregion

        public bool SetPatentes(int idFamilia, IEnumerable<int> idsPatentesSeleccionadas)
        {
            var actuales = PatenteDao.GetInstance().GetPatentesFamilia(idFamilia);
            var actualesIds = new HashSet<int>(actuales.Select(p => p.Id));
            var nuevasIds = new HashSet<int>(idsPatentesSeleccionadas ?? Enumerable.Empty<int>());

            var altas = nuevasIds.Except(actualesIds).ToList();
            var bajas = actualesIds.Except(nuevasIds).ToList();

            foreach (var idPat in bajas)
                if (!PatenteDao.GetInstance().CheckPatenteAsing(idPat))
                    throw new Exception("No se puede quitar la última asignación de una patente (huérfana).");

            foreach (var idAlta in altas)
            {
                var ok = AddUpdatePatente(new BE.Familia { Id = idFamilia }, new BE.Patente { Id = idAlta }, new BE.DVH { dvh = null });
                if (!ok) throw new Exception("No se pudo asignar una patente a la familia.");
            }

            foreach (var idBaja in bajas)
            {
                var ok = DelPatente(new BE.Familia { Id = idFamilia }, new BE.Patente { Id = idBaja });
                if (!ok) throw new Exception("No se pudo quitar una patente de la familia.");
            }

            DVVDao.GetInstance().AddUpdateDVV(new BE.DVV
            {
                tabla = "FamiliaPatente",
                dvv = DVVDao.GetInstance().CalculateDVV("FamiliaPatente")
            });

            return true;
        }
    }
}
