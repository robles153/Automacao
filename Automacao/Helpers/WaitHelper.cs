using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
namespace Automacao.Helpers;

public static class WaitHelper
{
    public static IWebElement EsperarElemento( IWebDriver driver, By by, int segundos = 10)
    {
        var wait = new WebDriverWait( driver, TimeSpan.FromSeconds(segundos));

        return wait.Until( ExpectedConditions.ElementIsVisible(by));
    }
}
