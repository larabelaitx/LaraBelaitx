using System;
using System.Collections.Generic;
using BE;

namespace BLL.Contracts
{
    public interface ITarjetaService
    {
        Tarjeta GetById(int idTarjeta);
        List<Tarjeta> GetAll();
        List<Tarjeta> GetByCuenta(int idCuenta);

        // Alta / Edición / Baja (física por ahora)
        int Crear(Tarjeta t);
        bool Actualizar(Tarjeta t);
        bool Eliminar(int idTarjeta);

        // Reglas de negocio mínimas
        bool PuedeCrearParaCuenta(int idCliente, int idCuenta, out string motivoBloqueo);

        // Utilidades
        string GenerarNroTarjeta(string bin6);
        byte[] GenerarCVV(int length = 3);

        // (Opcional) Alertas de tablero
        List<string> GetAlertasOperativas();
    }
}
