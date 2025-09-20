// backend/Program.cs

using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Services; // ✨ Adicionado para acessar os serviços
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurações de servidor Kestrel e limites de formulário
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
    options.ValueLengthLimit = 100 * 1024 * 1024;
});

// Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Adiciona serviços de controllers e configuração de JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignora propriedades nulas na serialização
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // Converte Enums (como TicketStatus) para strings no JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Serviços para API Explorer e Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define o esquema de segurança Bearer (JWT) para o Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header usando o esquema Bearer. \r\n\r\n Digite 'Bearer' [espaço] e então seu token.\r\n\r\nExemplo: \"Bearer 12345abcdef\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Serviços essenciais da aplicação
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// ✨✨✨ REGISTRO DOS SERVIÇOS DA APLICAÇÃO ✨✨✨
// Registra o serviço de criptografia como Singleton (uma única instância para toda a aplicação)
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
// Registra os serviços de negócio como Scoped (uma nova instância para cada requisição HTTP)
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IClientService, ClientService>(); 
// Configuração do DbContext com PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração de Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new ArgumentException("Jwt:SecretKey não está configurado.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Em produção, considere validar (true)
            ValidateAudience = false, // Em produção, considere validar (true)
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Construção da aplicação
var app = builder.Build();

// Configuração do pipeline de middleware HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ordem correta dos middlewares
app.UseRouting();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication(); // Verifica se há um token JWT e valida-o
app.UseAuthorization();  // Verifica se o usuário autenticado tem as permissões (roles) necessárias

app.MapControllers();

app.Run();