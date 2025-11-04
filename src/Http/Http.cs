using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WodToolkit.Http
{
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
        /// 设置 Cookie 字符串（批量添加）
        /// </summary>
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
        public bool HasCookie(string name)
        {
            return !string.IsNullOrEmpty(name) && _cookies.ContainsKey(name);
        }

        /// <summary>
        /// 获取所有 Cookie 的字典副本
        /// </summary>
        public Dictionary<string, string> GetAllCookies()
        {
            return new Dictionary<string, string>(_cookies);
        }

        /// <summary>
        /// 获取 Cookie 字符串（URL 编码）
        /// </summary>
        public string GetCookieString()
        {
            return string.Join("; ", _cookies.Select(kv =>
                $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
        }

        /// <summary>
        /// 获取原始 Cookie 字符串（无编码）
        /// </summary>
        public string GetRawCookieString()
        {
            return string.Join("; ", _cookies.Select(kv => $"{kv.Key}={kv.Value}"));
        }

        /// <summary>
        /// 删除指定 Cookie
        /// </summary>
        public CookieManager RemoveCookie(string name)
        {
            if (!string.IsNullOrEmpty(name))
                _cookies.Remove(name);
            return this;
        }

        /// <summary>
        /// 清空所有 Cookie
        /// </summary>
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
        public int StatusCode { get; set; }
        public string RequestHeaders { get; set; }
        public Dictionary<string, string> RequestHeadersArray { get; set; }
        public string ResponseHeaders { get; set; }
        public Dictionary<string, string> ResponseHeadersArray { get; set; }
        public string Body { get; set; }
        public CookieManager CookieManager { get; set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public byte[] rawResult { get; set; }
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
        public string Url { get; set; }
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public object Data { get; set; }
        public string Headers { get; set; }
        public Dictionary<string, string> HeadersArray { get; set; } = new Dictionary<string, string>();
        public CookieManager CookieManager { get; set; } = new CookieManager();
        public int Timeout { get; set; } = 15;
        public string Proxy { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        /// <summary>
        /// 是否跟随重定向 (默认 true) (false: 不跟随重定向)
        /// </summary>
        public bool FollowLocation { get; set; }
        public bool CompleteProtocolHeaders { get; set; } = true;
        public bool SslVerifyPeer { get; set; }
        public bool SslVerifyHost { get; set; }
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
        /// 设置代理
        /// </summary>
        public HttpRequestClass SetProxy(string ip = "", string user = "", string pwd = "")
        {
            _requestParams.Proxy = ip;
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

                // 创建HTTP处理程序
                using var handler = new HttpClientHandler();
                ConfigureHandler(handler);

                // 创建HTTP客户端
                using var client = new HttpClient(handler);
                ConfigureClient(client);  // 配置客户端参数（超时、UA等）

                // 创建请求消息
                var request = CreateRequest();

                // 设置请求内容（智能处理普通数据和文件上传）
                SetRequestContent(request, data);

                // 执行请求并处理响应
                return ExecuteRequest(client, request);
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
        public string FieldName { get; set; }
        public string FilePath { get; set; }
        public Stream FileStream { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
