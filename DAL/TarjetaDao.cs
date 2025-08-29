using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace DAL
{
    public class TarjetaDao : Mappers.ICRUD<BE.Tarjeta>
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Crypto.Decript(
            FileHelper.GetInstance(configFilePath).ReadFile());

        private static TarjetaDao _instance;
        public static TarjetaDao GetInstance() => _instance ?? (_instance = new TarjetaDao());
        private TarjetaDao() { }

        public BE.Tarjeta GetById(int id)
        {
            const string sql = @"
                SELECT TOP 1 IdTarjeta, CuentaId, NumeroTarjeta, BIN, Titular,
                       FechaEmision, FechaVencimiento, Marca, Tipo
                FROM Tarjeta WHERE IdTarjeta=@Id;";
            var dt = Services.SqlHelpers.GetInstance(_connString)
                    .GetDataTable(sql, new List<SqlParameter> { new SqlParameter("@Id", id) });
            return DAL.Mappers.MPTarjeta.GetInstance().Map(dt);
        }

        public List<BE.Tarjeta> GetAll()
        {
            const string sql = @"
                SELECT IdTarjeta, CuentaId, NumeroTarjeta, BIN, Titular,
                       FechaEmision, FechaVencimiento, Marca, Tipo
                FROM Tarjeta ORDER BY IdTarjeta DESC;";
            var dt = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sql);
            return DAL.Mappers.MPTarjeta.GetInstance().MapTarjetas(dt);
        }

        public List<BE.Tarjeta> GetByCuenta(int cuentaId)
        {
            const string sql = @"
                SELECT IdTarjeta, CuentaId, NumeroTarjeta, BIN, Titular,
                       FechaEmision, FechaVencimiento, Marca, Tipo
                FROM Tarjeta WHERE CuentaId=@Cta ORDER BY IdTarjeta DESC;";
            var ps = new List<SqlParameter> { new SqlParameter("@Cta", cuentaId) };
            var dt = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sql, ps);
            return DAL.Mappers.MPTarjeta.GetInstance().MapTarjetas(dt);
        }

        public int Add(BE.Tarjeta t)
        {
            const string sql = @"
                INSERT INTO Tarjeta
                (CuentaId, NumeroTarjeta, BIN, Titular, FechaEmision, FechaVencimiento, Marca, Tipo)
                VALUES
                (@CuentaId, @Numero, @BIN, @Titular, @FEmision, @FVto, @Marca, @Tipo);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@CuentaId", t.CuentaId),
                new SqlParameter("@Numero", (object)t.NumeroTarjeta ?? DBNull.Value),
                new SqlParameter("@BIN", (object)t.BIN ?? DBNull.Value),
                new SqlParameter("@Titular", (object)t.Titular ?? DBNull.Value),
                new SqlParameter("@FEmision", t.FechaEmision == default ? (object)DBNull.Value : t.FechaEmision),
                new SqlParameter("@FVto", t.FechaVencimiento == default ? (object)DBNull.Value : t.FechaVencimiento),
                new SqlParameter("@Marca", (object)t.Marca ?? DBNull.Value),
                new SqlParameter("@Tipo", (object)t.Tipo ?? DBNull.Value)
            };
            return (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(sql, ps);
        }

        public bool Update(BE.Tarjeta t)
        {
            const string sql = @"
                UPDATE Tarjeta SET
                    NumeroTarjeta=@Numero, BIN=@BIN, Titular=@Titular,
                    FechaEmision=@FEmision, FechaVencimiento=@FVto,
                    Marca=@Marca, Tipo=@Tipo
                WHERE IdTarjeta=@Id;";
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@Id", t.IdTarjeta),
                new SqlParameter("@Numero", (object)t.NumeroTarjeta ?? DBNull.Value),
                new SqlParameter("@BIN", (object)t.BIN ?? DBNull.Value),
                new SqlParameter("@Titular", (object)t.Titular ?? DBNull.Value),
                new SqlParameter("@FEmision", t.FechaEmision == default ? (object)DBNull.Value : t.FechaEmision),
                new SqlParameter("@FVto", t.FechaVencimiento == default ? (object)DBNull.Value : t.FechaVencimiento),
                new SqlParameter("@Marca", (object)t.Marca ?? DBNull.Value),
                new SqlParameter("@Tipo", (object)t.Tipo ?? DBNull.Value)
            };
            return Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps) > 0;
        }

        public bool Delete(BE.Tarjeta t)
        {
            const string sql = "DELETE FROM Tarjeta WHERE IdTarjeta=@Id;";
            var ps = new List<SqlParameter> { new SqlParameter("@Id", t.IdTarjeta) };
            return Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps) > 0;
        }

        // ICRUD
        bool Mappers.ICRUD<BE.Tarjeta>.Add(BE.Tarjeta alta) => Add(alta) > 0;
        List<BE.Tarjeta> Mappers.ICRUD<BE.Tarjeta>.GetAll() => GetAll();
        BE.Tarjeta Mappers.ICRUD<BE.Tarjeta>.GetById(int id) => GetById(id);
        bool Mappers.ICRUD<BE.Tarjeta>.Update(BE.Tarjeta update) => Update(update);
        bool Mappers.ICRUD<BE.Tarjeta>.Delete(BE.Tarjeta delete) => Delete(delete);
    }
}
