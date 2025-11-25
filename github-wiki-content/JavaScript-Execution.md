# JavaScript 执行

WodToolKit 提供了两种 JavaScript 执行方式：**JintRunner**（推荐）和 **NodeJsRunner**。

## JintRunner（推荐）

JintRunner 是纯 .NET 实现，无需安装 Node.js，性能更好，集成更方便。

### 基本用法

```csharp
using WodToolkit.Script;

using (var jintRunner = new JintRunner())
{
    // 执行 JavaScript 代码
    var result = await jintRunner.ExecuteScriptAsync(@"
        console.log('Hello from Jint!');
        return 42;
    ");
    
    Console.WriteLine($"输出: {result.Output}");
    Console.WriteLine($"成功: {result.Success}");
}
```

### 调用方法

```csharp
using (var jintRunner = new JintRunner())
{
    string scriptCode = @"
        function add(a, b) {
            return a + b;
        }
        module.exports = { add };
    ";
    
    var result = await jintRunner.CallMethodFromScriptAsync(scriptCode, "add", 5, 3);
    if (result.Success)
    {
        int sum = jintRunner.GetResult<int>(result);
        Console.WriteLine($"5 + 3 = {sum}"); // 输出: 8
    }
}
```

### 处理复杂对象

```csharp
using (var jintRunner = new JintRunner())
{
    var user = new {
        firstName = "John",
        lastName = "Doe",
        age = 25
    };
    
    var result = await jintRunner.CallMethodFromScriptAsync(@"
        function processUser(user) {
            return {
                fullName: user.firstName + ' ' + user.lastName,
                isAdult: user.age >= 18
            };
        }
        module.exports = { processUser };
    ", "processUser", user);
    
    if (result.Success)
    {
        dynamic processed = jintRunner.GetResult<dynamic>(result);
        Console.WriteLine($"全名: {processed.fullName}");
        Console.WriteLine($"是否成年: {processed.isAdult}");
    }
}
```

### 处理数组

```csharp
using (var jintRunner = new JintRunner())
{
    var numbers = new int[] { 1, 2, 3, 4, 5 };
    
    var result = await jintRunner.CallMethodFromScriptAsync(@"
        function sumArray(arr) {
            return arr.reduce((a, b) => a + b, 0);
        }
        module.exports = { sumArray };
    ", "sumArray", numbers);
    
    if (result.Success)
    {
        int total = jintRunner.GetResult<int>(result);
        Console.WriteLine($"总和: {total}"); // 输出: 15
    }
}
```

## NodeJsRunner

NodeJsRunner 需要系统安装 Node.js，支持完整的 Node.js 生态系统。

### 基本用法

```csharp
using WodToolkit.Script;

using (var nodeRunner = new NodeJsRunner())
{
    // 检查 Node.js 是否可用
    if (!nodeRunner.IsNodeAvailable())
    {
        Console.WriteLine("Node.js 未安装");
        return;
    }
    
    // 执行 JavaScript 文件
    var result = await nodeRunner.ExecuteScriptFileAsync("./script.js");
    Console.WriteLine(result.Output);
}
```

### 调用文件中的方法

```csharp
using (var nodeRunner = new NodeJsRunner())
{
    // 调用 script.js 文件中的 add 方法
    var result = await nodeRunner.CallMethodAsync("./script.js", "add", 5, 3);
    if (result.Success)
    {
        int sum = nodeRunner.GetResult<int>(result);
        Console.WriteLine($"5 + 3 = {sum}");
    }
}
```

## JavaScript 文件示例

创建 `test_script.js` 文件：

```javascript
// 简单的加法函数
function add(a, b) {
    return a + b;
}

// 对象处理函数
function processUser(user) {
    return {
        fullName: `${user.firstName} ${user.lastName}`,
        email: user.email,
        isAdult: user.age >= 18
    };
}

// 数组处理函数
function sumArray(arr) {
    return arr.reduce((a, b) => a + b, 0);
}

// 导出函数（CommonJS 格式）
module.exports = {
    add,
    processUser,
    sumArray
};
```

## 使用建议

**推荐使用 JintRunner**：无需安装 Node.js，性能更好，集成更方便。

**使用 NodeJsRunner**：当需要完整的 Node.js 生态系统支持时（如 npm 包、Node.js API 等）。

## API 兼容性

两种执行器具有完全相同的 API，可以轻松切换：

```csharp
// 可以轻松切换
IJavaScriptRunner runner = new JintRunner(); // 或 new NodeJsRunner()

var result = await runner.ExecuteScriptAsync("console.log('Hello');");
```

