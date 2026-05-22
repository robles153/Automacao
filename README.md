# Automacao

Automacao em C# com Selenium WebDriver para o teste tecnico de RPA/automacao.

## O que a automacao faz

- Acessa a pagina `https://selectorshub.com/xpath-practice-page/`.
- Interage com shadow DOM e nested shadow DOM.
- Preenche usuario e pizza usando dados do `appsettings.json`.
- Marca o checkbox da primeira linha da tabela.
- Captura `username` e `status` da linha marcada.
- Acessa a pagina `https://selectorshub.com/iframe-in-shadow-dom/`.
- Clica no link `Learning Hub` dentro do shadow DOM.
- Captura texto clicado, URL aberta e titulo da nova aba.
- Fecha a nova aba e volta para a pagina principal.
- Interage com iframe dentro de shadow DOM.
- Preenche `Current Crush Name`, clica em `Connect Now` e captura a confirmacao.
- Interage com nested iframe, preenche `Destiny` e clica em `Close It`.
- Persiste as evidencias no SQL Server.
- Gera logs em arquivo `.txt`.
- Gera screenshot automaticamente em caso de erro.

## Requisitos

- .NET 8 SDK
- Google Chrome
- SQL Server
- Banco `AutomationTest`

## Banco de dados

Execute o script:

```text
docs/create-database.sql
```

Ele cria o banco `AutomationTest` e a tabela `ExecucaoAutomacao`.

Campos persistidos:

- `AcaoExecutada`
- `ValorCapturado`
- `Sucesso`
- `DataExecucao`

## Configuracao

Atualize a connection string em:

```text
Automacao/appsettings.json
```

Exemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-NP40629;Database=AutomationTest;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "DadosTeste": {
    "Usuario": "Marcos",
    "Pizza": "Calabresa",
    "CurrentCrushName": "Teste Crush",
    "Destiny": "Automacao RPA"
  }
}
```

## Arquitetura

O projeto foi separado em responsabilidades simples:

- `Services/AutomationService.cs`: orquestra a execucao das etapas.
- `Pages/PracticePage.cs`: contem as interacoes da pagina de shadow DOM e tabela.
- `Pages/IframeShadowDomPage.cs`: contem as interacoes com shadow DOM, iframe e nested iframe.
- `Repositories/IExecucaoRepository.cs`: contrato de persistencia.
- `Repositories/SqlExecucaoRepository.cs`: implementacao da persistencia em SQL Server.
- `Models/DadosTeste.cs`: parametros externos lidos do `appsettings.json`.

## Como executar

Na raiz do repositorio:

```powershell
dotnet restore
dotnet build .\Automacao.sln
dotnet run --project .\Automacao\Automacao.csproj
```

## Evidencias geradas

Logs:

```text
Automacao/Logs/log-dd-MM-yyyy.txt
```

Screenshots em erro:

```text
Automacao/bin/Debug/net8.0/Screenshots
```

Consulta para verificar os dados salvos:

```sql
SELECT *
FROM ExecucaoAutomacao
ORDER BY Id DESC;
```
