# WodToolKit

[English Version](docs/README_EN.md) | 中文版本

轻量级.NET工具库，提供各类常用功能的封装，旨在简化开发工作，提高开发效率。已实现HTTP请求处理、Cookie管理、JSON解析、内存缓存、线程池、AES加密、JavaScript执行、身份证验证等功能。

## 功能特性

- **HTTP请求处理**：简化HTTP客户端操作，支持各种HTTP方法和请求配置，支持HTTP/HTTPS和SOCKS4/SOCKS5代理
- **Cookie管理**：完整的Cookie管理功能，支持添加、获取、删除和批量操作
- **JSON解析**：灵活的JSON序列化和反序列化，支持动态类型和自定义类型
- **URL工具**：URL参数处理、排序和转换工具
- **AES加密**：安全的AES加密和解密功能，支持多种加密模式和填充方式
- **内存缓存**：基于内存的临时缓存实现，支持TTL设置和自动清理
- **线程池管理**：简单高效的线程池实现，支持任务队列和任务等待
- **JavaScript执行**：支持两种执行方式，JintRunner（纯.NET实现，无需Node.js）和NodeJsRunner（需要Node.js），支持代码字符串和文件执行、方法调用、变量管理、函数调用等丰富功能
- **身份证验证**：提供中国身份证号码验证、地址提取、性别识别等功能，支持18位身份证号码的完整校验
- **.NET Standard 2.1兼容**：支持.NET Core、.NET Framework和其他兼容平台
- **模块化设计**：各功能模块相互独立，便于扩展和维护
- **持续更新**：计划逐步添加更多常用功能模块

## 安装

通过NuGet包管理器安装：

```powershell
Install-Package WodToolKit
```

或者使用.NET CLI：

```powershell
dotnet add package WodToolKit
```

## 快速开始

### 内存缓存示例

```csharp
using WodToolkit.src.Cache;

// 创建临时缓存实例（每30秒清理一次，默认TTL为300秒）
var cache = new TempCache<string, string>(TimeSpan.FromSeconds(30), 300);

// 设置缓存项
cache.Set("key1", "value1");
cache.Set("key2", "value2", 60); // 自定义TTL为60秒

// 获取缓存项
if (cache.TryGetValue("key1", out string value))
{
    Console.WriteLine($"缓存值: {value}");
}

// 移除缓存项
cache.Remove("key2");

// 清空缓存
// cache.Clear();

// 使用完毕后释放资源
// cache.Dispose();
```

### 线程池使用示例

```csharp
using WodToolkit.src.Thread;

// 创建线程池（4个工作线程）
var threadPool = new SimpleThreadPool(4);

// 添加任务到线程池
for (int i = 0; i < 10; i++)
{
    int taskId = i;
    threadPool.QueueTask(() => {
        Console.WriteLine($"执行任务 {taskId}, 线程ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        System.Threading.Thread.Sleep(100); // 模拟工作
    });
}

// 等待所有任务完成
threadPool.Wait();

// 释放线程池资源
threadPool.Dispose();

Console.WriteLine("所有任务执行完毕");
```

### HTTP请求示例

```csharp
using WodToolkit.Http;
using System.Net;
using System.Collections.Generic;

// 1. 发送GET请求
var httpRequest = new HttpRequestClass();
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
var responseData = httpRequest.GetResponse();

// 处理响应
if (responseData.StatusCode == 200)
{
    Console.WriteLine(responseData.Body); // 响应内容
    Console.WriteLine(responseData.StatusCode); // 状态码
    Console.WriteLine(responseData.ResponseHeaders); // 响应头
}

// 2. 发送带查询参数的GET请求
var getWithParams = new HttpRequestClass();
getWithParams.Open("https://api.example.com/search?keyword=test&page=1", HttpMethod.Get).Send();
var searchResponse = getWithParams.GetResponse();

// 3. 发送POST请求（表单数据）
var postRequest = new HttpRequestClass();
// 创建表单数据
var formData = new Dictionary<string, string>
{
    { "username", "admin" },
    { "password", "password123" }
};
// 发送POST请求
postRequest.Open("https://api.example.com/login", HttpMethod.Post).Send(formData);
var loginResponse = postRequest.GetResponse();

// 4. 发送JSON数据的POST请求
var jsonRequest = new HttpRequestClass();
// 直接使用匿名对象发送JSON请求（会自动设置Content-Type为application/json）
// 发送请求
jsonRequest.Open("https://api.example.com/users", HttpMethod.Post).Send(new 
{
    name = "测试",
    age = 25,
});
var userResponse = jsonRequest.GetResponse();

// 5. 发送异步请求示例
var asyncRequest = new HttpRequestClass();
// 设置超时时间
asyncRequest.SetTimeout(30); // 30秒
// 设置UserAgent
asyncRequest.SetUserAgent("Mozilla/5.0 WodToolkit");
// 异步发送请求
await asyncRequest.Open("https://api.example.com/data", HttpMethod.Get).SendAsync();
var asyncResponse = asyncRequest.GetResponse();

// 6. 使用Cookie管理器的请求
var cookieRequest = new HttpRequestClass();
// 设置Cookie
cookieRequest.SetCookieString("session=abc123; user=admin");
// 发送请求，会自动带上设置的Cookie
cookieRequest.Open("https://api.example.com/protected", HttpMethod.Get).Send();
var cookieResponse = cookieRequest.GetResponse();
// 获取响应中的Cookie
string cookies = cookieResponse.Cookie;
Console.WriteLine(cookies);

// 7. 使用HTTP代理的请求
var httpProxyRequest = new HttpRequestClass();
// 设置HTTP代理（原有方法，保持向后兼容）
httpProxyRequest.SetProxy("http://proxy.example.com:8080", "username", "password");
httpProxyRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
var httpProxyResponse = httpProxyRequest.GetResponse();

// 8. 使用SOCKS5代理的请求
var socks5Request = new HttpRequestClass();
// 设置SOCKS5代理（新方法，支持所有代理类型）
socks5Request.SetProxy(ProxyType.Socks5, "proxy.example.com", 1080, "username", "password");
socks5Request.Open("https://api.example.com/data", HttpMethod.Get).Send();
var socks5Response = socks5Request.GetResponse();

// 9. 使用SOCKS4代理的请求
var socks4Request = new HttpRequestClass();
// 设置SOCKS4代理（无需用户名密码）
socks4Request.SetProxy(ProxyType.Socks4, "proxy.example.com", 1080);
socks4Request.Open("https://api.example.com/data", HttpMethod.Get).Send();
var socks4Response = socks4Request.GetResponse();

// 10. 使用HTTP代理（通过新方法）
var httpProxyRequest2 = new HttpRequestClass();
// 新方法也支持HTTP代理
httpProxyRequest2.SetProxy(ProxyType.Http, "proxy.example.com", 8080, "username", "password");
httpProxyRequest2.Open("https://api.example.com/data", HttpMethod.Get).Send();
var httpProxyResponse2 = httpProxyRequest2.GetResponse();

// 11. 取消代理设置
var request = new HttpRequestClass();
request.SetProxy("http://proxy.example.com:8080"); // 设置代理
request.RemoveProxy(); // 取消代理，后续请求将直连
request.Open("https://api.example.com/data", HttpMethod.Get).Send();
```

### Cookie管理示例

```csharp
using WodToolKit.Http;

// 创建Cookie管理器
var cookieManager = new CookieManager();

// 设置Cookie
cookieManager.SetCookie("sessionId", "abc123");
cookieManager.SetCookie("userId", "user123");

// 获取Cookie字符串
string cookieString = cookieManager.GetCookieString();
Console.WriteLine(cookieString);

// 获取单个Cookie
string sessionId = cookieManager.GetCookieValue("sessionId");
```

### JSON解析示例

```csharp
using WodToolKit.Json;

// 解析JSON字符串
string json = "{\"name\": \"Example\", \"value\": 42}";
dynamic result = EasyJson.ParseJsonToDynamic(json);

// 访问动态对象属性
Console.WriteLine(result.name); // 输出: Example
Console.WriteLine(result.value); // 输出: 42

// 解析为强类型对象
var obj = EasyJson.ParseJsonObject<MyClass>(json);
```

### AES加密示例

```csharp
using WodToolKit.Encode;

// 创建AES加密实例
var aes = new AesCrypt();

// 加密字符串
string plainText = "Hello, World!";
string key = "YourSecretKey123456";
string encrypted = aes.Encrypt(plainText, key);
Console.WriteLine($"加密后: {encrypted}");

// 解密字符串
string decrypted = aes.Decrypt(encrypted, key);
Console.WriteLine($"解密后: {decrypted}");
```

### JavaScript执行与方法调用示例

WodToolKit 提供了两种 JavaScript 执行器：`NodeJsRunner`（需要 Node.js）和 `JintRunner`（纯 .NET 实现，无需 Node.js）。

#### 方式一：使用 JintRunner（推荐，无需 Node.js）

```csharp
using WodToolkit.Script;

// 创建 Jint 执行器（纯 .NET 实现，无需安装 Node.js）
using (var jintRunner = new JintRunner())
{
    // 1. 基本的JavaScript代码执行
    var result = await jintRunner.ExecuteScriptAsync(@"
        function test() {
            console.log('Hello from Jint!');
            return 42;
        }
        
        test();");
    
    Console.WriteLine($"成功: {result.Success}");
    Console.WriteLine($"输出: {result.Output}"); // 输出: Hello from Jint!
    
    // 2. 执行 JavaScript 文件
    var fileResult = await jintRunner.ExecuteScriptFileAsync("./test_script.js");
    Console.WriteLine($"文件执行结果: {fileResult.Output}");
    
    // 3. 调用 JavaScript 文件中的方法
    var addResult = await jintRunner.CallMethodAsync("./test_script.js", "add", 5, 3);
    if (addResult.Success)
    {
        int sum = jintRunner.GetResult<int>(addResult);
        Console.WriteLine($"5 + 3 = {sum}"); // 输出: 8
    }
    
    // 4. 从代码字符串中调用方法
    string scriptCode = @"
        function multiply(a, b) {
            return a * b;
        }
        
        module.exports = { multiply };
    ";
    
    var multiplyResult = await jintRunner.CallMethodFromScriptAsync(scriptCode, "multiply", 7, 8);
    if (multiplyResult.Success)
    {
        int product = jintRunner.GetResult<int>(multiplyResult);
        Console.WriteLine($"7 * 8 = {product}"); // 输出: 56
    }
    
    // 5. 传递复杂对象参数
    var user = new {
        firstName = "John",
        lastName = "Doe",
        email = "john@example.com",
        age = 25
    };
    
    var userResult = await jintRunner.CallMethodAsync("./test_script.js", "processUser", user);
    if (userResult.Success)
    {
        dynamic processedUser = jintRunner.GetResult<dynamic>(userResult);
        Console.WriteLine($"全名: {processedUser.fullName}"); // 输出: John Doe
        Console.WriteLine($"邮箱: {processedUser.email}");
        Console.WriteLine($"是否成年: {processedUser.isAdult}"); // 输出: true
    }
    
    // 6. 处理数组和集合
    var numbers = new int[] { 1, 2, 3, 4, 5 };
    var sumResult = await jintRunner.CallMethodFromScriptAsync(@"
        function sumArray(arr) {
            return arr.reduce((a, b) => a + b, 0);
        }
        module.exports = { sumArray };
    ", "sumArray", numbers);
    
    if (sumResult.Success)
    {
        int total = jintRunner.GetResult<int>(sumResult);
        Console.WriteLine($"数组总和: {total}"); // 输出: 15
    }
    
    // 7. 同步方法调用（适用于简单场景）
    var syncResult = jintRunner.ExecuteScript("console.log('同步执行'); 1 + 1;");
    Console.WriteLine($"同步执行结果: {syncResult.Output}");
    
    // 8. 直接执行并获取结果值（Evaluate）
    var evalResult = jintRunner.Evaluate("2 + 3 * 4");
    Console.WriteLine($"计算结果: {evalResult.AsNumber()}"); // 输出: 14
    
    // 9. 设置和获取变量
    jintRunner.SetValue("userName", "John");
    jintRunner.SetValue("userAge", 25);
    var name = jintRunner.GetValue("userName");
    var age = jintRunner.GetValue("userAge");
    Console.WriteLine($"用户名: {name.AsString()}, 年龄: {age.AsNumber()}");
    
    // 10. 直接调用函数（Invoke）
    jintRunner.Evaluate(@"
        function multiply(a, b) {
            return a * b;
        }
    ");
    var multiplyResult = jintRunner.Invoke("multiply", 5, 6);
    Console.WriteLine($"5 * 6 = {multiplyResult.AsNumber()}"); // 输出: 30
    
    // 11. 获取所有函数和变量
    var functions = jintRunner.GetFunctions();
    var variables = jintRunner.GetVariables();
    Console.WriteLine($"函数数量: {functions.Count}, 变量数量: {variables.Count}");
    
    // 12. 控制台输出管理
    jintRunner.Evaluate("console.log('普通输出'); console.warn('警告'); console.error('错误');");
    Console.WriteLine($"控制台输出: {jintRunner.ConsoleOutput}");
    Console.WriteLine($"错误输出: {jintRunner.ConsoleError}");
    
    // 13. 错误处理
    var errorResult = await jintRunner.ExecuteScriptAsync("throw new Error('测试错误');");
    if (!errorResult.Success)
    {
        Console.WriteLine($"执行失败: {errorResult.Error}");
    }
}
```

#### 方式二：使用 NodeJsRunner（需要安装 Node.js）

```csharp
using WodToolkit.Script;

// 创建 Node.js 执行器（需要系统已安装 Node.js）
using (var nodeRunner = new NodeJsRunner())
{
    // 检查 Node.js 是否可用
    if (!nodeRunner.IsNodeAvailable())
    {
        Console.WriteLine("Node.js 未安装或不可用");
        return;
    }
    
    // 1. 基本的JavaScript代码执行
    var result = await nodeRunner.ExecuteScriptAsync(@"
        function test() {
            console.log('Hello from Node.js!');
            return 42;
        }
        
        test();");
    
    Console.WriteLine($"成功: {result.Success}");
    Console.WriteLine($"输出: {result.Output}");
    
    // 2. 调用 JavaScript 文件中的方法
    var addResult = await nodeRunner.CallMethodAsync("./test_script.js", "add", 5, 3);
    if (addResult.Success)
    {
        int sum = nodeRunner.GetResult<int>(addResult);
        Console.WriteLine($"5 + 3 = {sum}");
    }
    
    // 3. 调用异步函数（Node.js 支持完整的 async/await）
    var asyncResult = await nodeRunner.CallMethodAsync("./test_script.js", "fetchData", 123);
    if (asyncResult.Success)
    {
        dynamic data = nodeRunner.GetResult<dynamic>(asyncResult);
        Console.WriteLine($"ID: {data.id}");
        Console.WriteLine($"名称: {data.name}");
    }
}
```

#### JavaScript 文件示例（test_script.js）

```javascript
// 简单的加法函数
function add(a, b) {
    return a + b;
}

// 对象处理函数
function processUser(user) {
    return {
        fullName: `${user.firstName} ${user.lastName}`,
        email: user.email,
        isAdult: user.age >= 18
    };
}

// 数组处理函数
function sumArray(arr) {
    return arr.reduce((a, b) => a + b, 0);
}

// 字符串处理函数
function formatMessage(name, age) {
    return `姓名: ${name}, 年龄: ${age}`;
}

// 异步函数（Node.js 支持）
async function fetchData(id) {
    await new Promise(resolve => setTimeout(resolve, 300));
    return {
        id: id,
        name: `Item ${id}`,
        status: 'active'
    };
}

// 导出函数（CommonJS 格式）
module.exports = {
    add,
    processUser,
    sumArray,
    formatMessage,
    fetchData
};
```

#### 使用建议

- **推荐使用 JintRunner**：无需安装 Node.js，性能更好，集成更方便，功能更丰富（支持 Evaluate、Invoke、SetValue、GetValue 等高级功能）
- **使用 NodeJsRunner**：当需要完整的 Node.js 生态系统支持时（如 npm 包、Node.js API 等）
- **两种执行器核心 API 完全兼容**：可以轻松切换，无需修改调用代码
- **JintRunner 额外功能**：提供变量管理、直接函数调用、控制台输出管理等高级功能，这些功能在 NodeJsRunner 中不可用

### 身份证验证示例

```csharp
using WodToolkit.src.Common;

// 1. 验证身份证号码是否合法
string idCard = "110101199001011234";
bool isValid = IDCard.IsIdCard(idCard);
Console.WriteLine($"身份证是否合法: {isValid}"); // 输出: true/false

// 2. 获取身份证对应的地址信息（省、市、区/县）
List<string> addressList = IDCard.GetCardAddress(idCard);
if (addressList.Count >= 2)
{
    Console.WriteLine($"省份: {addressList[0]}");
    Console.WriteLine($"城市: {addressList[1]}");
    if (addressList.Count >= 3)
    {
        Console.WriteLine($"区县: {addressList[2]}");
    }
}
else if (addressList.Count == 1)
{
    Console.WriteLine($"省份: {addressList[0]}");
}

// 3. 获取身份证对应的性别
string gender = IDCard.GetGender(idCard);
Console.WriteLine($"性别: {gender}"); // 输出: "男" 或 "女"

// 4. 完整示例：验证并提取身份证信息
string testIdCard = "110101199001011234";
if (IDCard.IsIdCard(testIdCard))
{
    Console.WriteLine("身份证验证通过");
    
    var address = IDCard.GetCardAddress(testIdCard);
    Console.WriteLine($"地址: {string.Join(" ", address)}");
    
    string genderInfo = IDCard.GetGender(testIdCard);
    Console.WriteLine($"性别: {genderInfo}");
}
else
{
    Console.WriteLine("身份证号码不合法");
}
```

## 项目架构与组织

WodToolKit采用模块化设计，各功能模块相互独立，便于扩展和维护。整体架构围绕核心功能模块展开，通过清晰的命名空间和类层次结构提供统一的使用体验。

### 核心模块关系

```
WodToolKit
├── Http/          # HTTP请求与Cookie管理
│   ├── HttpRequestClass  # HTTP客户端实现
│   └── CookieManager     # Cookie管理
├── Json/          # JSON处理
│   └── EasyJson          # 简化的JSON序列化与反序列化
├── Cache/         # 缓存功能
│   └── TempCache         # 内存缓存实现
├── Thread/        # 线程管理
│   └── SimpleThreadPool  # 线程池实现
├── Encode/        # 加密功能
│   └── AesCrypt          # AES加密解密
├── Script/        # 脚本执行
│   ├── JintRunner        # Jint JavaScript执行器（纯.NET）
│   └── NodeJsRunner      # Node.js脚本执行器
└── Common/        # 通用工具
    └── IDCard            # 身份证验证与信息提取
```

### 项目结构

```
├── src/
│   ├── Cache/          # 缓存相关功能
│   ├── Common/         # 通用工具功能（身份证验证等）
│   ├── Encode/         # 加密相关功能
│   ├── Http/           # HTTP相关功能
│   ├── Json/           # JSON处理功能
│   ├── Script/         # JavaScript执行和方法调用功能
│   └── Thread/         # 线程管理功能
├── WodToolkit.csproj   # 项目文件
└── README.md           # 项目文档
```

## 依赖项

- System.Text.Json - JSON处理
- Microsoft.CSharp - 动态类型支持
- System.Security.Cryptography - 加密功能支持

## 许可证

本项目采用MIT许可证。详情请参阅[LICENSE](LICENSE)文件。

## 问题反馈与贡献

### 问题反馈

我们非常重视您的使用体验，如果您在使用过程中遇到任何问题或有任何建议，请通过以下方式反馈：

1. **GitHub Issues**：在项目仓库中创建新的Issue，详细描述您遇到的问题或提出的功能建议
   - 请尽可能提供详细的复现步骤
   - 如遇错误，请提供完整的错误信息和堆栈跟踪
   - 说明您使用的WodToolKit版本和环境信息

2. **代码贡献**

   我们非常欢迎社区贡献！如果您想要为项目贡献代码，请遵循以下步骤：

   1. **Fork 项目仓库**
      - 在GitHub上点击"Fork"按钮创建您自己的仓库副本

   2. **克隆仓库**
      ```bash
      git clone https://github.com/thiswod/WodToolKit.git
      cd WodToolKit
      ```

   3. **创建分支**
      ```bash
      git checkout -b feature/您的功能名称
      ```

   4. **实现功能或修复问题**
      - 请确保您的代码风格与项目保持一致
      - 为新功能添加适当的测试用例
      - 更新相关文档

   5. **提交更改**
      ```bash
      git commit -m "描述您的更改"
      ```

   6. **推送到您的Fork**
      ```bash
      git push origin feature/您的功能名称
      ```

   7. **创建Pull Request**
      - 在GitHub上导航到原始仓库
      - 点击"New Pull Request"按钮
      - 选择您的分支并提交PR
      - 请在PR描述中详细说明您的更改内容和目的

### 开发规范

为了保持代码质量和一致性，请遵循以下开发规范：

1. 代码风格：遵循C#标准代码约定
2. 注释：为公共方法和类添加适当的XML文档注释
3. 测试：新功能应包含单元测试
4. 兼容性：确保代码与.NET Standard 2.1兼容

## 作者

Wod