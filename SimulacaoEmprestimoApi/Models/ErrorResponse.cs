namespace SimulacaoEmprestimoApi.Models
{
    public class ErrorResponse
    {
        public bool Sucesso { get; set; } = false;
        public int StatusCode { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? Detalhes { get; set; }
        public string? Caminho { get; set; }
    }
}
