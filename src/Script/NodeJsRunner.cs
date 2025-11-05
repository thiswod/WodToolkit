using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WodToolkit.Script
{
    /// <summary>
    /// Node.js JavaScript执行器，用于在.NET应用中调用Node.js执行JavaScript代码
    /// </summary>
    public class NodeJsRunner : IDisposable
    {
        private readonly string _nodePath;
        private bool _disposed;

        /// <summary>
        /// 初始化NodeJsRunner实例
        /// </summary>
        /// <param name="nodePath">Node.js可执行文件路径，默认为"node"（系统PATH中查找）</param>
        public NodeJsRunner(string nodePath = "node")
        {
            _nodePath = nodePath;
            
            // 验证Node.js是否可用
            if (!IsNodeAvailable())
            {
                throw new InvalidOperationException("无法找到Node.js。请确保Node.js已安装并且在系统PATH中，或提供正确的路径。");
            }
        }

        /// <summary>
        /// 异步执行JavaScript代码
        /// </summary>
        /// <param name="javascriptCode">要执行的JavaScript代码字符串</param>
        /// <returns>包含执行结果的对象</returns>
        public async Task<JavaScriptExecutionResult> ExecuteScriptAsync(string javascriptCode)
        {
            if (string.IsNullOrEmpty(javascriptCode))
                throw new ArgumentNullException(nameof(javascriptCode));

            if (_disposed)
                throw new ObjectDisposedException(nameof(NodeJsRunner));

            // 创建临时文件来存储JavaScript代码
            string tempFilePath = null;
            try
            {
                tempFilePath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.js");
                await File.WriteAllTextAsync(tempFilePath, javascriptCode, Encoding.UTF8);

                return await ExecuteScriptFileAsync(tempFilePath);
            }
            finally
            {
                // 清理临时文件
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch { /* 忽略删除失败 */ }
                }
            }
        }

        /// <summary>
        /// 异步执行JavaScript文件
        /// </summary>
        /// <param name="filePath">JavaScript文件路径</param>
        /// <returns>包含执行结果的对象</returns>
        public async Task<JavaScriptExecutionResult> ExecuteScriptFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("JavaScript文件不存在", filePath);

            if (_disposed)
                throw new ObjectDisposedException(nameof(NodeJsRunner));

            var result = new JavaScriptExecutionResult();

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _nodePath,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                // 输出和错误收集
                StringBuilder outputBuilder = new StringBuilder();
                StringBuilder errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        errorBuilder.AppendLine(e.Data);
                };

                // 启动进程
                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // 等待进程完成
                    await Task.Run(() => process.WaitForExit());

                    // 设置结果
                    result.Success = process.ExitCode == 0;
                    result.Output = outputBuilder.ToString().Trim();
                    result.Error = errorBuilder.ToString().Trim();
                    result.ExitCode = process.ExitCode;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    result.ExitCode = -1;
                }
            }

            return result;
        }

        /// <summary>
        /// 同步执行JavaScript代码
        /// </summary>
        /// <param name="javascriptCode">要执行的JavaScript代码字符串</param>
        /// <returns>包含执行结果的对象</returns>
        public JavaScriptExecutionResult ExecuteScript(string javascriptCode)
        {
            return ExecuteScriptAsync(javascriptCode).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 同步执行JavaScript文件
        /// </summary>
        /// <param name="filePath">JavaScript文件路径</param>
        /// <returns>包含执行结果的对象</returns>
        public JavaScriptExecutionResult ExecuteScriptFile(string filePath)
        {
            return ExecuteScriptFileAsync(filePath).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 检查Node.js是否可用
        /// </summary>
        /// <returns>如果Node.js可用则返回true</returns>
        private bool IsNodeAvailable()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = _nodePath,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    process.Start();
                    process.WaitForExit(2000); // 2秒超时
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                // 这里可以释放任何需要释放的托管资源
            }
        }

        ~NodeJsRunner()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// JavaScript执行结果
    /// </summary>
    public class JavaScriptExecutionResult
    {
        /// <summary>
        /// 执行是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 执行输出
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// 进程退出代码
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// 获取完整结果信息
        /// </summary>
        /// <returns>包含所有结果信息的字符串</returns>
        public override string ToString()
        {
            return $"Success: {Success}\nExitCode: {ExitCode}\nOutput: {Output}\nError: {Error}";
        }
    }
}