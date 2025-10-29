using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DAL.Mappers;

namespace DAL
{
    public class IdiomaDao
    {
        private static IdiomaDao _inst;
        public static IdiomaDao GetInstance() => _inst ?? (_inst = new IdiomaDao());
        private IdiomaDao() { }

        // Usamos la cadena centralizada (AppConn -> ConnectionFactory)
        private static string Cnn => ConnectionFactory.Current;

        public BE.Idioma GetById(int idIdioma)
        {
            const string sql = @"SELECT IdIdioma, Nombre FROM Idioma WHERE IdIdioma = @id";
            var ps = new List<SqlParameter> { new SqlParameter("@id", idIdioma) };
            DataTable dt = Services.SqlHelpers.GetInstance(Cnn).GetDataTable(sql, ps);
            return MPIdioma.GetInstance().Map(dt);
        }

        public BE.Idioma GetIdiomaUsuario(int idUsuario)
        {
            const string sql = @"
                SELECT I.IdIdioma, I.Nombre
                FROM Idioma AS I
                INNER JOIN Usuario AS U ON U.IdIdioma = I.IdIdioma
                WHERE U.IdUsuario = @u";
            var ps = new List<SqlParameter> { new SqlParameter("@u", idUsuario) };
            DataTable dt = Services.SqlHelpers.GetInstance(Cnn).GetDataTable(sql, ps);
            return MPIdioma.GetInstance().Map(dt);
        }
    }
}
