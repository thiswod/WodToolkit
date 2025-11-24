using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WodToolkit.Http
{
    #region 代理类型枚举
    /// <summary>
    /// 代理类型枚举
    /// </summary>
    public enum ProxyType
    {
        /// <summary>
        /// HTTP/HTTPS 代理
        /// </summary>
        Http,
        /// <summary>
        /// SOCKS4 代理
        /// </summary>
        Socks4,
        /// <summary>
        /// SOCKS5 代理
        /// </summary>
        Socks5
    }
    #endregion
    #region Cookie 管理器
    /// <summary>
    /// Cookie 管理器（完整实现）
    /// </summary>
    public class CookieManager
    {
        private readonly Dictionary<string, string> _cookies = new Dictionary<string, string>();

        /// <summary>
        /// 设置单个 Cookie
        /// </summary>
        /// <param name="name">Cookie 名称</param>
        /// <param name="value">Cookie 值</param>
        /// <returns>当前 CookieManager 实例（用于链式调用）</returns>
        /// <exception cref="ArgumentNullException">如果名称为空</exception>
        public CookieManager SetCookie(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (value == "deleted" || string.IsNullOrEmpty(value))
                _cookies.Remove(name);
            else
                _cookies[name] = value;

            return this;
        }
        /// <summary>
        /// 批量设置 Cookie（通过字典）
        /// </summary>
        /// <param name="cookies">包含 Cookie 名称和值的字典</param>
        /// <returns>当前 CookieManager 实例（用于链式调用）</returns>
        public CookieManager SetCookie(Dictionary<string, string> cookies)
        {
            if (cookies == null)
                return this;

            foreach (var cookie in cookies)
            {
                SetCookie(cookie.Key, cookie.Value);
            }
            return this;
        }
        /// <summary>
        /// 批量设置 Cookie（通过 Cookie 字符串）
        /// </summary>
        /// <param name="cookieString">Cookie 字符串，格式为key1=value1; key2=value2</param>
        /// <returns>当前 CookieManager 实例（用于链式调用）</returns>
        public CookieManager SetCookie(string cookieString)
        {
            if (string.IsNullOrWhiteSpace(cookieString))
                return this;

            var cookies = cookieString.Split(';')
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .Select(c =>
                {
                    var separatorIndex = c.IndexOf('=');
                    if (separatorIndex > 0)
                    {
                        var name = c.Substring(0, separatorIndex).Trim();
                        var value = separatorIndex < c.Length - 1
                            ? c.Substring(separatorIndex + 1).Trim()
                            : string.Empty;
                        return (Name: name, Value: value);
                    }
                    return (Name: c, Value: string.Empty);
                });

            foreach (var cookie in cookies)
            {
                SetCookie(cookie.Name, cookie.Value);
            }

            return this;
        }
        /// <summary>
        /// 从 CoreWebView2CookieManager 导入所有 Cookie
        /// </summary>
        /// <param name="webViewCookieManager">CoreWebView2CookieManager 实例</param>
        /// <returns>导入 Cookie 后的 CookieManager 实例（用于链式调用）</returns>
        /// <exception cref="ArgumentNullException">如果 webViewCookieManager 为空</exception>
        /// <exception cref="NotSupportedException">如果 webViewCookieManager 不支持 GetCookiesAsync 方法</exception>
        public async Task ImportFromWebView2Async(object webViewCookieManager)
        {
            if (webViewCookieManager == null)
                throw new ArgumentNullException(nameof(webViewCookieManager));

            // 使用反射获取 GetCookiesAsync 方法
            var getCookiesMethod = webViewCookieManager.GetType().GetMethod("GetCookiesAsync", new Type[] { typeof(string) });
            if (getCookiesMethod == null)
                throw new NotSupportedException("传入的 webViewCookieManager 不支持 GetCookiesAsync 方法");

            dynamic cookies = await (dynamic)getCookiesMethod.Invoke(webViewCookieManager, new object[] { null });
            foreach (object cookie in cookies)
            {
                SetCookie(cookie.GetType().GetProperty("Name")?.GetValue(cookie)?.ToString(),
                          cookie.GetType().GetProperty("Value")?.GetValue(cookie)?.ToString());
            }
        }
        /// <summary>
        /// 导出所有 Cookie 到 CoreWebView2CookieManager
        /// </summary>
        /// <param name="webViewCookieManager">CoreWebView2CookieManager 实例</param>
        /// <returns>导出 Cookie 后的 CookieManager 实例（用于链式调用）</returns>
        /// <exception cref="ArgumentNullException">如果 webViewCookieManager 为空</exception>
        /// <exception cref="NotSupportedException">如果 webViewCookieManager 不支持 CreateCookie 或 AddOrUpdateCookie 方法</exception>
        public async Task ExportToWebView2Async(object webViewCookieManager)
        {
            if (webViewCookieManager == null)
                throw new ArgumentNullException(nameof(webViewCookieManager));

            foreach (var kv in _cookies)
            {
                // 使用 CreateCookie 创建 Cookie 实例，然后 AddOrUpdateCookie 添加到管理器
                // 使用反射调用 CreateCookie 方法
                var createCookieMethod = webViewCookieManager.GetType().GetMethod("CreateCookie", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
                if (createCookieMethod == null)
                    throw new NotSupportedException("传入的 webViewCookieManager 不支持 CreateCookie 方法");

                var cookie = createCookieMethod.Invoke(webViewCookieManager, new object[] { kv.Key, kv.Value, null, null });
                // 使用反射调用 AddOrUpdateCookie 方法
                var addOrUpdateCookieMethod = webViewCookieManager.GetType().GetMethod("AddOrUpdateCookie", new Type[] { cookie.GetType() });
                if (addOrUpdateCookieMethod == null)
                    throw new NotSupportedException("传入的 webViewCookieManager 不支持 AddOrUpdateCookie 方法");

                addOrUpdateCookieMethod.Invoke(webViewCookieManager, new object[] { cookie });
            }
        }
        /// <summary>
        /// 获取指定 Cookie 的值
        /// </summary>
        /// <param name="name">Cookie 名称</param>
        /// <returns>Cookie 值（如果存在）；否则为空字符串</returns>
        /// <exception cref="ArgumentNullException">如果 name 为空</exception>
        public string GetCookieValue(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            _cookies.TryGetValue(name, out string value);
            return value;
        }

        /// <summary>
        /// 检查指定 Cookie 是否存在
        /// </summary>
        /// <param name="name">Cookie 名称</param>
        /// <returns>如果 Cookie 存在则为 true；否则为 false</returns>
        /// <exception cref="ArgumentNullException">如果 name 为空</exception>
        public bool HasCookie(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return _cookies.ContainsKey(name);
        }

        /// <summary>
        /// 获取所有 Cookie 的字典副本
        /// </summary>
        /// <returns>包含所有 Cookie 键值对的字典副本</returns>
        public Dictionary<string, string> GetAllCookies()
        {
            return new Dictionary<string, string>(_cookies);
        }

        /// <summary>
        /// 获取 Cookie 字符串（URL 编码）
        /// </summary>
        /// <returns>格式化的 Cookie 字符串，格式为 key1=value1; key2=value2</returns>
        public string GetCookieString()
        {
            return string.Join("; ", _cookies.Select(kv =>
                $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
        }

        /// <summary>
        /// 获取原始 Cookie 字符串（无编码）
        /// </summary>
        /// <returns>格式化的 Cookie 字符串，格式为 key1=value1; key2=value2</returns>
        public string GetRawCookieString()
        {
            return string.Join("; ", _cookies.Select(kv => $"{kv.Key}={kv.Value}"));
        }

        /// <summary>
        /// 删除指定 Cookie
        /// </summary>
        /// <param name="name">Cookie 名称</param>
        /// <returns>当前 CookieManager 实例（用于链式调用）</returns>
        /// <exception cref="ArgumentNullException">如果 name 为空</exception>
        public CookieManager RemoveCookie(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (!string.IsNullOrEmpty(name))
                _cookies.Remove(name);
            return this;
        }

        /// <summary>
        /// 清空所有 Cookie
        /// </summary>
        /// <returns>当前 CookieManager 实例（用于链式调用）</returns>
        public CookieManager ClearCookies()
        {
            _cookies.Clear();
            return this;
        }
    }
    #endregion
    #region HTTP 响应数据
    /// <summary>
    /// HTTP 响应数据（PHP 库的 C# 实现）
    /// </summary>
    public class HttpResponseData
    {
        /// <summary>
        /// HTTP 状态码
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// 请求头字符串
        /// </summary>
        public string RequestHeaders { get; set; }
        /// <summary>
        /// 请求头字典
        /// </summary>
        public Dictionary<string, string> RequestHeadersArray { get; set; }
        /// <summary>
        /// 响应头字符串
        /// </summary>
        public string ResponseHeaders { get; set; }
        /// <summary>
        /// 响应头字典
        /// </summary>
        public Dictionary<string, string> ResponseHeadersArray { get; set; }
        /// <summary>
        /// 响应体字符串
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// Cookie 管理器
        /// </summary>
        public CookieManager CookieManager { get; set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public byte[] rawResult { get; set; }
        /// <summary>
        /// 响应头 Cookie 字符串
        /// </summary>
        public string Cookie { get; set; }
    }
    #endregion
    #region HTTP 请求参数
    /// <summary>
    /// HTTP 请求参数（PHP 库的 C# 实现）
    /// </summary>
    public class HttpRequestParameter
    {
        private readonly HttpRequestClass _parent;
        /// <summary>
        /// 要上传的文件列表
        /// </summary>
        public List<FileUploadContent> Files { get; } = new List<FileUploadContent>();
        /// <summary>
        /// 请求 URL
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// HTTP 请求方法（默认 Get）
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        /// <summary>
        /// 请求数据对象
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 请求头字符串
        /// </summary>
        public string Headers { get; set; }
        /// <summary>
        /// 请求头字典数组
        /// </summary>
        public Dictionary<string, string> HeadersArray { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Cookie 管理器
        /// </summary>
        public CookieManager CookieManager { get; set; } = new CookieManager();
        /// <summary>
        /// 请求超时时间（单位：秒，默认 15）
        /// </summary>
        public int Timeout { get; set; } = 15;
        /// <summary>
        /// 代理服务器地址
        /// </summary>
        public string Proxy { get; set; }
        /// <summary>
        /// 代理类型（默认 HTTP）
        /// </summary>
        public ProxyType ProxyType { get; set; } = ProxyType.Http;
        /// <summary>
        /// 代理服务器用户名
        /// </summary>
        public string ProxyUsername { get; set; }
        /// <summary>
        /// 代理服务器密码
        /// </summary>
        public string ProxyPassword { get; set; }
        /// <summary>
        /// 是否跟随重定向 (默认 true) (false: 不跟随重定向)
        /// </summary>
        public bool FollowLocation { get; set; }
        /// <summary>
        /// 是否完整协议头（默认 true）
        /// </summary>
        public bool CompleteProtocolHeaders { get; set; } = true;
        /// <summary>
        /// 是否验证 SSL 证书对等性
        /// </summary>
        public bool SslVerifyPeer { get; set; }
        /// <summary>
        /// 是否验证 SSL 主机名
        /// </summary>
        public bool SslVerifyHost { get; set; }
        /// <summary>
        /// User-Agent 字符串
        /// </summary>
        public string UserAgent { get; set; }

        public HttpRequestParameter(HttpRequestClass parent)
        {
            _parent = parent;
        }

        public HttpRequestClass Set() => _parent;
        public HttpRequestClass Send(object data = null) => _parent.Send(data);
    }
    #endregion
    #region HTTP 请求类
    /// <summary>
    /// HTTP 请求类
    /// </summary>
    public class HttpRequestClass : IDisposable
    {
        // 添加锁对象
        private readonly object _lock = new object();

        private HttpRequestParameter _requestParams;
        private HttpResponseData _responseData = new HttpResponseData();

        // 存储临时请求头的字典
        private readonly Dictionary<string, string> _temporaryHeaders = new Dictionary<string, string>();


        public HttpRequestClass(string url = null, HttpMethod method = null, CookieManager cookieManager = null)
        {
            _requestParams = new HttpRequestParameter(this)
            {
                Url = url,
                Method = method ?? HttpMethod.Get
            };

            if (cookieManager != null)
                BindCookie(ref cookieManager);
        }
        public HttpRequestClass SetTimeout(int timeout = 30)
        {
            _requestParams.Timeout = 30;
            return this;
        }
        /// <summary>
        /// 绑定外部 Cookie 管理器
        /// </summary>
        public HttpRequestClass BindCookie(ref CookieManager cookieManager)
        {
            cookieManager = _requestParams.CookieManager;
            return this;
        }

        /// <summary>
        /// 获取请求参数对象
        /// </summary>
        public HttpRequestParameter Set() => _requestParams;

        /// <summary>
        /// 设置请求 URL 和方法
        /// </summary>
        public HttpRequestClass Open(string url, HttpMethod method = null)
        {
            lock (_lock)
            {
                _requestParams.Url = url;
                _requestParams.Method = method ?? HttpMethod.Get;
            }
            return this;
        }

        /// <summary>
        /// 设置 SSL 验证
        /// </summary>
        public HttpRequestClass SetSslVerification(bool verifyPeer = true, bool verifyHost = true)
        {
            _requestParams.SslVerifyPeer = verifyPeer;
            _requestParams.SslVerifyHost = verifyHost;
            return this;
        }

        /// <summary>
        /// 设置 UserAgent
        /// </summary>
        public HttpRequestClass SetUserAgent(string userAgent)
        {
            _requestParams.UserAgent = userAgent;
            return this;
        }

        /// <summary>
        /// 设置 Cookie 字符串
        /// </summary>
        public HttpRequestClass SetCookieString(string cookie)
        {
            _requestParams.CookieManager.SetCookie(cookie);
            return this;
        }

        /// <summary>
        /// 设置代理（HTTP/HTTPS 代理）
        /// </summary>
        public HttpRequestClass SetProxy(string ip = "", string user = "", string pwd = "")
        {
            _requestParams.Proxy = ip;
            _requestParams.ProxyType = ProxyType.Http;
            _requestParams.ProxyUsername = user;
            _requestParams.ProxyPassword = pwd;
            return this;
        }
        /// <summary>
        /// 设置代理（支持 HTTP/HTTPS、SOCKS4 和 SOCKS5）
        /// </summary>
        /// <param name="proxyType">代理类型（Http、Socks4 或 Socks5）</param>
        /// <param name="host">代理服务器地址（不含协议前缀）</param>
        /// <param name="port">代理服务器端口</param>
        /// <param name="user">代理用户名（可选，HTTP 和 SOCKS5 支持）</param>
        /// <param name="pwd">代理密码（可选，HTTP 和 SOCKS5 支持）</param>
        /// <returns>当前 HttpRequestClass 实例（用于链式调用）</returns>
        public HttpRequestClass SetProxy(ProxyType proxyType, string host, int port, string user = "", string pwd = "")
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("代理服务器地址不能为空", nameof(host));
            }

            _requestParams.ProxyType = proxyType;

            // 根据代理类型设置代理地址格式
            if (proxyType == ProxyType.Http)
            {
                // HTTP 代理需要完整的 URL 格式
                _requestParams.Proxy = $"http://{host}:{port}";
            }
            else
            {
                // SOCKS 代理使用 host:port 格式
                _requestParams.Proxy = $"{host}:{port}";
            }

            _requestParams.ProxyUsername = user;
            _requestParams.ProxyPassword = pwd;
            return this;
        }
        /// <summary>
        /// 取消代理设置（适用于需要临时禁用代理的场景）
        /// </summary>
        public HttpRequestClass RemoveProxy()
        {
            _requestParams.Proxy = null;
            _requestParams.ProxyUsername = null;
            _requestParams.ProxyPassword = null;
            return this;
        }
        /// <summary>
        /// 获取 Cookie 管理器
        /// </summary>
        public CookieManager CookieManager() => _requestParams.CookieManager;
        /// <summary>
        /// 设置 Cookie 管理器
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        public CookieManager Set_CookieManager(CookieManager _)
        {
            _requestParams.CookieManager = _;
            return _requestParams.CookieManager;
        }

        // <summary>
        /// 发送 HTTP 请求（支持 object 和 string 类型参数）
        /// </summary>
        public HttpRequestClass Send(object data = null)
        {
            lock (_lock)
            {
                // 支持string类型的数据直接传入
                if (data is string stringData)
                {
                    // 特殊处理：当data是string时，保留原始行为
                    return SendString(stringData);
                }
            }


            return SendCore(data);
        }
        /// <summary>
        /// 发送 HTTP 请求（支持 string 类型参数）
        /// </summary>
        public HttpRequestClass Send(string content)
        {
            lock (_lock)
            {
                return SendCore(content);
            }
        }
        public async Task<HttpRequestClass> SendAsync(object data = null, CancellationToken ct = default)
        {
            return await Task.Run(() => Send(data), ct);
        }
        /// <summary>
        /// 发送字符串类型请求体（与Send方法兼容）
        /// </summary>
        public HttpRequestClass SendString(string content)
        {
            // 调用Send(string)方法实现
            return Send(content);
        }
        // 核心发送方法
        private HttpRequestClass SendCore(object data)
        {
            try
            {
                // 验证URL格式（可取消注释）
                // if (string.IsNullOrEmpty(_requestParams.Url) || !Uri.IsWellFormedUriString(_requestParams.Url, UriKind.Absolute))
                //     throw new ArgumentException("Invalid or missing URL");

                // 初始化响应数据
                _responseData = new HttpResponseData();
                _responseData.CookieManager = _requestParams.CookieManager;

                // 创建请求消息
                var request = CreateRequest();

                // 设置请求内容（智能处理普通数据和文件上传）
                SetRequestContent(request, data);

                // 检查是否使用 SOCKS 代理
                if (!string.IsNullOrEmpty(_requestParams.Proxy) && 
                    (_requestParams.ProxyType == ProxyType.Socks4 || _requestParams.ProxyType == ProxyType.Socks5))
                {
                    // 使用 SOCKS 代理
                    return ExecuteRequestWithSocks(request, data);
                }
                else
                {
                    // 使用标准 HTTP 代理或直连
                    using var handler = new HttpClientHandler();
                    ConfigureHandler(handler);

                    // 创建HTTP客户端
                    using var client = new HttpClient(handler);
                    ConfigureClient(client);  // 配置客户端参数（超时、UA等）

                    // 执行请求并处理响应
                    return ExecuteRequest(client, request);
                }
            }
            finally
            {
                // 无论请求成功失败，每次请求后立即清除临时头
                lock (_lock)
                {
                    _temporaryHeaders.Clear();
                }

                // 清除文件流（如果有）
                foreach (var file in _requestParams.Files)
                {
                    file.FileStream?.Dispose();
                }
                _requestParams.Files.Clear();
            }
        }

        /// <summary>
        /// 使用 SOCKS 代理执行请求
        /// </summary>
        private HttpRequestClass ExecuteRequestWithSocks(HttpRequestMessage request, object data)
        {
            try
            {
                // 解析代理地址
                var proxyUri = new Uri(_requestParams.Proxy);
                var proxyHost = proxyUri.Host;
                var proxyPort = proxyUri.Port > 0 ? proxyUri.Port : (_requestParams.ProxyType == ProxyType.Socks5 ? 1080 : 1080);

                // 创建 SOCKS Handler
                using var handler = new SocksHttpMessageHandler(
                    proxyHost, proxyPort, _requestParams.ProxyType,
                    _requestParams.ProxyUsername, _requestParams.ProxyPassword,
                    _requestParams.FollowLocation, _requestParams.SslVerifyPeer);

                // 创建 HTTP 客户端
                using var client = new HttpClient(handler);
                ConfigureClient(client);

                // 执行请求
                return ExecuteRequest(client, request);
            }
            catch (Exception ex)
            {
                CreateErrorResponse(ex);
                return this;
            }
        }

        /// <summary>
        /// 获取响应数据
        /// </summary>
        public HttpResponseData GetResponse() => _responseData;

        private void ConfigureHandler(HttpClientHandler handler)
        {
            handler.UseCookies = false;
            handler.AllowAutoRedirect = _requestParams.FollowLocation;

            // ✅ 添加自动解压支持（GZIP 和 DEFLATE）
            //handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            // 唯一的代理设置代码块
            if (!string.IsNullOrEmpty(_requestParams.Proxy))
            {
                handler.UseProxy = true;
                handler.Proxy = new WebProxy(_requestParams.Proxy)
                {
                    Credentials = new NetworkCredential(
                        _requestParams.ProxyUsername,
                        _requestParams.ProxyPassword)
                };
            }
            else
            {
                // 确保代理被禁用
                handler.UseProxy = false;
            }

            // SSL验证设置
            if (!_requestParams.SslVerifyPeer)
            {
                // 警告：禁用SSL证书验证存在安全风险
                // 仅在明确需要的情况下使用此选项（例如测试环境）
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    // 记录SSL验证被禁用的信息
                    Debug.WriteLine("Warning: SSL certificate validation is disabled. This is insecure in production environments.");
                    return true; // 仍然接受所有证书，但添加了警告
                };
            }
        }
        #region 临时请求头实现

        // 存储临时请求头的字典

        /// <summary>
        /// 设置临时请求头（仅对下一次请求有效）
        /// </summary>
        public HttpRequestClass SetTemporaryHeader(string name, string value)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(value))
                {
                    // 值为空时移除该头
                    if (_temporaryHeaders.ContainsKey(name))
                    {
                        _temporaryHeaders.Remove(name);
                    }
                }
                else
                {
                    _temporaryHeaders[name] = value;
                }
            }
            return this;
        }

        #endregion
        #region 文件上传
        /// <summary>
        /// 添加文件到请求（用于文件上传）
        /// </summary>
        /// <param name="fieldName">表单字段名</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="contentType">文件内容类型（可选）</param>
        /// <param name="fileName">自定义文件名（可选）</param>
        public HttpRequestClass AddFile(string fieldName, string filePath, string contentType = null, string fileName = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var fileInfo = new FileInfo(filePath);
            var fileContent = new FileUploadContent
            {
                FieldName = fieldName,
                FilePath = filePath,
                FileName = fileName ?? fileInfo.Name,
                ContentType = contentType ?? GetMimeType(fileInfo.Extension)
            };

            _requestParams.Files.Add(fileContent);
            return this;
        }

        /// <summary>
        /// 添加文件流到请求（用于文件上传）
        /// </summary>
        /// <param name="fieldName">表单字段名</param>
        /// <param name="stream">文件流</param>
        /// <param name="fileName">文件名</param>
        /// <param name="contentType">文件内容类型</param>
        public HttpRequestClass AddFile(string fieldName, Stream stream, string fileName, string contentType)
        {
            if (stream == null || !stream.CanRead)
                throw new ArgumentException("Invalid stream");

            var fileContent = new FileUploadContent
            {
                FieldName = fieldName,
                FileStream = stream,
                FileName = fileName,
                ContentType = contentType
            };

            _requestParams.Files.Add(fileContent);
            return this;
        }

        /// <summary>
        /// 添加字节数组文件（用于文件上传）
        /// </summary>
        /// <param name="fieldName">表单字段名</param>
        /// <param name="data">文件数据</param>
        /// <param name="fileName">文件名</param>
        /// <param name="contentType">文件内容类型</param>
        public HttpRequestClass AddFile(string fieldName, byte[] data, string fileName, string contentType)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Invalid file data");

            var fileContent = new FileUploadContent
            {
                FieldName = fieldName,
                FileData = data,
                FileName = fileName,
                ContentType = contentType
            };

            _requestParams.Files.Add(fileContent);
            return this;
        }

        /// <summary>
        /// 清除所有已添加的文件
        /// </summary>
        public HttpRequestClass ClearFiles()
        {
            _requestParams.Files.Clear();
            return this;
        }

        // 获取文件扩展名对应的MIME类型
        private string GetMimeType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";

            extension = extension.ToLowerInvariant();

            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                _ => "application/octet-stream"
            };
        }
        #endregion
        private void ConfigureClient(HttpClient client)
        {
            // 1. 设置超时
            client.Timeout = TimeSpan.FromSeconds(_requestParams.Timeout);

            // 2. 【修复】使用安全方式设置User-Agent - 放在这里！
            if (!string.IsNullOrEmpty(_requestParams.UserAgent))
            {
                // ✅ 正确用法：跳过严格验证
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "User-Agent",
                    _requestParams.UserAgent
                );
            }
        }


        private HttpRequestMessage CreateRequest()
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(_requestParams.Url),
                Method = _requestParams.Method
            };

            // 3. 【重点】SetRequestHeaders的位置 - 应该放在这里！
            SetRequestHeaders(request); // 👈 正确位置！

            return request;
        }
        // 4. 独立的SetRequestHeaders方法实现
        private void SetRequestHeaders(HttpRequestMessage request)
        {
            // 1. 处理Cookie头（如果Cookie管理器中有Cookie）
            var cookieString = _requestParams.CookieManager.GetCookieString();
            if (!string.IsNullOrEmpty(cookieString))
            {
                request.Headers.TryAddWithoutValidation("Cookie", cookieString);
            }

            // 2. 添加常规自定义请求头
            foreach (var header in _requestParams.HeadersArray)
            {
                try
                {
                    // 跳过头部值验证
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"头设置错误 [{header.Key}]: {ex.Message}");
                }
            }

            // 3. 添加临时请求头（优先级高于常规头）
            lock (_lock)
            {
                foreach (var header in _temporaryHeaders)
                {
                    try
                    {
                        // 先移除同名的头（如果有）
                        request.Headers.Remove(header.Key);

                        // 添加新的临时头
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"临时头设置错误 [{header.Key}]: {ex.Message}");
                    }
                }
            }

            // 4. 自动补全默认协议头（如果需要）
            if (_requestParams.CompleteProtocolHeaders)
            {
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Language", "zh-cn");
                request.Headers.TryAddWithoutValidation("Referer", _requestParams.Url);
                //request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
                request.Headers.TryAddWithoutValidation("Connection", "Keep-Alive");
            }
        }
        private Dictionary<string, string> GetRequestHeaders()
        {
            var headers = new Dictionary<string, string>();

            // 添加自定义头
            if (!string.IsNullOrEmpty(_requestParams.Headers))
            {
                foreach (var line in _requestParams.Headers.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.Contains(':'))
                    {
                        var parts = line.Split(':', 2);
                        headers[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }

            // 添加数组头
            foreach (var header in _requestParams.HeadersArray)
            {
                headers[header.Key] = header.Value;
            }

            // 添加 Cookie
            var cookieString = _requestParams.CookieManager.GetCookieString();
            if (!string.IsNullOrEmpty(cookieString))
            {
                headers["Cookie"] = cookieString;
            }

            // 添加默认协议头
            if (_requestParams.CompleteProtocolHeaders)
            {
                AddDefaultHeaders(headers);
            }

            return headers;
        }
        /// <summary>
        /// 添加单个请求头
        /// </summary>
        public HttpRequestClass SetRequestHeader(string name, string value)
        {
            _requestParams.HeadersArray[name] = value;
            return this;
        }
        /// <summary>
        /// 设置是否跟随重定向（false禁止重定向）(默认禁止)
        /// </summary>
        public HttpRequestClass SetFollowLocation(bool follow)
        {
            _requestParams.FollowLocation = follow;
            return this;
        }
        private void AddDefaultHeaders(Dictionary<string, string> headers)
        {
            var defaults = new Dictionary<string, string>
            {
                ["Accept"] = "*/*",
                ["Accept-Language"] = "zh-cn",
                ["Referer"] = _requestParams.Url,
                ["Pragma"] = "no-cache",
                ["Connection"] = "Keep-Alive"
            };

            // 添加缺失的默认头
            foreach (var def in defaults)
            {
                if (!headers.ContainsKey(def.Key))
                {
                    headers[def.Key] = def.Value;
                }
            }

            // 确保有 UserAgent
            if (!headers.ContainsKey("User-Agent") && string.IsNullOrEmpty(_requestParams.UserAgent))
            {
                headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
            }
        }

        // 智能设置请求内容
        private void SetRequestContent(HttpRequestMessage request, object data)
        {
            // 如果有文件上传，使用multipart/form-data格式
            if (_requestParams.Files.Count > 0)
            {
                var multipartContent = new MultipartFormDataContent();

                // 添加普通表单字段
                AddFormDataContent(multipartContent, data);

                // 添加文件内容
                AddFileContent(multipartContent);

                request.Content = multipartContent;
            }
            else
            {
                // 没有文件上传时，使用普通内容格式
                SetRegularContent(request, data);
            }
        }
        private void AddFormDataContent(MultipartFormDataContent multipartContent, object data)
        {
            if (data == null) return;

            switch (data)
            {
                case string str:
                    // 处理字符串形式的表单数据
                    var formData = ParseFormData(str);
                    foreach (var field in formData)
                    {
                        multipartContent.Add(new StringContent(field.Value), field.Key);
                    }
                    break;

                case Dictionary<string, string> dict:
                    foreach (var field in dict)
                    {
                        multipartContent.Add(new StringContent(field.Value), field.Key);
                    }
                    break;

                case IDictionary<string, string> idict:
                    foreach (var field in idict)
                    {
                        multipartContent.Add(new StringContent(field.Value), field.Key);
                    }
                    break;

                default:
                    // 其他对象自动序列化为JSON
                    var json = JsonSerializer.Serialize(data);
                    multipartContent.Add(new StringContent(json, Encoding.UTF8, "application/json"), "json_data");
                    break;
            }
        }
        // 添加文件内容到multipart
        private void AddFileContent(MultipartFormDataContent multipartContent)
        {
            foreach (var file in _requestParams.Files)
            {
                HttpContent fileContent;

                if (file.FileStream != null)
                {
                    // 处理流类型文件
                    fileContent = new StreamContent(file.FileStream);
                }
                else if (file.FileData != null)
                {
                    // 处理字节数组类型文件
                    fileContent = new ByteArrayContent(file.FileData);
                }
                else
                {
                    // 处理文件路径类型文件
                    var fileBytes = File.ReadAllBytes(file.FilePath);
                    fileContent = new ByteArrayContent(fileBytes);
                }

                // 设置内容类型
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                // 添加到multipart
                multipartContent.Add(fileContent, file.FieldName, file.FileName);
            }
        }
        // 设置普通请求内容
        private void SetRegularContent(HttpRequestMessage request, object data)
        {
            if (data == null) return;

            switch (data)
            {
                case string str:
                    request.Content = new StringContent(str, Encoding.UTF8);
                    break;

                case Dictionary<string, string> formData:
                    request.Content = new FormUrlEncodedContent(formData);
                    break;

                case byte[] bytes:
                    request.Content = new ByteArrayContent(bytes);
                    break;

                case Stream stream:
                    request.Content = new StreamContent(stream);
                    break;

                default:
                    var json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    break;
            }

            // 智能补全Content-Type
            SetDefaultContentType(request.Content);
        }
        // 自动设置默认Content-Type
        private void SetDefaultContentType(HttpContent content)
        {
            // 如果开发者已经设置类型，则不覆盖
            if (content == null || content.Headers.ContentType != null) return;

            // 使用传统的类型检查代替C# 9.0的类型模式匹配
            if (content is StringContent)
            {
                var sc = (StringContent)content;
                // 检查是否是文本内容
                var value = sc.ReadAsStringAsync().Result;
                if (value.StartsWith("{") || value.StartsWith("["))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
                else
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                }
            }
            else if (content is FormUrlEncodedContent)
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded; Charset=UTF-8");
            }
            else if (content is ByteArrayContent || content is StreamContent)
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            }
        }
        // 解析字符串形式的表单数据
        private Dictionary<string, string> ParseFormData(string formData)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(formData))
                return result;

            var pairs = formData.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    var key = Uri.UnescapeDataString(parts[0]);
                    var value = Uri.UnescapeDataString(parts[1]);
                    result[key] = value;
                }
            }

            return result;
        }
        private HttpRequestClass ExecuteRequest(HttpClient client, HttpRequestMessage request)
        {
            try
            {
                var response = client.SendAsync(request).Result;
                ProcessResponse(response);
            }
            catch (AggregateException ae)
            {
                // 提取真实异常
                var ex = ae.InnerException ?? ae;
                CreateErrorResponse(ex);
            }
            catch (Exception ex)
            {
                CreateErrorResponse(ex);
            }
            return this;
        }
        private void CreateErrorResponse(Exception ex)
        {
            _responseData = new HttpResponseData
            {
                StatusCode = 0, // 自定义错误状态
                Body = JsonSerializer.Serialize(new
                {
                    error = true,
                    message = ex.Message,
                    type = ex.GetType().Name
                }),
                RequestHeaders = ex is HttpRequestException hre ?
                    hre.ToString() : string.Empty
            };
        }
        private void ProcessResponse(HttpResponseMessage response)
        {
            // 获取原始响应字节流
            byte[] rawBytes = response.Content.ReadAsByteArrayAsync().Result;

            // 尝试自动解压 GZIP 内容
            string body = TryDecompressGzip(rawBytes, response.Content.Headers.ContentEncoding);

            // 如果解压失败，使用原始文本
            if (body == null)
            {
                body = Encoding.UTF8.GetString(rawBytes);
            }

            // 处理响应头
            var responseHeaders = new StringBuilder();
            var responseHeadersArray = new Dictionary<string, string>();

            foreach (var header in response.Headers)
            {
                var value = string.Join("; ", header.Value);
                responseHeaders.AppendLine($"{header.Key}: {value}");
                responseHeadersArray[header.Key] = value;
            }

            // 处理 Set-Cookie
            var cookieHeaders = new StringBuilder();
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                foreach (var cookie in setCookies)
                {
                    cookieHeaders.AppendLine($"Set-Cookie: {cookie}");

                    // 更新 Cookie 管理器
                    var cookieParts = cookie.Split(';')[0].Split('=');
                    if (cookieParts.Length >= 2)
                    {
                        var name = cookieParts[0].Trim();
                        var value = cookieParts[1].Trim();
                        _requestParams.CookieManager.SetCookie(name, value);
                    }
                }
            }

            // 填充响应数据
            _responseData = new HttpResponseData
            {
                StatusCode = (int)response.StatusCode,
                RequestHeaders = _requestParams.Headers,
                RequestHeadersArray = _requestParams.HeadersArray,
                ResponseHeaders = responseHeaders.ToString() + cookieHeaders,
                ResponseHeadersArray = responseHeadersArray,
                Body = body,
                CookieManager = _requestParams.CookieManager,
                Cookie = _requestParams.CookieManager.GetCookieString(),
                rawResult = rawBytes
            };
        }
        // GZIP 解压方法
        private string TryDecompressGzip(byte[] compressedData, ICollection<string> contentEncoding)
        {
            if (compressedData == null || compressedData.Length == 0)
                return null;

            // 检查是否为 GZIP 压缩
            if (contentEncoding != null && contentEncoding.Contains("gzip"))
            {
                try
                {
                    using (var compressedStream = new MemoryStream(compressedData))
                    using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    using (var resultStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(resultStream);
                        return Encoding.UTF8.GetString(resultStream.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GZIP解压失败: {ex.Message}");
                    return null;
                }
            }
            return null;
        }
        /// <summary>
        /// 克隆当前实例
        /// </summary>
        /// <returns></returns>
        public HttpRequestClass Clone()
        {
            var clone = new HttpRequestClass();

            // 深拷贝 HttpRequestParameter
            clone._requestParams = new HttpRequestParameter(clone)
            {
                Url = _requestParams.Url,
                Method = _requestParams.Method,
                Data = _requestParams.Data, // 注意：这里假设Data是值类型或已经被正确处理的引用类型
                Headers = _requestParams.Headers,
                Timeout = _requestParams.Timeout,
                Proxy = _requestParams.Proxy,
                ProxyType = _requestParams.ProxyType,
                ProxyUsername = _requestParams.ProxyUsername,
                ProxyPassword = _requestParams.ProxyPassword,
                FollowLocation = _requestParams.FollowLocation,
                CompleteProtocolHeaders = _requestParams.CompleteProtocolHeaders,
                SslVerifyPeer = _requestParams.SslVerifyPeer,
                SslVerifyHost = _requestParams.SslVerifyHost,
                UserAgent = _requestParams.UserAgent
            };

            // 深拷贝 HeadersArray
            if (_requestParams.HeadersArray != null)
            {
                clone._requestParams.HeadersArray = new Dictionary<string, string>();
                foreach (var header in _requestParams.HeadersArray)
                {
                    clone._requestParams.HeadersArray[header.Key] = header.Value;
                }
            }

            // 深拷贝 CookieManager
            var cookieManager = new CookieManager();
            if (_requestParams.CookieManager != null)
            {
                // 假设CookieManager有GetAllCookies方法返回所有cookie的字典
                var allCookies = _requestParams.CookieManager.GetAllCookies();
                if (allCookies != null)
                {
                    cookieManager.SetCookie(allCookies);
                }
            }
            clone.Set_CookieManager(cookieManager);

            return clone;
        }
        public void Dispose()
        {
            // ✅ 空实现（因为不需要特殊资源清理）
            // 或者完全移除Dispose方法
        }
    }
    #endregion
    /// <summary>
    /// 文件上传内容类
    /// </summary>
    public class FileUploadContent
    {
        /// <summary>
        /// 表单字段名
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 文件流
        /// </summary>
        public Stream FileStream { get; set; }
        /// <summary>
        /// 文件数据字节数组
        /// </summary>
        public byte[] FileData { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 内容类型（MIME类型）
        /// </summary>
        public string ContentType { get; set; }
    }
    #region SOCKS 代理实现
    /// <summary>
    /// 支持 SOCKS 代理的 HttpMessageHandler
    /// </summary>
    internal class SocksHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _proxyHost;
        private readonly int _proxyPort;
        private readonly ProxyType _proxyType;
        private readonly string _proxyUsername;
        private readonly string _proxyPassword;
        private readonly bool _allowAutoRedirect;
        private readonly bool _sslVerifyPeer;

        public SocksHttpMessageHandler(string proxyHost, int proxyPort, ProxyType proxyType,
            string proxyUsername, string proxyPassword, bool allowAutoRedirect, bool sslVerifyPeer)
        {
            _proxyHost = proxyHost;
            _proxyPort = proxyPort;
            _proxyType = proxyType;
            _proxyUsername = proxyUsername;
            _proxyPassword = proxyPassword;
            _allowAutoRedirect = allowAutoRedirect;
            _sslVerifyPeer = sslVerifyPeer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri;
            var destinationHost = uri.Host;
            var destinationPort = uri.Port > 0 ? uri.Port : (uri.Scheme == "https" ? 443 : 80);

            // 建立 SOCKS 连接
            Socket socket = null;
            NetworkStream stream = null;
            try
            {
                socket = SocksWebProxy.CreateSocksConnection(
                    _proxyHost, _proxyPort, _proxyType,
                    destinationHost, destinationPort,
                    _proxyUsername, _proxyPassword);

                stream = new NetworkStream(socket, true);

                // 如果是 HTTPS，需要建立 TLS 连接
                Stream transportStream = stream;
                if (uri.Scheme == "https")
                {
                    var sslStream = new System.Net.Security.SslStream(stream, false, (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        return !_sslVerifyPeer || sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                    });
                    // .NET Standard 2.1 中 AuthenticateAsClientAsync 不接受 CancellationToken
                    await sslStream.AuthenticateAsClientAsync(destinationHost);
                    transportStream = sslStream;
                }

                // 构建 HTTP 请求
                var requestBuilder = new StringBuilder();
                requestBuilder.Append($"{request.Method} {uri.PathAndQuery} HTTP/1.1\r\n");
                requestBuilder.Append($"Host: {destinationHost}\r\n");

                // 添加请求头
                foreach (var header in request.Headers)
                {
                    if (header.Key.ToLowerInvariant() == "host")
                        continue; // 已经添加了
                    foreach (var value in header.Value)
                    {
                        requestBuilder.Append($"{header.Key}: {value}\r\n");
                    }
                }

                // 添加内容
                if (request.Content != null)
                {
                    var contentHeaders = request.Content.Headers;
                    foreach (var header in contentHeaders)
                    {
                        foreach (var value in header.Value)
                        {
                            requestBuilder.Append($"{header.Key}: {value}\r\n");
                        }
                    }
                    requestBuilder.Append("\r\n");
                    // .NET Standard 2.1 中 ReadAsByteArrayAsync 不接受 CancellationToken
                    var contentBytes = await request.Content.ReadAsByteArrayAsync();
                    var requestBytes = Encoding.UTF8.GetBytes(requestBuilder.ToString());
                    await transportStream.WriteAsync(requestBytes, 0, requestBytes.Length, cancellationToken);
                    await transportStream.WriteAsync(contentBytes, 0, contentBytes.Length, cancellationToken);
                }
                else
                {
                    requestBuilder.Append("\r\n");
                    var requestBytes = Encoding.UTF8.GetBytes(requestBuilder.ToString());
                    await transportStream.WriteAsync(requestBytes, 0, requestBytes.Length, cancellationToken);
                }

                // 读取响应
                var responseBytes = new List<byte>();
                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await transportStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    responseBytes.AddRange(buffer.Take(bytesRead));
                    // 简单的检查：如果已经读取了完整的响应头，可以尝试解析
                    if (responseBytes.Count > 4)
                    {
                        var responseText = Encoding.UTF8.GetString(responseBytes.ToArray());
                        if (responseText.Contains("\r\n\r\n"))
                        {
                            // 检查 Content-Length 来确定是否读取完整
                            var headerEnd = responseText.IndexOf("\r\n\r\n");
                            var headers = responseText.Substring(0, headerEnd);
                            var contentLengthMatch = System.Text.RegularExpressions.Regex.Match(headers, @"Content-Length:\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (contentLengthMatch.Success)
                            {
                                var contentLength = int.Parse(contentLengthMatch.Groups[1].Value);
                                var bodyStart = headerEnd + 4;
                                if (responseBytes.Count >= bodyStart + contentLength)
                                {
                                    break; // 已读取完整响应
                                }
                            }
                            else if (responseText.Contains("Transfer-Encoding: chunked", StringComparison.OrdinalIgnoreCase))
                            {
                                // 分块传输，需要特殊处理（简化实现，读取到连接关闭）
                                // 这里简化处理，实际应该解析 chunked 编码
                            }
                        }
                    }
                }

                // 解析响应
                var responseText2 = Encoding.UTF8.GetString(responseBytes.ToArray());
                var headerEnd2 = responseText2.IndexOf("\r\n\r\n");
                if (headerEnd2 < 0)
                {
                    throw new HttpRequestException("无效的 HTTP 响应");
                }

                var statusLine = responseText2.Substring(0, responseText2.IndexOf("\r\n"));
                var statusParts = statusLine.Split(' ');
                var statusCode = int.Parse(statusParts[1]);

                var headersText = responseText2.Substring(0, headerEnd2);
                var bodyText = responseText2.Substring(headerEnd2 + 4);

                var response = new HttpResponseMessage((HttpStatusCode)statusCode);
                response.Content = new StringContent(bodyText, Encoding.UTF8);

                // 解析响应头
                var headerLines = headersText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < headerLines.Length; i++)
                {
                    var colonIndex = headerLines[i].IndexOf(':');
                    if (colonIndex > 0)
                    {
                        var headerName = headerLines[i].Substring(0, colonIndex).Trim();
                        var headerValue = headerLines[i].Substring(colonIndex + 1).Trim();
                        if (!response.Headers.TryAddWithoutValidation(headerName, headerValue))
                        {
                            response.Content.Headers.TryAddWithoutValidation(headerName, headerValue);
                        }
                    }
                }

                return response;
            }
            finally
            {
                stream?.Dispose();
                socket?.Close();
            }
        }
    }

    /// <summary>
    /// SOCKS 代理实现类（支持 SOCKS4 和 SOCKS5）
    /// </summary>
    internal class SocksWebProxy : IWebProxy
    {
        private readonly string _proxyHost;
        private readonly int _proxyPort;
        private readonly ProxyType _proxyType;
        private readonly ICredentials _credentials;

        public SocksWebProxy(string proxyHost, int proxyPort, ProxyType proxyType, ICredentials credentials = null)
        {
            _proxyHost = proxyHost ?? throw new ArgumentNullException(nameof(proxyHost));
            _proxyPort = proxyPort;
            _proxyType = proxyType;
            _credentials = credentials;
        }

        public ICredentials Credentials
        {
            get => _credentials;
            set { /* SOCKS 代理的凭据在构造时设置 */ }
        }

        public Uri GetProxy(Uri destination)
        {
            return new Uri($"socks://{_proxyHost}:{_proxyPort}");
        }

        public bool IsBypassed(Uri host)
        {
            return false; // 所有请求都通过代理
        }

        /// <summary>
        /// 创建 SOCKS 代理连接
        /// </summary>
        public static Socket CreateSocksConnection(string proxyHost, int proxyPort, ProxyType proxyType, 
            string destinationHost, int destinationPort, string username = null, string password = null)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(proxyHost, proxyPort);

            if (proxyType == ProxyType.Socks5)
            {
                // SOCKS5 握手
                var authRequired = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
                
                // 发送认证方法
                var authMethods = new List<byte>();
                if (authRequired)
                {
                    authMethods.Add(0x02); // 用户名/密码认证
                }
                authMethods.Add(0x00); // 无认证
                
                var authRequest = new byte[2 + authMethods.Count];
                authRequest[0] = 0x05; // SOCKS 版本
                authRequest[1] = (byte)authMethods.Count;
                Array.Copy(authMethods.ToArray(), 0, authRequest, 2, authMethods.Count);
                socket.Send(authRequest);

                // 接收认证方法响应
                var authResponse = new byte[2];
                socket.Receive(authResponse);
                if (authResponse[0] != 0x05)
                {
                    socket.Close();
                    throw new Exception("SOCKS5 握手失败：版本不匹配");
                }

                // 如果需要认证
                if (authResponse[1] == 0x02 && authRequired)
                {
                    // 发送用户名/密码
                    var usernameBytes = Encoding.UTF8.GetBytes(username);
                    var passwordBytes = Encoding.UTF8.GetBytes(password);
                    var credRequest = new byte[3 + usernameBytes.Length + passwordBytes.Length];
                    credRequest[0] = 0x01; // 认证版本
                    credRequest[1] = (byte)usernameBytes.Length;
                    Array.Copy(usernameBytes, 0, credRequest, 2, usernameBytes.Length);
                    credRequest[2 + usernameBytes.Length] = (byte)passwordBytes.Length;
                    Array.Copy(passwordBytes, 0, credRequest, 3 + usernameBytes.Length, passwordBytes.Length);
                    socket.Send(credRequest);

                    // 接收认证响应
                    var credResponse = new byte[2];
                    socket.Receive(credResponse);
                    if (credResponse[1] != 0x00)
                    {
                        socket.Close();
                        throw new Exception("SOCKS5 认证失败");
                    }
                }
                else if (authResponse[1] != 0x00)
                {
                    socket.Close();
                    throw new Exception($"SOCKS5 不支持的认证方法: {authResponse[1]}");
                }

                // 发送连接请求
                var connectRequest = new List<byte> { 0x05, 0x01, 0x00 }; // VER, CMD, RSV
                
                // 解析目标地址
                if (IPAddress.TryParse(destinationHost, out var ipAddress))
                {
                    // IP 地址
                    connectRequest.Add(0x01); // ATYP = IPv4
                    connectRequest.AddRange(ipAddress.GetAddressBytes());
                }
                else
                {
                    // 域名
                    var hostBytes = Encoding.UTF8.GetBytes(destinationHost);
                    connectRequest.Add(0x03); // ATYP = 域名
                    connectRequest.Add((byte)hostBytes.Length);
                    connectRequest.AddRange(hostBytes);
                }
                
                // 端口（大端序）
                connectRequest.Add((byte)(destinationPort >> 8));
                connectRequest.Add((byte)(destinationPort & 0xFF));
                
                socket.Send(connectRequest.ToArray());

                // 接收连接响应
                var connectResponse = new byte[10];
                var received = socket.Receive(connectResponse);
                if (received < 4 || connectResponse[0] != 0x05 || connectResponse[1] != 0x00)
                {
                    socket.Close();
                    throw new Exception($"SOCKS5 连接失败: {connectResponse[1]}");
                }
            }
            else if (proxyType == ProxyType.Socks4)
            {
                // SOCKS4 连接
                var connectRequest = new List<byte>();
                
                // 命令和端口
                connectRequest.Add(0x04); // SOCKS 版本
                connectRequest.Add(0x01); // CONNECT 命令
                connectRequest.Add((byte)(destinationPort >> 8)); // 端口高字节
                connectRequest.Add((byte)(destinationPort & 0xFF)); // 端口低字节
                
                // IP 地址或域名
                if (IPAddress.TryParse(destinationHost, out var ipAddress))
                {
                    connectRequest.AddRange(ipAddress.GetAddressBytes());
                }
                else
                {
                    // SOCKS4a 支持域名
                    connectRequest.Add(0x00);
                    connectRequest.Add(0x00);
                    connectRequest.Add(0x00);
                    connectRequest.Add(0x01); // 非零表示域名
                    var hostBytes = Encoding.UTF8.GetBytes(destinationHost);
                    connectRequest.AddRange(hostBytes);
                }
                
                // 用户名（如果提供）
                if (!string.IsNullOrEmpty(username))
                {
                    var usernameBytes = Encoding.UTF8.GetBytes(username);
                    connectRequest.AddRange(usernameBytes);
                }
                connectRequest.Add(0x00); // 结束符
                
                // 如果是域名，添加域名
                if (!IPAddress.TryParse(destinationHost, out _))
                {
                    var hostBytes = Encoding.UTF8.GetBytes(destinationHost);
                    connectRequest.AddRange(hostBytes);
                    connectRequest.Add(0x00); // 结束符
                }
                
                socket.Send(connectRequest.ToArray());

                // 接收连接响应
                var connectResponse = new byte[8];
                var received = socket.Receive(connectResponse);
                if (received < 2 || connectResponse[0] != 0x00 || connectResponse[1] != 0x5A)
                {
                    socket.Close();
                    throw new Exception($"SOCKS4 连接失败: {connectResponse[1]}");
                }
            }

            return socket;
        }
    }
    #endregion
}
