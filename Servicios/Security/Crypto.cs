using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Servicios.Security
{
    public static class Crypto
    {
        //TODO: Actualizar o parametrizar
        private static string publickey = "12345678";
        private static string secretkey = "87654321";
        public static string Encript(string enctriptar)
        {
            try
            {
                string toReturn;
                byte[] secretkeyByte = { };
                secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(enctriptar);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    toReturn = Convert.ToBase64String(ms.ToArray());
                }
                return toReturn;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e.InnerException);
            }
        }
        public static string Decript(string desencriptar)
        {
            try
            {
                string toReturn;
                byte[] privatekeyByte = { };
                privatekeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = new byte[desencriptar.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(desencriptar.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    toReturn = encoding.GetString(ms.ToArray());
                }
                return toReturn;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e.InnerException);
            }
        }
        public static string EncriptMD5(string stringAEncriptar)
        {
            string hash = "";
            if (!String.IsNullOrEmpty(stringAEncriptar))
            {
                MD5 md5 = MD5.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(stringAEncriptar);
                byte[] datoEncodeado = md5.ComputeHash(bytes);
                hash = BitConverter.ToString(datoEncodeado).Replace("-", "");

            }


            return hash;
        }

        public static string GenPassword()
        {
            const string caracteres = "0123456789ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyz";
            const int max = 8;
            string[] chars = new string[max];
            Random random = new Random();

            for (int i = 0; i < max; i++)
            {
                chars[i] += caracteres[random.Next(0, caracteres.Length - 1)];
            }


            return string.Join("", chars);
        }
    }
}
