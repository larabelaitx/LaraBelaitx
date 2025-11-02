using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DAL
{
    /// <summary>
    /// Mantenimiento de BD (backup/restore) usando la cadena centralizada (ConnectionFactory).
    /// </summary>
    public static class DbMaintenance
    {
        public static void Backup(string folderPath, int partitions = 1, string databaseName = null)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Ruta de backup vacía.", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (partitions < 1) partitions = 1;

            var csb = new SqlConnectionStringBuilder(ConnectionFactory.Current);
            string db = string.IsNullOrWhiteSpace(databaseName) ? csb.InitialCatalog : databaseName;
            if (string.IsNullOrWhiteSpace(db))
                throw new InvalidOperationException("No se pudo determinar la base de datos (Initial Catalog).");

            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string baseName = Path.Combine(folderPath, $"{db}_{stamp}");

            string toDisks = (partitions == 1)
                ? $"DISK = N'{(baseName + ".bak").Replace("'", "''")}'"
                : string.Join(", ", Enumerable.Range(1, partitions)
                      .Select(i => $"DISK = N'{(baseName + $"_p{i:00}.bak").Replace("'", "''")}'"));

            string backupSql = $@"
BACKUP DATABASE [{db}]
TO {toDisks}
WITH INIT, COPY_ONLY, COMPRESSION, CHECKSUM, NAME = N'Backup {db} {stamp}', STATS = 10;";

            string verifySql = $@"
RESTORE VERIFYONLY FROM {toDisks} WITH CHECKSUM;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(backupSql, cn))
            {
                cmd.CommandTimeout = 0; // por si tarda
                cmd.ExecuteNonQuery();
                // Verificación del set de backup
                cmd.CommandText = verifySql;
                cmd.ExecuteNonQuery();
            }
        }

        public static void Restore(string[] fullPaths, string databaseName = null)
        {
            if (fullPaths == null || fullPaths.Length == 0)
                throw new ArgumentException("No se indicaron archivos .bak.", nameof(fullPaths));

            foreach (var f in fullPaths)
                if (string.IsNullOrWhiteSpace(f) || !File.Exists(f))
                    throw new FileNotFoundException("Archivo de backup inexistente.", f ?? "(null)");

            var csb = new SqlConnectionStringBuilder(ConnectionFactory.Current);
            string db = string.IsNullOrWhiteSpace(databaseName) ? csb.InitialCatalog : databaseName;
            if (string.IsNullOrWhiteSpace(db))
                throw new InvalidOperationException("No se pudo determinar la base de datos (Initial Catalog).");

            string fromDisks = string.Join(", ", fullPaths.Select(p => $"DISK = N'{p.Replace("'", "''")}'"));

            // Usar master para el RESTORE
            var masterCnn = new SqlConnectionStringBuilder(ConnectionFactory.Current)
            {
                InitialCatalog = "master"
            }.ConnectionString;

            using (var cn = new SqlConnection(masterCnn))
            {
                cn.Open();

                // Poner SINGLE_USER (sin transacción)
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = $@"
IF DB_ID('{db.Replace("'", "''")}') IS NOT NULL
BEGIN
    ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END";
                    cmd.ExecuteNonQuery();
                }

                try
                {
                    // RESTORE (sin transacción; SQL Server no lo permite dentro de user transactions)
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = $@"
RESTORE DATABASE [{db}]
FROM {fromDisks}
WITH REPLACE, RECOVERY, CHECKSUM, STATS = 10;";
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    // Volver a MULTI_USER aunque falle el restore
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = $@"IF DB_ID('{db.Replace("'", "''")}') IS NOT NULL
ALTER DATABASE [{db}] SET MULTI_USER;";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
