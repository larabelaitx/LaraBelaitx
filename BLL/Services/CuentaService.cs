using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using DAL;
using DAL.Mappers;


namespace BLL.Services
{
    public class CuentaService : ICuentaService
    {
        private readonly CuentaDao _dao = CuentaDao.GetInstance();

        // Cada vez que se pide un número aleatorio, la secuencia se sigue avanzando
        // y no se repite aunque se invoque en milisegundos consecutivos.
        //Lo uso en CBU, alias, etc
        private static readonly Random _rnd = new Random();

        // Lectura
        public List<Cuenta> GetAll() => _dao.GetAll();
        public List<Cuenta> GetByCliente(int clienteId)
        {
            if (clienteId <= 0) throw new ArgumentException("ClienteId inválido.", nameof(clienteId));
            return _dao.GetByCliente(clienteId);
        }

        public Cuenta GetById(int idCuenta)
        {
            if (idCuenta <= 0) throw new ArgumentException("IdCuenta inválido.", nameof(idCuenta));
            return _dao.GetById(idCuenta);
        }

        // Escritura
        public int Crear(Cuenta c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            if (c.ClienteId <= 0) throw new ArgumentException("ClienteId es obligatorio.", nameof(c.ClienteId));
            if (ExisteParaCliente(c.ClienteId))
                throw new InvalidOperationException("El cliente ya posee una cuenta.");

            // Completar por defecto / autogenerar
            if (string.IsNullOrWhiteSpace(c.NumeroCuenta))
                c.NumeroCuenta = GenerarNumeroCuenta();

            if (string.IsNullOrWhiteSpace(c.CBU))
                c.CBU = GenerarCBU();

            if (string.IsNullOrWhiteSpace(c.Alias))
                c.Alias = GenerarAlias();

            if (string.IsNullOrWhiteSpace(c.Moneda))
                c.Moneda = "ARS";

            if (c.FechaApertura == default(DateTime))
                c.FechaApertura = DateTime.Today;

            c.NumeroCuenta = SoloDigitos(c.NumeroCuenta);
            c.CBU = SoloDigitos(c.CBU);
            c.Alias = (c.Alias ?? "").Trim().ToLowerInvariant();

            if (c.NumeroCuenta.Length != 12)
                throw new ArgumentException("El número de cuenta debe tener 12 dígitos.", nameof(c.NumeroCuenta));

            if (c.CBU.Length != 22)
                throw new ArgumentException("El CBU debe tener 22 dígitos.", nameof(c.CBU));

            if (!AliasValido(c.Alias))
                throw new ArgumentException("El alias debe tener formato palabra.palabra.palabra", nameof(c.Alias));

            if (ExisteNumero(c.NumeroCuenta))
                throw new InvalidOperationException("Ya existe una cuenta con ese número.");

            if (ExisteCBU(c.CBU))
                throw new InvalidOperationException("Ya existe una cuenta con ese CBU.");

            if (ExisteAlias(c.Alias))
                throw new InvalidOperationException("Ya existe una cuenta con ese alias.");

            return _dao.Add(c);
        }

        public bool Actualizar(Cuenta c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            if (c.IdCuenta <= 0) throw new ArgumentException("IdCuenta inválido.", nameof(c.IdCuenta));

            // En edición normalmente solo permitir TipoCuenta / Moneda / FechaApertura / Saldo / Estado / Observaciones si tuvieras
            // Si decidís permitir cambiar alias/cbu/número, deberías revalidar unicidad aquí también.

            return _dao.Update(c);
        }

        public bool Eliminar(int idCuenta)
        {
            var c = GetById(idCuenta);
            if (c == null) return false;
            return _dao.Delete(c);
        }

        //Reglas
        public bool ExisteParaCliente(int clienteId)
        {
            // si GetByCliente devuelve algo, ya tiene cuenta
            return _dao.GetByCliente(clienteId).Any();
        }

        public bool ExisteNumero(string numeroCuenta)
        {
            if (string.IsNullOrWhiteSpace(numeroCuenta)) return false;
            var n = SoloDigitos(numeroCuenta);
            return _dao.GetAll().Any(x => SoloDigitos(x.NumeroCuenta) == n);
        }

        public bool ExisteCBU(string cbu)
        {
            if (string.IsNullOrWhiteSpace(cbu)) return false;
            var c = SoloDigitos(cbu);
            return _dao.GetAll().Any(x => SoloDigitos(x.CBU) == c);
        }

        public bool ExisteAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias)) return false;
            var a = alias.Trim().ToLowerInvariant();
            return _dao.GetAll().Any(x => (x.Alias ?? "").Trim().ToLowerInvariant() == a);
        }

        // Generadores
        public string GenerarNumeroCuenta()
        {
            // 12 dígitos
            // (dos bloques de 6 evita problemas con int.MaxValue)
            return RandDigits(6) + RandDigits(6);
        }

        public string GenerarCBU()
        {
            // 22 dígitos
            return RandDigits(8) + RandDigits(8) + RandDigits(6);
        }

        public string GenerarAlias()
        {
            // palabra.palabra.palabra
            return Word() + "." + Word() + "." + Word();
        }

        // Helpers internos
        private static string SoloDigitos(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var arr = s.Where(char.IsDigit).ToArray();
            return new string(arr);
        }

        private static bool AliasValido(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias)) return false;
            var parts = alias.Split('.');
            if (parts.Length != 3) return false;
            return parts.All(p => p.Length >= 2 && p.All(ch => char.IsLetter(ch)));
        }

        private static string RandDigits(int len)
        {
            // Genera exactamente len dígitos (0–9)
            var chars = new char[len];
            for (int i = 0; i < len; i++)
                chars[i] = (char)('0' + _rnd.Next(0, 10));
            // Evitar primer dígito 0 cuando len == 6/8 no es crítico
            if (len > 0 && chars[0] == '0') chars[0] = (char)('1' + _rnd.Next(0, 9));
            return new string(chars);
        }

        private static readonly string[] _diccionario = new[]
        {
            "cielo","pera","melon","pampa","luz","nube","sol","eco","lira","mate",
            "norte","sur","este","oeste","limon","uva","tigre","puma","lobo","roble","ambar","rio","lago","sierra"
        };

        private static string Word()
        {
            return _diccionario[_rnd.Next(_diccionario.Length)];
        }
    }
}
