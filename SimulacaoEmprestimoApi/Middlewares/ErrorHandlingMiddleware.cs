//O middleware intercepta a execução do pipeline HTTP, captura exceções não tratadas lançadas por middlewares/handlers
//subsequentes, registra o erro no ILogger e retorna uma resposta JSON padronizada ao cliente com um código HTTP apropriado
//(ex.: 400, 401, 404, 500). Isso evita que exceções "vazem" sem tratamento e centraliza a resposta de erro.

using SimulacaoEmprestimoApi.Models;
using System.Net;
using System.Text.Json;

namespace SimulacaoEmprestimoApi.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next; //chama próx middleware/endpoint do pipeline
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) //método obrigatório chamado para cada request
        {
            try
            {
                //O try envolve a chamada a _next(context) para capturar qualquer exceção ocorrida depois deste middleware.
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        //Mesmo após a exceção tratada, o middleware evita que o servidor reinicie; ele apenas forma uma resposta apropriada.

        private async Task HandleExceptionAsync(HttpContext context, Exception ex) // acionado caso ocorra exceção
        {
            _logger.LogError(ex, "Erro não tratado: {Mensagem}", ex.Message);

            context.Response.ContentType = "application/json";//define que a resposta será JSON

            var statusCode = ex switch // Mapeamento de status HTTP
            {
                ArgumentException => (int)HttpStatusCode.BadRequest, //400 (entrada inválida)
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, //401 (acesso não autorizado)
                KeyNotFoundException => (int)HttpStatusCode.NotFound, //404 (não encontrado)
                _ => (int)HttpStatusCode.InternalServerError //500 (erro interno), se não for nenhum dos anteriores
            };

            context.Response.StatusCode = statusCode;

            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            // esconde informações sensíveis em ambiente de produção
            var detalhes = env.IsDevelopment() ? ex.InnerException?.Message : "Erro interno no servidor";

            var response = new ErrorResponse
            {
                Sucesso = false,
                StatusCode = statusCode,
                Mensagem = ex.Message,
                Detalhes = detalhes,
                Caminho = context.Request.Path //rota onde o erro ocorreu (útil para debugging)
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            //escreve a resposta json no corpo da requisicao http
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
