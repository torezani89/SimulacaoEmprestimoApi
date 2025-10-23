using System.ComponentModel.DataAnnotations;

namespace SimulacaoEmprestimoApi.Models
{
    public class UsuarioLoginDto
    {
        [Required(ErrorMessage = "Digite o email")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Digite a senha")]
        public string Senha { get; set; }
    }
}
