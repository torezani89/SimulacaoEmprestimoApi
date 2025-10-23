using Microsoft.EntityFrameworkCore;
using SimulacaoEmprestimoApi.Data;
using SimulacaoEmprestimoApi.Models;

namespace SimulacaoEmprestimoApi.Services
{
    public class UsuarioService : IUsuarioService
    {

        private readonly AppDbContext _dbContext;
        private readonly ILogger<UsuarioService> _logger;
        private readonly ISenhaservice _senhaService;

        public UsuarioService(AppDbContext dbContext, ILogger<UsuarioService> logger, ISenhaservice senhaService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _senhaService = senhaService;
        }

        public async Task<ResponseModel<UsuarioModel>> LoginAsync(UsuarioLoginDto usuarioLoginDto)
        {
            _logger.LogInformation("Tentativa de login para o email: {Email}", usuarioLoginDto.Email);
            ResponseModel<UsuarioModel> response = new ResponseModel<UsuarioModel>();

            try
            {
                var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(userBanco => userBanco.Email == usuarioLoginDto.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuário não encontrado para o email: {Email}", usuarioLoginDto.Email);
                    response.Mensagem = "Usuário não localizado!";
                    response.Status = false;
                    return response;
                }

                if (!_senhaService.VerificaSenhaHash(usuarioLoginDto.Senha, usuario.SenhaHash, usuario.SenhaSalt))
                {
                    _logger.LogWarning("Senha inválida para o usuário: {Email}", usuarioLoginDto.Email);
                    response.Mensagem = "Senha inválida";
                    response.Status = false;
                    return response;
                }

                string token = _senhaService.CriarToken(usuario); // retorna o token que é uma string
                usuario.Token = token;
                _dbContext.Usuarios.Update(usuario);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Login realizado com sucesso para o usuário: {UsuarioId}", usuario.Id);

                response.Dados = usuario;
                response.Mensagem = "Usuário logado com sucesso";
                response.Status = true;
            }
            catch (Exception erro)
            {
                _logger.LogError(erro, "Erro durante o login para o email: {Email}", usuarioLoginDto.Email);
                response.Mensagem = erro.Message;
                response.Status = false;
            }
            return response;
        }

        public async Task<ResponseModel<UsuarioModel>> RegistrarUsuarioAsync(UsuarioCriacaoDto usuarioCriacaoDto)
        {
            _logger.LogInformation("Iniciando registro de novo usuário: {Email}", usuarioCriacaoDto.Email);
            ResponseModel<UsuarioModel> response = new ResponseModel<UsuarioModel>();

            try
            {
                //if (!await VerificaUsuarioRepetidoAsync(usuarioCriacaoDto))
                //{
                //    _logger.LogWarning("Tentativa de cadastro com email/usuário já existente: {Email}", usuarioCriacaoDto.Email);
                //    response.Mensagem = "Email ou Usuário já cadastrados.";
                //    response.Status = false;
                //    return response;
                //}

                _senhaService.CriarSenhaHash(usuarioCriacaoDto.Senha, out byte[] senhaHash, out byte[] senhaSalt);

                UsuarioModel usuario = new UsuarioModel()
                {
                    Usuario = usuarioCriacaoDto.Usuario,
                    Nome = usuarioCriacaoDto.Nome,
                    Sobrenome = usuarioCriacaoDto.Sobrenome,
                    Email = usuarioCriacaoDto.Email,
                    SenhaHash = senhaHash,
                    SenhaSalt = senhaSalt
                };

                _dbContext.Add(usuario);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Usuário registrado com sucesso. ID: {UsuarioId}, Email: {Email}", usuario.Id, usuario.Email);

                response.Dados = usuario;
                response.Mensagem = "Usuário cadastrado com sucesso";
                response.Status = true;
                return response; // Compilador converte automaticamente para Task
            }
            catch (Exception erro)
            {
                _logger.LogError(erro, "Erro ao registrar usuário {Email}. Detalhes: {ErroDetalhes}", usuarioCriacaoDto.Email, erro.Message);
                response.Mensagem = erro.Message;
                response.Status = false;
                return await Task.FromResult(response); // Corrigido para retornar uma Task
            }
        }
    }
}
