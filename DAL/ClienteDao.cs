using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    public class ClienteDao : Mappers.ICRUD<BE.Cliente>
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Services.Security.Crypto.Decript(
            Services.Helpers.FileHelper.GetInstance(configFilePath).ReadFile());

        private static ClienteDao _instance;
        public static ClienteDao GetInstance() => _instance ?? (_instance = new ClienteDao());
        private ClienteDao() { }

        // BÚSQUEDA/FILTROS
        public List<BE.Cliente> Buscar(string nomApe = null, string documento = null,
                                       string estadoCivil = null, string situacionFiscal = null, bool? esPep = null)
        {
            const string sql = @"
                SELECT IdCliente, Nombre, Apellido, FechaNacimiento, LugarNacimiento, Nacionalidad,
                       EstadoCivil, DocumentoIdentidad, CUITCUILCDI, Domicilio, Telefono,
                       CorreoElectronico, Ocupacion, SituacionFiscal, EsPEP
                FROM Cliente
                WHERE (@NomApe IS NULL OR (Nombre + ' ' + Apellido) LIKE '%' + @NomApe + '%')
                  AND (@Doc    IS NULL OR DocumentoIdentidad LIKE @Doc + '%')
                  AND (@EC     IS NULL OR EstadoCivil = @EC)
                  AND (@SF     IS NULL OR SituacionFiscal = @SF)
                  AND (@PEP IS NULL OR EsPEP = @PEP)
                ORDER BY Apellido, Nombre;";

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@NomApe", (object)NullIfEmpty(nomApe) ?? DBNull.Value),
                new SqlParameter("@Doc",    (object)NullIfEmpty(documento) ?? DBNull.Value),
                new SqlParameter("@EC",     (object)NullIfEmpty(estadoCivil) ?? DBNull.Value),
                new SqlParameter("@SF",     (object)NullIfEmpty(situacionFiscal) ?? DBNull.Value),
                new SqlParameter("@PEP",    (object)(esPep.HasValue ? (esPep.Value ? 1 : 0) : (int?)null) ?? DBNull.Value)
            };

            var dt = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sql, ps);
            return DAL.Mappers.MPCliente.GetInstance().MapClientes(dt);
        }

        public BE.Cliente GetById(int id)
        {
            const string sql = @"
                SELECT TOP 1 IdCliente, Nombre, Apellido, FechaNacimiento, LugarNacimiento, Nacionalidad,
                       EstadoCivil, DocumentoIdentidad, CUITCUILCDI, Domicilio, Telefono,
                       CorreoElectronico, Ocupacion, SituacionFiscal, EsPEP
                FROM Cliente WHERE IdCliente = @Id;";
            var ps = new List<SqlParameter> { new SqlParameter("@Id", id) };
            var dt = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sql, ps);
            return DAL.Mappers.MPCliente.GetInstance().Map(dt);
        }

        public List<BE.Cliente> GetAll()
        {
            const string sql = @"
                SELECT IdCliente, Nombre, Apellido, FechaNacimiento, LugarNacimiento, Nacionalidad,
                       EstadoCivil, DocumentoIdentidad, CUITCUILCDI, Domicilio, Telefono,
                       CorreoElectronico, Ocupacion, SituacionFiscal, EsPEP
                FROM Cliente
                ORDER BY Apellido, Nombre;";
            var dt = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sql);
            return DAL.Mappers.MPCliente.GetInstance().MapClientes(dt);
        }

        // ALTAS/BAJAS/MODIFICACIONES (con DVH opcional)
        public int Add(BE.Cliente c, BE.DVH dvh = null)
        {
            const string sql = @"
                INSERT INTO Cliente
                (Nombre, Apellido, FechaNacimiento, LugarNacimiento, Nacionalidad, EstadoCivil,
                 DocumentoIdentidad, CUITCUILCDI, Domicilio, Telefono, CorreoElectronico,
                 Ocupacion, SituacionFiscal, EsPEP {0})
                VALUES
                (@Nombre,@Apellido,@FechaNacimiento,@LugarNacimiento,@Nacionalidad,@EstadoCivil,
                 @Documento,@CUIT,@Domicilio,@Telefono,@Correo,@Ocupacion,@SituacionFiscal,@EsPEP {1});
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var columnasDVH = dvh == null ? "" : ", DVH";
            var valoresDVH = dvh == null ? "" : ", @DVH";

            var cmdText = string.Format(sql, columnasDVH, valoresDVH);

            var ps = Params(c);
            if (dvh != null) ps.Add(new SqlParameter("@DVH", dvh.dvh));

            var id = (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(cmdText, ps);

            // (Opcional) DVV:
            // var dvv = new BE.DVV { tabla = "Cliente", dvv = DVVDao.GetInstance().CalculateDVV("Cliente") };
            // DVVDao.GetInstance().AddUpdateDVV(dvv);

            return id;
        }

        public bool Update(BE.Cliente c, BE.DVH dvh = null)
        {
            var dvhSet = dvh == null ? "" : ", DVH=@DVH";
            var sql = $@"
                UPDATE Cliente SET
                    Nombre=@Nombre, Apellido=@Apellido, FechaNacimiento=@FechaNacimiento,
                    LugarNacimiento=@LugarNacimiento, Nacionalidad=@Nacionalidad,
                    EstadoCivil=@EstadoCivil, DocumentoIdentidad=@Documento,
                    CUITCUILCDI=@CUIT, Domicilio=@Domicilio, Telefono=@Telefono,
                    CorreoElectronico=@Correo, Ocupacion=@Ocupacion,
                    SituacionFiscal=@SituacionFiscal, EsPEP=@EsPEP
                    {dvhSet}
                WHERE IdCliente=@Id;";

            var ps = Params(c);
            ps.Add(new SqlParameter("@Id", c.IdCliente));
            if (dvh != null) ps.Add(new SqlParameter("@DVH", dvh.dvh));

            var rows = Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps);

            // (Opcional) DVV:
            // var dvv = new BE.DVV { tabla = "Cliente", dvv = DVVDao.GetInstance().CalculateDVV("Cliente") };
            // DVVDao.GetInstance().AddUpdateDVV(dvv);

            return rows > 0;
        }

        public bool Delete(BE.Cliente c)
        {
            const string sql = "DELETE FROM Cliente WHERE IdCliente=@Id;";
            var ps = new List<SqlParameter> { new SqlParameter("@Id", c.IdCliente) };
            var rows = Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps);

            // (Opcional) DVV también
            return rows > 0;
        }

        // ---- ICRUD<T> (si no usás estos, podés dejarlos NotImplemented) ----
        bool Mappers.ICRUD<BE.Cliente>.Add(BE.Cliente alta) => Add(alta) > 0;
        BE.Cliente Mappers.ICRUD<BE.Cliente>.GetById(int id) => GetById(id);
        bool Mappers.ICRUD<BE.Cliente>.Update(BE.Cliente update) => Update(update);
        bool Mappers.ICRUD<BE.Cliente>.Delete(BE.Cliente delete) => Delete(delete);

        private static List<SqlParameter> Params(BE.Cliente c) => new List<SqlParameter>
        {
            new SqlParameter("@Nombre", (object)c.Nombre ?? DBNull.Value),
            new SqlParameter("@Apellido", (object)c.Apellido ?? DBNull.Value),
            new SqlParameter("@FechaNacimiento", c.FechaNacimiento == default ? (object)DBNull.Value : c.FechaNacimiento),
            new SqlParameter("@LugarNacimiento", (object)c.LugarNacimiento ?? DBNull.Value),
            new SqlParameter("@Nacionalidad", (object)c.Nacionalidad ?? DBNull.Value),
            new SqlParameter("@EstadoCivil", (object)c.EstadoCivil ?? DBNull.Value),
            new SqlParameter("@Documento", (object)c.DocumentoIdentidad ?? DBNull.Value),
            new SqlParameter("@CUIT", (object)c.CUITCUILCDI ?? DBNull.Value),
            new SqlParameter("@Domicilio", (object)c.Domicilio ?? DBNull.Value),
            new SqlParameter("@Telefono", (object)c.Telefono ?? DBNull.Value),
            new SqlParameter("@Correo", (object)c.CorreoElectronico ?? DBNull.Value),
            new SqlParameter("@Ocupacion", (object)c.Ocupacion ?? DBNull.Value),
            new SqlParameter("@SituacionFiscal", (object)c.SituacionFiscal ?? DBNull.Value),
            new SqlParameter("@EsPEP", c.EsPEP)
        };

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
