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
    public class DBDao
    {

        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Crypto.Decript(FileHelper.GetInstance(configFilePath).ReadFile());

        #region Singleton
        private static DBDao _instance;
        public static DBDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DBDao();
            }

            return _instance;
        }
        #endregion

        public void Backup(string path, int partitions)
        {
            using (SqlConnection connection = new SqlConnection(_connString))
            {
                connection.Open();

                string fechaHora = DateTime.Now.ToString("yyyy-MM-dd, HH-mm-ss");

                string backupCommand = $"BACKUP DATABASE [PamperoControl] TO ";

                for (int i = 1; i <= partitions; i++)
                {
                    string partitionFile = Path.Combine(path, $"Backup{i}_{fechaHora}.bak");

                    backupCommand += $"DISK = '{partitionFile}'";

                    if (i < partitions)
                    {
                        backupCommand += ", ";
                    }
                }

                backupCommand += " WITH FORMAT, NAME = 'Backup_Partitioned', INIT, CHECKSUM, STATS = 10";

                SqlCommand command = new SqlCommand(backupCommand, connection);
                command.ExecuteNonQuery();
            }

        }
        public void Restore(string[] paths)
        {
            using (SqlConnection connection = new SqlConnection(_connString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("USE master", connection);

                try
                {
                    command.ExecuteNonQuery();

                    command.CommandText = "ALTER DATABASE [PamperoControl] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                    command.ExecuteNonQuery();

                    string devices = string.Join(", ", paths.Select(path => $"DISK = '{path}'"));

                    // Crear el comando de restauración con múltiples dispositivos
                    string restoreCommand = $@"RESTORE DATABASE [PamperoControl] FROM {devices} WITH REPLACE, RECOVERY, CHECKSUM;";

                    command.CommandText = restoreCommand;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al restaurar el backup: {ex.Message}");
                }
                finally
                {
                    command.CommandText = "ALTER DATABASE [PamperoControl] SET MULTI_USER;";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
