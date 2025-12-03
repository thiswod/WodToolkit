# 身份证验证与信息提取

WodToolKit 提供了完整的中国身份证号码验证和信息提取功能，包括身份证号码合法性校验、地址信息提取和性别识别。

## 功能概述

`IDCard` 类提供了以下静态方法：

- `IsIdCard(string idCard)` - 验证身份证号码是否合法
- `GetCardAddress(string card)` - 根据身份证号码提取地址信息（省、市、区/县）
- `GetGender(string card)` - 根据身份证号码提取性别信息

## 基本使用

### 验证身份证号码

```csharp
using WodToolkit.src.Common;

string idCard = "110101199001011234";
bool isValid = IDCard.IsIdCard(idCard);

if (isValid)
{
    Console.WriteLine("身份证号码合法");
}
else
{
    Console.WriteLine("身份证号码不合法");
}
```

### 获取地址信息

`GetCardAddress` 方法返回一个 `List<string>`，包含最多 3 个元素：

- 第 1 个元素：省份名称
- 第 2 个元素：城市名称
- 第 3 个元素：区/县名称

```csharp
string idCard = "110101199001011234";
List<string> addressList = IDCard.GetCardAddress(idCard);

if (addressList.Count >= 2)
{
    Console.WriteLine($"省份: {addressList[0]}");
    Console.WriteLine($"城市: {addressList[1]}");
    
    if (addressList.Count >= 3)
    {
        Console.WriteLine($"区县: {addressList[2]}");
    }
}
else if (addressList.Count == 1)
{
    // 如果无法获取详细地址，至少返回省份信息
    Console.WriteLine($"省份: {addressList[0]}");
}
```

**地址提取逻辑**：

1. 首先尝试从内置的地区数据库中精确匹配省、市、区/县信息
2. 如果匹配到至少 2 级（省 + 市），直接返回完整地址列表
3. 如果匹配不到或只有 1 级，则根据身份证前两位省份代码，从省份映射表中返回省份名称

### 获取性别信息

中国二代身份证的倒数第二位（第 17 位）用于表示性别：
- 奇数为男性
- 偶数为女性

```csharp
string idCard = "110101199001011234";
string gender = IDCard.GetGender(idCard);

Console.WriteLine($"性别: {gender}"); // 输出: "男" 或 "女"
```

**注意**：如果身份证号码不合法或长度不正确，`GetGender` 方法会返回空字符串。

## 完整示例

```csharp
using WodToolkit.src.Common;
using System.Collections.Generic;

string idCard = "110101199001011234";

// 1. 验证身份证号码
if (IDCard.IsIdCard(idCard))
{
    Console.WriteLine("✓ 身份证号码验证通过");
    
    // 2. 获取地址信息
    List<string> address = IDCard.GetCardAddress(idCard);
    if (address.Count > 0)
    {
        Console.WriteLine($"地址: {string.Join(" ", address)}");
    }
    
    // 3. 获取性别
    string gender = IDCard.GetGender(idCard);
    if (!string.IsNullOrEmpty(gender))
    {
        Console.WriteLine($"性别: {gender}");
    }
}
else
{
    Console.WriteLine("✗ 身份证号码不合法");
}
```

## API 参考

### IsIdCard

验证 18 位身份证号码是否合法。

**方法签名**：
```csharp
public static bool IsIdCard(string idCard)
```

**参数**：
- `idCard` (string): 18 位身份证号码

**返回值**：
- `bool`: `true` 表示身份证号码合法，`false` 表示不合法

**验证规则**：
- 身份证号码必须为 18 位
- 前 17 位必须为数字
- 最后一位可以是数字或字母 X（大小写均可）
- 通过加权因子和校验码算法验证最后一位校验码是否正确

### GetCardAddress

根据身份证号码提取地址信息。

**方法签名**：
```csharp
public static List<string> GetCardAddress(string card)
```

**参数**：
- `card` (string): 身份证号码（至少需要前 6 位地区码）

**返回值**：
- `List<string>`: 地址信息列表，最多包含 3 个元素（省、市、区/县）

**说明**：
- 如果身份证号码长度不足 6 位，返回空列表
- 如果无法从地区数据库匹配到详细地址，会根据前两位省份代码返回省份名称
- 返回列表的元素数量可能为 0、1、2 或 3

### GetGender

根据身份证号码提取性别信息。

**方法签名**：
```csharp
public static string GetGender(string card)
```

**参数**：
- `card` (string): 18 位身份证号码

**返回值**：
- `string`: "男" 或 "女"，如果身份证无效则返回空字符串

**说明**：
- 如果身份证号码长度不是 18 位，返回空字符串
- 如果身份证号码不合法（可通过 `IsIdCard` 验证），返回空字符串
- 根据第 17 位数字的奇偶性判断性别

## 注意事项

1. **身份证号码格式**：所有方法都要求使用 18 位身份证号码（中国二代身份证标准）

2. **地址数据**：地址信息来源于内置的地区数据库，覆盖全国省、市、区/县信息

3. **性能考虑**：`IsIdCard` 方法执行速度很快，适合在大量数据验证场景中使用

4. **错误处理**：所有方法都包含基本的参数校验，无效输入会返回安全的默认值（`false`、空列表或空字符串）

5. **线程安全**：所有方法都是静态方法，线程安全，可以在多线程环境中使用

## 使用场景

- 用户注册时的身份证号码验证
- 表单数据校验
- 身份证信息提取和展示
- 数据清洗和验证
- 批量身份证号码校验

## 相关链接

- [快速开始](../README.md#快速开始)
- [项目主页](../README.md)

