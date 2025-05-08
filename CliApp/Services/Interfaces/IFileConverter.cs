namespace CliApp.Services.Interfaces;

public interface IFileConverter
{
    Task Convert(string path, string[] patterns, string encoding);
}