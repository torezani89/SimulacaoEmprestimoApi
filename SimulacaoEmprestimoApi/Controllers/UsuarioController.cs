using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulacaoEmprestimoApi.Models;
using SimulacaoEmprestimoApi.Services;

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
    }
}
