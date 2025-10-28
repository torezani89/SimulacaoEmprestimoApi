using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimulacaoEmprestimoApi.Models;
using SimulacaoEmprestimoApi.Pagination;
using SimulacaoEmprestimoApi.Services;
using System.Text.Json;

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
        [HttpGet("listar")] // GET para /api/Simulacao/listar (busca tudo de uma vez no banco -> sem paginação)
        public async Task<IActionResult> ListarSimulacoesPersistidas()
        {
            //throw new Exception("Erro simulado para testes"); // ### forçar erro para testes ###
            List<SimulacaoModel> simulacoes = await _simulacaoService.ListarSimulacoesPersistidasAsync();
            return Ok(simulacoes);
        }

        //[Authorize]
        [HttpGet("paginacao")] //GET /api/simulacao/paginacao?pageNumber=1&pageSize=10
        public ActionResult<IEnumerable<SimulacaoModel>> ListarSimulacoesPersistidasComPaginacao([FromQuery] SimulacaoParameters simulacaoParams)
        {
            var simulacoesPaginadas = _simulacaoService.ListarSimulacoesPersistidasAsyncComPaginacao(simulacaoParams);

            var metadata = new
            {
                simulacoesPaginadas.TotalCount,
                simulacoesPaginadas.TotalPages,
                simulacoesPaginadas.PageSize,
                simulacoesPaginadas.CurrentPage,
                simulacoesPaginadas.HasNext,
                simulacoesPaginadas.HasPrevious
            };

            //Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata)); // usando package Newtonsoft.Json

            var options = new JsonSerializerOptions // definir options se usar System.Text.Json.JsonSerializer
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Com opções para camelCase (igual Newtonsoft)
                WriteIndented = false // false para headers (otimizado)
            };
            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(metadata, options));

            return Ok(simulacoesPaginadas);
        }

        [Authorize]
        [HttpGet("filtro-paginado")] //GET /api/simulacao/filtro-paginado?valorMin=2000&valorMax=10000&prazoMin=12&prazoMax=24&pageNumber=1&pageSize=10

        public ActionResult<IEnumerable<SimulacaoModel>> ListarSimulacoesFiltradasComPaginacao([FromQuery] SimulacaoParameters simulacaoParams,
                                                            [FromQuery] decimal? valorMin = null, [FromQuery] decimal? valorMax = null,
                                                            [FromQuery] int? prazoMin = null, [FromQuery] int? prazoMax = null)
        {
            var simulacoesPaginadas = _simulacaoService.ListarSimulacoesFiltradasComPaginacao(simulacaoParams, valorMin, valorMax, prazoMin, prazoMax);

            var metadata = new
            {
                simulacoesPaginadas.TotalCount,
                simulacoesPaginadas.TotalPages,
                simulacoesPaginadas.PageSize,
                simulacoesPaginadas.CurrentPage,
                simulacoesPaginadas.HasNext,
                simulacoesPaginadas.HasPrevious
            };

            // Adiciona metadados no header
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };
            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(metadata, options));

            return Ok(simulacoesPaginadas);
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
