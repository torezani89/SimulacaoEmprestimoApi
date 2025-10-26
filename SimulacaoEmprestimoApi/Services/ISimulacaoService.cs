using Microsoft.AspNetCore.Mvc;
using SimulacaoEmprestimoApi.Models;
using SimulacaoEmprestimoApi.Pagination;

namespace SimulacaoEmprestimoApi.Services
{
    public interface ISimulacaoService
    {
        Task<SimulacaoResponse> SimularAsync(SimulacaoRequest request);
        SimulacaoResponse? ObterSimulacaoCachePorId(long idSimulacao);
        Task<List<SimulacaoModel>> ListarSimulacoesPersistidasAsync();
        Task<List<SimulacaoModel>> ListarSimulacoesPersistidasAsyncComPaginacao(SimulacaoParameters simulacaoParams);
        Task<SimulacaoResponse> ObterSimulacaoPorIdAsync(long idSimulacao);
        Task<SimulacaoResponse?> AtualizarSimulacaoAsync(long idSimulacao, SimulacaoRequest request);
        Task<bool> ExcluirSimulacaoAsync(long idSimulacao);
        Task<SimulacaoEstatisticasResponse> ObterEstatisticasAsync();
        Task<HealthCheckResponse> VerificarSaudeAsync();

    }
}
