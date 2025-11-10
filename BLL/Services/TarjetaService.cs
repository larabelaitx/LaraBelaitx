using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using BLL.Contracts;
using DAL;

namespace BLL.Services
{
    public class TarjetaService : ITarjetaService
    {
        private readonly TarjetaDao _dao;
        private readonly IClienteService _clientes;
        private readonly ICuentaService _cuentas;

        public TarjetaService() : this(TarjetaDao.GetInstance(), new ClienteService(), new CuentaService()) { }
        public TarjetaService(TarjetaDao dao, IClienteService cli, ICuentaService ctas)
        {
            _dao = dao ?? TarjetaDao.GetInstance();
            _clientes = cli ?? new ClienteService();
            _cuentas = ctas ?? new CuentaService();
        }

        public Tarjeta GetById(int idTarjeta) => _dao.GetById(idTarjeta);
        public List<Tarjeta> GetAll() => _dao.GetAll();
        public List<Tarjeta> GetByCuenta(int idCuenta) => _dao.GetByCuenta(idCuenta);

        public bool PuedeCrearParaCuenta(int idCliente, int idCuenta, out string motivoBloqueo)
        {
            motivoBloqueo = null;

            // Cliente
            var cli = _clientes.GetById(idCliente);
            if (cli == null) { motivoBloqueo = "El cliente no existe."; return false; }
            if (!EsClienteActivo(cli))
            {
                motivoBloqueo = "El cliente no está activo.";
                return false;
            }

            // Cuenta
            var cta = _cuentas.GetById(idCuenta);
            if (cta == null) { motivoBloqueo = "La cuenta no existe."; return false; }
            if (!EsCuentaActiva(cta))
            {
                motivoBloqueo = "La cuenta no está activa/abierta.";
                return false;
            }

            // Única tarjeta vigente por cuenta
            var tarjetas = _dao.GetByCuenta(idCuenta) ?? new List<Tarjeta>();
            var yaVigente = tarjetas.Any(t => t.FechaVencimiento > DateTime.Today);
            if (yaVigente)
            {
                motivoBloqueo = "La cuenta ya posee una tarjeta vigente.";
                return false;
            }

            return true;
        }

        public int Crear(Tarjeta t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            if (!PuedeCrearParaCuenta(t.ClienteId, t.CuentaId, out var motivo))
                throw new InvalidOperationException(motivo);

            if (string.IsNullOrWhiteSpace(t.NumeroTarjeta))
                t.NumeroTarjeta = GenerarNroTarjeta(string.IsNullOrEmpty(t.BIN) ? "450799" : t.BIN); // BIN demo Visa

            if (t.CVV == null || t.CVV.Length == 0)
                t.CVV = GenerarCVV();

            if (t.FechaEmision == default)
                t.FechaEmision = DateTime.Today;

            if (t.FechaVencimiento == default)
                t.FechaVencimiento = t.FechaEmision.AddYears(5);

            if (string.IsNullOrWhiteSpace(t.Marca))
                t.Marca = "Visa";

            if (string.IsNullOrWhiteSpace(t.Titular))
            {
                var cli = _clientes.GetById(t.ClienteId);
                if (cli != null)
                    t.Titular = $"{(cli.Apellido ?? "").Trim()}, {(cli.Nombre ?? "").Trim()}".Trim(' ', ',');
            }

            return _dao.Add(t);
        }

        public bool Actualizar(Tarjeta t)
        {
            if (t == null || t.IdTarjeta <= 0) throw new ArgumentException("Tarjeta inválida.");
            if (string.IsNullOrWhiteSpace(t.NumeroTarjeta)) throw new InvalidOperationException("Número de tarjeta requerido.");
            return _dao.Update(t);
        }

        public bool Eliminar(int idTarjeta)
        {
            var t = _dao.GetById(idTarjeta);
            if (t == null) return false;
            return _dao.Delete(t);
        }

        // --------- Generadores ---------

        public string GenerarNroTarjeta(string bin6)
        {
            if (string.IsNullOrWhiteSpace(bin6) || bin6.Length < 6) bin6 = "450799";
            var rnd = new Random();
            var body = new char[9];
            for (int i = 0; i < body.Length; i++) body[i] = (char)('0' + rnd.Next(0, 10));
            var parcial = bin6 + new string(body); // 6 + 9 = 15
            int check = Luhn(parcial);
            return parcial + check.ToString();
        }

        private static int Luhn(string digits15)
        {
            int sum = 0; bool alt = true;
            for (int i = digits15.Length - 1; i >= 0; i--)
            {
                int n = digits15[i] - '0';
                if (alt) { n *= 2; if (n > 9) n -= 9; }
                sum += n; alt = !alt;
            }
            int mod = sum % 10;
            return (10 - mod) % 10;
        }

        public byte[] GenerarCVV(int length = 3)
        {
            var rnd = new Random();
            var bytes = new byte[length];
            for (int i = 0; i < length; i++) bytes[i] = (byte)('0' + rnd.Next(0, 10));
            return bytes;
        }

        // --------- Alertas para el Menú ---------

        public List<string> GetAlertasOperativas()
        {
            var alertas = new List<string>();

            var cuentas = _cuentas.GetAll() ?? new List<Cuenta>();
            foreach (var c in cuentas)
            {
                if (!EsCuentaActiva(c)) continue;

                var cli = _clientes.GetById(c.ClienteId);
                if (!EsClienteActivo(cli)) continue;

                var tarjetas = _dao.GetByCuenta(c.IdCuenta) ?? new List<Tarjeta>();
                var vigente = tarjetas.Any(t => t.FechaVencimiento > DateTime.Today);
                if (!vigente)
                    alertas.Add($"Cuenta {c.NumeroCuenta} del cliente {cli.Apellido}, {cli.Nombre}: sin tarjeta vigente.");
            }

            return alertas;
        }

        // --------- Helpers de estado (sin EstadoDisplay ni EstadoUsuarioId) ---------

        private static bool EsClienteActivo(Cliente c)
        {
            if (c == null) return false;

            // Preferimos BE.Estado.Name si existe
            if (c.Estado != null && !string.IsNullOrWhiteSpace(c.Estado.Name))
            {
                var n = c.Estado.Name.Trim().ToLowerInvariant();
                // Ajustá los nombres a los que uses realmente en tu tabla Estado
                return !(n == "inactivo" || n == "bloqueado" || n == "eliminado" || n == "baja");
            }

            // Si tenés un flag bool Activo en Cliente
            var prop = c.GetType().GetProperty("Activo");
            if (prop != null && prop.PropertyType == typeof(bool))
                return (bool)prop.GetValue(c);

            // Fallback conservador
            return false;
        }

        private static bool EsCuentaActiva(Cuenta ct)
        {
            if (ct == null) return false;

            if (ct.Estado != null && !string.IsNullOrWhiteSpace(ct.Estado.Name))
            {
                var n = ct.Estado.Name.Trim().ToLowerInvariant();
                return (n == "abierta" || n == "activa" || n == "habilitada");
            }

            var prop = ct.GetType().GetProperty("EstadoId");
            if (prop != null && prop.PropertyType == typeof(int))
                return (int)prop.GetValue(ct) == 1; // 1 = Abierta/Activa (ajustá si difiere)

            return true;
        }
    }
}
