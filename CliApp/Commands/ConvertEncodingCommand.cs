using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using CliApp.Services.Interfaces;

namespace CliApp.Commands;

public class ConvertEncodingSettings : CommandSettings
{
    [CommandArgument(0, "<folder>")]
    public required DirectoryInfo Directory { get; init; }

    [CommandOption("--pattern")]
    [DefaultValue("*.*")]
    public string Pattern { get; init; } = "*.*";

    [CommandOption("--encoding")]
    [DefaultValue("big5")]
    public string Encoding { get; init; } = "big5";
}

public class ConvertEncodingCommand : AsyncCommand<ConvertEncodingSettings>
{
    private readonly IFileConverter _fileConverter;
    private readonly IProgressRenderer _progressRenderer;

    public ConvertEncodingCommand(IFileConverter fileConverter, IProgressRenderer progressRenderer)
    {
        _fileConverter = fileConverter;
        _progressRenderer = progressRenderer;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ConvertEncodingSettings settings)
    {
        try
        {
            if (!settings.Directory.Exists)
            {
                AnsiConsole.MarkupLine($"[red]錯誤：資料夾 '{settings.Directory.FullName}' 不存在[/]");
                return -1;
            }

            var patterns = settings.Pattern.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();

            if (patterns.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]錯誤：請至少提供一個檔案模式[/]");
                return -1;
            }

            if (string.IsNullOrWhiteSpace(settings.Encoding))
            {
                AnsiConsole.MarkupLine("[red]錯誤：請提供有效的編碼[/]");
                return -1;
            }

            await _fileConverter.Convert(settings.Directory.FullName, patterns, settings.Encoding);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]錯誤：{ex.Message}[/]");
            return -1;
        }
    }
}