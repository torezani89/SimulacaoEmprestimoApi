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
            //try
            //{
                HealthCheckResponse status = await _simulacaoService.VerificarSaudeAsync();
                return Ok(status);
            //}
            //catch (Exception ex) // tratamento de erros centralizado em ErrorHandlingMiddleware
            //{ 
            //    return StatusCode(500, new {status = "Erro", detalhe = ex.Message});
            //}
        }

        [HttpPost("simular")] // POST(request) para /api/Simulacao/simular
        public async Task<IActionResult> Simular([FromBody] SimulacaoRequest request)
        {
            if (request.ValorDesejado <= 0 || request.Prazo <= 0)
                return BadRequest("Valor e prazo devem ser maiores que zero.");

            bool forcarErro = false;
            if (forcarErro)
            {
                throw new UnauthorizedAccessException("simulação UnauthorizedAccessException lançado no bloco try{} do controller");
            }
            var resultado = await _simulacaoService.SimularAsync(request);
            return Ok(resultado);
        }

        //[Authorize]
        [HttpGet("listar")] // GET para /api/Simulacao/listar
        public async Task<IActionResult> ListarSimulacoesPersistidas()
        {
            bool forcarErro = false;
            if (forcarErro)
            {
                throw new Exception("Erro simulado para testes");
            }
            List<SimulacaoModel> simulacoes = await _simulacaoService.ListarSimulacoesPersistidasAsync();
            return Ok(simulacoes);
        }

        [Authorize]
        [HttpGet("buscar/{idSimulacao:long}")]
        public async Task<IActionResult> ObterSimulacaoPorId(long idSimulacao)
        {
            var simulacao = await _simulacaoService.ObterSimulacaoPorIdAsync(idSimulacao);
            if (simulacao == null)
                return NotFound(new { erro = "Simulação não encontrada ou expirada do cache." });
            return Ok(simulacao);
        }

        [Authorize]
        [HttpPut("editar/{idSimulacao:long}")]
        public async Task<IActionResult> AtualizarSimulacao(long idSimulacao, [FromBody] SimulacaoRequest request)
        {
            var response = await _simulacaoService.AtualizarSimulacaoAsync(idSimulacao, request);
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("deletar/{idSimulacao:long}")]
        public async Task<IActionResult> ExcluirSimulacao(long idSimulacao)
        {
                bool excluida = await _simulacaoService.ExcluirSimulacaoAsync(idSimulacao);
                return Ok(new { mensagem = "Simulação excluída com sucesso." });
        }

        [Authorize]
        [HttpGet("cache/{idSimulacao:long}")]
        public IActionResult ObterSimulacaoCache(long idSimulacao)
        {
            SimulacaoResponse? simulacao = _simulacaoService.ObterSimulacaoCachePorId(idSimulacao);
            if (simulacao == null)
                return NotFound(new { erro = "Simulação não encontrada ou expirada do cache." });
            return Ok(simulacao);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> ObterEstatisticas()
        {
            SimulacaoEstatisticasResponse estatisticas = await _simulacaoService.ObterEstatisticasAsync();
            return Ok(estatisticas);
        }
    }
}
