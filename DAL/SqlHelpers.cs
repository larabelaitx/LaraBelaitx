using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class SqlHelpers
    {
        #region Singleton
        private static SqlHelpers _instance;
        public static SqlHelpers GetInstance(string cnn = null)
        {
            return _instance ?? (_instance = new SqlHelpers(cnn));
        }

        private readonly string _connectionString;

        private SqlHelpers(string cnn)
        {
            _connectionString = cnn ?? ConnectionFactory.Current;
        }
        #endregion

        /// <summary>
        /// Ejecuta un SELECT y devuelve los resultados en un DataTable.
        /// </summary>
        public DataTable GetDataTable(string query, List<SqlParameter> parameters = null)
        {
            var dt = new DataTable();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.CommandType = CommandType.Text; // 🔑 clave: evita que se interprete como SP
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters.ToArray());

                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        /// <summary>
        /// Ejecuta un comando de acción (INSERT, UPDATE, DELETE) y devuelve las filas afectadas.
        /// </summary>
        public int ExecuteQuery(string query, List<SqlParameter> parameters = null)
        {
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.CommandType = CommandType.Text; // importante también acá
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters.ToArray());

                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Ejecuta una consulta escalar (ej: SELECT COUNT(*), MAX(...)) y devuelve el resultado.
        /// </summary>
        public object ExecuteScalar(string query, List<SqlParameter> parameters = null)
        {
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters.ToArray());

                cn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
