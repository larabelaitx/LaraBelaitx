using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DAL.Mappers;

namespace DAL
{
    public class TarjetaDao : Mappers.ICRUD<BE.Tarjeta>
    {
        private static TarjetaDao _instance;
        public static TarjetaDao GetInstance() => _instance ?? (_instance = new TarjetaDao());
        private TarjetaDao() { }

        public BE.Tarjeta GetById(int id)
        {
            // Alias a los nombres que espera el BE/Mapper
            const string sql = @"
                SELECT TOP 1
                    t.IdTarjeta,
                    t.IdCuenta      AS CuentaId,
                    t.IdCliente     AS ClienteId,
                    t.NroTarjeta    AS NumeroTarjeta,
                    t.NombreTitular AS Titular,
                    t.FechaEmision,
                    t.FechaVencimiento,
                    t.Banco         AS Marca,
                    t.CVV
                FROM dbo.Tarjeta t
                WHERE t.IdTarjeta = @Id;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return MPTarjeta.GetInstance().Map(dt);
                }
            }
        }

        public List<BE.Tarjeta> GetAll()
        {
            const string sql = @"
                SELECT
                    t.IdTarjeta,
                    t.IdCuenta      AS CuentaId,
                    t.IdCliente     AS ClienteId,
                    t.NroTarjeta    AS NumeroTarjeta,
                    t.NombreTitular AS Titular,
                    t.FechaEmision,
                    t.FechaVencimiento,
                    t.Banco         AS Marca,
                    t.CVV
                FROM dbo.Tarjeta t
                ORDER BY t.IdTarjeta DESC;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return MPTarjeta.GetInstance().MapTarjetas(dt);
            }
        }

        public List<BE.Tarjeta> GetByCuenta(int cuentaId)
        {
            const string sql = @"
                SELECT
                    t.IdTarjeta,
                    t.IdCuenta      AS CuentaId,
                    t.IdCliente     AS ClienteId,
                    t.NroTarjeta    AS NumeroTarjeta,
                    t.NombreTitular AS Titular,
                    t.FechaEmision,
                    t.FechaVencimiento,
                    t.Banco         AS Marca,
                    t.CVV
                FROM dbo.Tarjeta t
                WHERE t.IdCuenta = @Cta
                ORDER BY t.IdTarjeta DESC;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Cta", cuentaId);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return MPTarjeta.GetInstance().MapTarjetas(dt);
                }
            }
        }

        public int Add(BE.Tarjeta t)
        {
            // Usamos las columnas reales; mapeamos Marca -> Banco, NumeroTarjeta -> NroTarjeta, Titular -> NombreTitular
            const string sql = @"
                INSERT INTO dbo.Tarjeta
                (IdCuenta, IdCliente, NroTarjeta, NombreTitular, FechaEmision, FechaVencimiento, Banco, CVV, DVH)
                VALUES
                (@IdCuenta, @IdCliente, @NroTarjeta, @NombreTitular, @FEmision, @FVto, @Banco, @CVV, @DVH);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdCuenta", t.CuentaId);
                cmd.Parameters.AddWithValue("@IdCliente", t.ClienteId);
                cmd.Parameters.AddWithValue("@NroTarjeta", (object)t.NumeroTarjeta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreTitular", (object)t.Titular ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FEmision", t.FechaEmision == default ? (object)DBNull.Value : t.FechaEmision);
                cmd.Parameters.AddWithValue("@FVto", t.FechaVencimiento == default ? (object)DBNull.Value : t.FechaVencimiento);
                cmd.Parameters.AddWithValue("@Banco", (object)t.Marca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CVV", (object)t.CVV ?? DBNull.Value);

                // Calculá tu DVH como lo tengas implementado; por ahora dejamos vacío/null seguro:
                cmd.Parameters.AddWithValue("@DVH", DBNull.Value);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(BE.Tarjeta t)
        {
            const string sql = @"
                UPDATE dbo.Tarjeta SET
                    IdCuenta      = @IdCuenta,
                    IdCliente     = @IdCliente,
                    NroTarjeta    = @NroTarjeta,
                    NombreTitular = @NombreTitular,
                    FechaEmision  = @FEmision,
                    FechaVencimiento = @FVto,
                    Banco         = @Banco,
                    CVV           = @CVV
                WHERE IdTarjeta = @Id;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Id", t.IdTarjeta);
                cmd.Parameters.AddWithValue("@IdCuenta", t.CuentaId);
                cmd.Parameters.AddWithValue("@IdCliente", t.ClienteId);
                cmd.Parameters.AddWithValue("@NroTarjeta", (object)t.NumeroTarjeta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreTitular", (object)t.Titular ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FEmision", t.FechaEmision == default ? (object)DBNull.Value : t.FechaEmision);
                cmd.Parameters.AddWithValue("@FVto", t.FechaVencimiento == default ? (object)DBNull.Value : t.FechaVencimiento);
                cmd.Parameters.AddWithValue("@Banco", (object)t.Marca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CVV", (object)t.CVV ?? DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(BE.Tarjeta t)
        {
            const string sql = "DELETE FROM dbo.Tarjeta WHERE IdTarjeta=@Id;";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Id", t.IdTarjeta);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ICRUD explícito
        bool Mappers.ICRUD<BE.Tarjeta>.Add(BE.Tarjeta alta) => Add(alta) > 0;
        List<BE.Tarjeta> Mappers.ICRUD<BE.Tarjeta>.GetAll() => GetAll();
        BE.Tarjeta Mappers.ICRUD<BE.Tarjeta>.GetById(int id) => GetById(id);
        bool Mappers.ICRUD<BE.Tarjeta>.Update(BE.Tarjeta update) => Update(update);
        bool Mappers.ICRUD<BE.Tarjeta>.Delete(BE.Tarjeta delete) => Delete(delete);
    }
}
