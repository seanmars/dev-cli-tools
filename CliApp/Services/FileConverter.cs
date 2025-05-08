using System.Text;
using CliApp.Services.Interfaces;
using Microsoft.Extensions.FileSystemGlobbing;

namespace CliApp.Services;

public class FileConverter : IFileConverter
{
    private readonly IProgressRenderer _progressRenderer;

    public FileConverter(IProgressRenderer progressRenderer)
    {
        _progressRenderer = progressRenderer;
    }

    public async Task Convert(string path, string[] patterns, string encoding)
    {
        var matcher = new Matcher();
        foreach (var pattern in patterns)
        {
            matcher.AddInclude(pattern);
        }

        var files = matcher.GetResultsInFullPath(path);
        var fileList = files.ToList();
        var totalFiles = fileList.Count;
        var currentFile = 0;

        if (totalFiles == 0)
        {
            await _progressRenderer.RenderError("找不到符合條件的檔案");
            return;
        }

        foreach (var file in fileList)
        {
            try
            {
                await ConvertFile(file, encoding);
                currentFile++;
                await _progressRenderer.RenderProgress(currentFile, totalFiles);
            }
            catch (Exception ex)
            {
                await _progressRenderer.RenderError($"處理檔案 '{file}' 時發生錯誤：{ex.Message}");
            }
        }

        await _progressRenderer.RenderSuccess($"完成！共處理 {totalFiles} 個檔案");
    }

    private static bool IsValidUtf8(Span<byte> buffer)
    {
        var i = 0;

        while (i < buffer.Length)
        {
            if (buffer[i] <= 0x7F) // 單字節 ASCII
            {
                i++;
                continue;
            }

            // 檢查多字節序列
            if (buffer[i] >= 0xC2 && buffer[i] <= 0xDF) // 2 字節序列
            {
                if (i + 1 >= buffer.Length)
                {
                    return false;
                }

                if (buffer[i + 1] < 0x80 || buffer[i + 1] > 0xBF)
                {
                    return false;
                }

                i += 2;
            }
            else if (buffer[i] >= 0xE0 && buffer[i] <= 0xEF) // 3 字節序列
            {
                if (i + 2 >= buffer.Length)
                {
                    return false;
                }

                // 特殊情況處理
                if (buffer[i] == 0xE0 && (buffer[i + 1] < 0xA0 || buffer[i + 1] > 0xBF))
                {
                    return false;
                }

                if (buffer[i] == 0xED && (buffer[i + 1] < 0x80 || buffer[i + 1] > 0x9F))
                {
                    return false;
                }

                if ((buffer[i + 1] < 0x80 || buffer[i + 1] > 0xBF))
                {
                    return false;
                }

                if (buffer[i + 2] < 0x80 || buffer[i + 2] > 0xBF)
                {
                    return false;
                }

                i += 3;
            }
            else if (buffer[i] >= 0xF0 && buffer[i] <= 0xF4) // 4 字節序列
            {
                if (i + 3 >= buffer.Length)
                {
                    return false;
                }

                // 特殊情況處理
                if (buffer[i] == 0xF0 && (buffer[i + 1] < 0x90 || buffer[i + 1] > 0xBF))
                {
                    return false;
                }

                if (buffer[i] == 0xF4 && (buffer[i + 1] < 0x80 || buffer[i + 1] > 0x8F))
                {
                    return false;
                }

                if ((buffer[i + 1] < 0x80 || buffer[i + 1] > 0xBF))
                {
                    return false;
                }

                if (buffer[i + 2] < 0x80 || buffer[i + 2] > 0xBF)
                {
                    return false;
                }

                if (buffer[i + 3] < 0x80 || buffer[i + 3] > 0xBF)
                {
                    return false;
                }

                i += 4;
            }
            else // 無效的 UTF-8 字節
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> IsUtf8(string filePath)
    {
        var buffer = (await File.ReadAllBytesAsync(filePath)).AsSpan();

        // 檢查 BOM (Byte Order Mark)
        if (buffer.Length >= 3 &&
            buffer[0] == 0xEF &&
            buffer[1] == 0xBB &&
            buffer[2] == 0xBF)
        {
            return true; // 有 UTF-8 BOM
        }

        // 無 BOM，執行 UTF-8 有效性檢查
        return IsValidUtf8(buffer);
    }

    private async Task ConvertFile(string filePath, string encoding)
    {
        var fileName = Path.GetFileName(filePath);

        if (await IsUtf8(filePath))
        {
            await _progressRenderer.RenderSuccess($"檔案 '{fileName}' 已是 UTF-8 編碼，不需轉換");
            await _progressRenderer.RenderFileProgress(fileName, 100);
            return;
        }

        await _progressRenderer.RenderFileProgress(fileName, 0);

        string? content;
        Encoding detectedEncoding;
        using (var reader = new StreamReader(filePath, Encoding.GetEncoding(encoding), true))
        {
            content = await reader.ReadToEndAsync();
            detectedEncoding = reader.CurrentEncoding;
        }

        await _progressRenderer.RenderFileProgress(fileName, 50);

        // 使用 UTF-8 無 BOM 寫入檔案
        await File.WriteAllTextAsync(filePath, content, new UTF8Encoding(false));
        await _progressRenderer.RenderFileProgress(fileName, 100);
        await _progressRenderer.RenderSuccess($"檔案 '{fileName}' 已從 {detectedEncoding.EncodingName} 轉換為 UTF-8 編碼");
    }
}