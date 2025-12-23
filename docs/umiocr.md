# OCR 识别

WodToolKit 集成了 UmiOCR，提供了便捷的图片文字识别功能。UmiOCR 是一个开源的 OCR 识别服务，支持多种语言和输出格式。

## 功能概述

`UmiOCR` 类提供了以下功能：

- 图片文字识别
- 支持多种识别语言（中文、英文、中英混合等）
- 支持多种输出格式（text、json、jsonl）
- 支持角度自动检测
- 支持忽略区域设置
- 支持自定义 UmiOCR 服务地址

## 前置要求

使用 UmiOCR 功能前，需要先启动 UmiOCR 服务。UmiOCR 是一个独立的 OCR 服务程序，默认运行在 `http://127.0.0.1:1224`。

### 安装 UmiOCR

1. 访问 [UmiOCR 官网](https://github.com/hiroi-sora/Umi-OCR) 下载并安装
2. 启动 UmiOCR 服务
3. 确保服务运行在默认端口 1224（或使用自定义地址）

## 基本使用

### 创建 UmiOCR 实例

```csharp
using WodToolkit.src.UmiOCR;

// 使用默认地址（http://127.0.0.1:1224）
var umiOcr = new UmiOCR();

// 使用自定义地址
var customOcr = new UmiOCR("http://192.168.1.100:1224");
```

### 基本 OCR 识别

```csharp
// 最简单的使用方式
string result = umiOcr.Ocr("image.jpg");
Console.WriteLine(result);
```

## 参数说明

`Ocr` 方法支持以下参数：

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `file` | `string` | 必填 | 图片文件路径 |
| `language` | `string` | `"简体中文"` | 识别语言，可选值：简体中文、繁体中文、英文、日文等 |
| `format` | `string` | `"text"` | 输出格式，可选值：`text`（纯文本）、`json`、`jsonl` |
| `parser` | `string` | `"none"` | 后处理解析器，可选值：`none`（无）、`merge_line`、`merge_line_v2` 等 |
| `angle` | `bool` | `false` | 是否自动检测图片角度 |
| `ignoreArea` | `List<int[][]>` | `null` | 忽略区域数组，每一项为 `[[左上角x,y],[右下角x,y]]` |

## 使用示例

### 指定识别语言

```csharp
// 识别简体中文
string result = umiOcr.Ocr("image.jpg", language: "简体中文");

// 识别英文
string result = umiOcr.Ocr("image.jpg", language: "英文");

// 识别中英混合
string result = umiOcr.Ocr("image.jpg", language: "简体中文+英文");
```

### 指定输出格式

```csharp
// 输出纯文本（默认）
string textResult = umiOcr.Ocr("image.jpg", format: "text");

// 输出 JSON 格式
string jsonResult = umiOcr.Ocr("image.jpg", format: "json");

// 输出 JSONL 格式
string jsonlResult = umiOcr.Ocr("image.jpg", format: "jsonl");
```

### 启用角度检测

```csharp
// 自动检测图片角度并旋转
string result = umiOcr.Ocr("image.jpg", angle: true);
```

### 设置忽略区域

忽略区域用于指定图片中不需要识别的区域，每个区域由左上角和右下角坐标定义。

```csharp
using System.Collections.Generic;

// 定义忽略区域
var ignoreAreas = new List<int[][]>
{
    // 第一个忽略区域：从 (10, 20) 到 (100, 200)
    new int[][] { new int[] { 10, 20 }, new int[] { 100, 200 } },
    
    // 第二个忽略区域：从 (150, 250) 到 (300, 400)
    new int[][] { new int[] { 150, 250 }, new int[] { 300, 400 } }
};

string result = umiOcr.Ocr("image.jpg", ignoreArea: ignoreAreas);
```

### 使用后处理解析器

```csharp
// 不使用后处理（默认）
string result1 = umiOcr.Ocr("image.jpg", parser: "none");

// 使用行合并解析器
string result2 = umiOcr.Ocr("image.jpg", parser: "merge_line");

// 使用行合并解析器 v2
string result3 = umiOcr.Ocr("image.jpg", parser: "merge_line_v2");
```

### 完整参数示例

```csharp
var ignoreAreas = new List<int[][]>
{
    new int[][] { new int[] { 10, 20 }, new int[] { 100, 200 } }
};

string result = umiOcr.Ocr(
    file: "image.jpg",
    language: "简体中文",
    format: "json",
    parser: "merge_line",
    angle: true,
    ignoreArea: ignoreAreas
);
```

## 错误处理

```csharp
try
{
    string result = umiOcr.Ocr("image.jpg");
    Console.WriteLine($"识别成功: {result}");
}
catch (Exception ex)
{
    Console.WriteLine($"OCR 识别失败: {ex.Message}");
}
```

## API 参考

### UmiOCR 构造函数

```csharp
public UmiOCR(string? Url = "http://127.0.0.1:1224")
```

**参数**：
- `Url` (string?, 可选): UmiOCR 服务地址，默认为 `"http://127.0.0.1:1224"`

### Ocr 方法

```csharp
public string Ocr(
    string file, 
    string language = "简体中文", 
    string format = "text", 
    string parser = "none", 
    bool angle = false, 
    List<int[][]> ignoreArea = null
)
```

**参数**：
- `file` (string): 图片文件路径
- `language` (string): 识别语言，默认为 `"简体中文"`
- `format` (string): 输出格式，默认为 `"text"`
- `parser` (string): 后处理解析器，默认为 `"none"`
- `angle` (bool): 是否自动检测图片角度，默认为 `false`
- `ignoreArea` (List<int[][]>): 忽略区域数组，默认为 `null`

**返回值**：
- `string`: OCR 识别结果字符串

**异常**：
- 如果 OCR 识别失败，会抛出 `Exception` 异常，异常消息包含失败原因

## 注意事项

1. **服务要求**：使用前需要确保 UmiOCR 服务正在运行
2. **图片格式**：支持常见的图片格式（jpg、png、bmp 等）
3. **文件路径**：支持绝对路径和相对路径
4. **性能考虑**：OCR 识别是网络请求，可能需要一定时间，建议在异步环境中使用
5. **错误处理**：建议使用 try-catch 捕获可能的异常

## 使用场景

- 图片文字提取
- 文档扫描识别
- 截图文字识别
- 批量图片 OCR 处理
- 表单数据提取

## 相关链接

- [UmiOCR 官网](https://github.com/hiroi-sora/Umi-OCR)
- [快速开始](../README.md#快速开始)
- [项目主页](../README.md)

