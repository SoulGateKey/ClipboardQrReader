# Clipboard QR Code Reader

一个运行在 Windows 后台的剪贴板监听工具。复制含有二维码的图片后，程序自动识别其中的 QR Code，将结果写回剪贴板，并弹出 Windows 系统通知。

## 功能

- 持续监听系统剪贴板，检测图片变化
- 自动识别图片中的 QR Code（使用 ZXing.NET）
- 识别成功后弹出 Windows Toast 通知
- 将识别结果自动写回剪贴板，可直接粘贴使用
- 识别失败或图片中无 QR Code 时在控制台输出提示

## 环境要求

- Windows 10 1809（Build 17763）或更高版本
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## 快速开始

```bash
# 克隆项目
git clone <repo-url>
cd QRConverter

# 运行
dotnet run --project ClipboardQrReader
```

程序启动后在后台监听剪贴板，按 `Ctrl+C` 退出。

## 构建发布版本

```bash
dotnet publish ClipboardQrReader -c Release -r win-x64 --self-contained true -o ./publish
```

## 运行测试

```bash
dotnet test
```

## 项目结构

```
ClipboardQrReader/         # 主程序
  Program.cs               # 入口，模块初始化与生命周期管理
  ClipboardMonitor.cs      # 剪贴板监听（Win32 WM_CLIPBOARDUPDATE）
  QrDecoder.cs             # QR Code 识别（ZXing.NET）
  NotificationService.cs   # Windows Toast 通知
  ClipboardWriter.cs       # 剪贴板写入（STA 线程）
  ConsoleLogger.cs         # 带时间戳的控制台日志
  Models/QrDecodeResult.cs # 识别结果记录类型
ClipboardQrReader.Tests/   # 单元测试与属性测试（xUnit + FsCheck）
```

## 依赖

- [ZXing.Net](https://github.com/micjahn/ZXing.Net) — QR Code 识别与编码
- [Microsoft.Toolkit.Uwp.Notifications](https://github.com/CommunityToolkit/WindowsCommunityToolkit) — Windows Toast 通知
- [xUnit](https://xunit.net/) — 单元测试框架
- [FsCheck](https://fscheck.github.io/FsCheck/) — 属性测试框架
