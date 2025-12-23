using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WodToolKit.Http;
using WodToolKit.Json;

namespace WodToolKit.src.UmiOCR
{
    /// <summary>
    /// 文档识别类，用于处理 PDF 等文档的 OCR 识别
    /// </summary>
    public class Doc : @base
    {
        /// <summary>
        /// 文档任务状态结果（对应 /api/doc/result 返回结构）
        /// 文档结构参考 Umi-OCR 官方文档：https://github.com/hiroi-sora/Umi-OCR/blob/main/docs/http/api_doc.md
        /// </summary>
        public class DocTaskResult
        {
            /// <summary>
            /// 状态码，100 表示成功
            /// </summary>
            [JsonPropertyName("code")]
            public int Code { get; set; }

            /// <summary>
            /// 数据字段：
            /// - 成功时：为识别结果（字符串或结构化内容，取决于 format / is_data）
            /// - 失败时：为错误原因
            /// </summary>
            [JsonPropertyName("data")]
            public object Data { get; set; }

            /// <summary>
            /// 已识别完成的页数
            /// </summary>
            [JsonPropertyName("processed_count")]
            public int? ProcessedCount { get; set; }

            /// <summary>
            /// 总页数
            /// </summary>
            [JsonPropertyName("pages_count")]
            public int? PagesCount { get; set; }

            /// <summary>
            /// 是否已结束（成功或失败）
            /// </summary>
            [JsonPropertyName("is_done")]
            public bool? IsDone { get; set; }

            /// <summary>
            /// 任务状态：waiting / running / success / failure
            /// </summary>
            [JsonPropertyName("state")]
            public string State { get; set; }

            /// <summary>
            /// 失败时的原因
            /// </summary>
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }

        /// <summary>
        /// 文档识别结果
        /// </summary>
        public class DocRecognizeResult
        {
            /// <summary>
            /// 任务ID
            /// </summary>
            public string TaskId { get; set; }

            /// <summary>
            /// 下载链接
            /// </summary>
            public string DownloadUrl { get; set; }

            /// <summary>
            /// 保存的文件路径（如果执行了下载）
            /// </summary>
            public string SavedFilePath { get; set; }

            /// <summary>
            /// 识别结果文本（format = "text" 时有值）
            /// </summary>
            public string TextContent { get; set; }
        }

        /// <summary>
        /// 获取文档识别参数定义
        /// </summary>
        /// <returns>参数定义的 JSON 字符串</returns>
        public string GetOptions()
        {
            http.Open($"{UmiUrl}/api/doc/get_options", HttpMethod.Get);
            http.Send();
            try
            {
                return http.GetResponse().Body;
            }
            catch (Exception ex)
            {
                throw new Exception("获取参数定义失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 上传文档文件，启动识别任务
        /// </summary>
        /// <param name="filePath">文档文件路径（PDF 等）</param>
        /// <param name="options">配置参数字典（可选）</param>
        /// <returns>任务ID</returns>
        public string Upload(string filePath, Dictionary<string, object> options = null)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException($"文件不存在: {filePath}");
            }

            http.Open($"{UmiUrl}/api/doc/upload", HttpMethod.Post);

            // 额外做一次扩展名检查，避免 Umi-OCR 报空扩展名错误
            var ext = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(ext))
            {
                throw new Exception("上传文档失败：文件没有扩展名，Umi-OCR 只能识别带有扩展名的文件（如 .pdf）。");
            }

            // 添加文件：显式指定一个简单的英文文件名，避免某些情况下服务端解析不到扩展名
            var safeFileName = "document" + ext;
            http.AddFile("file", filePath, null, safeFileName);

            // 添加配置参数（如果有）
            Dictionary<string, string> formData = null;
            if (options != null && options.Count > 0)
            {
                string jsonOptions = JsonSerializer.Serialize(options);
                formData = new Dictionary<string, string>
                {
                    { "json", jsonOptions }
                };
            }

            http.Send(formData);
            
            try
            {
                var response = http.GetResponse();
                var result = EasyJson.ParseJsonToDynamic(response.Body);
                
                if (result.code == 100)
                {
                    return result.data.ToString();
                }
                else
                {
                    throw new Exception($"上传失败: {result.data}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("上传文档失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 查询任务状态
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="isData">是否返回识别结果，默认 false</param>
        /// <param name="isUnread">是否只返回未读结果，默认 true</param>
        /// <param name="format">返回格式，"dict"（默认）或 "text"</param>
        /// <returns><see cref="DocTaskResult"/> 任务状态信息</returns>
        public DocTaskResult GetResult(string taskId, bool isData = false, bool isUnread = true, string format = "dict")
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                throw new ArgumentException("任务ID不能为空");
            }

            http.Open($"{UmiUrl}/api/doc/result", HttpMethod.Post);
            
            var requestData = new Dictionary<string, object>
            {
                { "id", taskId },
                { "is_data", isData },
                { "is_unread", isUnread },
                { "format", format }
            };

            http.Send(requestData);
            
            try
            {
                var response = http.GetResponse();

                if (string.IsNullOrWhiteSpace(response.Body))
                {
                    throw new Exception("查询失败: 响应为空");
                }

                var result = JsonSerializer.Deserialize<DocTaskResult>(response.Body);
                if (result == null)
                {
                    throw new Exception("查询失败: 无法解析响应");
                }

                if (result.Code == 100)
                {
                    return result;
                }

                throw new Exception($"查询失败: {result.Data}");
            }
            catch (Exception ex)
            {
                throw new Exception("查询任务状态失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 等待任务完成（轮询方式）
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="pollInterval">轮询间隔（毫秒），默认 1000</param>
        /// <param name="timeout">超时时间（毫秒），默认 300000（5分钟）</param>
        /// <returns>任务完成后的结果（不强制包含识别内容）</returns>
        public DocTaskResult WaitForCompletion(string taskId, int pollInterval = 1000, int timeout = 300000)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                throw new ArgumentException("任务ID不能为空");
            }

            var startTime = DateTime.Now;
            
            while (true)
            {
                var result = GetResult(taskId, isData: false);

                // 检查是否完成
                if (result.IsDone == true)
                {
                    if (string.Equals(result.State, "success", StringComparison.OrdinalIgnoreCase))
                    {
                        // 任务成功，调用方如需内容可再按需调用 GetResult
                        return result;
                    }

                    if (string.Equals(result.State, "failure", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception($"任务失败: {result.Message ?? "未知错误"}");
                    }
                }

                // 检查超时
                if ((DateTime.Now - startTime).TotalMilliseconds > timeout)
                {
                    throw new TimeoutException("任务执行超时");
                }

                // 等待后继续轮询
                System.Threading.Thread.Sleep(pollInterval);
            }
        }

        /// <summary>
        /// 生成目标文件并获取下载链接
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="fileTypes">文件类型数组，可选值：pdfLayered（双层PDF）、pdfOneLayer（单层PDF）、txt、txtPlain、jsonl、csv</param>
        /// <param name="ignoreBlank">是否忽略空页，默认 true</param>
        /// <returns>包含下载链接和文件名的对象</returns>
        public dynamic Download(string taskId, List<string> fileTypes = null, bool ignoreBlank = true)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                throw new ArgumentException("任务ID不能为空");
            }

            if (fileTypes == null || fileTypes.Count == 0)
            {
                fileTypes = new List<string> { "pdfLayered" };
            }

            http.Open($"{UmiUrl}/api/doc/download", HttpMethod.Post);
            
            var requestData = new Dictionary<string, object>
            {
                { "id", taskId },
                { "file_types", fileTypes },
                { "ignore_blank", ignoreBlank }
            };

            http.Send(requestData);
            
            try
            {
                var response = http.GetResponse();
                var result = EasyJson.ParseJsonToDynamic(response.Body);
                
                if (result.code == 100)
                {
                    return new
                    {
                        downloadUrl = result.data.ToString(),
                        fileName = result.name?.ToString() ?? "download"
                    };
                }
                else
                {
                    throw new Exception($"生成下载链接失败: {result.data}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("生成下载链接失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 下载结果文件
        /// </summary>
        /// <param name="downloadUrl">下载链接</param>
        /// <param name="savePath">保存路径（可选，如果不指定则返回字节数组）</param>
        /// <returns>如果指定了保存路径则返回文件路径，否则返回文件字节数组</returns>
        public object DownloadFile(string downloadUrl, string savePath = null)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                throw new ArgumentException("下载链接不能为空");
            }

            http.Open(downloadUrl, HttpMethod.Get);
            http.Send();
            
            try
            {
                var response = http.GetResponse();
                // 使用原始字节数据，而不是字符串
                byte[] fileBytes = response.rawResult ?? Encoding.UTF8.GetBytes(response.Body ?? string.Empty);
                
                if (!string.IsNullOrWhiteSpace(savePath))
                {
                    // 确保目录存在
                    var directory = Path.GetDirectoryName(savePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    File.WriteAllBytes(savePath, fileBytes);
                    return savePath;
                }
                else
                {
                    return fileBytes;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("下载文件失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 清理任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>是否清理成功</returns>
        public bool Clear(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                throw new ArgumentException("任务ID不能为空");
            }

            http.Open($"{UmiUrl}/api/doc/clear/{taskId}", HttpMethod.Get);
            http.Send();
            
            try
            {
                var response = http.GetResponse();
                var result = EasyJson.ParseJsonToDynamic(response.Body);
                
                return result.code == 100;
            }
            catch (Exception ex)
            {
                throw new Exception("清理任务失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 完整的文档识别流程（上传 -> 等待完成 -> 下载 -> 清理）
        /// </summary>
        /// <param name="filePath">文档文件路径</param>
        /// <param name="options">配置参数（可选）</param>
        /// <param name="fileTypes">结果文件类型数组（可选，默认 pdfLayered）</param>
        /// <param name="savePath">
        /// 结果文件保存路径（可选）。
        /// - 为空时：自动在同目录生成「原文件名_ocr.pdf」
        /// - 不为空时：保存到指定路径
        /// </param>
        /// <param name="pollInterval">轮询间隔（毫秒），默认 1000</param>
        /// <param name="timeout">超时时间（毫秒），默认 300000（5分钟）</param>
        /// <param name="autoClear">是否自动清理任务，默认 true</param>
        /// <returns>
        /// 返回 <see cref="DocRecognizeResult"/>：
        /// - 想要路径：使用 result.SavedFilePath
        /// - 想要内容：使用 result.TextContent（识别文本）
        /// - 想要下载链接或任务ID：使用 result.DownloadUrl / result.TaskId
        /// </returns>
        public DocRecognizeResult Recognize(
            string filePath,
            Dictionary<string, object> options = null,
            List<string> fileTypes = null,
            string savePath = null,
            int pollInterval = 1000,
            int timeout = 300000,
            bool autoClear = true)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("文件路径不能为空", nameof(filePath));
            }

            string taskId = null;
            try
            {
                // 1. 上传文件
                taskId = Upload(filePath, options);

                // 2. 等待任务完成（先获取完整状态，确保成功）
                var finalResult = WaitForCompletion(taskId, pollInterval, timeout);

                // 3. 获取识别文本内容（format = text）
                string textContent = null;
                try
                {
                    var textResult = GetResult(taskId, isData: true, isUnread: true, format: "text");
                    textContent = textResult.Data?.ToString();
                }
                catch
                {
                    // 文本获取失败不影响文件下载，忽略即可
                }

                // 4. 获取下载链接
                var downloadInfo = Download(taskId, fileTypes);
                string downloadUrl = downloadInfo.downloadUrl;

                // 5. 计算默认保存路径（如果未指定）
                if (string.IsNullOrWhiteSpace(savePath))
                {
                    var dir = Path.GetDirectoryName(filePath);
                    if (string.IsNullOrEmpty(dir))
                    {
                        dir = Environment.CurrentDirectory;
                    }

                    var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                    // 默认按照 pdfLayered 结果，使用 .pdf 后缀
                    savePath = Path.Combine(dir, nameWithoutExt + "_ocr.pdf");
                }

                // 6. 下载文件到本地
                DownloadFile(downloadUrl, savePath);

                // 7. 返回统一结果对象
                return new DocRecognizeResult
                {
                    TaskId = taskId,
                    DownloadUrl = downloadUrl,
                    SavedFilePath = savePath,
                    TextContent = textContent
                };
            }
            finally
            {
                // 8. 清理任务
                if (autoClear && !string.IsNullOrWhiteSpace(taskId))
                {
                    try
                    {
                        Clear(taskId);
                    }
                    catch
                    {
                        // 忽略清理错误
                    }
                }
            }
        }
    }
}
