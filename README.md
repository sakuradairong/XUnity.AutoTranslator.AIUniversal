# XUnity.AutoTranslator.AIUniversal

**通用 AI 翻译插件 for XUnity.AutoTranslator**

一个基于 OpenAI 兼容 API 的通用翻译端点，支持接入绝大多数 AI 大模型翻译服务，包括 OpenAI、DeepSeek、通义千问、Kimi、Claude、Gemini、本地 Ollama 等任何提供 OpenAI 兼容接口的模型。

---

## 功能特点

- **通用兼容**：支持任何 OpenAI 兼容 API 的服务商
- **一键配置**：仅需填写 API URL、Key 和模型名称即可使用
- **自定义提示词**：可自定义系统提示词（System Prompt），灵活控制翻译风格
- **请求延迟**：内置可配置的请求延迟，避免触发 API 速率限制
- **垃圾检查控制**：可选禁用 XUnity 的 spam check，适合慢速 API
- **错误处理**：完善的 API 错误解析和提示
- **字体替换指南**：附赠字体检测工具，彻底解决中文方块字问题

---

## 支持的 AI 服务商

| 服务商 | API URL 示例 | 模型示例 |
|--------|-------------|---------|
| **OpenAI** | `https://api.openai.com/v1/chat/completions` | `gpt-4o-mini`, `gpt-3.5-turbo` |
| **DeepSeek** | `https://api.deepseek.com/v1/chat/completions` | `deepseek-chat`, `deepseek-reasoner` |
| **通义千问 (阿里云)** | `https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions` | `qwen-turbo`, `qwen-plus` |
| **Kimi (Moonshot)** | `https://api.moonshot.cn/v1/chat/completions` | `moonshot-v1-8k` |
| **智谱 AI (GLM)** | `https://open.bigmodel.cn/api/paas/v4/chat/completions` | `glm-4` |
| **Claude (通过中转)** | 使用 OpenAI 兼容中转接口 | `claude-3-5-sonnet` |
| **Gemini (Google)** | 使用 OpenAI 兼容中转接口 | `gemini-1.5-pro` |
| **本地 Ollama** | `http://localhost:11434/v1/chat/completions` | `qwen2.5`, `llama3` |
| **本地 vLLM** | `http://localhost:8000/v1/chat/completions` | 你部署的模型 |
| **OneAPI / 其他中转** | 中转商提供的 OpenAI 兼容地址 | 按中转商提供填写 |

> 提示：任何提供 OpenAI 兼容 `/v1/chat/completions` 接口的服务都可以使用。

---

## 安装方法

### 方法一：直接下载 DLL（推荐）

1. 从 Releases 下载 `AIUniversalTranslate.dll`
2. 将 DLL 文件放到 XUnity.AutoTranslator 的 `Translators` 文件夹中
3. 在 `Config.ini` 中配置参数（见下文）

### 方法二：自行编译

1. 将本项目文件夹复制到 XUnity.AutoTranslator 的 `src/Translators/` 目录下
2. 打开 `XUnity.AutoTranslator.sln` 解决方案
3. 编译 `AIUniversalTranslate` 项目（Release 模式）
4. 生成的 `AIUniversalTranslate.dll` 会自动复制到 `dist/Translators/` 目录

---

## 配置说明

在 `Config.ini` 的 `[AIUniversal]` 区域添加以下配置：

```ini
[AIUniversal]
; API 地址（完整路径，以 /v1/chat/completions 结尾）
ApiUrl=https://api.deepseek.com/v1/chat/completions

; API 密钥
ApiKey=sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

; 模型名称
Model=deepseek-chat

; 系统提示词（可选，默认是游戏翻译专用提示词）
SystemPrompt=你是一个专业的游戏翻译助手。请将用户提供的文本翻译成目标语言，保持原文的语气和风格。只输出翻译结果，不要添加解释，不要输出原文。

; 温度参数，控制创意程度（可选，默认 0.3）
Temperature=0.3

; 最大生成 token 数（可选，默认 2048）
MaxTokens=2048

; 请求间隔延迟（秒），避免触发速率限制（可选，默认 1.0）
DelaySeconds=1.0

; 是否禁用 spam 检查（可选，默认 false）
; 设为 true 可避免 XUnity 因翻译慢而报错停止
DisableSpamChecks=false
```

### 主配置选择翻译器

在 `Config.ini` 的 `[General]` 区域：

```ini
[General]
Language=en
FromLanguage=ja
Endpoint=AIUniversalTranslate
```

---

## 各服务商配置示例

### OpenAI
```ini
[AIUniversal]
ApiUrl=https://api.openai.com/v1/chat/completions
ApiKey=sk-your-openai-key
Model=gpt-4o-mini
```

### DeepSeek（推荐，性价比高）
```ini
[AIUniversal]
ApiUrl=https://api.deepseek.com/v1/chat/completions
ApiKey=sk-your-deepseek-key
Model=deepseek-chat
```

### 通义千问（阿里云）
```ini
[AIUniversal]
ApiUrl=https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions
ApiKey=sk-your-dashscope-key
Model=qwen-turbo
```

### 本地 Ollama（免费本地模型）
```ini
[AIUniversal]
ApiUrl=http://localhost:11434/v1/chat/completions
ApiKey=ollama
Model=qwen2.5
```

> 注意：Ollama 需要先用 `ollama pull qwen2.5` 下载模型，然后运行 `ollama serve` 启动服务。

---

## 字体替换指南（解决中文方块字）

XUnity.AutoTranslator 本身已经提供了字体替换功能，只需在 `Config.ini` 中配置即可。以下是完整操作步骤：

### 1. 查找系统中可用的中文字体

运行本项目附带的 `font_helper.py` 脚本，它会自动检测系统中支持中文的字体：

```bash
python font_helper.py
```

输出示例：
```
=== 支持中文的字体 ===
Microsoft YaHei          (微软雅黑)
SimHei                   (黑体)
SimSun                   (宋体)
Source Han Sans SC       (思源黑体)
Noto Sans CJK SC         (Noto 黑体)
```

### 2. 在 Config.ini 中配置字体替换

```ini
[Behaviour]
; 替换为支持中文的字体名称（与系统字体名称一致，不含文件扩展名）
OverrideFont=Microsoft YaHei

; 可选：覆盖字体大小
OverrideFontSize=18

; 可选：启用自动调整 UI 大小
ResizeUILineSpacing=2
```

### 3. 常用中文字体推荐

| 字体名称 | 说明 |
|---------|------|
| `Microsoft YaHei` | 微软雅黑，Windows 自带，最推荐 |
| `SimHei` | 黑体，Windows 自带 |
| `SimSun` | 宋体，Windows 自带 |
| `Source Han Sans SC` | 思源黑体，需自行安装 |
| `Noto Sans CJK SC` | Google Noto 黑体，需自行安装 |
| `WenQuanYi Micro Hei` | 文泉驿微米黑，Linux 常用 |

### 4. 手动查找字体的方法

如果 `font_helper.py` 无法运行，也可以手动查找：

**Windows：**
1. 打开 `C:\Windows\Fonts` 文件夹
2. 右键点击中文字体 → 属性 → 查看"字体名称"
3. 将名称填入 `OverrideFont=`（不带 `.ttf` 后缀）

**Linux：**
```bash
fc-list :lang=zh
```

**macOS：**
```bash
fc-list :lang=zh | grep -i "font"
```

### 5. 更多字体相关配置

```ini
[Behaviour]
; 覆盖字体（核心配置）
OverrideFont=Microsoft YaHei

; 覆盖字体大小（可选）
OverrideFontSize=18

; 按分辨率范围配置字体大小（高级）
OverrideFontSizeMin=14
OverrideFontSizeMax=24

; 在翻译时启用 UI 调整
EnableUIResizing=true

; 行间距调整
ResizeUILineSpacing=2
```

> 游戏中按 `ALT + F` 可切换字体的覆盖/默认状态。

---

## 故障排查

### 插件无法加载
- 检查 `ApiUrl` 和 `ApiKey` 是否已正确配置
- 检查 DLL 是否放在了正确的 `Translators` 目录

### 翻译结果为空或报错
- 检查 API Key 是否有效、余额是否充足
- 检查 `ApiUrl` 是否完整（必须以 `/v1/chat/completions` 结尾）
- 查看游戏日志或 XUnity 日志中的具体错误信息
- 尝试增大 `DelaySeconds` 避免触发速率限制

### 翻译速度太慢
- 降低 `DelaySeconds`（如改为 0.5）
- 使用响应速度更快的 API（如 DeepSeek 国内节点）
- 考虑使用本地 Ollama 模型（需配置好 GPU 加速）

### 中文显示为方块
- 按照上方"字体替换指南"配置 `OverrideFont`
- 确保配置的字体名称与系统实际字体名称完全一致（区分大小写）
- 尝试在游戏内按 `ALT + F` 切换字体

---

## 构建项目

```bash
# 将项目复制到 XUnity.AutoTranslator/src/Translators/AIUniversalTranslate
cd XUnity.AutoTranslator
dotnet build src/Translators/AIUniversalTranslate/AIUniversalTranslate.csproj -c Release
```

---

## 许可证

本项目采用与 XUnity.AutoTranslator 一致的许可证。

---

## 致谢

基于 [XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator) 项目开发。
