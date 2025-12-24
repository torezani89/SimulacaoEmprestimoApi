using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using SimulacaoEmprestimoApi.Data;
using SimulacaoEmprestimoApi.Middlewares;
using SimulacaoEmprestimoApi.Models;
using SimulacaoEmprestimoApi.Services;
using System.Text;

// Configuracao do Serilog
Log.Logger = new LoggerConfiguration() //Log.Logger é a instância estática global do Serilog
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Logs do namespace "Microsoft" só aparecem se forem Warning ou superior
    .Enrich.FromLogContext() //Permite adicionar propriedades contextuais aos logs dinamicamente
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day, //Novo arquivo a cada dia
        retainedFileCountLimit: 7, // mantém apenas os 7 arquivos de logs mais recentes
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

//Usa o Serilog como provider de logs. Conecta Serilog ao ILogger
builder.Host.UseSerilog(); //usa interface ILogger<T> padrão, mas com engine Serilog por trás

// ########################## Add services to the container ##########################

builder.Services.AddDbContext<AppDbContext>(options => // Configuração de conexão com o SQL Server
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

builder.Services.Configure<CacheSettings>( // Registrar as configurações de cache
    builder.Configuration.GetSection("CacheSettings"));

builder.Services.AddScoped<ISimulacaoService, SimulacaoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ISenhaservice, SenhaService>();

builder.Services.AddControllers();

// -------------------- JWT BEARER -------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // desative apenas em dev
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
// -------------------------------------------------

builder.Services.AddEndpointsApiExplorer();

// -------------------- Swagger + JWT -------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Simulação de Empréstimo API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Insira o token JWT gerado pelo login. Exemplo: Bearer {seu_token}",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    // Mostrar cadeado em todas as rotas
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// ####################################################################################

var app = builder.Build();

// Teste de conexão e mapeamento
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Tenta acessar a tabela
    var usuarios = db.Usuarios.ToList();
    Console.WriteLine($"Tabela USUARIOS encontrada: {usuarios.Count} registros");
}

if (app.Environment.IsDevelopment()) // Configure the HTTP request pipeline.
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<RequestResponseLoggingMiddleware>(); // Usa o middleware de logs de requisição/resposta
app.UseMiddleware<ErrorHandlingMiddleware>(); // middleware de tratamento de erros

app.UseAuthentication(); // adicionar para usar Autenticacao JWT

app.UseAuthorization();

app.MapControllers();

app.Run();
