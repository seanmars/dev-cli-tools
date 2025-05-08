using System.Text;
using CliApp.Commands;
using CliApp.Services;
using CliApp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var registrations = new ServiceCollection();
registrations.AddSingleton<IProgressRenderer, ProgressRenderer>();
registrations.AddSingleton<IFileConverter, FileConverter>();

var registrar = new TypeRegistrar(registrations);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddCommand<ConvertEncodingCommand>("convert")
        .WithDescription("將指定資料夾中的檔案轉換為 UTF-8 編碼")
        .WithExample(new[] { "convert", "C:\\files", "--pattern", "*.txt,*.config" });

    config.SetExceptionHandler((ex, _) =>
    {
        AnsiConsole.MarkupLine($"[red]發生錯誤：{ex.Message}[/]");
        return -1;
    });
});

return app.Run(args);

// TypeRegistrar implementation for DI
public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;

    public TypeRegistrar(IServiceCollection builder)
    {
        _builder = builder;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_builder.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _builder.AddSingleton(service, _ => factory());
    }
}

public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public object? Resolve(Type? type)
    {
        return type != null ? _provider.GetService(type) : null;
    }

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}