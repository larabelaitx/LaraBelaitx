using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using BE;
using Services;

namespace DAL
{
    public class DVVDao
    {
        // Singleton
        private static DVVDao _instance;
        public static DVVDao GetInstance() => _instance ?? (_instance = new DVVDao());
        private DVVDao() { }

        // ÚNICO cambio importante: conexión siempre desde DBDao
        private static string Conn => Services.AppConn.Get();

        public bool AddUpdateDVV(DVV dvv)
        {
            const string qCheck = "SELECT COUNT(*) FROM DVV WHERE Tabla=@Tabla";
            const string qIns = "INSERT INTO DVV (Tabla, DVV) VALUES (@Tabla, @DVV)";
            const string qUpd = "UPDATE DVV SET DVV=@DVV WHERE Tabla=@Tabla";

            if (dvv == null) return false;

            var p = new List<SqlParameter> {
                new SqlParameter("@Tabla", dvv.tabla),
                new SqlParameter("@DVV",   dvv.dvv ?? (object)DBNull.Value)
            };

            int count = Convert.ToInt32(SqlHelpers.GetInstance(Conn)
                            .ExecuteScalar(qCheck, new List<SqlParameter> { new SqlParameter("@Tabla", dvv.tabla) }));

            var sql = (count > 0) ? qUpd : qIns;
            return SqlHelpers.GetInstance(Conn).ExecuteQuery(sql, p) > 0;
        }

        public List<DVV> GetAllDVV()
        {
            const string q = "SELECT idDVV, Tabla, DVV FROM DVV";
            return DAL.Mappers.MPDVV.GetInstance()
                   .MapDVVs(SqlHelpers.GetInstance(Conn).GetDataTable(q));
        }

        public string CalculateDVV(string table)
        {
            var dvhs = DVHDao.GetInstance().GetDvhs(table);
            var sb = new StringBuilder();
            foreach (var d in dvhs) sb.Append(d.dvh);
            return DV.GetDV(sb.ToString());
        }
    }
}
