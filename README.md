# Dev CLI Tools

這是一個用於開發的命令列工具集，目前提供檔案編碼轉換功能。

## 安裝需求

- .NET 7.0 或更高版本

## 可用指令

### convert - 檔案編碼轉換

將指定資料夾中的檔案轉換為 UTF-8 編碼。

#### 語法

```bash
dev-cli-tools convert <folder> [--pattern <pattern>] [--encoding <encoding>]
```

#### 參數

- `<folder>` (必填)：要處理的資料夾路徑
- `--pattern` (選填)：檔案匹配模式，預設為 `*.*`
  - 可使用逗號分隔多個模式，例如：`*.txt,*.config`
- `--encoding` (選填)：原始檔案的編碼，預設為 `big5`

#### 範例

```bash
# 轉換 C:\files 資料夾下所有檔案的編碼
dev-cli-tools convert C:\files

# 只轉換 txt 和 config 檔案的編碼
dev-cli-tools convert C:\files --pattern "*.txt,*.config"

# 指定原始編碼為 gb2312
dev-cli-tools convert C:\files --encoding gb2312
```

## 錯誤處理

程式會在以下情況顯示錯誤訊息：

- 指定的資料夾不存在
- 未提供有效的檔案匹配模式
- 未提供有效的編碼
- 檔案處理過程中發生錯誤

所有錯誤訊息都會以紅色文字顯示在終端機中。
