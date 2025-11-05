# WodToolKit

A lightweight .NET toolkit that provides encapsulation of various common functionalities, designed to simplify development work and improve development efficiency. Currently, it implements HTTP request handling, Cookie management, and JSON parsing, with plans to continuously expand with more practical tools in the future.

## Features

- **HTTP Request Handling**: Simplified HTTP client operations, supporting various HTTP methods and request configurations
- **Cookie Management**: Complete cookie management functionality, supporting addition, retrieval, deletion, and batch operations
- **JSON Parsing**: Flexible JSON serialization and deserialization, supporting dynamic types and custom types
- **URL Tools**: URL parameter processing, sorting, and conversion utilities
- **AES Encryption**: Secure AES encryption and decryption functionality, supporting multiple encryption modes and padding methods
- **Memory Cache**: Memory-based temporary cache implementation with TTL settings and automatic cleanup
- **Thread Pool Management**: Simple and efficient thread pool implementation supporting task queuing and waiting
- **JavaScript Execution**: Execute JavaScript code via Node.js, supporting code string and file execution, as well as calling specific methods with parameters
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
using WodToolKit.Http;

// Create HTTP client
var httpClient = new HttpClient();

// Send GET request
var response = await httpClient.GetAsync("https://api.example.com/data");

// Process response
if (response.IsSuccessStatusCode)
{
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine(content);
}
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

```csharp
using WodToolKit.Script;

// Create Node.js executor (searches for node in system PATH by default)
using (var nodeRunner = new NodeJsRunner())
{
    // 1. Basic JavaScript code execution
    var result = await nodeRunner.ExecuteScriptAsync(@"
        function test() {
            console.log('Hello from JavaScript!');
            return 42;
        }
        
        test();");
    
    Console.WriteLine($"Success: {result.Success}");
    Console.WriteLine($"Output: {result.Output}");
    
    // 2. Call method from JavaScript file
    var addResult = await nodeRunner.CallMethodAsync("./test_script.js", "add", 5, 3);
    if (addResult.Success)
    {
        // Parse return result
        int sum = nodeRunner.GetResult<int>(addResult);
        Console.WriteLine($"5 + 3 = {sum}");
    }
    
    // 3. Pass object parameters
    var user = new {
        firstName = "John",
        lastName = "Doe",
        email = "john@example.com",
        age = 25
    };
    
    var userResult = await nodeRunner.CallMethodAsync("./test_script.js", "processUser", user);
    if (userResult.Success)
    {
        // Parse result to dynamic type
        dynamic processedUser = nodeRunner.GetResult<dynamic>(userResult);
        Console.WriteLine($"Full Name: {processedUser.fullName}");
        Console.WriteLine($"Email: {processedUser.email}");
        Console.WriteLine($"Is Adult: {processedUser.isAdult}");
    }
    
    // 4. Call method from code string
    string scriptCode = @"
        function multiply(a, b) {
            return a * b;
        }
        
        module.exports = { multiply };
    ";
    
    var multiplyResult = await nodeRunner.CallMethodFromScriptAsync(scriptCode, "multiply", 7, 8);
    if (multiplyResult.Success)
    {
        int product = nodeRunner.GetResult<int>(multiplyResult);
        Console.WriteLine($"7 * 8 = {product}");
    }
    
    // 5. Call async function
    var asyncResult = await nodeRunner.CallMethodAsync("./test_script.js", "fetchData", 123);
    if (asyncResult.Success)
    {
        dynamic data = nodeRunner.GetResult<dynamic>(asyncResult);
        Console.WriteLine($"ID: {data.id}");
        Console.WriteLine($"Name: {data.name}");
    }
}

/*
JavaScript file example (test_script.js):

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

// Async function
async function fetchData(id) {
    await new Promise(resolve => setTimeout(resolve, 300));
    return {
        id: id,
        name: `Item ${id}`,
        status: 'active'
    };
}

// Export functions
module.exports = {
    add,
    processUser,
    fetchData
};
*/
```

## Project Structure

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

## Contributions

Issues and Pull Requests are welcome!

## Author

Wod