---
layout: default
title: 内存缓存
---

# 内存缓存

WodToolKit 提供了基于内存的临时缓存实现，支持 TTL（生存时间）设置和自动清理。

## 基本用法

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

// 移除缓存项
cache.Remove("key2");

// 清空所有缓存
cache.Clear();
```

## 高级用法

### 自定义 TTL

```csharp
var cache = new TempCache<string, object>();

// 设置缓存项，TTL 为 60 秒
cache.Set("user:123", new { Name = "John", Age = 25 }, 60);

// 设置缓存项，使用默认 TTL（300秒）
cache.Set("config", new { Theme = "dark" });
```

### 检查缓存是否存在

```csharp
if (cache.ContainsKey("key1"))
{
    Console.WriteLine("缓存项存在");
}
```

### 获取所有键

```csharp
var keys = cache.GetKeys();
foreach (var key in keys)
{
    Console.WriteLine($"缓存键: {key}");
}
```

## 完整示例

```csharp
using WodToolkit.src.Cache;
using System;

var cache = new TempCache<string, string>(
    cleanupInterval: TimeSpan.FromSeconds(30), // 每30秒清理一次过期项
    defaultTtl: 300 // 默认TTL为300秒
);

// 设置缓存
cache.Set("session:user1", "active", 60); // 60秒后过期
cache.Set("session:user2", "active", 120); // 120秒后过期

// 获取缓存
if (cache.TryGetValue("session:user1", out string status))
{
    Console.WriteLine($"用户状态: {status}");
}

// 等待一段时间后，过期项会被自动清理
System.Threading.Thread.Sleep(65000);

// 使用完毕后释放资源
cache.Dispose();
```

