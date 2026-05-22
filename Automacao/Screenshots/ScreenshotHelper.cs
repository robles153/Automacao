using OpenQA.Selenium;

namespace Automacao.Screenshots
{
    public static class ScreenshotHelper
    {
        public static string TirarScreenshot(IWebDriver driver, string nomeArquivo)
        {
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();

            var pasta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");

            if (!Directory.Exists(pasta))
            {
                Directory.CreateDirectory(pasta);
            }

            var caminhoArquivo = Path.Combine(pasta, $"{nomeArquivo}-{DateTime.Now:yyyyMMdd-HHmmss}.png");

            screenshot.SaveAsFile(caminhoArquivo);

            return caminhoArquivo;
        }
    }
}

