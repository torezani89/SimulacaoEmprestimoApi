namespace SimulacaoEmprestimoApi.Models
{
    public class ParcelaResponse
    {
        public int Numero { get; set; }
        public decimal ValorAmortizacao { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorPrestacao { get; set; }
    }
}
