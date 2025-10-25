using SimulacaoEmprestimoApi.Models;

namespace SimulacaoEmprestimoApi.Services
{
    public interface IUsuarioService
    {
        Task<ResponseModel<UsuarioResponseDto>> RegistrarUsuarioAsync(UsuarioCriacaoDto usuarioCriacao);
        Task<ResponseModel<UsuarioResponseDto>> LoginAsync(UsuarioLoginDto loginRequest);
        Task<ResponseModel<IEnumerable<UsuarioModel>>> ListarUsuariosAsync();
        Task<ResponseModel<UsuarioResponseDto>> RemoverUsuarioAsync(int id);
        Task<UsuarioModel> ObterUsuarioPorIdAsync(int id);
    }
}
