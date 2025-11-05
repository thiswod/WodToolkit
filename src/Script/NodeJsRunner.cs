using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.Json;

namespace WodToolkit.Script
{
    /// <summary>
    /// Node.js JavaScript执行器，用于在.NET应用中调用Node.js执行JavaScript代码
    /// </summary>
    public class NodeJsRunner : IDisposable
    {
        // JSON序列化选项
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private readonly string _nodePath;
        private bool _disposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodePath">Node.js可执行文件路径，如果为null则使用系统环境变量中的node</param>
        public NodeJsRunner(string nodePath = null)
        {
            _nodePath = string.IsNullOrEmpty(nodePath) ? "node" : nodePath;
            
            // 验证Node.js是否可用
            if (!IsNodeAvailable())
            {
                throw new InvalidOperationException($"无法找到或启动Node.js。请确保Node.js已正确安装，且路径 '{_nodePath}' 有效。");
            }
        }

        /// <summary>
        /// 检查Node.js是否可用
        /// </summary>
        /// <returns>如果Node.js可用则返回true，否则返回false</returns>
        public bool IsNodeAvailable()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = _nodePath;
                    process.StartInfo.Arguments = "--version";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    return process.WaitForExit(2000);
                }
            }
            catch
            {
                return false;
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

            // 创建临时文件
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"temp_script_{Guid.NewGuid()}.js");
            try
            {
                // 写入代码到临时文件
                await File.WriteAllTextAsync(tempFilePath, javascriptCode, Encoding.UTF8);
                
                // 执行临时文件
                return await ExecuteScriptFileAsync(tempFilePath);
            }
            finally
            {
                // 清理临时文件
                if (File.Exists(tempFilePath))
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
        /// 同步执行JavaScript代码
        /// </summary>
        /// <param name="javascriptCode">要执行的JavaScript代码字符串</param>
        /// <returns>包含执行结果的对象</returns>
        public JavaScriptExecutionResult ExecuteScript(string javascriptCode)
        {
            return ExecuteScriptAsync(javascriptCode).GetAwaiter().GetResult();
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
                // 配置进程信息
                process.StartInfo.FileName = _nodePath;
                process.StartInfo.Arguments = $"\"{filePath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                // 收集输出和错误
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                try
                {
                    // 启动进程
                    process.Start();
                    
                    // 开始异步读取输出
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    
                    // 等待进程完成
                    await Task.Run(() => process.WaitForExit());
                    
                    // 等待所有输出读取完成
                    process.WaitForExit();
                    
                    // 设置结果
                    result.ExitCode = process.ExitCode;
                    result.Success = process.ExitCode == 0;
                    result.Output = outputBuilder.ToString().Trim();
                    result.Error = errorBuilder.ToString().Trim();
                    result.RawOutput = result.Output;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                }
            }

            return result;
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
        /// 异步调用JavaScript文件中的方法
        /// </summary>
        /// <param name="filePath">JavaScript文件路径</param>
        /// <param name="methodName">要调用的方法名</param>
        /// <param name="parameters">传递给方法的参数</param>
        /// <returns>包含执行结果的对象</returns>
        public async Task<JavaScriptExecutionResult> CallMethodAsync(string filePath, string methodName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentNullException(nameof(methodName));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("JavaScript文件不存在", filePath);
            if (_disposed)
                throw new ObjectDisposedException(nameof(NodeJsRunner));

            // 生成调用方法的包装代码
            string wrappedCode = GenerateMethodCallCode(filePath, methodName, parameters);

            // 创建临时文件来执行
            string tempFilePath = null;
            try
            {
                tempFilePath = Path.Combine(Path.GetTempPath(), $"temp_method_call_{Guid.NewGuid()}.js");
                await File.WriteAllTextAsync(tempFilePath, wrappedCode, Encoding.UTF8);

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
        /// 异步调用JavaScript代码中的方法
        /// </summary>
        /// <param name="javascriptCode">JavaScript代码字符串</param>
        /// <param name="methodName">要调用的方法名</param>
        /// <param name="parameters">传递给方法的参数</param>
        /// <returns>包含执行结果的对象</returns>
        public async Task<JavaScriptExecutionResult> CallMethodFromScriptAsync(string javascriptCode, string methodName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(javascriptCode))
                throw new ArgumentNullException(nameof(javascriptCode));
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentNullException(nameof(methodName));
            if (_disposed)
                throw new ObjectDisposedException(nameof(NodeJsRunner));

            // 创建临时文件存储代码
            string tempScriptPath = null;
            try
            {
                tempScriptPath = Path.Combine(Path.GetTempPath(), $"temp_script_{Guid.NewGuid()}.js");
                await File.WriteAllTextAsync(tempScriptPath, javascriptCode, Encoding.UTF8);

                return await CallMethodAsync(tempScriptPath, methodName, parameters);
            }
            finally
            {
                // 清理临时文件
                if (tempScriptPath != null && File.Exists(tempScriptPath))
                {
                    try
                    {
                        File.Delete(tempScriptPath);
                    }
                    catch { /* 忽略删除失败 */ }
                }
            }
        }

        /// <summary>
        /// 同步调用JavaScript文件中的方法
        /// </summary>
        /// <param name="filePath">JavaScript文件路径</param>
        /// <param name="methodName">要调用的方法名</param>
        /// <param name="parameters">传递给方法的参数</param>
        /// <returns>包含执行结果的对象</returns>
        public JavaScriptExecutionResult CallMethod(string filePath, string methodName, params object[] parameters)
        {
            return CallMethodAsync(filePath, methodName, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 同步调用JavaScript代码中的方法
        /// </summary>
        /// <param name="javascriptCode">JavaScript代码字符串</param>
        /// <param name="methodName">要调用的方法名</param>
        /// <param name="parameters">传递给方法的参数</param>
        /// <returns>包含执行结果的对象</returns>
        public JavaScriptExecutionResult CallMethodFromScript(string javascriptCode, string methodName, params object[] parameters)
        {
            return CallMethodFromScriptAsync(javascriptCode, methodName, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 生成调用方法的包装代码
        /// </summary>
        private string GenerateMethodCallCode(string filePath, string methodName, object[] parameters)
        {
            // 序列化参数为JSON
            string parametersJson = parameters != null && parameters.Length > 0
                ? string.Join(", ", Array.ConvertAll(parameters, p => JsonSerializer.Serialize(p, _jsonOptions)))
                : "";

            // 生成包装代码
            return $@"// 包装代码：加载模块并调用方法
const fs = require('fs');
const path = require('path');

// 加载目标文件
let moduleExports = {{}};
try {{
    // 尝试使用require加载
    moduleExports = require('{EscapePath(filePath)}');
}} catch (e) {{
    // 如果require失败，尝试读取文件内容并执行
    const scriptContent = fs.readFileSync('{EscapePath(filePath)}', 'utf8');
    (function() {{
        // 创建一个安全的执行环境
        const globalContext = {{
            console: console,
            require: require,
            module: {{ exports: {{}} }},
            exports: {{}}
        }};
        
        // 执行脚本
        (new Function('console', 'require', 'module', 'exports', scriptContent))(
            globalContext.console,
            globalContext.require,
            globalContext.module,
            globalContext.exports
        );
        
        // 获取导出
        moduleExports = globalContext.module.exports || globalContext.exports;
    }})();
}}

// 查找方法
let targetMethod;
try {{
    // 尝试从exports中获取
    if (typeof moduleExports === 'function') {{
        targetMethod = moduleExports;
    }} else if (typeof moduleExports === 'object' && moduleExports !== null) {{
        targetMethod = moduleExports['{EscapeJavaScriptIdentifier(methodName)}'] || 
                     moduleExports.{methodName};
    }}
    
    if (typeof targetMethod !== 'function') {{
        throw new Error(`未找到方法 '{methodName}'`);
    }}
    
    // 调用方法
    const result = targetMethod({parametersJson});
    
    // 处理Promise结果
    if (result && typeof result.then === 'function') {{
        // 处理异步方法
        result.then(function(asyncResult) {{
            try {{
                // 序列化结果
                const jsonResult = JSON.stringify(asyncResult);
                // 使用特殊标记包裹结果，便于解析
                console.log('__NODEJS_RUNNER_RESULT_START__' + jsonResult + '__NODEJS_RUNNER_RESULT_END__');
            }} catch (e) {{
                console.error('__NODEJS_RUNNER_ERROR__' + e.message);
            }}
        }}).catch(function(error) {{
            console.error('__NODEJS_RUNNER_ERROR__' + error.message);
        }});
        // 等待Promise完成
        setTimeout(() => {{}}, 1000);
    }} else {{
        // 处理同步结果
        try {{
            // 序列化结果
            const jsonResult = JSON.stringify(result);
            // 使用特殊标记包裹结果，便于解析
            console.log('__NODEJS_RUNNER_RESULT_START__' + jsonResult + '__NODEJS_RUNNER_RESULT_END__');
        }} catch (e) {{
            console.error('__NODEJS_RUNNER_ERROR__' + e.message);
        }}
    }}
}} catch (error) {{
    console.error('__NODEJS_RUNNER_ERROR__' + error.message);
}}";
        }

        /// <summary>
        /// 转义文件路径中的特殊字符
        /// </summary>
        private string EscapePath(string path)
        {
            return path.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        /// <summary>
        /// 转义JavaScript标识符
        /// </summary>
        private string EscapeJavaScriptIdentifier(string identifier)
        {
            // 如果标识符包含特殊字符，使用引号包裹
            if (!System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[a-zA-Z_$][a-zA-Z0-9_$]*$"))
            {
                return '"' + identifier.Replace('"', '\"').Replace("\n", "\\n") + '"';
            }
            return identifier;
        }

        /// <summary>
        /// 从执行结果中提取JSON返回值
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="result">执行结果</param>
        /// <returns>解析后的对象</returns>
        public T GetResult<T>(JavaScriptExecutionResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (!result.Success)
                throw new InvalidOperationException($"执行失败: {result.Error}");

            // 查找结果标记
            const string startMarker = "__NODEJS_RUNNER_RESULT_START__";
            const string endMarker = "__NODEJS_RUNNER_RESULT_END__";

            int startIndex = result.Output.IndexOf(startMarker);
            int endIndex = result.Output.IndexOf(endMarker, startIndex + startMarker.Length);

            if (startIndex >= 0 && endIndex > startIndex + startMarker.Length)
            {
                string jsonResult = result.Output.Substring(
                    startIndex + startMarker.Length,
                    endIndex - (startIndex + startMarker.Length)
                );

                try
                {
                    return JsonSerializer.Deserialize<T>(jsonResult, _jsonOptions);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("无法解析返回结果为指定类型", ex);
                }
            }

            // 如果没有找到标记，尝试直接解析输出
            try
            {
                return JsonSerializer.Deserialize<T>(result.Output, _jsonOptions);
            }
            catch
            {
                // 如果解析失败，抛出异常
                throw new InvalidOperationException("无法从输出中提取有效结果");
            }
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

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
        /// 原始输出（未处理的控制台输出）
        /// </summary>
        public string RawOutput { get; set; }

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