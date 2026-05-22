using Serilog;

namespace Automacao.Services;

public static class LoggerService
{
    public static void Configure()
    {
        var pastaLogs =
            Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "Logs"));

        if (!Directory.Exists(pastaLogs))
        {
            Directory.CreateDirectory(pastaLogs);
        }

        var caminhoArquivo =
            Path.Combine(
                pastaLogs,
                $"log-{DateTime.Now:dd-MM-yyyy}.txt");

        Log.Logger =
            new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    caminhoArquivo,
                    outputTemplate: "{Timestamp:dd/MM/yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 30,
                    shared: true)
                .CreateLogger();
    }
}

