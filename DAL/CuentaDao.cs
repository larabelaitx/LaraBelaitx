using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DAL;
using Services;

namespace DAL.Mappers
{
    public class CuentaDao : Mappers.ICRUD<BE.Cuenta>
    {
        private static CuentaDao _instance;
        public static CuentaDao GetInstance() => _instance ?? (_instance = new CuentaDao());
        private CuentaDao() { }

        private static string Cnn => ConnectionFactory.Current;

        private (string IdCuenta, string ClienteId, string Numero, string Saldo, string Fecha, string Estado) ResolveCols()
        {
            const string meta = "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Cuenta')";
            var dt = SqlHelpers.GetInstance(Cnn).GetDataTable(meta);
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow r in dt.Rows) cols.Add(Convert.ToString(r["name"]));

            string idCuenta = cols.Contains("IdCuenta") ? "IdCuenta" : "Id";
            string cliente = cols.Contains("ClienteId") ? "ClienteId" : (cols.Contains("IdCliente") ? "IdCliente" : null);
            string numero = cols.Contains("NumeroCuenta") ? "NumeroCuenta" : (cols.Contains("NroCuenta") ? "NroCuenta" : null);
            string saldo = cols.Contains("Saldo") ? "Saldo" : (cols.Contains("SaldoActual") ? "SaldoActual" : null);
            string fecha = cols.Contains("FechaApertura") ? "FechaApertura" : (cols.Contains("FchCreacion") ? "FchCreacion" : null);
            string estado = cols.Contains("Estado") ? "Estado" : (cols.Contains("IdEstado") ? "IdEstado" : null);

            return (idCuenta, cliente, numero, saldo, fecha, estado);
        }

        private string SelectNormalized(
            (string IdCuenta, string ClienteId, string Numero, string Saldo, string Fecha, string Estado) c,
            string whereOrder = "")
        {
            string selCliente = c.ClienteId != null ? $"c.[{c.ClienteId}]" : "NULL";
            string selNumero = c.Numero != null ? $"c.[{c.Numero}]" : "NULL";
            string selSaldo = c.Saldo != null ? $"c.[{c.Saldo}]" : "CAST(0 AS decimal(18,2))";
            string selFecha = c.Fecha != null ? $"c.[{c.Fecha}]" : "NULL";
            string selEstado = c.Estado != null ? $"c.[{c.Estado}]" : "NULL";

            return $@"
                SELECT
                    c.[{c.IdCuenta}]   AS IdCuenta,
                    {selCliente}       AS ClienteId,
                    {selNumero}        AS NumeroCuenta,
                    {selSaldo}         AS Saldo,
                    {selFecha}         AS FechaApertura,
                    {selEstado}        AS Estado
                FROM dbo.Cuenta c
                {whereOrder}";
        }


        public BE.Cuenta GetById(int id)
        {
            var c = ResolveCols();
            var where = $"WHERE c.[{c.IdCuenta}] = @Id";
            var sql = SelectNormalized(c, where);

            var ps = new List<SqlParameter> { new SqlParameter("@Id", id) };
            var dt = SqlHelpers.GetInstance(Cnn).GetDataTable(sql, ps);
            return MPCuenta.GetInstance().Map(dt);
        }

        public List<BE.Cuenta> GetAll()
        {
            var c = ResolveCols();
            var sql = SelectNormalized(c, $"ORDER BY c.[{c.IdCuenta}] DESC");
            var dt = SqlHelpers.GetInstance(Cnn).GetDataTable(sql);
            return MPCuenta.GetInstance().MapCuentas(dt);
        }

        public List<BE.Cuenta> GetByCliente(int clienteId)
        {
            var c = ResolveCols();
            var where = c.ClienteId != null
                ? $"WHERE c.[{c.ClienteId}] = @Cli ORDER BY c.[{c.IdCuenta}] DESC"
                : "WHERE 1=0";

            var sql = SelectNormalized(c, where);
            var ps = new List<SqlParameter> { new SqlParameter("@Cli", clienteId) };
            var dt = SqlHelpers.GetInstance(Cnn).GetDataTable(sql, ps);
            return MPCuenta.GetInstance().MapCuentas(dt);
        }

        public List<BE.Cuenta> Buscar(string cliente = null, string tipo = null, string estado = null)
        {
            var c = ResolveCols();
            var baseSql = SelectNormalized(c, "");

            string sql;
            var ps = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(cliente) && c.ClienteId != null)
            {
                sql = $@"
                    SELECT x.*,
                           (cl.Apellido + ', ' + cl.Nombre) AS ClienteNombre
                    FROM ({baseSql}) x
                    INNER JOIN dbo.Cliente cl ON cl.IdCliente = x.ClienteId
                    WHERE 1=1 ";

                ps.Add(new SqlParameter("@Cli", "%" + cliente.Trim() + "%"));
                sql += " AND (cl.Apellido + ', ' + cl.Nombre) LIKE @Cli ";
            }
            else
            {
                sql = $"SELECT x.* FROM ({baseSql}) x WHERE 1=1 ";
            }

            if (!string.IsNullOrWhiteSpace(estado) && c.Estado != null)
            {
                sql += " AND x.Estado = @Estado ";
                ps.Add(new SqlParameter("@Estado", estado.Trim()));
            }

            sql += $" ORDER BY x.IdCuenta DESC;";

            var dt = SqlHelpers.GetInstance(Cnn).GetDataTable(sql, ps);
            return MPCuenta.GetInstance().MapCuentas(dt);
        }

        public int Add(BE.Cuenta cu)
        {
            var cols = ResolveCols();

            string sql = $@"
                INSERT INTO dbo.Cuenta ({(cols.ClienteId ?? "/*ClienteId?*/")}, {(cols.Numero ?? "/*Numero?*/")},
                                         {(cols.Saldo ?? "/*Saldo?*/")}, {(cols.Fecha ?? "/*Fecha?*/")}, {(cols.Estado ?? "/*Estado?*/")})
                VALUES (@CliId, @Num, @Saldo, @Fec, @Estado);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@CliId",  cu.ClienteId),
                new SqlParameter("@Num",    (object)cu.NumeroCuenta ?? DBNull.Value),
                new SqlParameter("@Saldo",  cu.Saldo),
                new SqlParameter("@Fec",    cu.FechaApertura == default ? (object)DBNull.Value : cu.FechaApertura),
                new SqlParameter("@Estado", (object)cu.Estado ?? DBNull.Value)
            };

            return (int)SqlHelpers.GetInstance(Cnn).ExecuteScalar(sql, ps);
        }

        public bool Update(BE.Cuenta cu)
        {
            var c = ResolveCols();

            string sql = $@"
            UPDATE dbo.Cuenta SET
                {(c.ClienteId != null ? $"[{c.ClienteId}] = @CliId," : "")}
                {(c.Numero != null ? $"[{c.Numero}]   = @Num," : "")}
                {(c.Saldo != null ? $"[{c.Saldo}]    = @Saldo," : "")}
                {(c.Fecha != null ? $"[{c.Fecha}]    = @Fec," : "")}
                {(c.Estado != null ? $"[{c.Estado}]   = @Estado," : "")}
                [{c.IdCuenta}] = [{c.IdCuenta}]
            WHERE [{c.IdCuenta}] = @Id;";

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@Id",    cu.IdCuenta),
                new SqlParameter("@CliId", cu.ClienteId),
                new SqlParameter("@Num",   (object)cu.NumeroCuenta ?? DBNull.Value),
                new SqlParameter("@Saldo", cu.Saldo),
                new SqlParameter("@Fec",   cu.FechaApertura == default ? (object)DBNull.Value : cu.FechaApertura),
                new SqlParameter("@Estado",(object)cu.Estado ?? DBNull.Value)
            };

            return SqlHelpers.GetInstance(Cnn).ExecuteQuery(sql, ps) > 0;
        }

        public bool Delete(BE.Cuenta cta)
        {
            var c = ResolveCols();
            string sql = $"DELETE FROM dbo.Cuenta WHERE [{c.IdCuenta}] = @Id;";
            var ps = new List<SqlParameter> { new SqlParameter("@Id", cta.IdCuenta) };
            return SqlHelpers.GetInstance(Cnn).ExecuteQuery(sql, ps) > 0;
        }

        bool Mappers.ICRUD<BE.Cuenta>.Add(BE.Cuenta alta) => Add(alta) > 0;
        List<BE.Cuenta> Mappers.ICRUD<BE.Cuenta>.GetAll() => GetAll();
        BE.Cuenta Mappers.ICRUD<BE.Cuenta>.GetById(int id) => GetById(id);
        bool Mappers.ICRUD<BE.Cuenta>.Update(BE.Cuenta update) => Update(update);
        bool Mappers.ICRUD<BE.Cuenta>.Delete(BE.Cuenta delete) => Delete(delete);
    }
}
