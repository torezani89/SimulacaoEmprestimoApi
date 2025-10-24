using Microsoft.AspNetCore.Mvc;
using SimulacaoEmprestimoApi.Models;
using SimulacaoEmprestimoApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace SimulacaoEmprestimoApi.Controllers
{
    [ApiController] // habilita validação automática sem chamar ModelState.IsValid
    [Route("api/[controller]")] // rota base: https://localhost:7204/api/Simulacao
    public class SimulacaoController : ControllerBase
    {
        private readonly ISimulacaoService _simulacaoService;

        public SimulacaoController(ISimulacaoService simulacaoService)
        {
            _simulacaoService = simulacaoService;
        }

        [HttpGet("health")] //GET → /api/simulacao/health
        public async Task<IActionResult> VerificarSaude()
        {
            try
            {
                HealthCheckResponse status = await _simulacaoService.VerificarSaudeAsync();
                return Ok(status);
            }
            catch (Exception ex)
            { 
                return StatusCode(500, new {status = "Erro", detalhe = ex.Message});
            }
        }

        [HttpPost("simular")] // POST(request) para /api/Simulacao/simular
        public async Task<IActionResult> Simular([FromBody] SimulacaoRequest request)
        {
            if (request.ValorDesejado <= 0 || request.Prazo <= 0)
                return BadRequest("Valor e prazo devem ser maiores que zero.");
            bool forcarErro = false;
            if (forcarErro)
            {
                //throw new Exception("Erro simulado para testes");
                //throw new ArgumentException();
                throw new UnauthorizedAccessException();
                //throw new KeyNotFoundException();
            }

            try
            {
                var resultado = await _simulacaoService.SimularAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        //[Authorize]
        [HttpGet("listar")]
        public async Task<IActionResult> ListarSimulacoesPersistidas()
        {
            bool forcarErro = true;
            if (forcarErro)
            {
                throw new Exception("Erro simulado para testes");
                //throw new ArgumentException();
                //throw new UnauthorizedAccessException();
                //throw new KeyNotFoundException();
            }
            List<SimulacaoModel> simulacoes = await _simulacaoService.ListarSimulacoesPersistidasAsync();
            return Ok(simulacoes);
        }

        [Authorize]
        [HttpGet("buscar/{idSimulacao:long}")]
        public async Task<IActionResult> ObterSimulacaoPorId(long idSimulacao)
        {
            try
            {
                var simulacao = await _simulacaoService.ObterSimulacaoPorIdAsync(idSimulacao);
                if (simulacao == null)
                    return NotFound(new { erro = "Simulação não encontrada ou expirada do cache." });
                return Ok(simulacao);
            }
            catch (Exception ex)
            {
                //return NotFound(new { erro = ex.Message });
                return BadRequest(new { erro = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("editar/{idSimulacao:long}")]
        public async Task<IActionResult> AtualizarSimulacao(long idSimulacao, [FromBody] SimulacaoRequest request)
        {
            try
            {
                var response = await _simulacaoService.AtualizarSimulacaoAsync(idSimulacao, request);
                if (response == null) return NotFound(new { erro = "Simulação não encontrada" });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("deletar/{idSimulacao:long}")]
        public async Task<IActionResult> ExcluirSimulacao(long idSimulacao)
        {
            try
            {
                bool excluida = await _simulacaoService.ExcluirSimulacaoAsync(idSimulacao);

                if (!excluida) return NotFound(new { erro = "Simulação não encontrada" });

                return Ok(new { mensagem = "Simulação excluída com sucesso." });
            }
            catch (Exception ex)
            { 
                return BadRequest(new { erro = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("cache/{idSimulacao:long}")]
        public IActionResult ObterSimulacaoCache(long idSimulacao)
        {
            try
            {
                SimulacaoResponse? simulacao = _simulacaoService.ObterSimulacaoCachePorId(idSimulacao);
                if (simulacao == null)
                    return NotFound(new { erro = "Simulação não encontrada ou expirada do cache." });
                return Ok(simulacao);
            }
            catch (Exception ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> ObterEstatisticas()
        {
            try
            {
                SimulacaoEstatisticasResponse estatisticas = await _simulacaoService.ObterEstatisticasAsync();
                return Ok(estatisticas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { err = ex.Message });
            }
        }
    }
}
