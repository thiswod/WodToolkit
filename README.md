# WodToolKit

轻量级.NET工具库，提供各类常用功能的封装，旨在简化开发工作，提高开发效率。目前已实现HTTP请求处理、Cookie管理和JSON解析功能，未来将持续扩展更多实用工具。

## 功能特性

- **HTTP请求处理**：简化HTTP客户端操作，支持各种HTTP方法和请求配置
- **Cookie管理**：完整的Cookie管理功能，支持添加、获取、删除和批量操作
- **JSON解析**：灵活的JSON序列化和反序列化，支持动态类型和自定义类型
- **URL工具**：URL参数处理、排序和转换工具
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

## 项目结构

```
├── src/
│   ├── Http/           # HTTP相关功能
│   └── Json/           # JSON处理功能
├── WodToolkit.csproj   # 项目文件
└── README.md           # 项目文档
```

## 依赖项

- System.Text.Json - JSON处理
- Microsoft.CSharp - 动态类型支持

## 许可证

本项目采用MIT许可证。详情请参阅[LICENSE](LICENSE)文件。

## 贡献

欢迎提交Issue和Pull Request！

## 作者

Wod