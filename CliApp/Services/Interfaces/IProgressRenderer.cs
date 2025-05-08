namespace CliApp.Services.Interfaces;

public interface IProgressRenderer
{
    Task RenderProgress(int current, int total);
    Task RenderError(string message);
    Task RenderSuccess(string message);
    Task RenderFileProgress(string fileName, double progress);
}
