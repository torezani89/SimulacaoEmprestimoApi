using System.ComponentModel.DataAnnotations.Schema;

namespace SimulacaoEmprestimoApi.Models
{
    public class SimulacaoResponse
    {
        public long IdSimulacao { get; set; }
        public decimal ValorDesejado { get; set; }
        public int Prazo { get; set; }
        public int CodigoProduto { get; set; }
        public string DescricaoProduto { get; set; } = string.Empty;
        [Column(TypeName = "decimal(10,9)")]
        public decimal TaxaJuros { get; set; }
        public List<TabelaParcelasResponse> ResultadoSimulacao { get; set; } = new();
    }
}
