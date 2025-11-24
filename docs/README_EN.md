# WodToolKit

[English Version](README_EN.md) | [中文版本](../README.md)

A lightweight .NET toolkit that provides encapsulation of various common functionalities, designed to simplify development work and improve development efficiency. It has implemented HTTP request handling, Cookie management, JSON parsing, memory caching, thread pool, AES encryption, JavaScript execution, and other features.

## Features

- **HTTP Request Handling**: Simplified HTTP client operations, supporting various HTTP methods and request configurations, with support for HTTP/HTTPS and SOCKS4/SOCKS5 proxies
- **Cookie Management**: Complete cookie management functionality, supporting addition, retrieval, deletion, and batch operations
- **JSON Parsing**: Flexible JSON serialization and deserialization, supporting dynamic types and custom types
- **URL Tools**: URL parameter processing, sorting, and conversion utilities
- **AES Encryption**: Secure AES encryption and decryption functionality, supporting multiple encryption modes and padding methods
- **Memory Cache**: Memory-based temporary cache implementation with TTL settings and automatic cleanup
- **Thread Pool Management**: Simple and efficient thread pool implementation supporting task queuing and waiting
- **JavaScript Execution**: Supports two execution methods, JintRunner (pure .NET implementation, no Node.js required) and NodeJsRunner (requires Node.js), supporting code string and file execution, as well as calling specific methods with parameters
- **.NET Standard 2.1 Compatibility**: Supports .NET Core, .NET Framework, and other compatible platforms
- **Modular Design**: Each functional module is independent, facilitating extension and maintenance
- **Continuous Updates**: Plans to gradually add more common function modules

## Installation

Install via NuGet Package Manager:

```powershell
Install-Package WodToolKit
```

Or using .NET CLI:

```powershell
dotnet add package WodToolKit
```

## Quick Start

### Memory Cache Example

```csharp
using WodToolkit.src.Cache;

// Create temporary cache instance (cleans every 30 seconds, default TTL is 300 seconds)
var cache = new TempCache<string, string>(TimeSpan.FromSeconds(30), 300);

// Set cache items
cache.Set("key1", "value1");
cache.Set("key2", "value2", 60); // Custom TTL of 60 seconds

// Get cache items
if (cache.TryGetValue("key1", out string value))
{
    Console.WriteLine($"Cache value: {value}");
}

// Remove cache items
cache.Remove("key2");

// Clear cache
// cache.Clear();

// Dispose resources when done
// cache.Dispose();
```

### Thread Pool Usage Example

```csharp
using WodToolkit.src.Thread;

// Create thread pool (4 worker threads)
var threadPool = new SimpleThreadPool(4);

// Add tasks to thread pool
for (int i = 0; i < 10; i++)
{
    int taskId = i;
    threadPool.QueueTask(() => {
        Console.WriteLine($"Executing task {taskId}, Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        System.Threading.Thread.Sleep(100); // Simulate work
    });
}

// Wait for all tasks to complete
threadPool.Wait();

// Release thread pool resources
threadPool.Dispose();

Console.WriteLine("All tasks completed");
```

### HTTP Request Example

```csharp
using WodToolkit.Http;
using System.Net;
using System.Collections.Generic;

// 1. Send GET request
var httpRequest = new HttpRequestClass();
httpRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
var responseData = httpRequest.GetResponse();

// Process response
if (responseData.StatusCode == 200)
{
    Console.WriteLine(responseData.Body); // Response content
    Console.WriteLine(responseData.StatusCode); // Status code
    Console.WriteLine(responseData.ResponseHeaders); // Response headers
}

// 2. Send GET request with query parameters
var getWithParams = new HttpRequestClass();
getWithParams.Open("https://api.example.com/search?keyword=test&page=1", HttpMethod.Get).Send();
var searchResponse = getWithParams.GetResponse();

// 3. Send POST request with form data
var postRequest = new HttpRequestClass();
// Create form data
var formData = new Dictionary<string, string>
{
    { "username", "admin" },
    { "password", "password123" }
};
// Send POST request
postRequest.Open("https://api.example.com/login", HttpMethod.Post).Send(formData);
var loginResponse = postRequest.GetResponse();

// 4. Send POST request with JSON data
var jsonRequest = new HttpRequestClass();
// Send JSON request directly using anonymous object (automatically sets Content-Type to application/json)
// Send request
jsonRequest.Open("https://api.example.com/users", HttpMethod.Post).Send(new 
{
    name = "Test",
    age = 25,
});
var userResponse = jsonRequest.GetResponse();

// 5. Asynchronous request example
var asyncRequest = new HttpRequestClass();
// Set timeout
asyncRequest.SetTimeout(30); // 30 seconds
// Set UserAgent
asyncRequest.SetUserAgent("Mozilla/5.0 WodToolkit");
// Send asynchronous request
await asyncRequest.Open("https://api.example.com/data", HttpMethod.Get).SendAsync();
var asyncResponse = asyncRequest.GetResponse();

// 6. Request with Cookie Manager
var cookieRequest = new HttpRequestClass();
// Set Cookie
cookieRequest.SetCookieString("session=abc123; user=admin");
// Send request, will automatically include the set Cookie
cookieRequest.Open("https://api.example.com/protected", HttpMethod.Get).Send();
var cookieResponse = cookieRequest.GetResponse();
// Get Cookie from response
string cookies = cookieResponse.Cookie;
Console.WriteLine(cookies);

// 7. Request with HTTP proxy
var httpProxyRequest = new HttpRequestClass();
// Set HTTP proxy (original method, maintains backward compatibility)
httpProxyRequest.SetProxy("http://proxy.example.com:8080", "username", "password");
httpProxyRequest.Open("https://api.example.com/data", HttpMethod.Get).Send();
var httpProxyResponse = httpProxyRequest.GetResponse();

// 8. Request with SOCKS5 proxy
var socks5Request = new HttpRequestClass();
// Set SOCKS5 proxy (new method, supports all proxy types)
socks5Request.SetProxy(ProxyType.Socks5, "proxy.example.com", 1080, "username", "password");
socks5Request.Open("https://api.example.com/data", HttpMethod.Get).Send();
var socks5Response = socks5Request.GetResponse();

// 9. Request with SOCKS4 proxy
var socks4Request = new HttpRequestClass();
// Set SOCKS4 proxy (no username/password required)
socks4Request.SetProxy(ProxyType.Socks4, "proxy.example.com", 1080);
socks4Request.Open("https://api.example.com/data", HttpMethod.Get).Send();
var socks4Response = socks4Request.GetResponse();

// 10. Request with HTTP proxy (using new method)
var httpProxyRequest2 = new HttpRequestClass();
// New method also supports HTTP proxy
httpProxyRequest2.SetProxy(ProxyType.Http, "proxy.example.com", 8080, "username", "password");
httpProxyRequest2.Open("https://api.example.com/data", HttpMethod.Get).Send();
var httpProxyResponse2 = httpProxyRequest2.GetResponse();

// 11. Remove proxy settings
var request = new HttpRequestClass();
request.SetProxy("http://proxy.example.com:8080"); // Set proxy
request.RemoveProxy(); // Remove proxy, subsequent requests will connect directly
request.Open("https://api.example.com/data", HttpMethod.Get).Send();
```

### Cookie Management Example

```csharp
using WodToolKit.Http;

// Create Cookie manager
var cookieManager = new CookieManager();

// Set cookies
cookieManager.SetCookie("sessionId", "abc123");
cookieManager.SetCookie("userId", "user123");

// Get cookie string
string cookieString = cookieManager.GetCookieString();
Console.WriteLine(cookieString);

// Get single cookie
string sessionId = cookieManager.GetCookieValue("sessionId");
```

### JSON Parsing Example

```csharp
using WodToolKit.Json;

// Parse JSON string
string json = "{\"name\": \"Example\", \"value\": 42}";
dynamic result = EasyJson.ParseJsonToDynamic(json);

// Access dynamic object properties
Console.WriteLine(result.name); // Output: Example
Console.WriteLine(result.value); // Output: 42

// Parse to strongly typed object
var obj = EasyJson.ParseJsonObject<MyClass>(json);
```

### AES Encryption Example

```csharp
using WodToolKit.Encode;

// Create AES encryption instance
var aes = new AesCrypt();

// Encrypt string
string plainText = "Hello, World!";
string key = "YourSecretKey123456";
string encrypted = aes.Encrypt(plainText, key);
Console.WriteLine($"Encrypted: {encrypted}");

// Decrypt string
string decrypted = aes.Decrypt(encrypted, key);
Console.WriteLine($"Decrypted: {decrypted}");
```

### JavaScript Execution and Method Call Example

WodToolKit provides two JavaScript executors: `NodeJsRunner` (requires Node.js) and `JintRunner` (pure .NET implementation, no Node.js needed).

#### Method 1: Using JintRunner (Recommended, No Node.js Required)

```csharp
using WodToolkit.Script;

// Create Jint executor (pure .NET implementation, no Node.js installation needed)
using (var jintRunner = new JintRunner())
{
    // 1. Basic JavaScript code execution
    var result = await jintRunner.ExecuteScriptAsync(@"
        function test() {
            console.log('Hello from Jint!');
            return 42;
        }
        
        test();");
    
    Console.WriteLine($"Success: {result.Success}");
    Console.WriteLine($"Output: {result.Output}"); // Output: Hello from Jint!
    
    // 2. Execute JavaScript file
    var fileResult = await jintRunner.ExecuteScriptFileAsync("./test_script.js");
    Console.WriteLine($"File execution result: {fileResult.Output}");
    
    // 3. Call method from JavaScript file
    var addResult = await jintRunner.CallMethodAsync("./test_script.js", "add", 5, 3);
    if (addResult.Success)
    {
        int sum = jintRunner.GetResult<int>(addResult);
        Console.WriteLine($"5 + 3 = {sum}"); // Output: 8
    }
    
    // 4. Call method from code string
    string scriptCode = @"
        function multiply(a, b) {
            return a * b;
        }
        
        module.exports = { multiply };
    ";
    
    var multiplyResult = await jintRunner.CallMethodFromScriptAsync(scriptCode, "multiply", 7, 8);
    if (multiplyResult.Success)
    {
        int product = jintRunner.GetResult<int>(multiplyResult);
        Console.WriteLine($"7 * 8 = {product}"); // Output: 56
    }
    
    // 5. Pass complex object parameters
    var user = new {
        firstName = "John",
        lastName = "Doe",
        email = "john@example.com",
        age = 25
    };
    
    var userResult = await jintRunner.CallMethodAsync("./test_script.js", "processUser", user);
    if (userResult.Success)
    {
        dynamic processedUser = jintRunner.GetResult<dynamic>(userResult);
        Console.WriteLine($"Full Name: {processedUser.fullName}"); // Output: John Doe
        Console.WriteLine($"Email: {processedUser.email}");
        Console.WriteLine($"Is Adult: {processedUser.isAdult}"); // Output: true
    }
    
    // 6. Handle arrays and collections
    var numbers = new int[] { 1, 2, 3, 4, 5 };
    var sumResult = await jintRunner.CallMethodFromScriptAsync(@"
        function sumArray(arr) {
            return arr.reduce((a, b) => a + b, 0);
        }
        module.exports = { sumArray };
    ", "sumArray", numbers);
    
    if (sumResult.Success)
    {
        int total = jintRunner.GetResult<int>(sumResult);
        Console.WriteLine($"Array sum: {total}"); // Output: 15
    }
    
    // 7. Synchronous method calls (for simple scenarios)
    var syncResult = jintRunner.ExecuteScript("console.log('Synchronous execution'); 1 + 1;");
    Console.WriteLine($"Synchronous execution result: {syncResult.Output}");
    
    // 8. Error handling
    var errorResult = await jintRunner.ExecuteScriptAsync("throw new Error('Test error');");
    if (!errorResult.Success)
    {
        Console.WriteLine($"Execution failed: {errorResult.Error}");
    }
}
```

#### Method 2: Using NodeJsRunner (Requires Node.js Installation)

```csharp
using WodToolkit.Script;

// Create Node.js executor (requires Node.js to be installed on the system)
using (var nodeRunner = new NodeJsRunner())
{
    // Check if Node.js is available
    if (!nodeRunner.IsNodeAvailable())
    {
        Console.WriteLine("Node.js is not installed or not available");
        return;
    }
    
    // 1. Basic JavaScript code execution
    var result = await nodeRunner.ExecuteScriptAsync(@"
        function test() {
            console.log('Hello from Node.js!');
            return 42;
        }
        
        test();");
    
    Console.WriteLine($"Success: {result.Success}");
    Console.WriteLine($"Output: {result.Output}");
    
    // 2. Call method from JavaScript file
    var addResult = await nodeRunner.CallMethodAsync("./test_script.js", "add", 5, 3);
    if (addResult.Success)
    {
        int sum = nodeRunner.GetResult<int>(addResult);
        Console.WriteLine($"5 + 3 = {sum}");
    }
    
    // 3. Call async functions (Node.js supports full async/await)
    var asyncResult = await nodeRunner.CallMethodAsync("./test_script.js", "fetchData", 123);
    if (asyncResult.Success)
    {
        dynamic data = nodeRunner.GetResult<dynamic>(asyncResult);
        Console.WriteLine($"ID: {data.id}");
        Console.WriteLine($"Name: {data.name}");
    }
}
```

#### JavaScript File Example (test_script.js)

```javascript
// Simple addition function
function add(a, b) {
    return a + b;
}

// Object processing function
function processUser(user) {
    return {
        fullName: `${user.firstName} ${user.lastName}`,
        email: user.email,
        isAdult: user.age >= 18
    };
}

// Array processing function
function sumArray(arr) {
    return arr.reduce((a, b) => a + b, 0);
}

// String processing function
function formatMessage(name, age) {
    return `Name: ${name}, Age: ${age}`;
}

// Async function (Node.js support)
async function fetchData(id) {
    await new Promise(resolve => setTimeout(resolve, 300));
    return {
        id: id,
        name: `Item ${id}`,
        status: 'active'
    };
}

// Export functions (CommonJS format)
module.exports = {
    add,
    processUser,
    sumArray,
    formatMessage,
    fetchData
};
```

#### Usage Recommendations

- **Recommended: Use JintRunner**: No Node.js installation required, better performance, easier integration
- **Use NodeJsRunner**: When you need full Node.js ecosystem support (e.g., npm packages, Node.js APIs)
- **Both executors have identical APIs**: Easy to switch between them without modifying calling code

## Project Architecture and Organization

WodToolKit adopts a modular design where each functional module is independent, facilitating extension and maintenance. The overall architecture revolves around core functional modules, providing a unified user experience through clear namespaces and class hierarchies.

### Core Module Relationships

```
WodToolKit
├── Http/          # HTTP request and Cookie management
│   ├── HttpRequestClass  # HTTP client implementation
│   └── CookieManager     # Cookie management
├── Json/          # JSON processing
│   └── EasyJson          # Simplified JSON serialization and deserialization
├── Cache/         # Caching functionality
│   └── TempCache         # Memory cache implementation
├── Thread/        # Thread management
│   └── SimpleThreadPool  # Thread pool implementation
├── Encode/        # Encryption functionality
│   └── AesCrypt          # AES encryption and decryption
└── Script/        # Script execution
    └── NodeJsRunner      # Node.js script executor
```

### Project Structure

```
├── src/
│   ├── Cache/          # Cache related functionality
│   ├── Encode/         # Encryption related functionality
│   ├── Http/           # HTTP related functionality
│   ├── Json/           # JSON processing functionality
│   ├── Script/         # JavaScript execution and method call functionality
│   └── Thread/         # Thread management functionality
├── WodToolkit.csproj   # Project file
└── README.md           # Project documentation
```

## Dependencies

- System.Text.Json - JSON processing
- Microsoft.CSharp - Dynamic type support
- System.Security.Cryptography - Encryption functionality support

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Issue Reporting and Contributions

### Reporting Issues

We highly value your user experience. If you encounter any problems or have any suggestions during use, please provide feedback through the following methods:

1. **GitHub Issues**: Create a new Issue in the project repository, detailing the problem you encountered or the feature suggestion you have
   - Please provide detailed reproduction steps as much as possible
   - In case of errors, please provide complete error information and stack traces
   - Specify the WodToolKit version and environment information you are using

2. **Code Contributions**

   We warmly welcome community contributions! If you would like to contribute code to the project, please follow these steps:

   1. **Fork the project repository**
      - Click the "Fork" button on GitHub to create your own copy of the repository

   2. **Clone the repository**
      ```bash
      git clone https://github.com/thiswod/WodToolKit.git
      cd WodToolKit
      ```

   3. **Create a branch**
      ```bash
      git checkout -b feature/your-feature-name
      ```

   4. **Implement features or fix issues**
      - Ensure your code style is consistent with the project
      - Add appropriate test cases for new features
      - Update relevant documentation

   5. **Commit changes**
      ```bash
      git commit -m "Describe your changes"
      ```

   6. **Push to your Fork**
      ```bash
      git push origin feature/your-feature-name
      ```

   7. **Create a Pull Request**
      - Navigate to the original repository on GitHub
      - Click the "New Pull Request" button
      - Select your branch and submit the PR
      - Please provide a detailed description of your changes and their purpose in the PR description

### Development Guidelines

To maintain code quality and consistency, please follow these development guidelines:

1. Code style: Follow C# standard code conventions
2. Comments: Add appropriate XML documentation comments for public methods and classes
3. Testing: New features should include unit tests
4. Compatibility: Ensure code is compatible with .NET Standard 2.1

## Author

Wod