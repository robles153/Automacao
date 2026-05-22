using Serilog;

namespace Automacao.Helpers;

public static class RetryHelper
{
    public static void Executar(string acao, Action operacao, int tentativas = 3, int intervaloMilissegundos = 1000)
    {
        for (var tentativa = 1; tentativa <= tentativas; tentativa++)
        {
            try
            {
                operacao();

                return;
            }
            catch (Exception ex) when (tentativa < tentativas)
            {
                Log.Warning(
                    ex,
                    "Falha ao executar {Acao}. Tentativa {Tentativa} de {Tentativas}",
                    acao,
                    tentativa,
                    tentativas);

                Thread.Sleep(intervaloMilissegundos);
            }
        }

        operacao();
    }
}
