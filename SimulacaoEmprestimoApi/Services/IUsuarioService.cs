using SimulacaoEmprestimoApi.Models;

namespace SimulacaoEmprestimoApi.Services
{
    public interface IUsuarioService
    {
        Task<ResponseModel<UsuarioModel>> RegistrarUsuarioAsync(UsuarioCriacaoDto usuarioCriacao);
        Task<ResponseModel<UsuarioModel>> LoginAsync(UsuarioLoginDto loginRequest);
    }
}
