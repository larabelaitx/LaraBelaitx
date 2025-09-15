using System;
using BE;
using DAL;
using Svc = global::Services;

namespace BLL
{
    public static class Bitacora
    {
        private static readonly BitacoraDao _dao = BitacoraDao.GetInstance();
        public static void Info(int? usuarioId, string descripcion) => Write(1, usuarioId, descripcion);
        public static void Warn(int? usuarioId, string descripcion) => Write(2, usuarioId, descripcion);
        public static void Error(int? usuarioId, string descripcion) => Write(3, usuarioId, descripcion);
        private static void Write(int criticidad, int? userId, string desc)
        {
            _dao.Add(new BE.Bitacora
            {
                Criticidad = new BE.Criticidad(criticidad),
                Usuario = userId.HasValue ? new BE.Usuario { Id = userId.Value } : null,
                Descripcion = desc,
                Fecha = DateTime.UtcNow
            }, new DVH { dvh = Svc.DV.GetDV($"{criticidad}|{userId}|{desc}|{DateTime.UtcNow:O}") });
        }
    }
}
