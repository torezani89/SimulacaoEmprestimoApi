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
        public async Task<ActionResult<ResponseModel<UsuarioResponseDto>>> RegistrarUsuario(UsuarioCriacaoDto usuarioCriacao)
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
        public async Task<ActionResult<ResponseModel<IEnumerable<UsuarioModel>>>> ListarUsuarios()
        {
            ResponseModel < IEnumerable <UsuarioModel>> response = await _usuarioService.ListarUsuariosAsync();
            return Ok(response);
        }

        [HttpGet("buscar/{id:int}")]
        public async Task<ActionResult<UsuarioModel>>? ObterUsuario(int id)
        {
            UsuarioModel? response = await _usuarioService.ObterUsuarioPorIdAsync(id);
            return Ok(response);
        }

        [HttpDelete("remover/{id}")]
        public async Task<IActionResult> RemoverUsuario(int id)
        {
            //throw new ArgumentNullException("Controller: Erro simulado para testes"); // ### forçar erro para testes ###
            var response = await _usuarioService.RemoverUsuarioAsync(id);

            return Ok(response);
        }
    }
}
