namespace SimulacaoEmprestimoApi.Models
{
    public class HealthCheckResponse
    {
        public string StatusApi { get; set; } = "Desconhecido";
        public string StatusBanco { get; set; } = "Desconhecido";
        public DateTime DataVerificacao { get; set; } = DateTime.Now;
        public string? MensagemErroBanco { get; set; }
        public long UptimeSegundos { get; set; }
    }
}
