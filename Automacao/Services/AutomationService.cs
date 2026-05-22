using Automacao.Models;
using Automacao.Pages;

using Serilog;
namespace Automacao.Services;

public class AutomationService
{
    private readonly PracticePage _practicePage;
    private readonly IframeShadowDomPage _iframeShadowDomPage;
    private readonly DadosTeste _dadosTeste;

    public AutomationService(PracticePage practicePage, IframeShadowDomPage iframeShadowDomPage, DadosTeste dadosTeste)
    {
        _practicePage = practicePage;

        _iframeShadowDomPage = iframeShadowDomPage;

        _dadosTeste = dadosTeste;
    }

    public void Executar()
    {
        Log.Information("Dados de teste carregados do appsettings.json");

        _practicePage.Executar(_dadosTeste);

        _iframeShadowDomPage.Executar(_dadosTeste);
    }
}
