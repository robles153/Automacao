using Automacao.Helpers;
using Automacao.Models;
using Automacao.Repositories;
using OpenQA.Selenium;
using Serilog;

namespace Automacao.Pages;

public class PracticePage
{
    private readonly IWebDriver _driver;
    private readonly IExecucaoRepository _execucaoRepository;

    public PracticePage(IWebDriver driver, IExecucaoRepository execucaoRepository)
    {
        _driver = driver;
        _execucaoRepository = execucaoRepository;
    }

    public void Executar(DadosTeste dadosTeste)
    {
        AcessarPagina();

        PreencherShadowDom(dadosTeste.Usuario);

        PreencherPizzaNestedShadowDom(dadosTeste.Pizza);

        MarcarPrimeiraLinhaTabela();
    }

    private void AcessarPagina()
    {
        _driver.Navigate().GoToUrl("https://selectorshub.com/xpath-practice-page/");

        Log.Information("Página acessada");

        SalvarExecucao("Abrir página", "Página aberta com sucesso");
    }

    private void PreencherShadowDom(string usuario)
    {
        var shadowHost = WaitHelper.EsperarElemento(_driver, By.CssSelector("#userName"));

        var shadowRoot = ShadowDomHelper.GetShadowRoot(_driver, shadowHost);

        var cabecalho = CapturarCabecalhoShadowDom(shadowRoot);

        Log.Information("Cabeçalho do shadow DOM capturado: {Cabecalho}",cabecalho);

        SalvarExecucao("Ler cabeçalho shadow DOM", cabecalho);

        var inputUsuario = shadowRoot.FindElement(By.CssSelector("input"));

        inputUsuario.SendKeys(usuario);

        Log.Information("Usuário preenchido: {Usuario}", usuario);

        SalvarExecucao("Preencher usuário", usuario);
    }

    private void PreencherPizzaNestedShadowDom(string pizza)
    {
        var userNameHost = WaitHelper.EsperarElemento(_driver, By.CssSelector("#userName"));

        var inputPizza =
            (IWebElement?)((IJavaScriptExecutor)_driver)
                .ExecuteScript(
                    @"
                    const userNameHost = arguments[0];
                    const userNameRoot = userNameHost.shadowRoot;
                    const app2Host = userNameRoot.querySelector('#app2');

                    if (!app2Host || !app2Host.shadowRoot) {
                        return null;
                    }

                    function buscarInputPizza(root) {
                        const seletores = [
                            'input[id*=""pizza"" i]',
                            'input[name*=""pizza"" i]',
                            'input[placeholder*=""pizza"" i]',
                            'input',
                            'textarea'
                        ];

                        for (const seletor of seletores) {
                            const elemento = root.querySelector(seletor);

                            if (elemento) {
                                return elemento;
                            }
                        }

                        const elementos = root.querySelectorAll('*');

                        for (const elemento of elementos) {
                            if (elemento.shadowRoot) {
                                const encontrado = buscarInputPizza(elemento.shadowRoot);

                                if (encontrado) {
                                    return encontrado;
                                }
                            }
                        }

                        return null;
                    }

                    return buscarInputPizza(app2Host.shadowRoot);
                    ",
                    userNameHost);

        if (inputPizza is null)
        {
            throw new NoSuchElementException("Não foi possível encontrar o campo de pizza dentro do nested shadow DOM #userName > #app2.");
        }

        ((IJavaScriptExecutor)_driver)
            .ExecuteScript("arguments[0].scrollIntoView({ block: 'center' });", inputPizza);

        inputPizza.Clear();
        inputPizza.SendKeys(pizza);

        Log.Information("Pizza preenchida no nested shadow DOM: {Pizza}", pizza);

        SalvarExecucao("Preencher pizza nested shadow DOM", pizza);
    }

    private void MarcarPrimeiraLinhaTabela()
    {
        var resultado = (Dictionary<string, object>?)((IJavaScriptExecutor)_driver)
                .ExecuteScript(
                    @"
                    const tabelas = Array.from(document.querySelectorAll('table'));

                    const tabelaUsuarios = tabelas.find(tabela => {
                        const textoCabecalho = Array
                            .from(tabela.querySelectorAll('th, td'))
                            .slice(0, 8)
                            .map(celula => celula.innerText.trim().toLowerCase())
                            .join('|');

                        return textoCabecalho.includes('username') &&
                            textoCabecalho.includes('status');
                    });

                    if (!tabelaUsuarios) {
                        return null;
                    }

                    tabelaUsuarios.scrollIntoView({ block: 'center' });

                    const linhas = Array.from(tabelaUsuarios.querySelectorAll('tr'));
                    const linhaCabecalho = linhas.find(linha =>
                        linha.innerText.toLowerCase().includes('username') &&
                        linha.innerText.toLowerCase().includes('status'));

                    const indiceCabecalho = linhas.indexOf(linhaCabecalho);
                    const primeiraLinhaDados = linhas
                        .slice(indiceCabecalho + 1)
                        .find(linha => linha.querySelector('input[type=""checkbox""]'));

                    if (!linhaCabecalho || !primeiraLinhaDados) {
                        return null;
                    }

                    const cabecalhos = Array
                        .from(linhaCabecalho.querySelectorAll('th, td'))
                        .map(celula => celula.innerText.trim().toLowerCase());

                    const indiceUsername = cabecalhos.findIndex(texto => texto.includes('username'));
                    const indiceStatus = cabecalhos.findIndex(texto => texto.includes('status'));
                    const celulas = Array.from(primeiraLinhaDados.querySelectorAll('td, th'));
                    const checkbox = primeiraLinhaDados.querySelector('input[type=""checkbox""]');

                    checkbox.scrollIntoView({ block: 'center' });

                    if (!checkbox.checked) {
                        checkbox.click();
                    }

                    return {
                        username: celulas[indiceUsername]?.innerText.trim() ?? '',
                        status: celulas[indiceStatus]?.innerText.trim() ?? '',
                        checkboxMarcado: checkbox.checked
                    };
                    ");

        if (resultado is null)
        {
            throw new NoSuchElementException("Não foi possível encontrar a tabela de usuários com checkbox, username e status.");
        }

        var username = resultado["username"]?.ToString() ?? string.Empty;

        var status = resultado["status"]?.ToString() ?? string.Empty;

        var checkboxMarcado = resultado["checkboxMarcado"]?.ToString() ?? "False";

        Log.Information("Checkbox da primeira linha marcado. Username: {Username}. Status: {Status}", username, status);

        SalvarExecucao("Marcar checkbox primeira linha", checkboxMarcado);

        SalvarExecucao("Capturar username tabela", username);

        SalvarExecucao("Capturar status tabela", status);
    }

    private static string CapturarCabecalhoShadowDom( ISearchContext shadowRoot)
    {
        var seletoresCabecalho = new[]
                                {
                                    "h1",
                                    "h2",
                                    "h3",
                                    "h4",
                                    "h5",
                                    "h6",
                                    "legend",
                                    "label",
                                    "div"
                                };

        foreach (var seletor in seletoresCabecalho)
        {
            var elementos = shadowRoot.FindElements(By.CssSelector(seletor));

            var texto = elementos
                    .Select(elemento => elemento.Text.Trim())
                    .FirstOrDefault(texto => !string.IsNullOrWhiteSpace(texto));

            if (!string.IsNullOrWhiteSpace(texto))
            {
                return texto;
            }
        }

        throw new NoSuchElementException("Não foi possível encontrar o cabeçalho dentro do shadow DOM #userName.");
    }

    private void SalvarExecucao( string acao, string valor, bool sucesso = true)
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

