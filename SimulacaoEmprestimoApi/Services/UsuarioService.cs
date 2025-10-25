using Microsoft.EntityFrameworkCore;
using SimulacaoEmprestimoApi.Data;
using SimulacaoEmprestimoApi.Models;
using System.Collections;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        // ======================================================
        // 📋 Listar Usuários
        // ======================================================
        public async Task<ResponseModel<IEnumerable<UsuarioModel>>> ListarUsuariosAsync()
        {
            _logger.LogInformation("Tentativa de listas usuários iniciada");

            List<UsuarioModel>? usuarios = await _dbContext.Usuarios.ToListAsync();

            if (usuarios == null || usuarios.Count == 0)
            {
                _logger.LogWarning("Nenhum usuário encontrado.");
                throw new KeyNotFoundException("Nenhum usuário encontrado.");
            }

            _logger.LogInformation("{Quantidade} usuários listados com sucesso", usuarios.Count);
            // IEnumerable.Count() → Método que pode executar a query SELECT COUNT(*)
            // List.Count → Propriedade que conta em memória (melhor)

            // usar no ambiente de produção para esconder dados sensíveis
            //IEnumerable<UsuarioResponseDto> usuariosResponseDtoList = usuarios.Select(usuario => usuario.ToUsuarioResponseDto()).ToList();
            // alterar tipo do returno do método no service/interface/controller para IEnumerable<UsuarioResponseDto>>

            return new ResponseModel<IEnumerable<UsuarioModel>> // provisório: permite visualizar Id/Token
            {
                Dados = usuarios,
                Mensagem = "Usuários listados com sucesso",
                Status = true
            };
        }

        // ======================================================
        // 🔐 Login
        // ======================================================
        public async Task<ResponseModel<UsuarioResponseDto>> LoginAsync(UsuarioLoginDto usuarioLoginDto)
        {
            _logger.LogInformation("Tentativa de login para o email: {Email}", usuarioLoginDto.Email);

            var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(userBanco => userBanco.Email == usuarioLoginDto.Email);

            if (usuario == null)
            {
                _logger.LogWarning("Usuário não encontrado para o email: {Email}", usuarioLoginDto.Email);
                throw new KeyNotFoundException("Usuário não localizado");
            }

            bool senhaValida = _senhaService.VerificaSenhaHash(usuarioLoginDto.Senha, usuario.SenhaHash, usuario.SenhaSalt);
            if (!senhaValida)
            {
                _logger.LogWarning("Senha inválida para o email: {Email}", usuarioLoginDto.Email);
                throw new UnauthorizedAccessException("Senha inválida");
            }

            string token = _senhaService.CriarToken(usuario); // retorna o token que é uma string
            usuario.Token = token;

            _dbContext.Usuarios.Update(usuario);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Login realizado com sucesso para o usuário: {UsuarioId}", usuario.Id);

            UsuarioResponseDto? usuarioResponse = usuario.ToUsuarioResponseDto(); // conversão com método de extensão

            return new ResponseModel<UsuarioResponseDto>
            {
                Dados = usuarioResponse,
                Mensagem = "Usuário logado com sucesso",
                Status = true
            };
        }

        // ======================================================
        //  Obter Usuário por Id
        // ======================================================
        public async Task<UsuarioModel> ObterUsuarioPorIdAsync(int id)
        {
            UsuarioModel? usuario = await _dbContext.Usuarios.FindAsync(id);

            if (usuario == null) throw new KeyNotFoundException($"Usuário ID {id} não encontrado.");

            //UsuarioResponseDto? usuarioResponse = usuario.ToUsuarioResponseDto(); // correto
            return usuario; // provisório: retorna usuario para usar o token (testes)
        }

        // ======================================================
        // 🧾 Registrar Usuário
        // ======================================================
        public async Task<ResponseModel<UsuarioResponseDto>> RegistrarUsuarioAsync(UsuarioCriacaoDto usuarioCriacaoDto)
        {
            _logger.LogInformation("Iniciando registro de novo usuário: {Email}", usuarioCriacaoDto.Email);

            //if (!await VerificaUsuarioRepetidoAsync(usuarioCriacaoDto))
            //{
            //    _logger.LogWarning("Tentativa de cadastro com email/usuário já existente: {Email}", usuarioCriacaoDto.Email);
            //    response.Mensagem = "Email ou Usuário já cadastrados.";
            //    response.Status = false;
            //    return response;
            //}
            bool usuarioExistente = await _dbContext.Usuarios.AnyAsync(u => u.Email == usuarioCriacaoDto.Email || u.Usuario == usuarioCriacaoDto.Usuario);

            if (usuarioExistente)
            {
                _logger.LogWarning("Tentativa de registro com email/usuário já existente: {Email}, {Usuário}", usuarioCriacaoDto.Email, usuarioCriacaoDto.Usuario);
                throw new ArgumentException("Email ou nome de usuário já cadastrados.");
            }
            _senhaService.CriarSenhaHash(usuarioCriacaoDto.Senha, out byte[] senhaHash, out byte[] senhaSalt);

            //UsuarioModel usuario = new UsuarioModel() // ### conversão manual ###
            //{
            //    Usuario = usuarioCriacaoDto.Usuario,
            //    Nome = usuarioCriacaoDto.Nome,
            //    Sobrenome = usuarioCriacaoDto.Sobrenome,
            //    Email = usuarioCriacaoDto.Email,
            //    SenhaHash = senhaHash,
            //    SenhaSalt = senhaSalt
            //};
            UsuarioModel usuario = usuarioCriacaoDto.ToUsuarioModel(senhaHash, senhaSalt); // conversão com método de extensão

            _dbContext.Add(usuario);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Usuário registrado com sucesso. ID: {UsuarioId}, Email: {Email}", usuario.Id, usuario.Email);

            UsuarioResponseDto? usuarioResponse = usuario.ToUsuarioResponseDto();

            return new ResponseModel<UsuarioResponseDto>
            {
                Dados = usuarioResponse,
                Mensagem = "Usuário cadastrado com sucesso",
                Status = true
            };
        }

        // ======================================================
        // 🗑️ Remover Usuário
        // ======================================================
        public async Task<ResponseModel<UsuarioResponseDto>> RemoverUsuarioAsync(int id)
        {
            _logger.LogInformation("Tentativa de remover usuário Id: {id}", id);

            UsuarioModel? usuario = await _dbContext.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                _logger.LogWarning("Usuário Id {id} não localizado no banco", id);
                throw new KeyNotFoundException($"UsuarioService.RemoverUsuarioAsync() : Usuário Id {id} não localizado no banco");
            }
            //throw new Exception("UsuarioService.RemoverUsuarioAsync(): Erro simulado para testes"); // ### forçar erro para testes ###

            _dbContext.Usuarios.Remove(usuario);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Usuário Id {id} removido com sucesso!", id);

            UsuarioResponseDto? usuarioResponse = usuario.ToUsuarioResponseDto();

            return new ResponseModel<UsuarioResponseDto>
            {
                Dados = usuarioResponse,
                Mensagem = $"Usuário {id} removido com sucesso",
                Status = true,
            };
        }
    }
}
