# Quick Start

This guide will help you get started with WodToolKit in 5 minutes.

## Prerequisites

- WodToolKit installed (see [[Installation Guide|Installation-EN]])
- Basic C# programming knowledge

## First Example: HTTP Request

Let's start with a simple HTTP GET request:

```csharp
using WodToolkit.Http;
using System.Net;

// Create HTTP request instance
var httpRequest = new HttpRequestClass();

// Send GET request
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();

// Get response
var response = httpRequest.GetResponse();

// Handle response
if (response.StatusCode == 200)
{
    Console.WriteLine($"Response content: {response.Body}");
    Console.WriteLine($"Status code: {response.StatusCode}");
}
```

## Example 2: Cookie Management

```csharp
using WodToolkit.Http;

// Create Cookie manager
var cookieManager = new CookieManager();

// Set cookies
cookieManager.SetCookie("sessionId", "abc123");
cookieManager.SetCookie("userId", "user456");

// Get cookie string
string cookieString = cookieManager.GetCookieString();
Console.WriteLine(cookieString); // Output: sessionId=abc123; userId=user456
```

## Example 3: JSON Processing

```csharp
using WodToolkit.Json;

// Parse JSON string
string json = "{\"name\": \"Example\", \"value\": 42}";
dynamic result = EasyJson.ParseJsonToDynamic(json);

// Access dynamic object properties
Console.WriteLine(result.name);  // Output: Example
Console.WriteLine(result.value); // Output: 42
```

## Example 4: Memory Cache

```csharp
using WodToolkit.src.Cache;

// Create cache instance (cleanup every 30 seconds, default TTL 300 seconds)
var cache = new TempCache<string, string>(TimeSpan.FromSeconds(30), 300);

// Set cache items
cache.Set("key1", "value1");
cache.Set("key2", "value2", 60); // Custom TTL 60 seconds

// Get cache item
if (cache.TryGetValue("key1", out string value))
{
    Console.WriteLine($"Cache value: {value}");
}
```

## Example 5: JavaScript Execution

```csharp
using WodToolkit.Script;

// Use JintRunner (recommended, no Node.js required)
using (var jintRunner = new JintRunner())
{
    var result = await jintRunner.ExecuteScriptAsync(@"
        function add(a, b) {
            return a + b;
        }
        module.exports = { add };
    ");
    
    var addResult = await jintRunner.CallMethodFromScriptAsync(
        result.Output, "add", 5, 3);
    
    if (addResult.Success)
    {
        int sum = jintRunner.GetResult<int>(addResult);
        Console.WriteLine($"5 + 3 = {sum}"); // Output: 8
    }
}
```

## Example 6: AES Encryption

```csharp
using WodToolkit.Encode;

// Create AES encryption instance
var aes = new AesCrypt();

// Encrypt string
string plainText = "Hello, World!";
string key = "YourSecretKey123456"; // Must be 32 bytes
string encrypted = aes.Encrypt(plainText, key);
Console.WriteLine($"Encrypted: {encrypted}");

// Decrypt string
string decrypted = aes.Decrypt(encrypted, key);
Console.WriteLine($"Decrypted: {decrypted}");
```

## Example 7: Thread Pool

```csharp
using WodToolkit.src.Thread;

// Create thread pool (4 worker threads)
var threadPool = new SimpleThreadPool(4);

// Add tasks to thread pool
for (int i = 0; i < 10; i++)
{
    int taskId = i;
    threadPool.QueueTask(() => {
        Console.WriteLine($"Executing task {taskId}");
        System.Threading.Thread.Sleep(100);
    });
}

// Wait for all tasks to complete
threadPool.Wait();

// Release thread pool resources
threadPool.Dispose();
```

## Complete Example: Combined Usage

```csharp
using WodToolkit.Http;
using WodToolkit.Json;
using WodToolkit.src.Cache;
using System.Net;

// 1. Create cache
var cache = new TempCache<string, string>(TimeSpan.FromSeconds(30), 300);

// 2. Check cache
if (!cache.TryGetValue("api_data", out string cachedData))
{
    // 3. Send HTTP request
    var httpRequest = new HttpRequestClass();
    httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
    var response = httpRequest.GetResponse();
    
    // 4. Parse JSON
    dynamic jsonData = EasyJson.ParseJsonToDynamic(response.Body);
    
    // 5. Cache result
    cache.Set("api_data", response.Body, 300);
    
    Console.WriteLine($"Got data: {jsonData.name}");
}
else
{
    Console.WriteLine("Getting data from cache");
    dynamic jsonData = EasyJson.ParseJsonToDynamic(cachedData);
    Console.WriteLine($"Cached data: {jsonData.name}");
}
```

## Next Steps

Now that you understand the basics of WodToolKit, you can check the following detailed documentation:

- [[HTTP Request Handling|HTTP-EN]] - Deep dive into HTTP functionality
- [[Cookie Management|Cookie-EN]] - Cookie management details
- [[JSON Processing|JSON-EN]] - JSON operation guide
- [[Memory Cache|Cache-EN]] - Cache usage instructions
- [[JavaScript Execution|JavaScript-Execution-EN]] - JavaScript execution details
- [[AES Encryption|AES-Encryption-EN]] - Encryption functionality
- [[Thread Pool Management|Thread-Pool-EN]] - Thread pool usage guide

## Get Help

If you encounter problems during use:

1. Check [[FAQ|FAQ-EN]]
2. Check [[API Reference|API-Reference-EN]]
3. Submit [GitHub Issue](https://github.com/thiswod/WodToolKit/issues)

---

**Language**: [中文 (Chinese)](Quick-Start) | English

