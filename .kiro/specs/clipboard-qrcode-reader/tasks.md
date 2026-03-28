# 实现计划：Clipboard QR Code Reader

## 概述

基于需求文档和技术设计，将功能分解为一系列递进式编码任务。每个任务聚焦于特定组件的实现，最终通过主程序串联所有模块。

## 任务列表

- [x] 1. 初始化项目结构与核心接口
  - 创建 .NET 8 控制台项目（Windows-only），配置 `<TargetFramework>net8.0-windows</TargetFramework>`
  - 添加 NuGet 依赖：`ZXing.Net`、`FsCheck.Xunit`、`xunit`
  - 定义 `QrDecodeResult` 记录类型
  - 创建各模块类文件的空白骨架（ClipboardMonitor、QrDecoder、NotificationService、ClipboardWriter、ConsoleLogger）
  - _需求：1.1、2.2、3.1_

- [x] 2. 实现 ConsoleLogger 模块
  - [x] 2.1 实现 `ConsoleLogger.Log(string message)` 静态方法
    - 格式：`[YYYY-MM-DD HH:mm:ss] message`，使用 `DateTime.Now`
    - _需求：6.3_
  - [ ]* 2.2 为 ConsoleLogger 编写属性测试（属性 6：日志时间戳格式不变式）
    - **属性 6：日志时间戳格式不变式**
    - **验证需求：6.3**
    - 对任意日志消息，断言输出前 21 字符匹配 `[YYYY-MM-DD HH:mm:ss]` 格式

- [x] 3. 实现 QrDecoder 模块
  - [x] 3.1 实现 `QrDecoder.Decode(Bitmap image)` 方法
    - 使用 ZXing.NET 的 `BarcodeReader` 解码 QR Code
    - 识别成功返回 `QrDecodeResult(true, text, null)`
    - 未找到 QR Code 返回 `QrDecodeResult(false, null, null)`
    - 异常时捕获并返回 `QrDecodeResult(false, null, ex.Message)`
    - _需求：3.1、3.2、3.3、3.4_
  - [ ]* 3.2 为 QrDecoder 编写属性测试（属性 2：QR Code 识别轮回）
    - **属性 2：QR Code 识别 Round Trip**
    - **验证需求：3.1、3.2**
    - 生成随机文本 → 用 ZXing.NET 编码为 QR Code Bitmap → 再解码，断言结果与原始文本相等
  - [ ]* 3.3 为 QrDecoder 编写属性测试（属性 3：无 QR Code 图片返回失败状态）
    - **属性 3：无 QR Code 图片返回失败状态**
    - **验证需求：3.3**
    - 生成随机纯色 Bitmap，断言 `Success == false` 且 `Text == null`

- [x] 4. 检查点 —— 确保已有测试全部通过
  - 确保所有测试通过，如有疑问请告知用户。

- [x] 5. 实现 NotificationService 模块
  - [x] 5.1 实现 `NotificationService.Show(string title, string body)` 方法
    - 使用 `Microsoft.Toolkit.Uwp.Notifications` 或 `ToastNotificationManager` 发送 Windows Toast 通知
    - 标题固定为「已识别 QR Code」
    - _需求：4.1、4.2、4.3_
  - [x] 5.2 实现 `NotificationService.Truncate(string text, int maxLength)` 私有方法
    - 若文本长度超过 `maxLength`，截断并追加 `…`；否则原样返回
    - 在 `Show` 中对 body 调用截断（上限约 200 字符）
    - _需求：4.4_
  - [ ]* 5.3 为 NotificationService 编写属性测试（属性 4：通知正文截断不变式）
    - **属性 4：通知正文截断不变式**
    - **验证需求：4.4**
    - 对任意字符串，断言：超出上限时结果长度 ≤ N 且以 `…` 结尾；不超出时结果与原始相等

- [x] 6. 实现 ClipboardWriter 模块
  - [x] 6.1 实现 `ClipboardWriter.Write(string text)` 方法
    - 确保在 STA 线程执行 `Clipboard.SetText(text)`
    - 写入成功返回 `true`；捕获异常时向 ConsoleLogger 输出错误并返回 `false`
    - _需求：5.1、5.3_
  - [ ]* 6.2 为 ClipboardWriter 编写属性测试（属性 5：剪贴板写入轮回）
    - **属性 5：剪贴板写入 Round Trip**
    - **验证需求：5.1**
    - 对任意文本，调用 `Write` 后立即读取剪贴板纯文本，断言与写入值相等

- [x] 7. 实现 ClipboardMonitor 模块
  - [x] 7.1 实现隐藏消息窗口（`HiddenWindow`）以接收 `WM_CLIPBOARDUPDATE`
    - 使用 Win32 `AddClipboardFormatListener` 注册监听
    - 在 `WndProc` 中响应 `WM_CLIPBOARDUPDATE` 消息，触发 `OnClipboardChanged`
    - _需求：1.1、1.2_
  - [x] 7.2 实现 `OnClipboardChanged` 处理逻辑
    - 检测剪贴板是否包含图片（`Clipboard.ContainsImage()`）
    - 非图片：忽略并继续监听（需求 1.4）
    - 图片读取失败：ConsoleLogger 输出并继续监听（需求 2.3）
    - 图片读取成功：传递给 QrDecoder（需求 2.2）
    - _需求：1.2、1.3、1.4、2.1、2.2、2.3_
  - [x] 7.3 实现 QR Code 识别结果的分发逻辑
    - 识别成功：先调用 ClipboardWriter.Write，再调用 NotificationService.Show（需求 5.2）
    - 写入失败：ConsoleLogger 输出，但仍调用 NotificationService.Show（需求 5.3）
    - 识别失败/无 QR Code：ConsoleLogger 输出相应提示（需求 6.1、6.2）
    - _需求：3.1–3.4、4.1、5.2、5.3、6.1、6.2_
  - [x] 7.4 实现 `Start()` 和 `Stop()` 及 `Dispose()`
    - `Start()`：注销旧监听（如有），注册新监听，启动消息循环
    - `Stop()`：调用 Win32 `RemoveClipboardFormatListener`，退出消息循环
    - _需求：1.1、7.2_
  - [ ]* 7.5 为 ClipboardMonitor 编写属性测试（属性 1：剪贴板图片传递完整性）
    - **属性 1：剪贴板图片传递完整性**
    - **验证需求：2.1、2.2**
    - 生成随机 Bitmap，模拟剪贴板读取，断言传递给 QrDecoder 的像素数据与原始一致

- [x] 8. 实现 Program 入口并串联所有模块
  - [x] 8.1 在 `Program.cs` 中初始化所有模块并启动 ClipboardMonitor
    - 程序启动时通过 ConsoleLogger 输出启动提示（需求 7.1）
    - 注册 `Console.CancelKeyPress` 处理 Ctrl+C，调用 `monitor.Stop()`（需求 7.2）
    - 程序退出时 ConsoleLogger 输出停止提示（需求 7.3）
    - 添加顶层全局异常处理，防止程序崩溃
    - _需求：7.1、7.2、7.3_
  - [ ]* 8.2 为属性 7 编写单元测试（写入失败不阻断通知）
    - **属性 7：��入失败不阻断通知**
    - **验证需求：5.3**
    - 模拟 ClipboardWriter.Write 返回 false，断言 NotificationService.Show 仍被调用一次

- [x] 9. 最终检查点 —— 确保所有测试通过
  - 确保所有测试通过，如有疑问请告知用户。

## 备注

- 标有 `*` 的子任务为可选项，可在 MVP 阶段跳过以加快进度
- 每个任务均引用了对应需求条目以保证可追溯性
- 属性测试使用 FsCheck，每个属性至少运行 100 次（`MaxTest = 100`）
- 单元测试使用 xUnit
- 剪贴板相关操作必须在 STA 线程执行
