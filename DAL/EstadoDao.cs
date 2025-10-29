// DAL/EstadoDao.cs
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using DAL.Mappers;

namespace DAL
{
    /// <summary>
    /// Acceso a Estados.
    /// Usa ConnectionFactory para obtener la conexión (centralizado).
    /// </summary>
    public sealed class EstadoDao
    {
        // -------- Singleton --------
        private static EstadoDao _inst;
        public static EstadoDao GetInstance() => _inst ?? (_inst = new EstadoDao());
        private EstadoDao() { }

        /// <summary>
        /// Obtiene un estado por Id.
        /// </summary>
        public BE.Estado GetById(int idEstado)
        {
            const string sql = @"SELECT IdEstado, Descripcion
                                 FROM Estado
                                 WHERE IdEstado = @id";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@id", idEstado);

                var dt = new DataTable();
                da.Fill(dt);

                return MPEstado.GetInstance().Map(dt);
            }
        }

        /// <summary>
        /// Obtiene el estado actual asignado a un usuario.
        /// </summary>
        public BE.Estado GetEstadoUsuario(int idUsuario)
        {
            const string sql = @"
                SELECT  E.IdEstado, E.Descripcion
                FROM    Estado AS E
                INNER JOIN Usuario AS U ON U.IdEstado = E.IdEstado
                WHERE   U.IdUsuario = @u";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@u", idUsuario);

                var dt = new DataTable();
                da.Fill(dt);

                return MPEstado.GetInstance().Map(dt);
            }
        }
    }
}
