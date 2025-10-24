using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulacaoEmprestimoApi.Models;
using SimulacaoEmprestimoApi.Services;
using System.Collections.Generic;

namespace SimulacaoEmprestimoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult<ResponseModel<UsuarioModel>>> RegistrarUsuario(UsuarioCriacaoDto usuarioCriacao)
        {
             var response = await _usuarioService.RegistrarUsuarioAsync(usuarioCriacao);
             return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseModel<UsuarioModel>>> Login(UsuarioLoginDto usuarioLoginDto)
        {
            var response = await _usuarioService.LoginAsync(usuarioLoginDto);
            return Ok(response);
        }

        [HttpGet("listar")]
        public async Task<ActionResult<ResponseModel<List<UsuarioModel>>>> ListarUsuarios()
        {
            ResponseModel < List < UsuarioModel >> response = await _usuarioService.ListarUsuariosAsync();
            return Ok(response);
        }

        [HttpDelete("remover/{id}")]
        public async Task<IActionResult> RemoverUsuario(int id)
        {
            var response = await _usuarioService.RemoverUsuarioAsync(id);

            if (!response.Status) // "se response.Status == false" (operação falhou)
            {
                // Retorna 404 se usuário não foi encontrado
                if (response.Mensagem.Contains("não localizado")) return NotFound(response);

                return BadRequest(response); // Retorna 400 para outros erros do cliente
            }

            return Ok(response);
        }
    }
}
