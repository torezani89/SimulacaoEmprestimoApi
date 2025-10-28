using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SimulacaoEmprestimoApi.Data;
using SimulacaoEmprestimoApi.Models;
using SimulacaoEmprestimoApi.Pagination;

namespace SimulacaoEmprestimoApi.Services
{
    public class SimulacaoService : ISimulacaoService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SimulacaoService> _logger;
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;
        private static readonly DateTime _inicioAplicacao = DateTime.Now;

        public SimulacaoService(AppDbContext dbContext, IMemoryCache cache, IOptions<CacheSettings> cacheSettings, ILogger<SimulacaoService> logger)
        {
            _dbContext = dbContext;
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
        }

        // ===============================================
        // 🔍 Health check
        // ===============================================
        public async Task<HealthCheckResponse> VerificarSaudeAsync()
        {
            HealthCheckResponse? response = new HealthCheckResponse
            {
                StatusApi = "OK",
                DataVerificacao = DateTime.Now,
                // calcula há quanto tempo a API está online
                UptimeSegundos = (long)(DateTime.Now - _inicioAplicacao).TotalSeconds
            };

            try
            {
                // consulta leve ao banco (sem forçar cache)
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
                response.StatusBanco = "OK";
            }
            catch (Exception ex)
            {
                response.StatusBanco = "Erro";
                response.MensagemErroBanco = ex.Message;
                _logger.LogError(ex, "Falha na verificação de conexão com o banco de dados");
            }

            return response; //Retorna todas as informações no formato JSON.

        }

        // ===============================================
        // 🧮 Simulação principal
        // ===============================================
        public async Task<SimulacaoResponse> SimularAsync(SimulacaoRequest request)
        {
            _logger.LogInformation("Iniciando simulação: Valor={ValorDesejado}, Prazo={Prazo}", request.ValorDesejado, request.Prazo);

            var produtos = await _dbContext.Produtos.ToListAsync();

            var produto = produtos.FirstOrDefault(p =>
                request.Prazo >= p.NU_MINIMO_MESES &&
                (p.NU_MAXIMO_MESES == null || request.Prazo <= p.NU_MAXIMO_MESES) &&
                request.ValorDesejado >= p.VR_MINIMO &&
                (p.VR_MAXIMO == null || request.ValorDesejado <= p.VR_MAXIMO)
            );

            if (produto == null)
            {
                _logger.LogWarning("Nenhum produto encontrado para os parâmetros informados. Valor={ValorDesejado}, Prazo={Prazo}", request.ValorDesejado, request.Prazo);
                throw new Exception("Nenhum produto compatível com os parâmetros informados.");
            }

            _logger.LogInformation("Produto selecionado: {Produto} (Taxa: {Taxa:P3})", produto.NO_PRODUTO, produto.PC_TAXA_JUROS);

            decimal taxa = produto.PC_TAXA_JUROS;
            int prazo = request.Prazo;
            decimal valor = request.ValorDesejado;

            SimulacaoModel simulacao = new SimulacaoModel()
            {
                CodigoProduto = produto.CO_PRODUTO,
                ValorDesejado = valor,
                Prazo = prazo,
                TaxaJuros = taxa,
                DataSimulacao = DateTime.Now
            };

            _dbContext.Simulacoes.Add(simulacao);
            await _dbContext.SaveChangesAsync(); //agora o obj simulacao tem o ID gerado pelo banco. Não é necessário refazer uma consulta nem buscar o ID

            long idSimulacao = simulacao.IdSimulacao; // possível acessar props atualizadas sem refazer consuta ao banco (EF)
            
            List<ParcelaResponse> parcelasSAC = CalcularSAC(valor, taxa, prazo);

            List<ParcelaResponse> parcelasPrice = CalcularPrice(valor, taxa, prazo);

            var parcelasBanco = new List<ParcelaModel>();

            foreach (var p in parcelasSAC)
            {
                parcelasBanco.Add(new ParcelaModel
                {
                    IdSimulacao = idSimulacao,
                    TipoAmortizacao = "SAC",
                    Numero = p.Numero,
                    ValorAmortizacao = p.ValorAmortizacao,
                    ValorJuros = p.ValorJuros,
                    ValorPrestacao = p.ValorPrestacao
                });
            }

            foreach (var p in parcelasPrice)
            {
                parcelasBanco.Add(new ParcelaModel
                {
                    IdSimulacao = idSimulacao,
                    TipoAmortizacao = "PRICE",
                    Numero = p.Numero,
                    ValorAmortizacao = p.ValorAmortizacao,
                    ValorJuros = p.ValorJuros,
                    ValorPrestacao = p.ValorPrestacao
                });
            }

            _dbContext.Parcelas.AddRange(parcelasBanco);
            await _dbContext.SaveChangesAsync();

            SimulacaoResponse response = new SimulacaoResponse
            {
                IdSimulacao = idSimulacao,
                CodigoProduto = produto.CO_PRODUTO,
                ValorDesejado = valor,
                Prazo = prazo,
                DescricaoProduto = produto.NO_PRODUTO,
                TaxaJuros = taxa,
                ResultadoSimulacao = new List<TabelaParcelasResponse>
                {
                    new TabelaParcelasResponse
                    {
                        Tipo = "SAC",
                        Parcelas = parcelasSAC
                    },
                    new TabelaParcelasResponse
                    {
                        Tipo = "PRICE",
                        Parcelas = parcelasPrice
                    }
                }
            };

            var cacheDuration = TimeSpan.FromMinutes(_cacheSettings.DuracaoMinutos);
            _cache.Set(idSimulacao, response, cacheDuration);

            _logger.LogInformation("Simulação concluída com sucesso para o produto {Produto}.", produto.NO_PRODUTO);
            return response;
        }

        // ===============================================
        // 📋 Listagem
        // ===============================================
        public async Task<List<SimulacaoModel>> ListarSimulacoesPersistidasAsync()
        {
            List<SimulacaoModel> simulacoes = await _dbContext.Simulacoes.ToListAsync();
            return simulacoes ?? new List<SimulacaoModel>();
            //catch (Exception ex) //centralizado em ErrorHandlingMiddleware
            //{
            //    throw new Exception($"Erro ao acessar simulações no banco: {ex.Message}", ex);
            //}
        }

        // ===============================================
        // 📋 Listagem com Paginacao
        // ===============================================
        public PagedList<SimulacaoModel> ListarSimulacoesPersistidasAsyncComPaginacao(SimulacaoParameters simulacaoParams)
        {
            _logger.LogInformation("Listando simulações com paginação: Página {PageNumber}, Tamanho {PageSize}",
                simulacaoParams.PageNumber, simulacaoParams.PageSize);

            // Define valores padrão se não informados
            int pageNumber = simulacaoParams.PageNumber <= 0 ? 1 : simulacaoParams.PageNumber;
            int pageSize = simulacaoParams.PageSize <= 0 ? 10 : simulacaoParams.PageSize;

            // Aplica paginação diretamente no banco
            var simulacoes = _dbContext.Simulacoes
                .OrderByDescending(s => s.DataSimulacao).AsQueryable(); // mais recentes primeiro
                //.Skip((pageNumber - 1) * pageSize) // pular registros das páginas anteriores à página selecionada
                //.Take(pageSize) // pega a quantidade de páginas passadas em pageSize
                //.ToListAsync();
            var simulacoesOrdenadas = PagedList<SimulacaoModel>.ToPagedList(simulacoes, simulacaoParams.PageNumber, simulacaoParams.PageSize);

            int qtddSimulacoes = simulacoesOrdenadas.Count();

            if (simulacoesOrdenadas == null || qtddSimulacoes == 0)
            {
                _logger.LogWarning("Nenhuma simulação encontrada na página {PageNumber}.", pageNumber);
                // exception capturada e formatada pelo ErrorHandlingMiddleware.
                throw new KeyNotFoundException("Nenhuma simulação encontrada para os parâmetros informados.");
            }

            _logger.LogInformation("{Quantidade} simulações retornadas na página {PageNumber}.", qtddSimulacoes, pageNumber);

            return simulacoesOrdenadas;
        }

        // ===============================================
        // 📋 Listagem com Paginacao + Filtros
        // ===============================================
        public PagedList<SimulacaoModel> ListarSimulacoesFiltradasComPaginacao(
    SimulacaoParameters simulacaoParams, decimal? valorMin = null, decimal? valorMax = null, int? prazoMin = null, int? prazoMax = null)
        {
            _logger.LogInformation("Listando simulações filtradas e paginadas. Página {Page}, Tamanho {Size}, Filtros: ValorMin={ValorMin}, ValorMax={ValorMax}, PrazoMin={PrazoMin}, PrazoMax={PrazoMax}",
                simulacaoParams.PageNumber, simulacaoParams.PageSize, valorMin, valorMax, prazoMin, prazoMax);

            int pageNumber = simulacaoParams.PageNumber <= 0 ? 1 : simulacaoParams.PageNumber;
            int pageSize = simulacaoParams.PageSize <= 0 ? 10 : simulacaoParams.PageSize;

            // 🔹 Cria uma query base sobre Simulacoes => permite aplicar filtros e pagination no banco, sem carregar tudo antes em memória (IEnumerable)
            IQueryable<SimulacaoModel> query = _dbContext.Simulacoes.AsQueryable();

            // 🔹 Aplica filtros dinâmicos
            if (valorMin.HasValue)
                query = query.Where(s => s.ValorDesejado >= valorMin.Value);

            if (valorMax.HasValue)
                query = query.Where(s => s.ValorDesejado <= valorMax.Value);

            if (prazoMin.HasValue)
                query = query.Where(s => s.Prazo >= prazoMin.Value);

            if (prazoMax.HasValue)
                query = query.Where(s => s.Prazo <= prazoMax.Value);

            // 🔹 Ordena por data (mais recentes primeiro)
            query = query.OrderByDescending(s => s.DataSimulacao);

            // 🔹 Cria lista paginada
            var pagedResult = PagedList<SimulacaoModel>.ToPagedList(query, pageNumber, pageSize);

            if (pagedResult == null || pagedResult.Count == 0)
            {
                _logger.LogWarning("Nenhuma simulação encontrada para os filtros informados.");
                throw new KeyNotFoundException("Nenhuma simulação encontrada para os filtros informados.");
            }

            _logger.LogInformation("{Count} simulações encontradas (página {Page}).", pagedResult.Count, pageNumber);

            return pagedResult;
        }

        // ===============================================
        // 🔎 Obter Simulação por ID
        // ===============================================
        public async Task<SimulacaoResponse> ObterSimulacaoPorIdAsync(long idSimulacao)
        {
            //SimulacaoModel? simulacao = await _dbContext.Simulacoes.FindAsync((idSimulacao)); // FindAsync não aceita Include
            SimulacaoModel? simulacao = await _dbContext.Simulacoes
            .Include(s => s.Parcelas) // EF faz join com tabela Parcelas e preenche a prop de navegação 'Parcelas' de SimulacaoModel
            .FirstOrDefaultAsync(s => s.IdSimulacao == idSimulacao);

            if (simulacao == null) throw new KeyNotFoundException($"Simulação ID {idSimulacao} não encontrada.");

            var produto = await _dbContext.Produtos.FirstOrDefaultAsync(p => p.CO_PRODUTO == simulacao.CodigoProduto);

            // Agrupa as parcelas por tipo de amortização (SAC, PRICE)
            var resultadoSimulacao = simulacao.Parcelas
                .GroupBy(p => p.TipoAmortizacao) // "SAC", "Price"
                .Select(grupo => new TabelaParcelasResponse
                {
                    Tipo = grupo.Key,
                    Parcelas = grupo.Select(p => new ParcelaResponse
                    {
                        Numero = p.Numero,
                        ValorAmortizacao = p.ValorAmortizacao,
                        ValorJuros = p.ValorJuros,
                        ValorPrestacao = p.ValorPrestacao
                    }).ToList()
                }).ToList();

            SimulacaoResponse response = new SimulacaoResponse
            {
                IdSimulacao = simulacao.IdSimulacao,
                CodigoProduto = simulacao.CodigoProduto,
                ValorDesejado = simulacao.ValorDesejado,
                Prazo = simulacao.Prazo,
                TaxaJuros = simulacao.TaxaJuros,
                DescricaoProduto = produto?.NO_PRODUTO ?? string.Empty,
                ResultadoSimulacao = resultadoSimulacao
            };

            return response;
        }

        // ===============================================
        // 🔄 Atualização
        // ===============================================
        public async Task<SimulacaoResponse?> AtualizarSimulacaoAsync(long idSimulacao, SimulacaoRequest request)
        {
            SimulacaoModel? simulacao = await _dbContext.Simulacoes
                .Include(s => s.Parcelas)
                .FirstOrDefaultAsync(s => s.IdSimulacao == idSimulacao);

            if (simulacao == null) throw new KeyNotFoundException($"Simulação ID {idSimulacao} não encontrada.");

            Produto? produto = await _dbContext.Produtos
                .FirstOrDefaultAsync(p =>
                request.Prazo >= p.NU_MINIMO_MESES && (p.NU_MAXIMO_MESES == null || request.Prazo <= p.NU_MAXIMO_MESES) &&
                request.ValorDesejado >= p.VR_MINIMO && (p.VR_MAXIMO == null || request.ValorDesejado <= p.VR_MAXIMO));

            if (produto == null) throw new ArgumentException("Nenhum produto commpatível com os novos parâmetros");

            // Atualiza dados principais da simulação
            simulacao.CodigoProduto = produto.CO_PRODUTO;
            simulacao.ValorDesejado = request.ValorDesejado;
            simulacao.Prazo = request.Prazo;
            simulacao.TaxaJuros = produto.PC_TAXA_JUROS;
            simulacao.DataSimulacao = DateTime.Now;

            //_dbContext.Simulacoes.Update(simulacao); ////implícito: EF faz tracking automático de entidade carregada por _dbContext
            //await _dbContext.SaveChangesAsync(); // será salvo junto com a gravação das parcelas

            // Recalcula as parcelas
            List<ParcelaResponse> parcelasSAC = CalcularSAC(request.ValorDesejado, produto.PC_TAXA_JUROS, request.Prazo);
            List<ParcelaResponse> parcelasPrice = CalcularPrice(request.ValorDesejado, produto.PC_TAXA_JUROS, request.Prazo);

            // Remove parcelas antigas do banco
            if (simulacao.Parcelas != null && simulacao.Parcelas.Any()) _dbContext.Parcelas.RemoveRange(simulacao.Parcelas);

            // Cria novas parcelas
            List<ParcelaModel> novasParcelas = new List<ParcelaModel>();
            foreach (ParcelaResponse p in parcelasSAC)
            {
                novasParcelas.Add(new ParcelaModel
                {
                    IdSimulacao = idSimulacao,
                    TipoAmortizacao = "SAC",
                    Numero = p.Numero,
                    ValorAmortizacao = p.ValorAmortizacao,
                    ValorJuros = p.ValorJuros,
                    ValorPrestacao = p.ValorPrestacao
                });
            }
            foreach (ParcelaResponse p in parcelasPrice)
            {
                novasParcelas.Add(new ParcelaModel
                {
                    IdSimulacao = idSimulacao,
                    TipoAmortizacao = "Price",
                    Numero = p.Numero,
                    ValorAmortizacao = p.ValorAmortizacao,
                    ValorJuros = p.ValorJuros,
                    ValorPrestacao = p.ValorPrestacao
                });
            }
            _dbContext.Parcelas.AddRange(novasParcelas);
            await _dbContext.SaveChangesAsync();
            // salva obj simulacao atualizado => _dbContext.Simulacoes.Update(simulacao)
            // executa exclusão das parcelas antigas => _dbContext.Parcelas.RemoveRange(...)
            // exeecuta inclusão das novas parcelas => _dbContext.Parcelas.AddRange(...)

            _cache.Remove(idSimulacao); // remove do cache para atualizar com a nova versão

            SimulacaoResponse response = new SimulacaoResponse
            {
                IdSimulacao = simulacao.IdSimulacao,
                ValorDesejado = simulacao.ValorDesejado,
                Prazo = simulacao.Prazo,
                CodigoProduto = produto.CO_PRODUTO,
                DescricaoProduto = produto.NO_PRODUTO,
                TaxaJuros = simulacao.TaxaJuros,
                ResultadoSimulacao = new List<TabelaParcelasResponse>
                {
                    new TabelaParcelasResponse
                    {
                        Tipo = "SAC",
                        Parcelas = parcelasSAC
                    },
                    new TabelaParcelasResponse
                    {
                        Tipo = "Price",
                        Parcelas = parcelasPrice
                    }
                }
            };

            _cache.Set(idSimulacao, response, TimeSpan.FromMinutes(_cacheSettings.DuracaoMinutos));

            return response;

        }

        // ===============================================
        // 🗑️ Exclusão
        // ===============================================
        public async Task<bool> ExcluirSimulacaoAsync(long idSimulacao)
        {
            var simulacao = await _dbContext.Simulacoes
                .Include(s => s.Parcelas)
                .FirstOrDefaultAsync(s => s.IdSimulacao == idSimulacao);

            if (simulacao == null) throw new KeyNotFoundException($"Simulação ID {idSimulacao} não encontrada."); ;

            if (simulacao.Parcelas != null && simulacao.Parcelas.Any()) _dbContext.Parcelas.RemoveRange(simulacao.Parcelas);
            // simulacao.Parcelas = null
            // simulacao.Parcelas.Any() = Exception! → Por isso precisa da verificação != null

            _dbContext.Simulacoes.Remove(simulacao);
            await _dbContext.SaveChangesAsync();

            _cache.Remove(idSimulacao);

            return true;
        }

        // ===============================================
        // 🔎 Obter Simulação do Cache
        // ===============================================
        public SimulacaoResponse? ObterSimulacaoCachePorId(long idSimulacao)
        {
            _cache.TryGetValue(idSimulacao, out SimulacaoResponse? simulacao);
            return simulacao;
        }

        // ===============================================
        // 📊 Estatísticas
        // ===============================================
        public async Task<SimulacaoEstatisticasResponse> ObterEstatisticasAsync()
        {
            int totalSimulacoes = await _dbContext.Simulacoes.CountAsync();

            if(totalSimulacoes == 0)
            {
                return new SimulacaoEstatisticasResponse
                {
                    TotalSimulacoes = 0,
                    ValorMedio = 0,
                    PrazoMedio = 0,
                    TaxaMedia = 0,
                    UltimaAtualizacao = DateTime.Now
                };
            }

            decimal valorMedio = await _dbContext.Simulacoes.AverageAsync(s => s.ValorDesejado);
            int prazoMedio = (int)Math.Round(await _dbContext.Simulacoes.AverageAsync(s => s.Prazo));
            decimal taxaMedia = await _dbContext.Simulacoes.AverageAsync(s => s.TaxaJuros);

            // Estatísticas agrupadas por produto
            List<EstatisticaProdutoResponse> estatisticasProdutos = await (
                from s in _dbContext.Simulacoes
                join p in _dbContext.Produtos on s.CodigoProduto equals p.CO_PRODUTO // inner join
                group new { s, p } by new { s.CodigoProduto, p.NO_PRODUTO } into g // agrupa os resultados por produto
                select new EstatisticaProdutoResponse
                {
                    CodigoProduto = g.Key.CodigoProduto,
                    DescricaoProduto = g.Key.NO_PRODUTO,
                    QuantidadeSimulacoes = g.Count(),
                    ValorMedio = Math.Round(g.Average(x => x.s.ValorDesejado), 2),
                    PrazoMedio = (int)Math.Round(g.Average(x => x.s.Prazo)),
                    TaxaMedia = Math.Round(g.Average(x => x.s.TaxaJuros), 6)
                }
              ).OrderBy(e => e.CodigoProduto).ToListAsync();

            return new SimulacaoEstatisticasResponse
            {
                TotalSimulacoes = totalSimulacoes,
                ValorMedio = Math.Round(valorMedio, 2),
                PrazoMedio = prazoMedio,
                TaxaMedia = Math.Round(taxaMedia, 6),
                UltimaAtualizacao = DateTime.Now,
                EstatisticasPorProduto = estatisticasProdutos
            };
        }

        // ################################## MÉTODOS AUXILIARES #################################

        private List<ParcelaResponse> CalcularSAC(decimal valor, decimal taxa, int prazo)
        {
            var parcelas = new List<ParcelaResponse>();
            decimal amortizacao = valor / prazo;
            decimal saldoDevedor = valor;

            for (int i = 1; i <= prazo; i++)
            {
                decimal juros = saldoDevedor * taxa;
                decimal prestacao = amortizacao + juros;

                parcelas.Add(new ParcelaResponse
                {
                    Numero = i,
                    ValorAmortizacao = Math.Round(amortizacao, 2),
                    ValorJuros = Math.Round(juros, 2),
                    ValorPrestacao = Math.Round(prestacao, 2)
                });

                saldoDevedor -= amortizacao;
            }

            return parcelas;
        }

        private List<ParcelaResponse> CalcularPrice(decimal valor, decimal taxa, int prazo)
        {
            var parcelas = new List<ParcelaResponse>();

            decimal prestacao = valor * (taxa * (decimal)Math.Pow((double)(1 + taxa), prazo)) /
                                ((decimal)Math.Pow((double)(1 + taxa), prazo) - 1);

            decimal saldoDevedor = valor;

            for (int i = 1; i <= prazo; i++)
            {
                decimal juros = saldoDevedor * taxa;
                decimal amortizacao = prestacao - juros;
                saldoDevedor -= amortizacao;

                parcelas.Add(new ParcelaResponse
                {
                    Numero = i,
                    ValorAmortizacao = Math.Round(amortizacao, 2),
                    ValorJuros = Math.Round(juros, 2),
                    ValorPrestacao = Math.Round(prestacao, 2)
                });
            }

            return parcelas;
        }
    }
}
