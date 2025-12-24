using System.ComponentModel.DataAnnotations;

namespace SimulacaoEmprestimoApi.Models
{
    public class SimulacaoRequest
    {
        [Required(ErrorMessage = "O campo 'ValorDesejado' é obrigatório.")]
        [Range(100, 10000000, ErrorMessage = "O valor desejado deve estar entre {1:C} e {2:C}.")]
        public decimal ValorDesejado { get; set; }
        [Required(ErrorMessage = "O campo 'Prazo' é obrigatório.")]
        [Range(1, 240, ErrorMessage = "O prazo deve estar entre {1} e {2} meses.")]
        public int Prazo { get; set; }
    }
}
