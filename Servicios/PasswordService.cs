using System;
using System.Security.Cryptography;
using System.Text;

namespace Services
{
    public static class PasswordService
    {
        public static (string Hash, string Salt, int Iterations) Hash(string plainPassword, int iterations = 10000)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            using (var pbkdf2 = new Rfc2898DeriveBytes(plainPassword, saltBytes, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(32);

                string hash = Convert.ToBase64String(hashBytes);
                string salt = Convert.ToBase64String(saltBytes);

                return (hash, salt, iterations);
            }
        }
        public static bool Verify(string plainPassword, string storedHash, string storedSalt, int iterations)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(plainPassword, saltBytes, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(32);
                string newHash = Convert.ToBase64String(hashBytes);

                return newHash == storedHash;
            }
        }
    }
}
