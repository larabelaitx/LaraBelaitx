using System;
using System.Data.SqlClient;
using System.IO;

namespace DAL
{
    /// <summary>
    /// Operaciones de mantenimiento de base de datos:
    /// - Exponer la cadena de conexión actual (desde ConnectionFactory/AppConn).
    /// - Backup y Restore (para la UI BackupRestore).
    /// </summary>
    public sealed class DBDao
    {
        private static readonly DBDao _inst = new DBDao();
        public static DBDao GetInstance() => _inst;
        private DBDao() { }

        /// <summary>
        /// Cadena de conexión actual (centralizada).
        /// </summary>
        public string ConnectionString => ConnectionFactory.Current;

        /// <summary>
        /// Realiza un BACKUP DATABASE a la carpeta indicada.
        /// Si partitions &gt; 1, genera múltiples archivos .bak (medios de backup).
        /// </summary>
        public void Backup(string targetFolder, int partitions = 1)
        {
            if (string.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("Carpeta destino inválida.", nameof(targetFolder));

            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            var csb = new SqlConnectionStringBuilder(ConnectionString);
            string db = csb.InitialCatalog;
            if (string.IsNullOrWhiteSpace(db))
                throw new InvalidOperationException("No se pudo determinar la base de datos (Initial Catalog).");

            // Archivos de salida
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string baseName = Path.Combine(targetFolder, $"{db}_{stamp}");

            // Construimos la cláusula TO DISK= ... [ , DISK = ... ]
            string toDisks;
            if (partitions <= 1)
            {
                string f1 = baseName + ".bak";
                toDisks = $"DISK = N'{f1.Replace("'", "''")}'";
            }
            else
            {
                var parts = new string[partitions];
                for (int i = 0; i < partitions; i++)
                {
                    string fi = $"{baseName}_p{i + 1}.bak";
                    parts[i] = $"DISK = N'{fi.Replace("'", "''")}'";
                }
                toDisks = string.Join(", ", parts);
            }

            // IMPORTANTE: usar WITH INIT para sobreescribir si existen, y COPY_ONLY para no afectar cadena de backups
            string sql = $@"
BACKUP DATABASE [{db}]
TO {toDisks}
WITH INIT, COPY_ONLY, NAME = N'Backup {db} {stamp}', STATS = 5;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandTimeout = 0; // por si tarda
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Restaura la base desde uno o varios archivos .bak.
        /// Usa SINGLE_USER y luego MULTI_USER para asegurar la restauración.
        /// </summary>
        public void Restore(string[] backupFiles)
        {
            if (backupFiles == null || backupFiles.Length == 0)
                throw new ArgumentException("Debés seleccionar al menos un archivo .bak", nameof(backupFiles));

            foreach (var f in backupFiles)
            {
                if (string.IsNullOrWhiteSpace(f) || !File.Exists(f))
                    throw new FileNotFoundException("Archivo de backup inexistente.", f ?? "(null)");
            }

            var csb = new SqlConnectionStringBuilder(ConnectionString);
            string db = csb.InitialCatalog;
            if (string.IsNullOrWhiteSpace(db))
                throw new InvalidOperationException("No se pudo determinar la base de datos (Initial Catalog).");

            // DISK = 'path' [, DISK = 'path2', ...]
            string fromDisks = string.Join(", ", Array.ConvertAll(backupFiles,
                f => $"DISK = N'{f.Replace("'", "''")}'"));

            // Restauramos desde master
            using (var cn = new SqlConnection(
                new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" }.ConnectionString))
            {
                cn.Open();
                using (var cmd = cn.CreateCommand())
                using (var tx = cn.BeginTransaction())
                {
                    cmd.Transaction = tx;
                    cmd.CommandTimeout = 0;

                    try
                    {
                        // Forzamos modo SINGLE_USER
                        cmd.CommandText = $@"
                            IF DB_ID('{db.Replace("'", "''")}') IS NOT NULL
                            BEGIN
                              ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            END";
                        cmd.ExecuteNonQuery();

                        // RESTORE (con REPLACE por si ya existe)
                        cmd.CommandText = $@"
                        RESTORE DATABASE [{db}]
                        FROM {fromDisks}
                        WITH REPLACE, STATS = 5;";
                        cmd.ExecuteNonQuery();

                        // Volver a MULTI_USER
                        cmd.CommandText = $@"ALTER DATABASE [{db}] SET MULTI_USER;";
                        cmd.ExecuteNonQuery();

                        tx.Commit();
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { /* ignore */ }
                        throw;
                    }
                }
            }
        }
    }
}
