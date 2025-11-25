# 常见问题

## 安装相关

### Q: 如何安装 WodToolKit？

A: 可以通过以下方式安装：

```powershell
# NuGet 包管理器
Install-Package WodToolKit

# .NET CLI
dotnet add package WodToolKit
```

详细说明请查看 [[安装指南|Installation]]。

### Q: 支持哪些 .NET 版本？

A: WodToolKit 基于 .NET Standard 2.1，支持：
- .NET Core 3.0+
- .NET Framework 4.6.1+
- .NET 5.0+
- .NET 6.0+
- .NET 7.0+
- .NET 8.0+

## 使用相关

### Q: HTTP 请求如何设置超时时间？

A: 使用 `SetTimeout()` 方法：

```csharp
var httpRequest = new HttpRequestClass();
httpRequest.SetTimeout(30); // 30秒超时
```

### Q: 如何发送带 Cookie 的请求？

A: 有两种方式：

```csharp
// 方式一：使用 SetCookieString
httpRequest.SetCookieString("session=abc123; user=admin");

// 方式二：使用 CookieManager
var cookieManager = httpRequest.CookieManager();
cookieManager.SetCookie("session", "abc123");
```

详细说明请查看 [[Cookie|Cookie 管理]]。

### Q: 如何解析 JSON 响应？

A: 可以使用动态类型或强类型：

```csharp
// 动态类型
dynamic result = EasyJson.ParseJsonToDynamic(response.Body);

// 强类型
var user = EasyJson.ParseJsonObject<User>(response.Body);
```

### Q: JavaScript 执行器应该选择哪个？

A: 
- **推荐使用 JintRunner**：无需安装 Node.js，性能更好，集成更方便
- **使用 NodeJsRunner**：当需要完整的 Node.js 生态系统支持时

### Q: 缓存数据何时会被清理？

A: 过期项会在设置的清理间隔时自动清理。默认每 60 秒清理一次，可以通过构造函数自定义：

```csharp
var cache = new TempCache<string, string>(
    cleanupInterval: TimeSpan.FromSeconds(30), // 每30秒清理
    defaultTtl: 300 // 默认TTL 300秒
);
```

## 错误处理

### Q: HTTP 请求失败如何处理？

A: 检查响应状态码：

```csharp
var response = httpRequest.GetResponse();
if (response.StatusCode != 200)
{
    Console.WriteLine($"错误: {response.StatusCode}");
    Console.WriteLine($"错误信息: {response.Body}");
}
```

### Q: JavaScript 执行失败如何处理？

A: 检查执行结果：

```csharp
var result = await jintRunner.ExecuteScriptAsync("...");
if (!result.Success)
{
    Console.WriteLine($"执行失败: {result.Error}");
}
```

## 性能相关

### Q: 缓存会影响性能吗？

A: 缓存存储在内存中，访问速度很快。但要注意：
- 控制缓存大小，避免占用过多内存
- 合理设置 TTL，避免缓存过期数据
- 使用完毕后记得释放资源

### Q: 线程池如何选择线程数？

A: 根据任务类型和系统资源选择：
- CPU 密集型任务：线程数 ≈ CPU 核心数
- IO 密集型任务：可以设置更多线程
- 一般建议：4-8 个线程

## 其他问题

### Q: 如何贡献代码？

A: 请查看 [[贡献指南|Contributing]]。

### Q: 在哪里可以报告问题？

A: 请在 [GitHub Issues](https://github.com/thiswod/WodToolKit/issues) 提交问题。

### Q: 如何获取最新版本信息？

A: 请查看 [[更新日志|Changelog]] 或访问 [NuGet](https://www.nuget.org/packages/WodToolKit)。

