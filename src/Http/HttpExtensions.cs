using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WodToolKit.Http.Extensions
{
    public class HttpExtensions
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
        #region URL参数相关方法
        /// <summary>
        /// 将 URL 参数按 ASCII 码排序（区分大小写）
        /// </summary>
        /// <param name="queryString">URL 参数字符串（格式 key1=value1&key2=value2）</param>
        /// <param name="encodeValue">是否对值进行 URL 编码（默认不编码）</param>
        /// <returns>按 ASCII 排序后的参数字符串</returns>
        public static string SortUrlParameters(string queryString, bool encodeValue = false)
        {
            if (string.IsNullOrWhiteSpace(queryString))
                return string.Empty;

            // 拆分成键值对
            var parameters = new Dictionary<string, string>();
            var keyValuePairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in keyValuePairs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 0) continue;

                string key = kv[0];
                string value = kv.Length > 1 ? kv[1] : "";

                // 仅保留第一个参数值（可选）
                if (!parameters.ContainsKey(key))
                {
                    // 选择性进行 URL 编码
                    value = encodeValue ? WebUtility.UrlEncode(value) : value;
                    parameters.Add(key, value);
                }
            }

            // 按 ASCII 顺序排序（区分大小写）
            var sorted = parameters
                .OrderBy(kv => kv.Key, StringComparer.Ordinal) // ASCII 顺序
                .ToArray();

            // 构建排序后的字符串
            var sb = new StringBuilder();
            foreach (var (key, value) in sorted)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(key).Append('=').Append(value);
            }

            return sb.ToString();
        }
        /// <summary>
        /// 对整个 URL 进行参数排序（保留协议和域名部分）
        /// </summary>
        /// <param name="url">完整 URL 字符串（包含协议、域名、路径和查询参数）</param>
        /// <param name="encodeValues">是否对参数值进行 URL 编码（默认不编码）</param>
        /// <returns>排序后的 URL 字符串</returns>
        /// <exception cref="ArgumentException">如果 URL 格式无效</exception>
        public static string SortUrlParametersInFullUrl(string url, bool encodeValues = false)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("无效的 URL");

            // 分离基本 URL 和参数部分
            var baseUri = uri.GetLeftPart(UriPartial.Path);
            var query = uri.Query.TrimStart('?');

            // 处理片段部分（锚点）
            string fragment = string.IsNullOrEmpty(uri.Fragment)
                ? "" : "#" + uri.Fragment.TrimStart('#');

            // 排序参数
            string sortedQuery = SortUrlParameters(query, encodeValues);

            return baseUri + (string.IsNullOrEmpty(sortedQuery)
                ? "" : "?" + sortedQuery) + fragment;
        }

        /// <summary>
        /// 将查询字符串转换为字典
        /// </summary>
        /// <param name="queryString">查询字符串，格式为key1=value1&key2=value2</param>
        /// <returns>包含键值对的字典</returns>
        public static Dictionary<string, string> QueryStringToDictionary(string queryString)
        {
            var parameters = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(queryString))
                return parameters;

            // 移除可能的问号前缀
            if (queryString.StartsWith('?'))
                queryString = queryString.Substring(1);

            var keyValuePairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in keyValuePairs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 0) continue;

                string key = kv[0];
                string value = kv.Length > 1 ? kv[1] : "";

                // URL解码值
                value = WebUtility.UrlDecode(value);

                // 如果键已存在，则覆盖
                if (parameters.ContainsKey(key))
                    parameters[key] = value;
                else
                    parameters.Add(key, value);
            }

            return parameters;
        }
        /// <summary>
        /// 将字典转换为查询字符串
        /// </summary>
        /// <param name="dictionary">包含键值对的字典</param>
        /// <param name="encodeValue">是否对值进行URL编码（默认编码）</param>
        /// <returns>格式化的查询字符串，格式为key1=value1&key2=value2</returns>
        public static string DictionaryToQueryString(Dictionary<string, string> dictionary, bool encodeValue = true)
        {
            if (dictionary == null || dictionary.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var kvp in dictionary)
            {
                if (sb.Length > 0)
                    sb.Append('&');

                sb.Append(kvp.Key);
                sb.Append('=');

                // 根据参数决定是否对值进行URL编码
                string value = encodeValue ? WebUtility.UrlEncode(kvp.Value ?? "") : (kvp.Value ?? "");
                sb.Append(value);
            }

            return sb.ToString();
        }
        private static readonly Random _random = new Random();
        /// <summary>
        /// 生成随机刷新码（16位随机数）
        /// </summary>
        /// <returns>16位随机刷新码字符串</returns>
        public static string refreshCode()
        {
            double refreshCode = _random.NextDouble();
            return $"{refreshCode:F16}";
        }
        /// <summary>
        /// 获取当前时间戳（13位时间戳）
        /// </summary>
        /// <returns>当前时间的13位时间戳字符串</returns>
        public static string GetTimeStamp()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return timestamp.ToString();
        }
        /// <summary>
        /// 获取URL中的参数
        /// </summary>
        /// <param name="url">完整 URL 字符串（包含协议、域名、路径和查询参数）</param>
        /// <returns>URL 中的查询参数部分（不包含问号）</returns>
        public static string GetQueryString(string url)
        {
            int index = url.IndexOf('?');
            if (index < 0) // 未找到问号
                return string.Empty;

            return url.Substring(index + 1); // 返回问号后的所有内容
        }
        /// <summary>
        /// 获取URL中的参数并排序
        /// </summary>
        /// <param name="url">完整 URL 字符串（包含协议、域名、路径和查询参数）</param>
        /// <returns>包含排序后的键值对的字典</returns>
        public static Dictionary<string, string> ParseQueryString(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.Contains("?"))
                return new Dictionary<string, string>();

            var parameters = new Dictionary<string, string>();
            int queryStart = url.IndexOf('?');
            if (queryStart < 0 || queryStart == url.Length - 1)
                return parameters;

            string queryString = url.Substring(queryStart + 1);
            string[] pairs = queryString.Split('&');

            foreach (string pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair))
                    continue;

                // 关键修复：使用第一个等号分割，保留后续等号
                int eqIndex = pair.IndexOf('=');
                if (eqIndex < 0)
                {
                    // 无等号的情况：作为key存入，value为空
                    parameters[HttpUtility.UrlDecode(pair)] = string.Empty;
                    continue;
                }

                string key = HttpUtility.UrlDecode(pair.Substring(0, eqIndex));
                string value = HttpUtility.UrlDecode(pair.Substring(eqIndex + 1));

                parameters[key] = value;
            }

            return parameters;
        }
        /// <summary>
        /// 将字典转换为查询字符串
        /// </summary>
        /// <param name="parameters">包含键值对的字典</param>
        /// <returns>格式化的查询字符串，格式为key1=value1&key2=value2</returns>
        public static string ToQueryString(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            bool first = true;

            foreach (var kvp in parameters)
            {
                if (!first)
                    sb.Append('&');
                else
                    first = false;

                // 对键和值进行URL编码
                string encodedKey = HttpUtility.UrlEncode(kvp.Key);
                string encodedValue = HttpUtility.UrlEncode(kvp.Value);

                sb.Append(encodedKey);
                sb.Append('=');
                sb.Append(encodedValue);
            }

            return sb.ToString();
        }
        #endregion
    }
}
