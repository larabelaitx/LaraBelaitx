using System;
using System.Security.Cryptography;

namespace Services
{
    public static class PasswordService
    {
        public const int HashSize = 32;      // 256 bits
        public const int SaltSize = 16;      // 128 bits
        public const int DefaultIterations = 100_000;

        /// <summary>
        /// Genera un nuevo salt aleatorio compatible con .NET Framework.
        /// </summary>
        public static byte[] NewSalt(int size = SaltSize)
        {
            var salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt); // Compatible con .NET 4.7.2
            }
            return salt;
        }

        /// <summary>
        /// Deriva un hash PBKDF2-SHA256 en bytes.
        /// </summary>
        public static byte[] Derive(string plainPassword, byte[] salt, int iterations = DefaultIterations, int hashSize = HashSize)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("El password no puede estar vacío.", nameof(plainPassword));
            if (salt == null || salt.Length == 0)
                throw new ArgumentException("El salt no puede estar vacío.", nameof(salt));

            using (var pbkdf2 = new Rfc2898DeriveBytes(plainPassword, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(hashSize);
            }
        }

        /// <summary>
        /// Crea hash + salt + iteraciones de un password (todo en bytes).
        /// </summary>
        public static (byte[] Hash, byte[] Salt, int Iterations) Hash(string plainPassword, int iterations = DefaultIterations)
        {
            var salt = NewSalt();
            var hash = Derive(plainPassword, salt, iterations);
            return (hash, salt, iterations);
        }

        /// <summary>
        /// Verifica si un password coincide con un hash y salt (todo en bytes).
        /// </summary>
        public static bool Verify(string plainPassword, byte[] storedHash, byte[] storedSalt, int iterations)
        {
            if (storedHash == null || storedSalt == null || iterations <= 0)
                return false;

            var computed = Derive(plainPassword, storedSalt, iterations, storedHash.Length);
            return SlowEquals(computed, storedHash);
        }

        /// <summary>
        /// Comparación segura (en tiempo constante).
        /// </summary>
        public static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }
    }
}
