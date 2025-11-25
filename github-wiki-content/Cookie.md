# Cookie 管理

WodToolKit 提供了完整的 Cookie 管理功能，支持添加、获取、删除和批量操作。

## 基本用法

### 创建 Cookie 管理器

```csharp
using WodToolkit.Http;

// 创建独立的 Cookie 管理器
var cookieManager = new CookieManager();

// 或者在 HTTP 请求中使用
var httpRequest = new HttpRequestClass();
var cookieManager = httpRequest.CookieManager();
```

### 设置单个 Cookie

```csharp
var cookieManager = new CookieManager();

// 设置 Cookie
cookieManager.SetCookie("sessionId", "abc123");
cookieManager.SetCookie("userId", "user456");

// 链式调用
cookieManager
    .SetCookie("sessionId", "abc123")
    .SetCookie("userId", "user456");
```

### 获取 Cookie 值

```csharp
// 获取单个 Cookie 值
string sessionId = cookieManager.GetCookieValue("sessionId");
Console.WriteLine(sessionId); // 输出: abc123

// 如果 Cookie 不存在，返回 null
string nonExistent = cookieManager.GetCookieValue("nonExistent");
if (nonExistent == null)
{
    Console.WriteLine("Cookie 不存在");
}
```

### 检查 Cookie 是否存在

```csharp
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
// 从 Cookie 字符串设置（格式：key1=value1; key2=value2）
cookieManager.SetCookie("sessionId=abc123; userId=user456; theme=dark");

// 支持 URL 编码的值
cookieManager.SetCookie("sessionId=abc%20123; userId=user456");
```

## 获取 Cookie

### 获取 Cookie 字符串

```csharp
// 获取 URL 编码的 Cookie 字符串（用于 HTTP 请求）
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

### 删除单个 Cookie

```csharp
// 方法一：使用 RemoveCookie 方法
cookieManager.RemoveCookie("sessionId");

// 方法二：通过设置值为 "deleted" 来删除
cookieManager.SetCookie("sessionId", "deleted");

// 方法三：设置空值
cookieManager.SetCookie("sessionId", "");
```

### 清空所有 Cookie

```csharp
cookieManager.ClearCookies();
```

## 在 HTTP 请求中使用

### 方式一：使用 SetCookieString

```csharp
using WodToolkit.Http;
using System.Net;

var httpRequest = new HttpRequestClass();

// 设置 Cookie 字符串
httpRequest.SetCookieString("session=abc123; user=admin");

// 发送请求，会自动带上 Cookie
httpRequest.Open("https://api.example.com/protected", HttpMethod.Get).Send();

// 获取响应中的 Cookie
var response = httpRequest.GetResponse();
string newCookies = response.Cookie;

// 更新 Cookie
if (!string.IsNullOrEmpty(newCookies))
{
    httpRequest.SetCookieString(newCookies);
}
```

### 方式二：使用 Cookie 管理器

```csharp
var httpRequest = new HttpRequestClass();
var cookieManager = httpRequest.CookieManager();

// 设置 Cookie
cookieManager.SetCookie("sessionId", "abc123");
cookieManager.SetCookie("userId", "user456");

// 发送请求，会自动带上 Cookie
httpRequest.Open("https://api.example.com/protected", HttpMethod.Get).Send();

// 处理响应中的新 Cookie
var response = httpRequest.GetResponse();
if (!string.IsNullOrEmpty(response.Cookie))
{
    cookieManager.SetCookie(response.Cookie);
}
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

## 相关文档

- [[HTTP|HTTP 请求处理]] - HTTP 客户端使用指南

