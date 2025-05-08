using CliApp.Services.Interfaces;
using Spectre.Console;

namespace CliApp.Services;

public class ProgressRenderer : IProgressRenderer
{
    public Task RenderProgress(int current, int total)
    {
        AnsiConsole.MarkupLine($"進度：[green]{current}/{total}[/]");
        return Task.CompletedTask;
    }

    public Task RenderError(string message)
    {
        AnsiConsole.MarkupLine($"[red]錯誤：{message}[/]");
        return Task.CompletedTask;
    }

    public Task RenderSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]成功：{message}[/]");
        return Task.CompletedTask;
    }

    public Task RenderFileProgress(string fileName, double progress)
    {
        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
            })
            .Start(ctx =>
            {
                var task = ctx.AddTask($"[green]{fileName}[/]");
                task.Value = progress;

                if (progress >= 100)
                {
                    AnsiConsole.MarkupLine($"[green]✓[/] {fileName}");
                }
            });

        return Task.CompletedTask;
    }
}