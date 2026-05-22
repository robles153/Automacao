using OpenQA.Selenium;

namespace Automacao.Helpers;

public static class ShadowDomHelper
{
    public static ISearchContext GetShadowRoot(IWebDriver driver, IWebElement element)
    {
        var shadowRoot = ((IJavaScriptExecutor)driver)
                .ExecuteScript(
                    "return arguments[0].shadowRoot",
                    element) as ISearchContext;

        if (shadowRoot is null)
        {
            throw new NoSuchElementException("O elemento informado não possui shadowRoot.");
        }

        return shadowRoot;
    }
}

