---
layout: default
title: WodToolKit - 轻量级.NET工具库
---

# WodToolKit

轻量级.NET工具库，提供各类常用功能的封装，旨在简化开发工作，提高开发效率。

## 功能特性

- **HTTP请求处理**：简化HTTP客户端操作，支持各种HTTP方法和请求配置，支持HTTP/HTTPS和SOCKS4/SOCKS5代理
- **Cookie管理**：完整的Cookie管理功能，支持添加、获取、删除和批量操作
- **JSON解析**：灵活的JSON序列化和反序列化，支持动态类型和自定义类型
- **JavaScript执行**：支持JintRunner（纯.NET）和NodeJsRunner两种方式
- **内存缓存**：基于内存的临时缓存实现，支持TTL设置和自动清理
- **AES加密**：安全的AES加密和解密功能

## 快速安装

```powershell
Install-Package WodToolKit
```

## 快速开始

```csharp
using WodToolkit.Http;

var httpRequest = new HttpRequestClass();
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
var response = httpRequest.GetResponse();
Console.WriteLine(response.Body);
```

[查看完整文档 →]({{ '/getting-started' | relative_url }})

## 项目信息

- **.NET Standard 2.1** 兼容
- **MIT 许可证**
- **GitHub**: [thiswod/WodToolKit](https://github.com/thiswod/WodToolKit)
- **NuGet**: [WodToolKit](https://www.nuget.org/packages/WodToolKit)
