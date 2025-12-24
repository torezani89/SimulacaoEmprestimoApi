namespace SimulacaoEmprestimoApi.Models
{
    public class TabelaParcelasResponse
    {
        public string Tipo { get; set; } = string.Empty; // "SAC" ou "PRICE"
        public List<ParcelaResponse> Parcelas { get; set; } = new();
    }
}
