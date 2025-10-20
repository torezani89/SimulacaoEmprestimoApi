namespace SimulacaoEmprestimoApi.Models
{
    public class ParcelaModel
    {
        public long IdParcela { get; set; }
        public long IdSimulacao { get; set; }
        public string TipoAmortizacao { get; set; } = string.Empty;
        public int Numero { get; set; }
        public decimal ValorAmortizacao { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorPrestacao { get; set; }

    }
}

