using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    public class CuentaDao : Mappers.ICRUD<BE.Cuenta>
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Services.Security.Crypto.Decript(
            Services.Helpers.FileHelper.GetInstance(configFilePath).ReadFile());

        private static CuentaDao _instance;
        public static CuentaDao GetInstance() => _instance ?? (_instance = new CuentaDao());
        private CuentaDao() { }

        public BE.Cuenta GetById(int id)
        {
            const string sql = @"
                SELECT TOP 1 IdCuenta, ClienteId, NumeroCuenta, CBU, Alias,
                       TipoCuenta, Moneda, Saldo, FechaApertura
                FROM Cuenta WHERE IdCuenta=@Id;";
            var dt = Services.SqlHelpers.GetInstance(_connString)
                    .GetDataTable(sql, new List<SqlParameter> { new SqlParameter("@Id", id) });
            return DAL.Mappers.MPCuenta.GetInstance().Map(dt);
        }

        public List<BE.Cuenta> GetAll()
        {
            const string sql = @"
                SELECT IdCuenta, ClienteId, NumeroCuenta, CBU, Alias,
                       TipoCuenta, Moneda, Saldo, FechaApertura
                FROM Cuenta ORDER BY IdCuenta DESC;";
            var dt = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sql);
            return DAL.Mappers.MPCuenta.GetInstance().MapCuentas(dt);
        }

        public List<BE.Cuenta> GetByCliente(int clienteId)
        {
            const string sql = @"
                SELECT IdCuenta, ClienteId, NumeroCuenta, CBU, Alias,
                       TipoCuenta, Moneda, Saldo, FechaApertura
                FROM Cuenta WHERE ClienteId=@Cli ORDER BY IdCuenta DESC;";
            var ps = new List<SqlParameter> { new SqlParameter("@Cli", clienteId) };
            var dt = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sql, ps);
            return DAL.Mappers.MPCuenta.GetInstance().MapCuentas(dt);
        }

        public int Add(BE.Cuenta c)
        {
            const string sql = @"
                INSERT INTO Cuenta
                (ClienteId, NumeroCuenta, CBU, Alias, TipoCuenta, Moneda, Saldo, FechaApertura)
                VALUES
                (@CliId, @Num, @CBU, @Alias, @Tipo, @Moneda, @Saldo, @Fec);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@CliId", c.ClienteId),
                new SqlParameter("@Num", (object)c.NumeroCuenta ?? DBNull.Value),
                new SqlParameter("@CBU", (object)c.CBU ?? DBNull.Value),
                new SqlParameter("@Alias", (object)c.Alias ?? DBNull.Value),
                new SqlParameter("@Tipo", (object)c.TipoCuenta ?? DBNull.Value),
                new SqlParameter("@Moneda", (object)c.Moneda ?? DBNull.Value),
                new SqlParameter("@Saldo", c.Saldo),
                new SqlParameter("@Fec", c.FechaApertura == default ? (object)DBNull.Value : c.FechaApertura)
            };
            return (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(sql, ps);
        }

        public bool Update(BE.Cuenta c)
        {
            const string sql = @"
                UPDATE Cuenta SET
                    NumeroCuenta=@Num, CBU=@CBU, Alias=@Alias,
                    TipoCuenta=@Tipo, Moneda=@Moneda, Saldo=@Saldo, FechaApertura=@Fec
                WHERE IdCuenta=@Id;";
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@Id", c.IdCuenta),
                new SqlParameter("@Num", (object)c.NumeroCuenta ?? DBNull.Value),
                new SqlParameter("@CBU", (object)c.CBU ?? DBNull.Value),
                new SqlParameter("@Alias", (object)c.Alias ?? DBNull.Value),
                new SqlParameter("@Tipo", (object)c.TipoCuenta ?? DBNull.Value),
                new SqlParameter("@Moneda", (object)c.Moneda ?? DBNull.Value),
                new SqlParameter("@Saldo", c.Saldo),
                new SqlParameter("@Fec", c.FechaApertura == default ? (object)DBNull.Value : c.FechaApertura)
            };
            return Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps) > 0;
        }

        public bool Delete(BE.Cuenta c)
        {
            const string sql = "DELETE FROM Cuenta WHERE IdCuenta=@Id;";
            var ps = new List<SqlParameter> { new SqlParameter("@Id", c.IdCuenta) };
            return Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps) > 0;
        }

        // ICRUD
        bool Mappers.ICRUD<BE.Cuenta>.Add(BE.Cuenta alta) => Add(alta) > 0;
        List<BE.Cuenta> Mappers.ICRUD<BE.Cuenta>.GetAll() => GetAll();
        BE.Cuenta Mappers.ICRUD<BE.Cuenta>.GetById(int id) => GetById(id);
        bool Mappers.ICRUD<BE.Cuenta>.Update(BE.Cuenta update) => Update(update);
        bool Mappers.ICRUD<BE.Cuenta>.Delete(BE.Cuenta delete) => Delete(delete);
    }
}
