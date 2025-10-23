using System.ComponentModel.DataAnnotations;

namespace SimulacaoEmprestimoApi.Models
{
    public class UsuarioCriacaoDto
    {
        [Required(ErrorMessage = "Digite o usuário")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite o nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite o sobrenome")]
        public string Sobrenome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite o email")]
        [EmailAddress(ErrorMessage="E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite a senha")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite a confirmação de senha")]
        [Compare("Senha", ErrorMessage = "As senhas não conferem")] // compara com o campo "Senha"
        public string ConfirmaSenha { get; set; } = string.Empty;
    }
}
