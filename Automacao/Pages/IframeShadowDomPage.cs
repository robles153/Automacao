using Automacao.Models;
using Automacao.Repositories;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace Automacao.Pages;

public class IframeShadowDomPage
{
    private readonly IWebDriver _driver;
    private readonly IExecucaoRepository _execucaoRepository;

    public IframeShadowDomPage(IWebDriver driver, IExecucaoRepository execucaoRepository)
    {
        _driver = driver;
        _execucaoRepository = execucaoRepository;
    }

    public void Executar( DadosTeste dadosTeste)
    {
        AcessarPagina();

        ClicarLearningHubNoShadowDom();

        InteragirComIframes( dadosTeste.CurrentCrushName, dadosTeste.Destiny);
    }

    private void AcessarPagina()
    {
        _driver.Navigate().GoToUrl("https://selectorshub.com/iframe-in-shadow-dom/");

        Log.Information("Página de iframe dentro de shadow DOM acessada");

        SalvarExecucao("Abrir página iframe shadow DOM", "Página aberta com sucesso");
    }

    private void ClicarLearningHubNoShadowDom()
    {
        var janelaPrincipal = _driver.CurrentWindowHandle;

        var janelasAntesDoClique = _driver.WindowHandles.ToList();

        var textoClicado = ((IJavaScriptExecutor)_driver)
                .ExecuteScript(
                    @"
                    function buscarLearningHub(root) {
                        const elementos = Array.from(root.querySelectorAll('a, button'));

                        const learningHub = elementos.find(elemento =>
                            elemento.innerText.trim().toLowerCase().includes('learning hub'));

                        if (learningHub) {
                            learningHub.scrollIntoView({ block: 'center' });
                            learningHub.click();

                            return learningHub.innerText.trim();
                        }

                        const todosElementos = Array.from(root.querySelectorAll('*'));

                        for (const elemento of todosElementos) {
                            if (elemento.shadowRoot) {
                                const texto = buscarLearningHub(elemento.shadowRoot);

                                if (texto) {
                                    return texto;
                                }
                            }
                        }

                        return null;
                    }

                    return buscarLearningHub(document);
                    ")
                ?.ToString();

        if (string.IsNullOrWhiteSpace(textoClicado))
        {
            throw new NoSuchElementException( "Não foi possível encontrar o link Learning Hub dentro do shadow DOM.");
        }

        var wait = new WebDriverWait( _driver, TimeSpan.FromSeconds(10));

        wait.Until(driver => driver.WindowHandles.Count > janelasAntesDoClique.Count);

        var novaJanela = _driver
                .WindowHandles
                .First(handle => !janelasAntesDoClique.Contains(handle));

        _driver
            .SwitchTo()
            .Window(novaJanela);

        wait.Until(driver =>
            !string.IsNullOrWhiteSpace(driver.Title) ||
            driver.Url != "about:blank");

        var urlAberta = _driver.Url;

        var tituloNovaAba = _driver.Title;

        Log.Information(
            "Learning Hub clicado. Texto: {Texto}. URL: {Url}. Título: {Titulo}",
            textoClicado,
            urlAberta,
            tituloNovaAba);

        SalvarExecucao( "Clicar Learning Hub", textoClicado);

        SalvarExecucao("Capturar URL nova aba", urlAberta);

        SalvarExecucao("Capturar título nova aba", tituloNovaAba);

        _driver.Close();

        _driver
            .SwitchTo()
            .Window(janelaPrincipal);

        Log.Information("Nova aba fechada e foco retornado para a página principal");
    }

    private void InteragirComIframes(string currentCrushName, string destiny)
    {
        EntrarIframeComCurrentCrushName();

        PreencherCurrentCrushName(currentCrushName);

        ClicarConnectNow();

        var confirmacao = CapturarConfirmacaoIframe();

        SalvarExecucao("Ler confirmação iframe", confirmacao);

        EntrarNestedIframe();

        PreencherDestiny(destiny);

        ClicarCloseIt();

        _driver
            .SwitchTo()
            .DefaultContent();

        Log.Information("Interações com iframe dentro de shadow DOM e nested iframe concluídas");
    }

    private void EntrarIframeComCurrentCrushName()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        var entrouNoIframe =
            wait.Until(driver =>
            {
                driver
                    .SwitchTo()
                    .DefaultContent();

                var iframes = ObterIframesDaPaginaEShadowDoms(driver);

                foreach (var iframe in iframes)
                {
                    try
                    {
                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("arguments[0].scrollIntoView({ block: 'center' });", iframe);

                        driver
                            .SwitchTo()
                            .Frame(iframe);

                        var possuiCampo =
                            ((IJavaScriptExecutor)driver)
                                .ExecuteScript(
                                    @"
                                    return !!document.querySelector(
                                        'input[placeholder*=""Current"" i], input[placeholder*=""Crush"" i], input[name*=""crush"" i], input[id*=""crush"" i]');
                                    ") as bool? == true;

                        if (possuiCampo)
                        {
                            return true;
                        }
                    }
                    catch (WebDriverException)
                    {
                    }

                    driver
                        .SwitchTo()
                        .DefaultContent();
                }

                return false;
            });

        if (!entrouNoIframe)
        {
            throw new NoSuchElementException("Não foi possível encontrar o iframe que contém o campo Current Crush Name.");
        }

        Log.Information("Iframe com campo Current Crush Name acessado");

        SalvarExecucao("Entrar iframe dentro do shadow DOM", "Iframe acessado");
    }

    private static List<IWebElement> ObterIframesDaPaginaEShadowDoms(IWebDriver driver)
    {
        var resultado = ((IJavaScriptExecutor)driver)
                .ExecuteScript(
                    @"
                    const iframes = [];

                    function buscarIframes(root) {
                        iframes.push(...Array.from(root.querySelectorAll('iframe')));

                        const elementos = Array.from(root.querySelectorAll('*'));

                        for (const elemento of elementos) {
                            if (elemento.shadowRoot) {
                                buscarIframes(elemento.shadowRoot);
                            }
                        }
                    }

                    buscarIframes(document);

                    return iframes;
                    ");

        if (resultado is IReadOnlyCollection<object> objetos)
        {
            return objetos
                .OfType<IWebElement>()
                .ToList();
        }

        return [];
    }

    private void PreencherCurrentCrushName( string currentCrushName)
    {
        var input = ObterElementoNoContextoAtual(
                @"
                const seletores = [
                    'input[placeholder*=""Current Crush Name"" i]',
                    'input[placeholder*=""Current"" i]',
                    'input[placeholder*=""Crush"" i]',
                    'input[name*=""crush"" i]',
                    'input[id*=""crush"" i]',
                    'input'
                ];

                for (const seletor of seletores) {
                    const elemento = document.querySelector(seletor);

                    if (elemento) {
                        return elemento;
                    }
                }

                return null;
                ",
                "Não foi possível encontrar o campo Current Crush Name dentro do iframe.");

        input.Clear();
        input.SendKeys(currentCrushName);

        Log.Information("Current Crush Name preenchido: {CurrentCrushName}", currentCrushName);

        SalvarExecucao("Preencher Current Crush Name", currentCrushName);
    }

    private void ClicarConnectNow()
    {
        var botao = ObterElementoNoContextoAtual(
                @"
                const elementos = Array.from(document.querySelectorAll('button, input[type=""button""], input[type=""submit""]'));

                return elementos.find(elemento => {
                    const texto = (elemento.innerText || elemento.value || '').trim().toLowerCase();

                    return texto.includes('connect now') || texto.includes('connect');
                }) ?? null;
                ",
                "Não foi possível encontrar o botão Connect Now dentro do iframe.");

        botao.Click();

        Log.Information("Botão Connect Now clicado");

        SalvarExecucao("Clicar Connect Now", "Botão clicado");
    }

    private string CapturarConfirmacaoIframe()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        return wait.Until(driver =>
        {
            var texto = ((IJavaScriptExecutor)driver)
                    .ExecuteScript(
                        @"
                        const textos = Array
                            .from(document.body.querySelectorAll('body, div, p, span, h1, h2, h3, h4, h5, h6'))
                            .map(elemento => elemento.innerText?.trim())
                            .filter(texto => texto && texto.length > 0);

                        const confirmacao = textos.find(texto =>
                            texto.toLowerCase().includes('connected') ||
                            texto.toLowerCase().includes('connect') ||
                            texto.toLowerCase().includes('crush'));

                        return confirmacao ?? document.body.innerText.trim();
                        ")
                    ?.ToString();

            texto = texto?.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto.Length > 500
                ? texto[..500]
                : texto;
        })!;
    }

    private void EntrarNestedIframe()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        var nestedIframe =
            wait.Until(driver =>
            {
                var iframes = driver.FindElements(By.CssSelector("iframe"));

                return iframes.FirstOrDefault();
            });

        if (nestedIframe is null)
        {
            throw new NoSuchElementException("Não foi possível encontrar o nested iframe dentro do iframe principal.");
        }

        _driver
            .SwitchTo()
            .Frame(nestedIframe);

        Log.Information("Nested iframe acessado");

        SalvarExecucao("Entrar nested iframe", "Nested iframe acessado");
    }

    private void PreencherDestiny(string destiny)
    {
        var input =
            ObterElementoNoContextoAtual(
                @"
                const seletores = [
                    'input[placeholder*=""Destiny"" i]',
                    'input[name*=""destiny"" i]',
                    'input[id*=""destiny"" i]',
                    'input',
                    'textarea'
                ];

                for (const seletor of seletores) {
                    const elemento = document.querySelector(seletor);

                    if (elemento) {
                        return elemento;
                    }
                }

                return null;
                ",
                "Não foi possível encontrar o campo Destiny dentro do nested iframe.");

        input.Clear();
        input.SendKeys(destiny);

        Log.Information("Destiny preenchido: {Destiny}", destiny);

        SalvarExecucao("Preencher Destiny", destiny);
    }

    private void ClicarCloseIt()
    {
        var botao =
            ObterElementoNoContextoAtual(
                @"
                const elementos = Array.from(document.querySelectorAll('button, input[type=""button""], input[type=""submit""]'));

                return elementos.find(elemento => {
                    const texto = (elemento.innerText || elemento.value || '').trim().toLowerCase();

                    return texto.includes('close it') || texto.includes('close');
                }) ?? null;
                ",
                "Não foi possível encontrar o botão Close It dentro do nested iframe.");

        botao.Click();

        Log.Information("Botão Close It clicado");

        SalvarExecucao("Clicar Close It", "Botão clicado");
    }

    private IWebElement ObterElementoNoContextoAtual(string script, string mensagemErro)
    {
        var wait =
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        var elemento =
            wait.Until(driver =>
                (IWebElement?)((IJavaScriptExecutor)driver)
                    .ExecuteScript(script));

        if (elemento is null)
        {
            throw new NoSuchElementException(
                mensagemErro);
        }

        ((IJavaScriptExecutor)_driver)
            .ExecuteScript("arguments[0].scrollIntoView({ block: 'center' });", elemento);

        return elemento;
    }

    private void SalvarExecucao(string acao, string valor, bool sucesso = true)
    {
        _execucaoRepository.Salvar(
            new ExecucaoAutomacao
            {
                AcaoExecutada = acao,
                ValorCapturado = valor,
                Sucesso = sucesso,
                DataExecucao = DateTime.Now
            });
    }
}
