# 快速开始

本指南将帮助您在 5 分钟内快速上手 WodToolKit。

## 前提条件

- 已安装 WodToolKit（参见 [[安装指南|Installation]]）
- 基本的 C# 编程知识

## 第一个示例：HTTP 请求

让我们从一个简单的 HTTP GET 请求开始：

```csharp
using WodToolkit.Http;
using System.Net;

// 创建 HTTP 请求实例
var httpRequest = new HttpRequestClass();

// 发送 GET 请求
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();

// 获取响应
var response = httpRequest.GetResponse();

// 处理响应
if (response.StatusCode == 200)
{
    Console.WriteLine($"响应内容: {response.Body}");
    Console.WriteLine($"状态码: {response.StatusCode}");
}
```

## 示例二：Cookie 管理

```csharp
using WodToolkit.Http;

// 创建 Cookie 管理器
var cookieManager = new CookieManager();

// 设置 Cookie
cookieManager.SetCookie("sessionId", "abc123");
cookieManager.SetCookie("userId", "user456");

// 获取 Cookie 字符串
string cookieString = cookieManager.GetCookieString();
Console.WriteLine(cookieString); // 输出: sessionId=abc123; userId=user456
```

## 示例三：JSON 处理

```csharp
using WodToolkit.Json;

// 解析 JSON 字符串
string json = "{\"name\": \"Example\", \"value\": 42}";
dynamic result = EasyJson.ParseJsonToDynamic(json);

// 访问动态对象属性
Console.WriteLine(result.name);  // 输出: Example
Console.WriteLine(result.value); // 输出: 42
```

## 示例四：内存缓存

```csharp
using WodToolkit.src.Cache;

// 创建缓存实例（每30秒清理一次，默认TTL为300秒）
var cache = new TempCache<string, string>(TimeSpan.FromSeconds(30), 300);

// 设置缓存项
cache.Set("key1", "value1");
cache.Set("key2", "value2", 60); // 自定义TTL为60秒

// 获取缓存项
if (cache.TryGetValue("key1", out string value))
{
    Console.WriteLine($"缓存值: {value}");
}
```

## 示例五：JavaScript 执行

```csharp
using WodToolkit.Script;

// 使用 JintRunner（推荐，无需 Node.js）
using (var jintRunner = new JintRunner())
{
    var result = await jintRunner.ExecuteScriptAsync(@"
        function add(a, b) {
            return a + b;
        }
        module.exports = { add };
    ");
    
    var addResult = await jintRunner.CallMethodFromScriptAsync(
        result.Output, "add", 5, 3);
    
    if (addResult.Success)
    {
        int sum = jintRunner.GetResult<int>(addResult);
        Console.WriteLine($"5 + 3 = {sum}"); // 输出: 8
    }
}
```

## 示例六：AES 加密

```csharp
using WodToolkit.Encode;

// 创建 AES 加密实例
var aes = new AesCrypt();

// 加密字符串
string plainText = "Hello, World!";
string key = "YourSecretKey123456"; // 必须是32字节
string encrypted = aes.Encrypt(plainText, key);
Console.WriteLine($"加密后: {encrypted}");

// 解密字符串
string decrypted = aes.Decrypt(encrypted, key);
Console.WriteLine($"解密后: {decrypted}");
```

## 示例七：线程池

```csharp
using WodToolkit.src.Thread;

// 创建线程池（4个工作线程）
var threadPool = new SimpleThreadPool(4);

// 添加任务到线程池
for (int i = 0; i < 10; i++)
{
    int taskId = i;
    threadPool.QueueTask(() => {
        Console.WriteLine($"执行任务 {taskId}");
        System.Threading.Thread.Sleep(100);
    });
}

// 等待所有任务完成
threadPool.Wait();

// 释放线程池资源
threadPool.Dispose();
```

## 完整示例：综合使用

```csharp
using WodToolkit.Http;
using WodToolkit.Json;
using WodToolkit.src.Cache;
using System.Net;

// 1. 创建缓存
var cache = new TempCache<string, string>(TimeSpan.FromSeconds(30), 300);

// 2. 检查缓存
if (!cache.TryGetValue("api_data", out string cachedData))
{
    // 3. 发送 HTTP 请求
    var httpRequest = new HttpRequestClass();
    httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
    var response = httpRequest.GetResponse();
    
    // 4. 解析 JSON
    dynamic jsonData = EasyJson.ParseJsonToDynamic(response.Body);
    
    // 5. 缓存结果
    cache.Set("api_data", response.Body, 300);
    
    Console.WriteLine($"获取到数据: {jsonData.name}");
}
else
{
    Console.WriteLine("从缓存获取数据");
    dynamic jsonData = EasyJson.ParseJsonToDynamic(cachedData);
    Console.WriteLine($"缓存数据: {jsonData.name}");
}
```

## 下一步

现在您已经了解了 WodToolKit 的基本用法，可以查看以下详细文档：

- [[HTTP 请求处理|HTTP]] - 深入了解 HTTP 功能
- [[Cookie 管理|Cookie]] - Cookie 管理详解
- [[JSON 处理|JSON]] - JSON 操作指南
- [[内存缓存|Cache]] - 缓存使用说明
- [[JavaScript 执行|JavaScript-Execution]] - JavaScript 执行详解
- [[AES 加密|AES-Encryption]] - 加密功能说明
- [[线程池管理|Thread-Pool]] - 线程池使用指南

## 获取帮助

如果您在使用过程中遇到问题：

1. 查看 [[常见问题|FAQ]]
2. 查看 [[API 参考|API-Reference]]
3. 提交 [GitHub Issue](https://github.com/thiswod/WodToolKit/issues)

---

**Language**: 中文 (Chinese) | [English](Quick-Start-EN)

