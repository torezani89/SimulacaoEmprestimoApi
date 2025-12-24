namespace SimulacaoEmprestimoApi.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; } // atribuído pelo banco de dados
        public string Usuario { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Sobrenome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // será criado quando o usuario fizer login
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime DataAlteracao { get; set; } = DateTime.Now;
        public byte[] SenhaHash { get; set; } = new byte[0];
        public byte[] SenhaSalt { get; set; } = new byte[0];
    }
}
