using System.ComponentModel.DataAnnotations.Schema;

namespace SimulacaoEmprestimoApi.Models
{
    public class SimulacaoModel
    {
        public long IdSimulacao { get; set; }
        public int CodigoProduto { get; set; }
        public decimal ValorDesejado { get; set; }
        public int Prazo { get; set; }
        [Column(TypeName = "decimal(10,9)")] // sem especificar, EF usa decimal(18,2) por padrão
        public decimal TaxaJuros { get; set; }
        public DateTime DataSimulacao { get; set; }

        public ICollection<ParcelaModel>? Parcelas { get; set; } // coleçao de navegação
    }
}
