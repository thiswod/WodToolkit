---
layout: home
title: WodToolKit - 轻量级.NET工具库
---

## 功能特性

<div class="features">
  <div class="feature-card">
    <h3>🚀 HTTP请求处理</h3>
    <p>简化HTTP客户端操作，支持各种HTTP方法和请求配置，支持HTTP/HTTPS和SOCKS4/SOCKS5代理</p>
  </div>
  
  <div class="feature-card">
    <h3>🍪 Cookie管理</h3>
    <p>完整的Cookie管理功能，支持添加、获取、删除和批量操作</p>
  </div>
  
  <div class="feature-card">
    <h3>📦 JSON解析</h3>
    <p>灵活的JSON序列化和反序列化，支持动态类型和自定义类型</p>
  </div>
  
  <div class="feature-card">
    <h3>⚡ JavaScript执行</h3>
    <p>支持JintRunner（纯.NET）和NodeJsRunner两种方式，无需额外依赖</p>
  </div>
  
  <div class="feature-card">
    <h3>💾 内存缓存</h3>
    <p>基于内存的临时缓存实现，支持TTL设置和自动清理</p>
  </div>
  
  <div class="feature-card">
    <h3>🔐 AES加密</h3>
    <p>安全的AES加密和解密功能，保护您的数据安全</p>
  </div>
</div>

## 快速安装

<div class="install-section">
  <h3>通过 NuGet 包管理器</h3>
  <pre><code class="language-powershell">Install-Package WodToolKit</code></pre>
  
  <h3>通过 .NET CLI</h3>
  <pre><code class="language-bash">dotnet add package WodToolKit</code></pre>
</div>

## 快速开始

<div class="code-example">
  <pre><code class="language-csharp">using WodToolKit.Http;

var httpRequest = new HttpRequestClass();
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
var response = httpRequest.GetResponse();
Console.WriteLine(response.Body);</code></pre>
</div>

<div class="cta-section">
  <a href="{{ '/getting-started' | relative_url }}" class="btn btn-primary">查看完整文档</a>
</div>

## 项目信息

<div class="project-info">
  <p><strong>.NET Standard 2.1</strong> 兼容 | <strong>MIT</strong> 许可证</p>
  <p>
    <a href="https://github.com/thiswod/WodToolKit" target="_blank">GitHub</a> |
    <a href="https://www.nuget.org/packages/WodToolKit" target="_blank">NuGet</a>
  </p>
</div>
