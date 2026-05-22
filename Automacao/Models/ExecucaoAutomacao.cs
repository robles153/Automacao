namespace Automacao.Models
{
    public class ExecucaoAutomacao
    {
        public string AcaoExecutada { get; set; } = string.Empty;

        public string ValorCapturado { get; set; } = string.Empty;

        public bool Sucesso { get; set; }

        public DateTime DataExecucao { get; set; }
    }
}

