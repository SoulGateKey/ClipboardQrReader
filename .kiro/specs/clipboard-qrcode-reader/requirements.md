# 需求文档

## 简介

本功能实现一个持续运行的 Windows 桌面小程序，用于监听系统剪贴板内容变化。当用户复制图片时，程序自动尝试识别图片中的 QR Code（二维码）。识别成功后，通过 Windows 系统通知告知用户，并将识别结果自动写回剪贴板；若图片中不含 QR Code 或识别失败，则仅在控制台输出相应提示。

## 术语表

- **Clipboard_Monitor**：剪贴板监听模块，负责持续检测剪贴板内容变化。
- **QR_Decoder**：二维码识别模块，负责从图片中解码 QR Code 内容。
- **Notification_Service**：Windows 系统通知模块，负责弹出系统级通知。
- **Clipboard_Writer**：剪贴板写入模块，负责将识别结果写回剪贴板。
- **Console_Logger**：控制台日志模块，负责输出运行状态与提示信息。
- **QR Code**：Quick Response Code，二维码，一种可存储文本信息的矩阵式条形码。

## 需求列表

### 需求 1：持续监听剪贴板

**用户故事：** 作为用户，我希望程序在后台持续监听剪贴板，以便复制图片时能自动触发二维码识别，无需手动操作。

#### 验收标准

1. THE Clipboard_Monitor SHALL 在程序启动后持续轮询或监听系统剪贴板的内容变化。
2. WHEN 剪贴板内容发生变化，THE Clipboard_Monitor SHALL 检测新内容的数据类型。
3. WHILE 程序处于运行状态，THE Clipboard_Monitor SHALL 保持对剪贴板的持续监听，不因单次处理完成而停止。
4. IF 剪贴板内容不是图片类型，THEN THE Clipboard_Monitor SHALL 忽略本次变化并继续监听。

---

### 需求 2：检测剪贴板图片

**用户故事：** 作为用户，我希望程序能准确判断剪贴板中是否包含图片，以便只对图片内容执行二维码识别。

#### 验收标准

1. WHEN 剪贴板内容变化被检测到，THE Clipboard_Monitor SHALL 判断剪贴板中是否存在位图（Bitmap）或图片格式数据。
2. WHEN 剪贴板中存在图片数据，THE Clipboard_Monitor SHALL 将图片数据传递给 QR_Decoder 进行识别。
3. IF 剪贴板中的图片数据无法被读取，THEN THE Clipboard_Monitor SHALL 向 Console_Logger 输出读取失败的提示，并继续监听。

---

### 需求 3：识别图片中的 QR Code

**用户故事：** 作为用户，我希望程序能自动识别图片中的二维码内容，以便快速获取二维码所携带的信息。

#### 验收标准

1. WHEN 接收到图片数据，THE QR_Decoder SHALL 尝试从图片中检测并解码 QR Code。
2. WHEN QR Code 识别成功，THE QR_Decoder SHALL 返回解码后的文本内容。
3. IF 图片中不存在 QR Code，THEN THE QR_Decoder SHALL 返回识别失败的状态。
4. IF QR Code 解码过程中发生错误，THEN THE QR_Decoder SHALL 返回错误状态，并附带可描述该错误的信息。

---

### 需求 4：识别成功后弹出系统通知

**用户故事：** 作为用户，我希望识别成功后收到 Windows 系统通知，以便在不查看控制台的情况下了解识别结果。

#### 验收标准

1. WHEN QR Code 识别成功，THE Notification_Service SHALL 弹出一条 Windows 系统通知。
2. THE Notification_Service SHALL 在通知标题中注明「已识别 QR Code」。
3. THE Notification_Service SHALL 在通知正文中显示识别出的文本内容。
4. IF 通知内容超过 Windows 系统通知的显示字符上限，THEN THE Notification_Service SHALL 截断显示内容并以省略号结尾，同时保证完整内容已写入剪贴板。

---

### 需求 5：识别成功后将结果写入剪贴板

**用户故事：** 作为用户，我希望识别结果自动写入剪贴板，以便直接粘贴使用，无需手动复制通知中的文字。

#### 验收标准

1. WHEN QR Code 识别成功，THE Clipboard_Writer SHALL 将识别出的文本内容以纯文本格式写入系统剪贴板。
2. THE Clipboard_Writer SHALL 在 Notification_Service 弹出通知之前或同时完成剪贴板写入。
3. IF 剪贴板写入操作失败，THEN THE Clipboard_Writer SHALL 向 Console_Logger 输出写入失败的提示，且 Notification_Service 仍应正常弹出通知。

---

### 需求 6：识别失败时输出控制台提示

**用户故事：** 作为用户，我希望在图片不含二维码或识别失败时，程序能在控制台给出提示，以便了解程序的处理结果。

#### 验收标准

1. IF 图片中未检测到 QR Code，THEN THE Console_Logger SHALL 输出提示，说明图片中未发现 QR Code。
2. IF QR Code 识别过程发生错误，THEN THE Console_Logger SHALL 输出提示，说明识别失败及失败原因。
3. THE Console_Logger SHALL 在每条输出信息中包含当前时间戳，格式为 `YYYY-MM-DD HH:mm:ss`。

---

### 需求 7：程序启动与退出

**用户故事：** 作为用户，我希望程序能以简单的方式启动和退出，以便日常使用。

#### 验收标准

1. THE Clipboard_Monitor SHALL 在程序启动时向 Console_Logger 输出启动成功的提示信息。
2. WHEN 用户在控制台按下 `Ctrl+C` 或发送终止信号，THE Clipboard_Monitor SHALL 停止监听并退出程序。
3. WHEN 程序退出，THE Console_Logger SHALL 输出程序已停止的提示信息。
