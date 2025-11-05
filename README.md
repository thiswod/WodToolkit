# WodToolKit

[English Version](docs/README_EN.md) | 中文版本

轻量级.NET工具库，提供各类常用功能的封装，旨在简化开发工作，提高开发效率。已实现HTTP请求处理、Cookie管理、JSON解析、内存缓存、线程池、AES加密、JavaScript执行等功能。

## 功能特性

- **HTTP请求处理**：简化HTTP客户端操作，支持各种HTTP方法和请求配置
- **Cookie管理**：完整的Cookie管理功能，支持添加、获取、删除和批量操作
- **JSON解析**：灵活的JSON序列化和反序列化，支持动态类型和自定义类型
- **URL工具**：URL参数处理、排序和转换工具
- **AES加密**：安全的AES加密和解密功能，支持多种加密模式和填充方式
- **内存缓存**：基于内存的临时缓存实现，支持TTL设置和自动清理
- **线程池管理**：简单高效的线程池实现，支持任务队列和任务等待
- **JavaScript执行**：通过Node.js执行JavaScript代码，支持代码字符串和文件执行，以及调用特定方法并传递参数
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
using WodToolKit.Http;

// 创建HTTP客户端
var httpClient = new HttpClient();

// 发送GET请求
var response = await httpClient.GetAsync("https://api.example.com/data");

// 处理响应
if (response.IsSuccessStatusCode)
{
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine(content);
}
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

```csharp
using WodToolKit.Script;

// 创建Node.js执行器（默认在系统PATH中查找node）
using (var nodeRunner = new NodeJsRunner())
{
    // 1. 基本的JavaScript代码执行
    var result = await nodeRunner.ExecuteScriptAsync(@"
        function test() {
            console.log('Hello from JavaScript!');
            return 42;
        }
        
        test();");
    
    Console.WriteLine($"成功: {result.Success}");
    Console.WriteLine($"输出: {result.Output}");
    
    // 2. 调用JavaScript文件中的方法
    var addResult = await nodeRunner.CallMethodAsync("./test_script.js", "add", 5, 3);
    if (addResult.Success)
    {
        // 解析返回结果
        int sum = nodeRunner.GetResult<int>(addResult);
        Console.WriteLine($"5 + 3 = {sum}");
    }
    
    // 3. 传递对象参数
    var user = new {
        firstName = "John",
        lastName = "Doe",
        email = "john@example.com",
        age = 25
    };
    
    var userResult = await nodeRunner.CallMethodAsync("./test_script.js", "processUser", user);
    if (userResult.Success)
    {
        // 动态类型解析结果
        dynamic processedUser = nodeRunner.GetResult<dynamic>(userResult);
        Console.WriteLine($"全名: {processedUser.fullName}");
        Console.WriteLine($"邮箱: {processedUser.email}");
        Console.WriteLine($"是否成年: {processedUser.isAdult}");
    }
    
    // 4. 从代码字符串中调用方法
    string scriptCode = @"
        function multiply(a, b) {
            return a * b;
        }
        
        module.exports = { multiply };
    ";
    
    var multiplyResult = await nodeRunner.CallMethodFromScriptAsync(scriptCode, "multiply", 7, 8);
    if (multiplyResult.Success)
    {
        int product = nodeRunner.GetResult<int>(multiplyResult);
        Console.WriteLine($"7 * 8 = {product}");
    }
    
    // 5. 调用异步函数
    var asyncResult = await nodeRunner.CallMethodAsync("./test_script.js", "fetchData", 123);
    if (asyncResult.Success)
    {
        dynamic data = nodeRunner.GetResult<dynamic>(asyncResult);
        Console.WriteLine($"ID: {data.id}");
        Console.WriteLine($"名称: {data.name}");
    }
}

/*
JavaScript文件示例（test_script.js）：

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

// 异步函数
async function fetchData(id) {
    await new Promise(resolve => setTimeout(resolve, 300));
    return {
        id: id,
        name: `Item ${id}`,
        status: 'active'
    };
}

// 导出函数
module.exports = {
    add,
    processUser,
    fetchData
};
*/
```

## 项目结构

```
├── src/
│   ├── Cache/          # 缓存相关功能
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

## 贡献

欢迎提交Issue和Pull Request！

## 作者

Wod