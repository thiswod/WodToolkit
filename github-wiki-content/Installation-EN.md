# Installation Guide

This guide will help you install WodToolKit into your project.

## System Requirements

- .NET Standard 2.1 or higher
- Supports the following platforms:
  - .NET Core 3.0+
  - .NET Framework 4.6.1+
  - .NET 5.0+
  - .NET 6.0+
  - .NET 7.0+
  - .NET 8.0+

## Installation Methods

### Method 1: Via NuGet Package Manager (Recommended)

In Visual Studio:

1. Right-click the project → **Manage NuGet Packages**
2. Search for `WodToolKit`
3. Click **Install**

Or use Package Manager Console:

```powershell
Install-Package WodToolKit
```

### Method 2: Via .NET CLI

```bash
dotnet add package WodToolKit
```

### Method 3: Via PackageReference

Add to your project file (`.csproj`):

```xml
<ItemGroup>
  <PackageReference Include="WodToolKit" Version="1.0.0.3" />
</ItemGroup>
```

### Method 4: Via Package Manager UI

1. In Visual Studio, select **Tools** → **NuGet Package Manager** → **Package Manager Console**
2. Run the following command:

```powershell
Install-Package WodToolKit
```

## Verify Installation

After installation, you can verify it with:

```csharp
using WodToolkit.Http;

// If it compiles successfully, installation is successful
var httpRequest = new HttpRequestClass();
Console.WriteLine("WodToolKit installed successfully!");
```

## Dependencies

WodToolKit will automatically install the following dependencies:

- `System.Text.Json` (9.0.10) - JSON processing
- `Microsoft.CSharp` (4.7.0) - Dynamic type support
- `System.Security.Cryptography.Algorithms` (4.3.0) - Encryption functionality support
- `Jint` (4.4.2) - JavaScript execution engine (for JintRunner)

## Update Package

To update to the latest version:

```powershell
Update-Package WodToolKit
```

Or use .NET CLI:

```bash
dotnet add package WodToolKit --version <latest-version>
```

## Uninstall

If you need to uninstall WodToolKit:

```powershell
Uninstall-Package WodToolKit
```

Or use .NET CLI:

```bash
dotnet remove package WodToolKit
```

## Next Steps

After installation, please check the [[Quick Start|Quick-Start-EN]] guide to start using WodToolKit.

---

**Language**: [中文 (Chinese)](Installation) | English

