using SimulacaoEmprestimoApi.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimulacaoEmprestimoApi.Extensions
{
    public class SimulacaoResponseDto
    {
        public long IdSimulacao { get; set; }
        public decimal ValorDesejado { get; set; }
        public int Prazo { get; set; }
        public int CodigoProduto { get; set; }
        //public string DescricaoProduto { get; set; } = string.Empty;
        public decimal TaxaJuros { get; set; }
    }

    public static class SimulacaoMappingExtensionDto
    {
        public static SimulacaoResponseDto ToSimulacaoResponseDto(this SimulacaoModel simulacao)
        {
            return new SimulacaoResponseDto
            {
                IdSimulacao = simulacao.IdSimulacao,
                ValorDesejado = simulacao.ValorDesejado,
                Prazo = simulacao.Prazo,
                CodigoProduto = simulacao.CodigoProduto,
                TaxaJuros = simulacao.TaxaJuros
            };
        }
    }
}
