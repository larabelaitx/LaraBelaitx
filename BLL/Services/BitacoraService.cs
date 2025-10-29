using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BE;
using DAL;

namespace BLL.Services
{
    public class BitacoraService
    {
        private readonly BitacoraDao _dao;

        public BitacoraService()
        {
            _dao = BitacoraDao.GetInstance();
        }

        public BitacoraService(string connString)
        {
            _dao = BitacoraDao.GetInstance();
        }

        // -------- Consulta de usuarios (para combos) --------
        public IEnumerable<string> Usuarios()
        {
            return _dao.GetUsuarios() ?? Enumerable.Empty<string>();
        }

        // -------- Búsqueda (nombre “Buscar”) --------
        public (IList<BitacoraEntry> Rows, int Total) Buscar(
            DateTime? desde,
            DateTime? hasta,
            string usuario,
            int page,
            int pageSize,
            string ordenar)
        {
            var (dt, total) = _dao.Search(desde, hasta, usuario, page, pageSize, ordenar);
            return (Mapear(dt), total);
        }

        // -------- Wrapper con el nombre “Search” para la UI legacy --------
        public (IList<BitacoraEntry> Rows, int Total) Search(
            DateTime? desde,
            DateTime? hasta,
            string usuario,
            int page,
            int pageSize,
            string ordenar)
        {
            return Buscar(desde, hasta, usuario, page, pageSize, ordenar);
        }

        // ================== Mapeo ==================
        private static List<BitacoraEntry> Mapear(DataTable dt)
        {
            var list = new List<BitacoraEntry>();
            if (dt == null || dt.Rows.Count == 0) return list;

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new BitacoraEntry
                {
                    Id = Get<int>(r, "IdBitacora"),
                    Fecha = Get<DateTime>(r, "Fecha"),
                    Usuario = Get<string>(r, "Usuario"),
                    Modulo = Get<string>(r, "Modulo"),
                    Accion = Get<string>(r, "Accion"),
                    // la columna en DB suele ser INT; si tu modelo usa byte?, convertimos correctamente:
                    Severidad = (byte)GetNullableByte(r, "Severidad"),
                    Mensaje = Get<string>(r, "Mensaje"),
                    IP = Get<string>(r, "IP"),
                    Host = Get<string>(r, "Host")
                });
            }
            return list;
        }

        private static T Get<T>(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col)) return default(T);
            var v = row[col];
            if (v == DBNull.Value || v == null) return default(T);
            return (T)Convert.ChangeType(v, typeof(T));
        }

        private static byte? GetNullableByte(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            if (v == DBNull.Value || v == null) return null;

            try { return Convert.ToByte(v); }
            catch { return null; } // evita el CS0266
        }

    }
}
