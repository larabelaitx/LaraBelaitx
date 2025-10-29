using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Services
{
    /// <summary>
    /// Cifrado simétrico (AES-CBC + IV aleatorio por mensaje) y utilidades legacy.
    /// - Reversible: Encrypt/Decrypt   (antes Encript/Decript)
    /// - Hash (no reversible) para “huellas”: EncriptMD5 (legacy) y PasswordService (recomendado)
    /// </summary>
    public static class Crypto
    {
        // IMPORTANTE: mover estas claves a configuración segura si es posible.
        private const string Passphrase = "ITX-Banco-2024";         // <- cámbiala y colócala en config
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("ITX-Salt-2024!!"); // 16+ bytes

        private const int Iterations = 10000; // PBKDF2
        private const int KeySizeBytes = 32;  // 256-bit

        // ====== Reemplazo de Encript/Decript con AES ======
        public static string Encript(string plainText) => Encrypt(plainText);
        public static string Decript(string cipherBase64) => Decrypt(cipherBase64);

        public static string Encrypt(string plainText)
        {
            if (plainText == null) plainText = string.Empty;

            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var kdf = new Rfc2898DeriveBytes(Passphrase, Salt, Iterations, HashAlgorithmName.SHA256))
                {
                    aes.Key = kdf.GetBytes(KeySizeBytes);
                }

                aes.GenerateIV(); // IV aleatorio por mensaje

                using (var ms = new MemoryStream())
                {
                    // Guardamos IV al inicio
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, Encoding.UTF8))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherBase64)
        {
            if (string.IsNullOrWhiteSpace(cipherBase64))
                return string.Empty;

            var allBytes = Convert.FromBase64String(cipherBase64);

            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var kdf = new Rfc2898DeriveBytes(Passphrase, Salt, Iterations, HashAlgorithmName.SHA256))
                {
                    aes.Key = kdf.GetBytes(KeySizeBytes);
                }

                int ivLen = aes.BlockSize / 8; // 16
                var iv = new byte[ivLen];
                Buffer.BlockCopy(allBytes, 0, iv, 0, ivLen);
                aes.IV = iv;

                using (var ms = new MemoryStream(allBytes, ivLen, allBytes.Length - ivLen))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        // ====== Legacy utilidades (no tocar llamadas existentes) ======
        public static string EncriptMD5(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            using (var md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = md5.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public static string GenPassword()
        {
            const string caracteres = "0123456789ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyz";
            const int max = 8;
            var sb = new StringBuilder(max);
            using (var rng = RandomNumberGenerator.Create())
            {
                var buf = new byte[4];
                for (int i = 0; i < max; i++)
                {
                    rng.GetBytes(buf);
                    int idx = (int)(BitConverter.ToUInt32(buf, 0) % (uint)caracteres.Length);
                    sb.Append(caracteres[idx]);
                }
            }
            return sb.ToString();
        }
    }
}
