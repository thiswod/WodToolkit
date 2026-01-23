using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WodToolKit.src.Common
{
    public class Common
    {
        /// <summary>
        /// 生成随机 16 进制字符串（线程安全）
        /// </summary>
        /// <param name="length">16 进制字符串长度</param>
        /// <param name="uppercase">是否大写（默认 true）</param>
        public static string Generate(int length = 32, bool uppercase = true)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");

            byte[] bytes = new byte[(length + 1) / 2]; // 每 2 字符 = 1 字节
            RandomNumberGenerator.Fill(bytes);

            string hex = BitConverter.ToString(bytes).Replace("-", "");
            hex = hex.Length > length ? hex[..length] : hex; // 调整长度

            return uppercase ? hex : hex.ToLowerInvariant();
        }
        /// <summary>
        /// 获取指定年份的生肖
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns></returns>
        public static string getChineseZodiac(int year)
        {
            string[] zodiacs = new string[] { "鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊", "猴", "鸡", "狗", "猪" };
            int startYear = 1900;
            int offset = (year - startYear) % 12;
            return zodiacs[offset];
        }

        /// <summary>
        /// 从文件路径获取 Base64 字符串
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Base64 字符串，如果文件不存在或读取失败则返回空字符串</returns>
        public static string Base64Encode(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return string.Empty;
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                return Convert.ToBase64String(fileBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 从字节数组获取 Base64 字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>Base64 字符串</returns>
        public static string Base64Encode(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            }

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 从字符串获取 Base64 字符串
        /// </summary>
        /// <param name="text">要转换的字符串</param>
        /// <param name="encoding">编码方式，默认为 UTF-8（传 null 表示使用 UTF-8）</param>
        /// <returns>Base64 字符串</returns>
        public static string Base64Encode(string text, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            encoding = encoding ?? Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileExtension(string fileName)
        {
            int index = fileName.LastIndexOf('.');
            if (index == -1)
            {
                return "";
            }
            return fileName.Substring(index + 1);
        }
        /// <summary>
        /// 取随机字符串，首位为字母，长度自定义
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            if (length <= 0)
                return string.Empty;

            Random random = new Random();
            StringBuilder result = new StringBuilder(length);
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            const string alphanumeric = "abcdefghijklmnopqrstuvwxyz0123456789";

            // 首位必须为字母
            result.Append(letters[random.Next(letters.Length)]);

            // 生成剩余字符
            for (int i = 1; i < length; i++)
            {
                result.Append(alphanumeric[random.Next(alphanumeric.Length)]);
            }

            return result.ToString();
        }
    }
}
