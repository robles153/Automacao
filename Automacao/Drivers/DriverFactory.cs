using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Automacao.Drivers;

public static class DriverFactory
{
    public static IWebDriver Create()
    {
        var service = ChromeDriverService.CreateDefaultService();

        service.HideCommandPromptWindow = true;

        service.SuppressInitialDiagnosticInformation = true;

        var options = new ChromeOptions();

        options.AddArgument("--start-maximized");

        options.AddExcludedArgument("enable-logging");

        return new ChromeDriver( service, options);
    }
}
