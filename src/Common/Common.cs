using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WodToolKit.src.Common
{
    public class Common
    {
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
    }
}
