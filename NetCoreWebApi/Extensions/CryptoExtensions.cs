using System;
using System.Security.Cryptography;
using System.Text;
using NetCoreWebApi.ModelSettings;
using Microsoft.Extensions.Configuration;

namespace NetCoreWebApi.Extensions
{
    public static class CryptoExtensions
    {
        private static NetCoreApiSettings webApiSettings;        
        private static NetCoreApiSettings WebApiSettings
        {
            get
            {
                if (CryptoExtensions.webApiSettings == null)
                {
                    var configuration = new ConfigurationBuilder()
                                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                        .AddEnvironmentVariables().Build();
                    webApiSettings = new NetCoreApiSettings();
                    configuration.GetSection("NetCoreWebApiSettings").Bind(webApiSettings);
                }

                return CryptoExtensions.webApiSettings;
            }
        }

        /// <summary>
        /// Decrypts a string using the private key
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public static string Decrypt(this string encryptedString)
        {
            string privateKey = CryptoExtensions.WebApiSettings.PrivateKeyRSA;
            var rsaEncrypt = RSA.Create();
            rsaEncrypt.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
            
            string decodedAuth = Encoding.UTF8.GetString(rsaEncrypt.Decrypt(Convert.FromBase64String(encryptedString), RSAEncryptionPadding.Pkcs1));
            return decodedAuth;
        }

        /// <summary>
        /// Encrypts a string using the private key
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public static string Encrypt(this string decryptedString)
        {
            string privateKey = CryptoExtensions.WebApiSettings.PrivateKeyRSA;
            var rsaEncrypt = RSA.Create();
            rsaEncrypt.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
            
            string decodedAuth = Convert.ToBase64String(rsaEncrypt.Encrypt(UTF8Encoding.UTF8.GetBytes(decryptedString), RSAEncryptionPadding.Pkcs1));
            return decodedAuth;
        }

        public static string Sha512(this string sourceString)
        {
            return Convert.ToBase64String(SHA512.Create().ComputeHash(UTF8Encoding.UTF8.GetBytes(sourceString)));
        }
    }
}

