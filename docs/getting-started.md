---
layout: default
title: 快速开始
---

# 快速开始

本指南将帮助您快速开始使用 WodToolKit。

## 安装

### 通过 NuGet 包管理器

```powershell
Install-Package WodToolKit
```

### 通过 .NET CLI

```bash
dotnet add package WodToolKit
```

### 通过 PackageReference

```xml
<PackageReference Include="WodToolKit" Version="1.0.1.4" />
```

## 第一个示例

### HTTP 请求示例

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

### Cookie 管理示例

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

### JavaScript 执行示例

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

## 下一步

- [HTTP 请求文档]({{ '/http' | relative_url }})
- [JavaScript 执行文档]({{ '/javascript' | relative_url }})
- [缓存使用文档]({{ '/cache' | relative_url }})

