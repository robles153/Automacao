using Automacao.Drivers;
using Automacao.Models;
using Automacao.Pages;
using Automacao.Repositories;
using Automacao.Screenshots;
using Automacao.Services;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Serilog;

LoggerService.Configure();

IWebDriver? driver = null;

try
{
    Log.Information(
        "Iniciando automação");

    var configuration =
        new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

    var dadosTeste =
        new DadosTeste
        {
            Usuario = configuration["DadosTeste:Usuario"]!,
            Pizza = configuration["DadosTeste:Pizza"]!,
            CurrentCrushName = configuration["DadosTeste:CurrentCrushName"]!,
            Destiny = configuration["DadosTeste:Destiny"]!
        };

    driver = DriverFactory.Create();

    IExecucaoRepository execucaoRepository = new SqlExecucaoRepository(configuration.GetConnectionString("DefaultConnection")!);

    var practicePage = new PracticePage(driver, execucaoRepository);

    var iframeShadowDomPage = new IframeShadowDomPage(driver, execucaoRepository);

    var automationService = new AutomationService( practicePage, iframeShadowDomPage, dadosTeste);

    automationService.Executar();

    Console.WriteLine();

    Console.WriteLine(
        "Pressione ENTER para finalizar...");

    Console.ReadLine();

    Log.Information( "Automação finalizada");
}
catch (Exception ex)
{
    Console.WriteLine(ex);

    Log.Error(ex, "Erro durante execução");

    if (driver is not null)
    {
        try
        {
            var screenshotPath = ScreenshotHelper.TirarScreenshot(driver, "erro");

            Log.Information("Screenshot salva em: {Path}", screenshotPath);
        }
        catch
        {
            Log.Warning("Não foi possível gerar screenshot");
        }
    }
}
finally
{
    driver?.Quit();

    Log.CloseAndFlush();
}
