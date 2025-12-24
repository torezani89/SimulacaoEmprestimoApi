using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SimulacaoEmprestimoApi.Middlewares
{
    //Atua como um gravador invisível no pipeline do ASP.NET Core
    // 1 - Captura o que o cliente envia (Request)
    // 2 - Deixa o pipeline rodar(Controllers, Services, etc)
    // 3 - Captura o que o servidor responde(Response)
    // 4 - Loga tudo com método, caminho, corpo e status
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next; // representa o próximo middleware na cadeia de execução
            _logger = logger;
        }

        //O ASP.NET Core chama o método Invoke() automaticamente para cada requisição HTTP
        public async Task Invoke(HttpContext context)
        {
            // ################ Log da Requisição ################

            // Request.Body é um Stream que só pode ser lido uma vez
            // EnableBuffering() faz uma cópia temporária do Request.Body na memória
            context.Request.EnableBuffering();
            //ReadToEndAsync(): Lê e registra o JSON enviado pelo cliente
            var requestBody = await new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
            context.Request.Body.Position = 0; // reseta o stream para o próximo middleware

            _logger.LogInformation("➡️ [REQUEST] {Method} {Path} | Body: {Body}",
                context.Request.Method,
                context.Request.Path,
                string.IsNullOrWhiteSpace(requestBody) ? "(vazio)" : requestBody);

            // ################ Caputura e Log a Resposta ################

            // Response.Body é um stream não legível diretamente, então o middleware precisa substituí-lo temporariamente.
            var originalBodyStream = context.Response.Body; // armazena o Response.Body original
            using var responseBody = new MemoryStream();
            // substituído por um MemoryStream para interceptar tudo o que seria escrito nele pelo controller
            context.Response.Body = responseBody;

            // executa o restante do pipeline (chama o controller e middlewares seguintes)
            await _next(context);

            // ponteiro do MemoryStream (context.Response.Body) é rebobinado
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            //lê o conteúdo JSON da resposta e converte em string
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation("⬅️ [RESPONSE] {StatusCode} {Path} | Body: {Body}",
                context.Response.StatusCode,
                context.Request.Path,
                string.IsNullOrWhiteSpace(responseText) ? "(vazio)" : responseText); // conteúdo da resposta é logado

            //copia o conteúdo do MemoryStream interceptado (responseBody) de volta para o stream original (originalBodyStream)
            //é o que o ASP.NET envia ao cliente
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

}
