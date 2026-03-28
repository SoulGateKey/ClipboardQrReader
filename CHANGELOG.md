# Changelog

## v1.0.0 — 2026-03-29

首个正式版本。

### 功能

- 后台持续监听系统剪贴板，无需手动触发
- 检测剪贴板中的图片并自动识别 QR Code（基于 ZXing.NET）
- 识别成功后弹出 Windows Toast 系统通知，标题显示「已识别 QR Code」
- 识别结果自动写回剪贴板，可直接粘贴使用
- 通知正文超长时自动截断并以省略号结尾，完整内容仍写入剪贴板
- 识别失败或图片中无 QR Code 时在控制台输出提示
- 支持 Ctrl+C 优雅退出
- 顶层全局异常捕获，防止程序意外崩溃

### 技术栈

- .NET 8 · Windows 10 1809+
- ZXing.Net 0.16.9
- Microsoft.Toolkit.Uwp.Notifications 7.1.3
- Win32 `AddClipboardFormatListener` / `WM_CLIPBOARDUPDATE`

### 下载

从 [Releases](../../releases/tag/v1.0.0) 页面下载 `ClipboardQrReader.exe`（单文件，免安装，需 Windows 10 1809 或更高版本）。
