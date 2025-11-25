# GitHub Wiki 使用说明

本文件夹包含了 WodToolKit 的 GitHub Wiki 页面内容。

## 如何使用这些文件

### 方法一：在 GitHub 网页上直接创建（推荐）

1. 访问您的仓库：`https://github.com/thiswod/WodToolKit`
2. 点击 **Wiki** 标签页
3. 点击 **Create the first page** 或 **New Page**
4. 复制对应文件的内容到编辑器中
5. 页面标题使用文件名（不含 .md 扩展名）
6. 保存页面

### 方法二：克隆 Wiki 仓库到本地

1. 克隆 Wiki 仓库：
```bash
git clone https://github.com/thiswod/WodToolKit.wiki.git
cd WodToolKit.wiki
```

2. 将本文件夹中的文件复制到克隆的仓库中

3. 提交并推送：
```bash
git add .
git commit -m "Add wiki pages"
git push origin master
```

## 文件说明

- `Home.md` - Wiki 主页
- `Installation.md` - 安装指南
- `Quick-Start.md` - 快速开始指南
- `HTTP.md` - HTTP 请求处理文档
- `Cookie.md` - Cookie 管理文档
- `JSON.md` - JSON 处理文档
- `Cache.md` - 内存缓存文档
- `Thread-Pool.md` - 线程池文档
- `AES-Encryption.md` - AES 加密文档
- `JavaScript-Execution.md` - JavaScript 执行文档
- `API-Reference.md` - API 参考文档
- `Examples.md` - 示例项目
- `FAQ.md` - 常见问题
- `Contributing.md` - 贡献指南
- `Changelog.md` - 更新日志

## 注意事项

1. GitHub Wiki 使用特殊的链接语法：`[[页面名称|显示文本]]`
2. 所有内部链接都已使用 GitHub Wiki 格式
3. 外部链接使用标准 Markdown 格式：`[文本](URL)`

## 创建顺序建议

建议按以下顺序创建页面：

1. Home（主页）
2. Installation（安装指南）
3. Quick-Start（快速开始）
4. 各功能模块文档
5. API-Reference、Examples、FAQ 等补充文档

