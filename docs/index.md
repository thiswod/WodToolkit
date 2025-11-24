---
layout: default
title: WodToolKit - 轻量级.NET工具库
---

# WodToolKit

<div class="hero">
  <p class="lead">轻量级.NET工具库，提供各类常用功能的封装，旨在简化开发工作，提高开发效率。</p>
</div>

## 功能特性

<div class="features">
  <div class="feature-card">
    <h3>🌐 HTTP请求处理</h3>
    <p>简化HTTP客户端操作，支持各种HTTP方法和请求配置，支持HTTP/HTTPS和SOCKS4/SOCKS5代理</p>
    <a href="{{ '/http' | relative_url }}">查看文档 →</a>
  </div>

  <div class="feature-card">
    <h3>🍪 Cookie管理</h3>
    <p>完整的Cookie管理功能，支持添加、获取、删除和批量操作</p>
    <a href="{{ '/cookie' | relative_url }}">查看文档 →</a>
  </div>

  <div class="feature-card">
    <h3>📦 JSON解析</h3>
    <p>灵活的JSON序列化和反序列化，支持动态类型和自定义类型</p>
    <a href="{{ '/json' | relative_url }}">查看文档 →</a>
  </div>

  <div class="feature-card">
    <h3>⚡ JavaScript执行</h3>
    <p>支持JintRunner（纯.NET）和NodeJsRunner两种方式，无需额外依赖</p>
    <a href="{{ '/javascript' | relative_url }}">查看文档 →</a>
  </div>

  <div class="feature-card">
    <h3>💾 内存缓存</h3>
    <p>基于内存的临时缓存实现，支持TTL设置和自动清理</p>
    <a href="{{ '/cache' | relative_url }}">查看文档 →</a>
  </div>

  <div class="feature-card">
    <h3>🔐 AES加密</h3>
    <p>安全的AES加密和解密功能，支持多种加密模式和填充方式</p>
    <a href="{{ '/aes' | relative_url }}">查看文档 →</a>
  </div>
</div>

## 快速安装

```powershell
# 通过 NuGet 安装
Install-Package WodToolKit

# 或使用 .NET CLI
dotnet add package WodToolKit
```

## 快速开始

```csharp
using WodToolkit.Http;

// 发送 HTTP 请求
var httpRequest = new HttpRequestClass();
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
var response = httpRequest.GetResponse();

Console.WriteLine(response.Body);
```

[查看完整快速开始指南 →]({{ '/getting-started' | relative_url }})

## 项目信息

- **.NET Standard 2.1** 兼容
- **MIT 许可证**
- **GitHub**: [thiswod/WodToolKit](https://github.com/thiswod/WodToolKit)
- **NuGet**: [WodToolKit](https://www.nuget.org/packages/WodToolKit)

