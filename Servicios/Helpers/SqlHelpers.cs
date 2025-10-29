using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Services
{
    public class SqlHelpers
    {
        private readonly string _connString;
        private static readonly Dictionary<string, SqlHelpers> _pool = new Dictionary<string, SqlHelpers>(StringComparer.Ordinal);

        private SqlHelpers(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("ConnString vacío.", nameof(connectionString));
            _connString = connectionString;
        }
        public static SqlHelpers GetInstance(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("ConnString vacío.", nameof(connectionString));

            SqlHelpers h;
            if (_pool.TryGetValue(connectionString, out h))
                return h;

            h = new SqlHelpers(connectionString);
            _pool[connectionString] = h;
            return h;
        }

        public string ConnString { get { return _connString; } }

        public DataTable GetDataTable(string query)
        {
            return GetDataTable(query, null);
        }

            public DataTable GetDataTable(string query, List<SqlParameter> parameters)
            {
                using (var conn = new SqlConnection(_connString))
                using (var cmd = new SqlCommand(query, conn) { CommandType = CommandType.Text })
                {
                    if (parameters != null && parameters.Count > 0)
                        cmd.Parameters.AddRange(parameters.ToArray());

                    conn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(rd);
                        return dt;
                    }
                }
        }

        public DataTable GetDataTableStored(string storedProc, List<SqlParameter> parameters)
        {
            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(storedProc, conn) { CommandType = CommandType.StoredProcedure })
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());

                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(rd);
                    return dt;
                }
            }
        }

        public int ExecuteQuery(string query)
        {
            return ExecuteQuery(query, null);
        }

        public int ExecuteQuery(string query, List<SqlParameter> parameters)
        {
            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(query, conn) { CommandType = CommandType.Text })
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int ExecuteQueryStored(string storedProc, List<SqlParameter> parameters)
        {
            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(storedProc, conn) { CommandType = CommandType.StoredProcedure })
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string query, List<SqlParameter> parameters)
        {
            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(query, conn))
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
        public int ExecuteQuery(SqlConnection cn, SqlTransaction tx, string query, List<SqlParameter> parameters)
        {
            using (var cmd = new SqlCommand(query, cn, tx) { CommandType = CommandType.Text })
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());
                return cmd.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(SqlConnection cn, SqlTransaction tx, string query, List<SqlParameter> parameters)
        {
            using (var cmd = new SqlCommand(query, cn, tx) { CommandType = CommandType.Text })
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());
                return cmd.ExecuteScalar();
            }
        }

        public DataTable GetDataTable(SqlConnection cn, SqlTransaction tx, string query, List<SqlParameter> parameters)
        {
            using (var cmd = new SqlCommand(query, cn, tx) { CommandType = CommandType.Text })
            {
                if (parameters != null && parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());
                using (var rd = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(rd);
                    return dt;
                }
            }
        }
    }
}
