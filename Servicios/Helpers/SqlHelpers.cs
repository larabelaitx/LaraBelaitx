using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SqlHelpers
    {
        private string _connString;
        private static SqlHelpers instance;

        //La clase solo puede instanciarse si se le provee un connectionString.
        private SqlHelpers(string connectionString)
        {
            this._connString = connectionString;
        }

        //Implementar Singleton para ofrecer una unica interface de creacion de instancias.
        public static SqlHelpers GetInstance(string connectionString)
        {
            if (instance == null)
            {
                instance = new SqlHelpers(connectionString);
            }
            return instance;
        }

        public string ConnString
        {
            get { return _connString; }
        }



        //Metodo que recibe una query de SQL (String) y devuelve un DataTable. (Lectura de Datos)
        public DataTable GetDataTable(string query)
        {
            DataTable dt = new DataTable();
            //creación de la conexión y command con using, para que se cierre en el Dispose
            using (SqlConnection conn = new SqlConnection(this._connString))
            //Le paso el CommandText y el connection al constructor del Command
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;
                conn.Open();
                //Load recibe un DataReader para rellenar el DataTable, ExecuteReader Toma el CommandText y el Connection crear un DataReader
                dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }

        public DataTable GetDataTable(string query, List<SqlParameter> parameters)
        {
            DataTable dt = new DataTable();
            //creación de la conexión y command con using, para que se cierre en el Dispose
            using (SqlConnection conn = new SqlConnection(this._connString))
            //Le paso el CommandText y el connection al constructor del Command
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (SqlParameter p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
                conn.Open();
                dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }

        //Metodo que recibe un nombre de Store Procedure (String) , recibe List<SqlParameters> y devuelve un DataTable (Lectura de Datos).
        public DataTable GetDataTableStored(string storedProc, List<SqlParameter> parameters)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(this._connString))
                using (SqlCommand cmd = new SqlCommand(storedProc, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    //Verifico que los paramestros no sean null y los agrego al Command
                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (SqlParameter p in parameters)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }
                    conn.Open();
                    dt.Load(cmd.ExecuteReader());
                }
                return dt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }

        //Metodo que recibe una query de SQL (String) y devuelve la cantidad de filas Afectadas (integer) (Delete, Update e Insert)
        public int ExecuteQuery(string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this._connString))
                //Le paso el CommandText y el connection al constructor del Command
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    //Se llama el metodo ExecuteNonQuery que devuelve el numero de filas afectadas
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e) { throw e; }

        }
        //Metodo con query y parameters
        public int ExecuteQuery(string query, List<SqlParameter> parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this._connString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (SqlParameter p in parameters)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }
                    conn.Open();

                    //Se llama el metodo ExecuteNonQuery que devuelve el numero de filas afectadas
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        //Metodos que recibe un nombre de un Store Procedure (String), recibe List<SqlParameters> y devuelve la cantidad de filas afectadas.
        public int ExecuteQueryStored(string storedProc, List<SqlParameter> parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this._connString))
                using (SqlCommand cmd = new SqlCommand(storedProc, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (SqlParameter p in parameters)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }
                    conn.Open();

                    //Según la Docu, si en la DB el StoredProcedure está SET NOCOUNT ON, el valor que devuelve el ExecueteNonQuery será -1
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public object ExecuteScalar(string query, List<SqlParameter> parameters)
        {
            using (SqlConnection connection = new SqlConnection(this._connString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null && parameters.Count > 0)
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }


    }
}
