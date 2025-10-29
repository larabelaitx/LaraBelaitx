using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DAL.Mappers;
using BE;

namespace DAL
{
    public class TarjetaDao : Mappers.ICRUD<BE.Tarjeta>
    {
        // Singleton
        private static TarjetaDao _instance;
        public static TarjetaDao GetInstance() => _instance ?? (_instance = new TarjetaDao());
        private TarjetaDao() { }

        // -------------------- CRUD BÁSICO --------------------

        public BE.Tarjeta GetById(int id)
        {
            const string sql = @"
                SELECT TOP 1 IdTarjeta, CuentaId, NumeroTarjeta, BIN, Titular,
                       FechaEmision, FechaVencimiento, Marca, Tipo
                FROM Tarjeta WHERE IdTarjeta=@Id;";

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
                SELECT IdTarjeta, CuentaId, NumeroTarjeta, BIN, Titular,
                       FechaEmision, FechaVencimiento, Marca, Tipo
                FROM Tarjeta ORDER BY IdTarjeta DESC;";

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
                SELECT IdTarjeta, CuentaId, NumeroTarjeta, BIN, Titular,
                       FechaEmision, FechaVencimiento, Marca, Tipo
                FROM Tarjeta WHERE CuentaId=@Cta ORDER BY IdTarjeta DESC;";

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
            const string sql = @"
                INSERT INTO Tarjeta
                (CuentaId, NumeroTarjeta, BIN, Titular, FechaEmision, FechaVencimiento, Marca, Tipo)
                VALUES
                (@CuentaId, @Numero, @BIN, @Titular, @FEmision, @FVto, @Marca, @Tipo);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@CuentaId", t.CuentaId);
                cmd.Parameters.AddWithValue("@Numero", (object)t.NumeroTarjeta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BIN", (object)t.BIN ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Titular", (object)t.Titular ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FEmision", t.FechaEmision == default ? (object)DBNull.Value : t.FechaEmision);
                cmd.Parameters.AddWithValue("@FVto", t.FechaVencimiento == default ? (object)DBNull.Value : t.FechaVencimiento);
                cmd.Parameters.AddWithValue("@Marca", (object)t.Marca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tipo", (object)t.Tipo ?? DBNull.Value);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(BE.Tarjeta t)
        {
            const string sql = @"
                UPDATE Tarjeta SET
                    NumeroTarjeta=@Numero, BIN=@BIN, Titular=@Titular,
                    FechaEmision=@FEmision, FechaVencimiento=@FVto,
                    Marca=@Marca, Tipo=@Tipo
                WHERE IdTarjeta=@Id;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Id", t.IdTarjeta);
                cmd.Parameters.AddWithValue("@Numero", (object)t.NumeroTarjeta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BIN", (object)t.BIN ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Titular", (object)t.Titular ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FEmision", t.FechaEmision == default ? (object)DBNull.Value : t.FechaEmision);
                cmd.Parameters.AddWithValue("@FVto", t.FechaVencimiento == default ? (object)DBNull.Value : t.FechaVencimiento);
                cmd.Parameters.AddWithValue("@Marca", (object)t.Marca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tipo", (object)t.Tipo ?? DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(BE.Tarjeta t)
        {
            const string sql = "DELETE FROM Tarjeta WHERE IdTarjeta=@Id;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Id", t.IdTarjeta);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // -------------------- ICRUD explícito --------------------
        bool Mappers.ICRUD<BE.Tarjeta>.Add(BE.Tarjeta alta) => Add(alta) > 0;
        List<BE.Tarjeta> Mappers.ICRUD<BE.Tarjeta>.GetAll() => GetAll();
        BE.Tarjeta Mappers.ICRUD<BE.Tarjeta>.GetById(int id) => GetById(id);
        bool Mappers.ICRUD<BE.Tarjeta>.Update(BE.Tarjeta update) => Update(update);
        bool Mappers.ICRUD<BE.Tarjeta>.Delete(BE.Tarjeta delete) => Delete(delete);
    }
}
