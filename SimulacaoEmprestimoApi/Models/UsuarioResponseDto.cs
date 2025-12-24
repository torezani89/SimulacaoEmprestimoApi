namespace SimulacaoEmprestimoApi.Models
{
    public class UsuarioResponseDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Sobrenome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
