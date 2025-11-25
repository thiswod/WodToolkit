# 安装指南

本指南将帮助您安装 WodToolKit 到您的项目中。

## 系统要求

- .NET Standard 2.1 或更高版本
- 支持以下平台：
  - .NET Core 3.0+
  - .NET Framework 4.6.1+
  - .NET 5.0+
  - .NET 6.0+
  - .NET 7.0+
  - .NET 8.0+

## 安装方法

### 方法一：通过 NuGet 包管理器（推荐）

在 Visual Studio 中：

1. 右键点击项目 → **管理 NuGet 程序包**
2. 搜索 `WodToolKit`
3. 点击 **安装**

或者使用包管理器控制台：

```powershell
Install-Package WodToolKit
```

### 方法二：通过 .NET CLI

```bash
dotnet add package WodToolKit
```

### 方法三：通过 PackageReference

在项目文件（`.csproj`）中添加：

```xml
<ItemGroup>
  <PackageReference Include="WodToolKit" Version="1.0.0.3" />
</ItemGroup>
```

### 方法四：通过 Package Manager UI

1. 在 Visual Studio 中，选择 **工具** → **NuGet 包管理器** → **程序包管理器控制台**
2. 运行以下命令：

```powershell
Install-Package WodToolKit
```

## 验证安装

安装完成后，您可以通过以下方式验证：

```csharp
using WodToolkit.Http;

// 如果能够成功编译，说明安装成功
var httpRequest = new HttpRequestClass();
Console.WriteLine("WodToolKit 安装成功！");
```

## 依赖项

WodToolKit 会自动安装以下依赖项：

- `System.Text.Json` (9.0.10) - JSON 处理
- `Microsoft.CSharp` (4.7.0) - 动态类型支持
- `System.Security.Cryptography.Algorithms` (4.3.0) - 加密功能支持
- `Jint` (4.4.2) - JavaScript 执行引擎（用于 JintRunner）

## 更新包

要更新到最新版本：

```powershell
Update-Package WodToolKit
```

或使用 .NET CLI：

```bash
dotnet add package WodToolKit --version <最新版本号>
```

## 卸载

如果需要卸载 WodToolKit：

```powershell
Uninstall-Package WodToolKit
```

或使用 .NET CLI：

```bash
dotnet remove package WodToolKit
```

## 下一步

安装完成后，请查看 [[快速开始|Quick-Start]] 指南开始使用 WodToolKit。

---

**Language**: 中文 (Chinese) | [English](Installation-EN)

