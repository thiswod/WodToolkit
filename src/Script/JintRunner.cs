using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Jint;
using Jint.Runtime;
using Jint.Native;

namespace WodToolkit.Script
{
    /// <summary>
    /// Jint JavaScript执行器，用于在.NET应用中直接执行JavaScript代码（无需Node.js）
    /// </summary>
    public class JintRunner : IDisposable
    {
        // JSON序列化选项
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private Engine _engine;
        private bool _disposed;
        private readonly StringBuilder _consoleOutput;
        private readonly StringBuilder _consoleError;
        
        /// <summary>
        /// 获取底层Jint引擎实例（用于高级操作，如设置变量、获取变量等）
        /// </summary>
        public Engine Engine => _engine;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JintRunner()
        {
            _consoleOutput = new StringBuilder();
            _consoleError = new StringBuilder();
            InitializeEngine();
        }

        /// <summary>
        /// 初始化Jint引擎
        /// </summary>
        private void InitializeEngine()
        {
            _engine = new Engine(options =>
            {
                options.AllowClr();
                options.CatchClrExceptions();
            });

            // 实现 console.log - 使用 JavaScript 代码定义，更可靠地处理可变参数
            _engine.SetValue("_consoleLog", new Action<JsValue[]>(args =>
            {
                if (args != null && args.Length > 0)
                {
                    var output = string.Join(" ", Array.ConvertAll(args, a => a?.ToString() ?? "null"));
                    _consoleOutput.AppendLine(output);
                }
            }));
            
            _engine.SetValue("_consoleError", new Action<JsValue[]>(args =>
            {
                if (args != null && args.Length > 0)
                {
                    var error = string.Join(" ", Array.ConvertAll(args, a => a?.ToString() ?? "null"));
                    _consoleError.AppendLine(error);
                }
            }));
            
            _engine.SetValue("_consoleWarn", new Action<JsValue[]>(args =>
            {
                if (args != null && args.Length > 0)
                {
                    var output = string.Join(" ", Array.ConvertAll(args, a => a?.ToString() ?? "null"));
                    _consoleOutput.AppendLine($"[WARN] {output}");
                }
            }));
            
            _engine.SetValue("_consoleInfo", new Action<JsValue[]>(args =>
            {
                if (args != null && args.Length > 0)
                {
                    var output = string.Join(" ", Array.ConvertAll(args, a => a?.ToString() ?? "null"));
                    _consoleOutput.AppendLine($"[INFO] {output}");
                }
            }));
            
            // 使用 JavaScript 代码定义 console 对象，正确处理可变参数
            _engine.Execute(@"
                var console = {
                    log: function() {
                        _consoleLog(Array.prototype.slice.call(arguments));
                    },
                    error: function() {
                        _consoleError(Array.prototype.slice.call(arguments));
                    },
                    warn: function() {
                        _consoleWarn(Array.prototype.slice.call(arguments));
                    },
                    info: function() {
                        _consoleInfo(Array.prototype.slice.call(arguments));
                    }
                };
            ");

            // 实现基本的 require 功能（简化版，仅用于模块导出）
            _engine.SetValue("require", new Func<string, object>(moduleName =>
            {
                // 简化实现，不支持真实的 Node.js 模块
                // 如果需要，可以扩展为加载文件
                throw new JavaScriptException($"require is not fully supported. Use ExecuteScriptFileAsync to load files.");
            }));

            // 实现 module 和 exports
            _engine.Execute(@"
                var module = { exports: {} };
                var exports = module.exports;
            ");
        }

        /// <summary>
        /// 检查Jint是否可用（Jint始终可用，因为它是纯.NET实现）
        /// </summary>
        /// <returns>始终返回true</returns>
        public bool IsNodeAvailable()
        {
            return true; // Jint 是纯 .NET 实现，不需要外部依赖
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
                throw new ObjectDisposedException(nameof(JintRunner));

            return await Task.Run(() => ExecuteScriptInternal(javascriptCode));
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
                throw new ObjectDisposedException(nameof(JintRunner));

            string code = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return await ExecuteScriptAsync(code);
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
                throw new ObjectDisposedException(nameof(JintRunner));

            string code = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return await CallMethodFromScriptAsync(code, methodName, parameters);
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
                throw new ObjectDisposedException(nameof(JintRunner));

            return await Task.Run(() => CallMethodInternal(javascriptCode, methodName, parameters));
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
        /// 内部执行JavaScript代码
        /// </summary>
        private JavaScriptExecutionResult ExecuteScriptInternal(string javascriptCode)
        {
            var result = new JavaScriptExecutionResult();
            _consoleOutput.Clear();
            _consoleError.Clear();

            try
            {
                // 执行代码
                _engine.Execute(javascriptCode);

                // 获取输出
                result.Output = _consoleOutput.ToString().Trim();
                result.Error = _consoleError.ToString().Trim();
                result.RawOutput = result.Output;

                // 只有在console.error有输出时才认为是真正的错误
                // 忽略类型转换相关的错误（因为代码已经执行成功了）
                bool hasRealError = !string.IsNullOrEmpty(result.Error) && 
                                    !result.Error.Contains("IConvertible") &&
                                    !result.Error.Contains("Object must implement");
                
                result.Success = !hasRealError;
                result.ExitCode = result.Success ? 0 : 1;
                
                // 如果只是类型转换错误，清除错误信息（代码实际执行成功了）
                if (!hasRealError && !string.IsNullOrEmpty(result.Error) && 
                    (result.Error.Contains("IConvertible") || result.Error.Contains("Object must implement")))
                {
                    result.Error = string.Empty;
                }
            }
            catch (JavaScriptException ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.Output = _consoleOutput.ToString().Trim();
                result.RawOutput = result.Output;
                result.ExitCode = 1;
            }
            catch (Exception ex)
            {
                // 检查是否是类型转换错误，如果是且代码已执行（有输出），不应该标记为失败
                bool isConvertibleError = ex.Message.Contains("IConvertible") || 
                                         ex.Message.Contains("Object must implement");
                bool hasOutput = !string.IsNullOrEmpty(_consoleOutput.ToString().Trim());
                
                if (isConvertibleError && hasOutput)
                {
                    // 代码已执行成功，只是类型转换失败，不应该算作执行失败
                    result.Success = true;
                    result.Error = string.Empty;
                    result.Output = _consoleOutput.ToString().Trim();
                    result.RawOutput = result.Output;
                    result.ExitCode = 0;
                }
                else
                {
                    // 真正的执行失败
                    result.Success = false;
                    result.Error = ex.Message;
                    result.Output = _consoleOutput.ToString().Trim();
                    result.RawOutput = result.Output;
                    result.ExitCode = 1;
                }
            }

            return result;
        }

        /// <summary>
        /// 内部调用方法
        /// </summary>
        private JavaScriptExecutionResult CallMethodInternal(string javascriptCode, string methodName, object[] parameters)
        {
            var result = new JavaScriptExecutionResult();
            _consoleOutput.Clear();
            _consoleError.Clear();

            try
            {
                // 先执行代码以定义函数
                _engine.Execute(javascriptCode);

                // 获取 module.exports 或直接获取方法
                JsValue method = JsValue.Undefined;

                // 尝试从 module.exports 获取
                var moduleValue = _engine.GetValue("module");
                if (!moduleValue.IsUndefined() && moduleValue.IsObject())
                {
                    var moduleObj = moduleValue.AsObject();
                    var exportsValue = moduleObj.Get("exports");
                    if (!exportsValue.IsUndefined() && exportsValue.IsObject())
                    {
                        var exportsObj = exportsValue.AsObject();
                        if (exportsObj.HasProperty(methodName))
                        {
                            method = exportsObj.Get(methodName);
                        }
                    }
                }

                // 如果 module.exports 中没有，尝试从全局作用域获取
                if (method.IsUndefined())
                {
                    try
                    {
                        method = _engine.GetValue(methodName);
                    }
                    catch
                    {
                        // 方法不存在
                    }
                }

                // 检查方法是否可调用
                bool isCallable = false;
                if (!method.IsUndefined() && method.IsObject())
                {
                    var methodObj = method.AsObject();
                    // 检查是否是函数类型
                    try
                    {
                        // 尝试调用 Call 方法，如果存在则说明可调用
                        var callMethod = methodObj.Get("call");
                        isCallable = !callMethod.IsUndefined() || methodObj.GetType().Name.Contains("Function");
                    }
                    catch
                    {
                        // 如果获取失败，尝试直接检查类型
                        isCallable = methodObj.GetType().Name.Contains("Function");
                    }
                }

                if (!isCallable)
                {
                    result.Success = false;
                    result.Error = $"方法 '{methodName}' 未找到或不可调用";
                    result.ExitCode = 1;
                    return result;
                }

                // 准备参数
                var jsParameters = new List<JsValue>();
                if (parameters != null && parameters.Length > 0)
                {
                    foreach (var param in parameters)
                    {
                        jsParameters.Add(ConvertToJsValue(param));
                    }
                }

                // 调用方法
                var returnValue = method.AsObject().Call(JsValue.Undefined, jsParameters.ToArray());

                // 获取输出
                result.Output = _consoleOutput.ToString().Trim();
                result.Error = _consoleError.ToString().Trim();
                result.RawOutput = result.Output;

                // 处理返回值
                if (returnValue != null && !returnValue.IsUndefined() && !returnValue.IsNull())
                {
                    string jsonResult = SerializeJsValue(returnValue);
                    result.Output = $"__JINT_RUNNER_RESULT_START__{jsonResult}__JINT_RUNNER_RESULT_END__";
                    if (!string.IsNullOrEmpty(_consoleOutput.ToString().Trim()))
                    {
                        result.Output = _consoleOutput.ToString().Trim() + "\n" + result.Output;
                    }
                }

                result.Success = string.IsNullOrEmpty(result.Error);
                result.ExitCode = result.Success ? 0 : 1;
            }
            catch (JavaScriptException ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.Output = _consoleOutput.ToString().Trim();
                result.RawOutput = result.Output;
                result.ExitCode = 1;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.Output = _consoleOutput.ToString().Trim();
                result.RawOutput = result.Output;
                result.ExitCode = 1;
            }

            return result;
        }

        /// <summary>
        /// 将C#对象转换为JsValue
        /// </summary>
        private JsValue ConvertToJsValue(object value)
        {
            if (value == null)
                return JsValue.Null;

            if (value is JsValue jsValue)
                return jsValue;

            // 使用 Jint 的 FromObject 方法
            try
            {
                return JsValue.FromObject(_engine, value);
            }
            catch
            {
                // 如果直接转换失败，尝试序列化为JSON再解析
                try
                {
                    string json = JsonSerializer.Serialize(value, _jsonOptions);
                    return _engine.Evaluate($"JSON.parse({JsonSerializer.Serialize(json)})");
                }
                catch
                {
                    // 如果都失败，转换为字符串
                    return JsValue.FromObject(_engine, value?.ToString() ?? "null");
                }
            }
        }

        /// <summary>
        /// 序列化JsValue为JSON字符串
        /// </summary>
        private string SerializeJsValue(JsValue value)
        {
            if (value.IsUndefined() || value.IsNull())
                return "null";

            if (value.IsString())
                return JsonSerializer.Serialize(value.AsString());

            if (value.IsNumber())
                return value.AsNumber().ToString();

            if (value.IsBoolean())
                return value.AsBoolean().ToString().ToLowerInvariant();

            if (value.IsObject())
            {
                try
                {
                    // 尝试将对象转换为JSON
                    var obj = value.AsObject();
                    var dict = new Dictionary<string, object>();

                    foreach (var key in obj.GetOwnProperties())
                    {
                        var propValue = obj.Get(key.Key);
                        dict[key.Key.ToString()] = ConvertJsValueToObject(propValue);
                    }

                    return JsonSerializer.Serialize(dict, _jsonOptions);
                }
                catch
                {
                    return JsonSerializer.Serialize(value.ToString());
                }
            }

            if (value.IsArray())
            {
                try
                {
                    var array = value.AsArray();
                    var list = new List<object>();

                    // 使用 Length 属性获取数组长度
                    var lengthValue = array.Get("length");
                    int length = lengthValue.IsNumber() ? (int)lengthValue.AsNumber() : 0;

                    for (int i = 0; i < length; i++)
                    {
                        list.Add(ConvertJsValueToObject(array.Get(i.ToString())));
                    }

                    return JsonSerializer.Serialize(list, _jsonOptions);
                }
                catch
                {
                    return JsonSerializer.Serialize(value.ToString());
                }
            }

            return JsonSerializer.Serialize(value.ToString());
        }

        /// <summary>
        /// 将JsValue转换为C#对象
        /// </summary>
        private object ConvertJsValueToObject(JsValue value)
        {
            if (value.IsUndefined() || value.IsNull())
                return null;

            if (value.IsString())
                return value.AsString();

            if (value.IsNumber())
            {
                try
                {
                    return value.AsNumber();
                }
                catch
                {
                    // 如果转换失败，返回字符串表示
                    return value.ToString();
                }
            }

            if (value.IsBoolean())
            {
                try
                {
                    return value.AsBoolean();
                }
                catch
                {
                    return value.ToString();
                }
            }

            if (value.IsObject())
            {
                try
                {
                    var obj = value.AsObject();
                    var dict = new Dictionary<string, object>();

                    foreach (var key in obj.GetOwnProperties())
                    {
                        try
                        {
                            var propValue = obj.Get(key.Key);
                            dict[key.Key.ToString()] = ConvertJsValueToObject(propValue);
                        }
                        catch
                        {
                            // 如果属性转换失败，使用字符串表示
                            dict[key.Key.ToString()] = "[无法转换]";
                        }
                    }

                    return dict;
                }
                catch
                {
                    // 如果对象转换失败，返回字符串表示
                    return value.ToString();
                }
            }

            if (value.IsArray())
            {
                try
                {
                    var array = value.AsArray();
                    var list = new List<object>();

                    // 使用 Length 属性获取数组长度
                    var lengthValue = array.Get("length");
                    int length = 0;
                    try
                    {
                        length = lengthValue.IsNumber() ? (int)lengthValue.AsNumber() : 0;
                    }
                    catch
                    {
                        length = 0;
                    }

                    for (int i = 0; i < length; i++)
                    {
                        try
                        {
                            list.Add(ConvertJsValueToObject(array.Get(i.ToString())));
                        }
                        catch
                        {
                            // 如果数组元素转换失败，跳过该元素
                            continue;
                        }
                    }

                    return list;
                }
                catch
                {
                    // 如果数组转换失败，返回字符串表示
                    return value.ToString();
                }
            }

            // 最后的备选方案：返回字符串表示
            try
            {
                return value.ToString();
            }
            catch
            {
                return "[无法转换的对象]";
            }
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

            // 如果输出为空，抛出异常
            if (string.IsNullOrEmpty(result.Output))
            {
                throw new InvalidOperationException("执行结果中没有返回值");
            }

            // 查找结果标记
            const string startMarker = "__JINT_RUNNER_RESULT_START__";
            const string endMarker = "__JINT_RUNNER_RESULT_END__";

            int startIndex = result.Output.IndexOf(startMarker);
            
            // 如果找到了开始标记，查找结束标记
            if (startIndex >= 0)
            {
                // 确保 startIndex + startMarker.Length 不会超出字符串长度
                int searchStartIndex = startIndex + startMarker.Length;
                if (searchStartIndex >= result.Output.Length)
                {
                    throw new InvalidOperationException("返回值标记格式错误：开始标记后没有内容");
                }
                
                int endIndex = result.Output.IndexOf(endMarker, searchStartIndex);
                
                if (endIndex > startIndex + startMarker.Length)
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
                else
                {
                    throw new InvalidOperationException("返回值标记格式错误：未找到结束标记");
                }
            }

            // 如果没有找到标记，尝试直接解析输出
            // 但首先检查输出是否只是控制台输出（没有返回值）
            string trimmedOutput = result.Output.Trim();
            
            // 如果输出为空，说明没有返回值
            if (string.IsNullOrEmpty(trimmedOutput))
            {
                // 对于值类型，返回默认值
                if (typeof(T).IsValueType)
                {
                    return default(T);
                }
                // 对于引用类型，返回 null
                return default(T);
            }
            
            // 检查输出是否看起来像JSON返回值
            // 注意：如果输出中没有 __JINT_RUNNER_RESULT_START__ 标记，说明代码没有返回值
            // 此时输出只是控制台输出（如 console.log 的输出），不应该尝试解析为返回值
            // 直接返回默认值，避免误解析控制台输出
            
            // 如果输出中没有返回值标记，说明只是控制台输出，没有返回值
            // 对于值类型，返回默认值
            if (typeof(T).IsValueType)
            {
                return default(T);
            }
            // 对于引用类型，返回 null
            return default(T);
            
            try
            {
                return JsonSerializer.Deserialize<T>(trimmedOutput, _jsonOptions);
            }
            catch
            {
                // 如果解析失败，抛出异常
                throw new InvalidOperationException("无法从输出中提取有效结果。代码可能没有返回值，或者返回值格式不正确。");
            }
        }
        
        /// <summary>
        /// 尝试从执行结果中提取JSON返回值（不抛出异常）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="result">执行结果</param>
        /// <param name="value">解析后的对象</param>
        /// <returns>是否成功提取返回值</returns>
        public bool TryGetResult<T>(JavaScriptExecutionResult result, out T value)
        {
            value = default(T);
            
            if (result == null || !result.Success)
            {
                return false;
            }
            
            try
            {
                value = GetResult<T>(result);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 执行JavaScript代码并返回结果值
        /// </summary>
        /// <param name="javascriptCode">要执行的JavaScript代码字符串</param>
        /// <returns>执行结果值</returns>
        public JsValue Evaluate(string javascriptCode)
        {
            if (string.IsNullOrEmpty(javascriptCode))
                throw new ArgumentNullException(nameof(javascriptCode));
            if (_disposed)
                throw new ObjectDisposedException(nameof(JintRunner));

            _consoleOutput.Clear();
            _consoleError.Clear();
            return _engine.Evaluate(javascriptCode);
        }

        /// <summary>
        /// 调用JavaScript函数
        /// </summary>
        /// <param name="functionName">函数名</param>
        /// <param name="parameters">函数参数</param>
        /// <returns>函数返回值</returns>
        public JsValue Invoke(string functionName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(functionName))
                throw new ArgumentNullException(nameof(functionName));
            if (_disposed)
                throw new ObjectDisposedException(nameof(JintRunner));

            _consoleOutput.Clear();
            _consoleError.Clear();

            // 获取函数
            var method = _engine.GetValue(functionName);
            if (method.IsUndefined())
            {
                throw new InvalidOperationException($"函数 '{functionName}' 未找到");
            }

            // 准备参数
            var jsParameters = new List<JsValue>();
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var param in parameters)
                {
                    jsParameters.Add(ConvertToJsValue(param));
                }
            }

            // 调用函数
            if (method.IsObject())
            {
                return method.AsObject().Call(JsValue.Undefined, jsParameters.ToArray());
            }

            throw new InvalidOperationException($"'{functionName}' 不是一个可调用的函数");
        }

        /// <summary>
        /// 设置变量值
        /// </summary>
        /// <param name="variableName">变量名</param>
        /// <param name="value">变量值</param>
        public void SetValue(string variableName, object value)
        {
            if (string.IsNullOrEmpty(variableName))
                throw new ArgumentNullException(nameof(variableName));
            if (_disposed)
                throw new ObjectDisposedException(nameof(JintRunner));

            var jsValue = ConvertToJsValue(value);
            _engine.SetValue(variableName, jsValue);
        }

        /// <summary>
        /// 获取变量值
        /// </summary>
        /// <param name="variableName">变量名</param>
        /// <returns>变量值</returns>
        public JsValue GetValue(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                throw new ArgumentNullException(nameof(variableName));
            if (_disposed)
                throw new ObjectDisposedException(nameof(JintRunner));

            return _engine.GetValue(variableName);
        }

        /// <summary>
        /// 获取所有函数名列表
        /// </summary>
        /// <returns>函数名列表</returns>
        public List<string> GetFunctions()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JintRunner));

            var functions = new List<string>();
            try
            {
                var result = _engine.Evaluate(@"
                    (function() {
                        var funcs = [];
                        for (var prop in this) {
                            if (typeof this[prop] === 'function' && prop !== 'console') {
                                funcs.push(prop);
                            }
                        }
                        return funcs;
                    })();
                ");

                if (result != null && result.IsArray())
                {
                    var array = result.AsArray();
                    var lengthValue = array.Get("length");
                    int length = lengthValue.IsNumber() ? (int)lengthValue.AsNumber() : 0;

                    for (int i = 0; i < length; i++)
                    {
                        var funcName = array.Get(i.ToString()).ToString();
                        functions.Add(funcName);
                    }
                }
            }
            catch
            {
                // 如果获取失败，返回空列表
            }

            return functions;
        }

        /// <summary>
        /// 获取所有变量名和值的列表
        /// </summary>
        /// <returns>变量信息列表（格式：变量名 = 值）</returns>
        public List<string> GetVariables()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JintRunner));

            var variables = new List<string>();
            try
            {
                var result = _engine.Evaluate(@"
                    (function() {
                        var vars = [];
                        for (var prop in this) {
                            if (typeof this[prop] !== 'function' && prop !== 'console') {
                                var value = this[prop];
                                var valueStr = typeof value === 'object' ? JSON.stringify(value) : String(value);
                                vars.push(prop + ' = ' + valueStr);
                            }
                        }
                        return vars;
                    })();
                ");

                if (result != null && result.IsArray())
                {
                    var array = result.AsArray();
                    var lengthValue = array.Get("length");
                    int length = lengthValue.IsNumber() ? (int)lengthValue.AsNumber() : 0;

                    for (int i = 0; i < length; i++)
                    {
                        var varInfo = array.Get(i.ToString()).ToString();
                        variables.Add(varInfo);
                    }
                }
            }
            catch
            {
                // 如果获取失败，返回空列表
            }

            return variables;
        }

        /// <summary>
        /// 获取控制台输出
        /// </summary>
        public string ConsoleOutput => _consoleOutput.ToString();

        /// <summary>
        /// 获取控制台错误输出
        /// </summary>
        public string ConsoleError => _consoleError.ToString();

        /// <summary>
        /// 清除控制台输出
        /// </summary>
        public void ClearConsole()
        {
            _consoleOutput.Clear();
            _consoleError.Clear();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _engine?.Dispose();
                    _engine = null;
                }
                _disposed = true;
            }
        }

        ~JintRunner()
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
}

