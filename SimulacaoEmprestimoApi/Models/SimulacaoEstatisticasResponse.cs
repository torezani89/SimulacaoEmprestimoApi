namespace SimulacaoEmprestimoApi.Models
{
    // reflete estatísticas agregadas das simulações já gravadas no banco
    public class SimulacaoEstatisticasResponse
    {
        public int TotalSimulacoes { get; set; }
        public decimal ValorMedio { get; set; }
        public int PrazoMedio { get; set; }
        public decimal TaxaMedia { get; set; }
        public DateTime UltimaAtualizacao { get; set; }
        public List<EstatisticaProdutoResponse> EstatisticasPorProduto { get; set; } = new();
    }

    public class EstatisticaProdutoResponse
    {
        public int CodigoProduto { get; set; }
        public string DescricaoProduto { get; set; } = string.Empty;
        public int QuantidadeSimulacoes { get; set; }
        public decimal ValorMedio { get; set; }
        public int PrazoMedio { get; set; }
        public decimal TaxaMedia { get; set; }
    }
}
