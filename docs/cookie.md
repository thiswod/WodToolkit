---
layout: default
title: Cookie管理
---

# Cookie 管理

WodToolKit 提供了完整的 Cookie 管理功能，支持添加、获取、删除和批量操作。

## 基本用法

```csharp
using WodToolkit.Http;

// 创建 Cookie 管理器
var cookieManager = new CookieManager();

// 设置单个 Cookie
cookieManager.SetCookie("sessionId", "abc123");
cookieManager.SetCookie("userId", "user456");

// 获取 Cookie 值
string sessionId = cookieManager.GetCookieValue("sessionId");
Console.WriteLine(sessionId); // 输出: abc123

// 检查 Cookie 是否存在
if (cookieManager.HasCookie("sessionId"))
{
    Console.WriteLine("Cookie 存在");
}
```

## 批量设置 Cookie

### 通过字典设置

```csharp
var cookies = new Dictionary<string, string>
{
    { "sessionId", "abc123" },
    { "userId", "user456" },
    { "theme", "dark" }
};

cookieManager.SetCookie(cookies);
```

### 通过字符串设置

```csharp
// 从 Cookie 字符串设置
cookieManager.SetCookie("sessionId=abc123; userId=user456; theme=dark");
```

## 获取 Cookie

### 获取 Cookie 字符串

```csharp
// 获取 URL 编码的 Cookie 字符串
string cookieString = cookieManager.GetCookieString();
Console.WriteLine(cookieString);
// 输出: sessionId=abc123; userId=user456

// 获取原始 Cookie 字符串（无编码）
string rawCookieString = cookieManager.GetRawCookieString();
```

### 获取所有 Cookie

```csharp
var allCookies = cookieManager.GetAllCookies();
foreach (var cookie in allCookies)
{
    Console.WriteLine($"{cookie.Key} = {cookie.Value}");
}
```

## 删除 Cookie

```csharp
// 删除单个 Cookie
cookieManager.RemoveCookie("sessionId");

// 或者通过设置值为 "deleted" 来删除
cookieManager.SetCookie("sessionId", "deleted");

// 清空所有 Cookie
cookieManager.ClearCookies();
```

## 在 HTTP 请求中使用

```csharp
using WodToolkit.Http;

var httpRequest = new HttpRequestClass();

// 设置 Cookie
httpRequest.SetCookieString("session=abc123; user=admin");

// 发送请求，会自动带上 Cookie
httpRequest.Open("https://api.example.com/protected", HttpMethod.Get).Send();

// 获取响应中的 Cookie
var response = httpRequest.GetResponse();
string newCookies = response.Cookie;

// 更新 Cookie 管理器
httpRequest.CookieManager().SetCookie(newCookies);
```

## 完整示例

```csharp
using WodToolkit.Http;

var cookieManager = new CookieManager();

// 设置多个 Cookie
cookieManager.SetCookie("sessionId", "abc123");
cookieManager.SetCookie("userId", "user456");
cookieManager.SetCookie("preferences", "theme=dark");

// 获取 Cookie 字符串用于 HTTP 请求
string cookieString = cookieManager.GetCookieString();

// 在 HTTP 请求中使用
var httpRequest = new HttpRequestClass();
httpRequest.SetCookieString(cookieString);
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();

var response = httpRequest.GetResponse();

// 处理响应中的新 Cookie
if (!string.IsNullOrEmpty(response.Cookie))
{
    cookieManager.SetCookie(response.Cookie);
}
```

