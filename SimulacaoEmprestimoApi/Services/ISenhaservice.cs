using SimulacaoEmprestimoApi.Models;

namespace SimulacaoEmprestimoApi.Services
{
    public interface ISenhaservice
    {
        void CriarSenhaHash(string senha, out byte[] senhaHash, out byte[] senhaSalt);
        bool VerificaSenhaHash(string senha, byte[] senhaSalt, byte[] senhaHash);
        string CriarToken(UsuarioModel usuario);
    }
}
