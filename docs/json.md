---
layout: default
title: JSON处理
---

# JSON 处理

WodToolKit 提供了灵活的 JSON 序列化和反序列化功能，支持动态类型和自定义类型。

## 基本用法

### 解析为动态类型

```csharp
using WodToolkit.Json;

string json = "{\"name\": \"Example\", \"value\": 42, \"active\": true}";

// 解析为动态对象
dynamic result = EasyJson.ParseJsonToDynamic(json);

// 访问属性
Console.WriteLine(result.name);    // 输出: Example
Console.WriteLine(result.value);   // 输出: 42
Console.WriteLine(result.active);  // 输出: true
```

### 解析为强类型对象

```csharp
// 定义类
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}

// 解析 JSON
string json = "{\"name\": \"John\", \"age\": 25, \"email\": \"john@example.com\"}";
var user = EasyJson.ParseJsonObject<User>(json);

Console.WriteLine($"姓名: {user.Name}");
Console.WriteLine($"年龄: {user.Age}");
```

## 序列化对象

```csharp
var user = new
{
    Name = "John",
    Age = 25,
    Email = "john@example.com"
};

// 序列化为 JSON 字符串
string json = EasyJson.SerializeObject(user);
Console.WriteLine(json);
// 输出: {"name":"John","age":25,"email":"john@example.com"}
```

## 处理复杂对象

### 嵌套对象

```csharp
string json = @"{
    ""user"": {
        ""name"": ""John"",
        ""address"": {
            ""city"": ""Beijing"",
            ""country"": ""China""
        }
    }
}";

dynamic result = EasyJson.ParseJsonToDynamic(json);
Console.WriteLine(result.user.name);              // 输出: John
Console.WriteLine(result.user.address.city);      // 输出: Beijing
```

### 数组处理

```csharp
string json = @"{
    ""users"": [
        {""name"": ""John"", ""age"": 25},
        {""name"": ""Jane"", ""age"": 30}
    ]
}";

dynamic result = EasyJson.ParseJsonToDynamic(json);

// 访问数组
foreach (var user in result.users)
{
    Console.WriteLine($"{user.name} - {user.age}");
}
```

## 完整示例

```csharp
using WodToolkit.Json;

// 创建对象
var data = new
{
    Name = "WodToolKit",
    Version = "1.0.0",
    Features = new[] { "HTTP", "JSON", "Cache" },
    Author = new
    {
        Name = "Wod",
        Email = "wod@example.com"
    }
};

// 序列化
string json = EasyJson.SerializeObject(data);
Console.WriteLine("序列化结果:");
Console.WriteLine(json);

// 反序列化
dynamic parsed = EasyJson.ParseJsonToDynamic(json);
Console.WriteLine("\n反序列化结果:");
Console.WriteLine($"名称: {parsed.Name}");
Console.WriteLine($"版本: {parsed.Version}");
Console.WriteLine($"作者: {parsed.Author.Name}");
```

