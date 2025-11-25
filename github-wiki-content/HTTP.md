# HTTP 请求处理

WodToolKit 提供了简单易用的 HTTP 客户端，支持各种 HTTP 方法和配置选项。

## 基本用法

### GET 请求

```csharp
using WodToolkit.Http;
using System.Net;

var httpRequest = new HttpRequestClass();
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
var response = httpRequest.GetResponse();

Console.WriteLine(response.Body);
```

### POST 请求（表单数据）

```csharp
var postRequest = new HttpRequestClass();
var formData = new Dictionary<string, string>
{
    { "username", "admin" },
    { "password", "password123" }
};

postRequest.Open("https://api.example.com/login", HttpMethod.Post).Send(formData);
var loginResponse = postRequest.GetResponse();
```

### POST 请求（JSON 数据）

```csharp
var jsonRequest = new HttpRequestClass();
jsonRequest.Open("https://api.example.com/users", HttpMethod.Post).Send(new 
{
    name = "测试用户",
    age = 25,
    email = "test@example.com"
});
var userResponse = jsonRequest.GetResponse();
```

## 代理设置

### HTTP 代理

```csharp
var httpRequest = new HttpRequestClass();
httpRequest.SetProxy("http://proxy.example.com:8080", "username", "password");
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
```

### SOCKS5 代理

```csharp
var socks5Request = new HttpRequestClass();
socks5Request.SetProxy(ProxyType.Socks5, "proxy.example.com", 1080, "username", "password");
socks5Request.Open("https://api.example.com/data", HttpMethod.Get).Send();
```

### SOCKS4 代理

```csharp
var socks4Request = new HttpRequestClass();
socks4Request.SetProxy(ProxyType.Socks4, "proxy.example.com", 1080);
socks4Request.Open("https://api.example.com/data", HttpMethod.Get).Send();
```

## Cookie 管理

```csharp
var cookieRequest = new HttpRequestClass();
cookieRequest.SetCookieString("session=abc123; user=admin");
cookieRequest.Open("https://api.example.com/protected", HttpMethod.Get).Send();

var response = cookieRequest.GetResponse();
// 自动处理响应中的 Cookie
string cookies = response.Cookie;
```

更多 Cookie 管理功能，请查看 [[Cookie|Cookie 管理]] 文档。

## 自定义请求头

```csharp
var request = new HttpRequestClass();
request.SetRequestHeader("Authorization", "Bearer token123");
request.SetRequestHeader("X-Custom-Header", "custom-value");
request.Open("https://api.example.com/data", HttpMethod.Get).Send();
```

## 超时设置

```csharp
var request = new HttpRequestClass();
request.SetTimeout(30); // 30秒超时
request.Open("https://api.example.com/data", HttpMethod.Get).Send();
```

## 异步请求

```csharp
var asyncRequest = new HttpRequestClass();
await asyncRequest.Open("https://api.example.com/data", HttpMethod.Get).SendAsync();
var response = asyncRequest.GetResponse();
```

## 文件上传

```csharp
var uploadRequest = new HttpRequestClass();
uploadRequest.AddFile("file", "path/to/file.jpg", "image/jpeg", "photo.jpg");
uploadRequest.Open("https://api.example.com/upload", HttpMethod.Post).Send();
```

## 完整示例

```csharp
using WodToolkit.Http;
using System.Net;

var httpRequest = new HttpRequestClass();

// 配置请求
httpRequest.SetTimeout(30);
httpRequest.SetUserAgent("MyApp/1.0");
httpRequest.SetCookieString("session=abc123");
httpRequest.SetRequestHeader("X-API-Key", "your-api-key");

// 设置代理（可选）
httpRequest.SetProxy(ProxyType.Socks5, "proxy.example.com", 1080, "user", "pass");

// 发送请求
httpRequest.Open("https://api.example.com/data", HttpMethod.Post).Send(new
{
    key = "value",
    number = 123
});

// 处理响应
var response = httpRequest.GetResponse();
if (response.StatusCode == 200)
{
    Console.WriteLine($"成功: {response.Body}");
    Console.WriteLine($"Cookie: {response.Cookie}");
}
else
{
    Console.WriteLine($"错误: {response.StatusCode}");
    Console.WriteLine($"错误信息: {response.Body}");
}
```

## 相关文档

- [[Cookie|Cookie 管理]] - Cookie 管理详细说明
- [[JSON|JSON 处理]] - JSON 数据处理

