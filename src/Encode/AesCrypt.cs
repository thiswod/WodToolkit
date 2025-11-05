using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WodToolkit.Encode
{
    public static class AesCrypt
    {
        private const int IvLength = 16;
        private const PaddingMode Padding = PaddingMode.PKCS7;
        private readonly static string aesKey = "PKc7nfCGdzg62yYYOCjsA3b2dtSqS54b";// 密钥 至少32位
        public static string Encrypt(string plaintext, string key = null)
        {
            key ??= aesKey;// 如果密钥为空，使用默认密钥
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length != 32)
                throw new ArgumentException("密钥必须为32字节长度");

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV(); // 生成随机IV
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();

            // 1. 写入IV
            ms.Write(aes.IV, 0, 16);

            // 2. 处理数据
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                cs.Write(plainBytes, 0, plainBytes.Length);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string ciphertext, string key = null)
        {
            key ??= aesKey;// 如果密钥为空，使用默认密钥
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length != 32)
                throw new ArgumentException("密钥长度必须为32字节", nameof(key));

            byte[] cipherBytes = Convert.FromBase64String(ciphertext);
            if (cipherBytes.Length < IvLength)
                throw new ArgumentException("无效的加密数据");

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // 提取前16字节为IV
            byte[] iv = new byte[IvLength];
            Buffer.BlockCopy(cipherBytes, 0, iv, 0, IvLength);
            aes.IV = iv;

            // 使用CryptoStream自动处理所有数据块
            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                // 写入去除IV后的实际加密数据
                cs.Write(cipherBytes, IvLength, cipherBytes.Length - IvLength);
            } // 在此处自动调用FlushFinalBlock处理填充

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
