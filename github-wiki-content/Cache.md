# 内存缓存

WodToolKit 提供了基于内存的临时缓存实现，支持 TTL（生存时间）设置和自动清理。

## 基本用法

### 创建缓存实例

```csharp
using WodToolkit.src.Cache;
using System;

// 创建缓存实例（每30秒清理一次，默认TTL为300秒）
var cache = new TempCache<string, string>(
    cleanupInterval: TimeSpan.FromSeconds(30),
    defaultTtl: 300
);

// 使用默认设置（每60秒清理一次，默认TTL为300秒）
var defaultCache = new TempCache<string, string>();
```

### 设置缓存项

```csharp
// 使用默认 TTL
cache.Set("key1", "value1");

// 自定义 TTL（秒）
cache.Set("key2", "value2", 60); // 60秒后过期

// 设置复杂对象
cache.Set("user:123", new { Name = "John", Age = 25 }, 120);
```

### 获取缓存项

```csharp
// 方式一：TryGetValue（推荐）
if (cache.TryGetValue("key1", out string value))
{
    Console.WriteLine($"缓存值: {value}");
}
else
{
    Console.WriteLine("缓存项不存在或已过期");
}
```

### 移除缓存项

```csharp
// 移除单个缓存项
cache.Remove("key2");

// 清空所有缓存
cache.Clear();
```

## 高级用法

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

// 使用完毕后释放资源
cache.Dispose();
```

## 使用场景

### API 响应缓存

```csharp
using WodToolkit.Http;
using WodToolkit.src.Cache;
using System.Net;

var cache = new TempCache<string, string>(TimeSpan.FromSeconds(30), 300);

async Task<string> GetApiData(string endpoint)
{
    // 检查缓存
    if (cache.TryGetValue(endpoint, out string cachedData))
    {
        Console.WriteLine("从缓存获取数据");
        return cachedData;
    }

    // 从 API 获取数据
    var httpRequest = new HttpRequestClass();
    httpRequest.Open(endpoint, HttpMethod.Get).Send();
    var response = httpRequest.GetResponse();

    if (response.StatusCode == 200)
    {
        // 缓存数据（5分钟）
        cache.Set(endpoint, response.Body, 300);
        Console.WriteLine("从 API 获取数据并缓存");
        return response.Body;
    }

    return null;
}
```

## 资源管理

### 使用 using 语句

```csharp
using (var cache = new TempCache<string, string>())
{
    cache.Set("key", "value");
    // 使用缓存...
} // 自动释放资源
```

## 注意事项

1. **内存使用**：缓存数据存储在内存中，注意控制缓存大小
2. **TTL 单位**：TTL 以秒为单位
3. **自动清理**：过期项会在清理间隔时自动删除
4. **线程安全**：`TempCache` 是线程安全的，可以在多线程环境中使用
5. **资源释放**：使用完毕后应调用 `Dispose()` 释放资源

## 相关文档

- [[HTTP|HTTP 请求处理]] - HTTP 客户端使用指南

