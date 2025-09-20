using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt; // ✨ 1. USING IMPORTANTE ✨

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

// Configuração de CORS (já inclui seu domínio de produção)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://suporte.softwarehousecaiuademello.com.br"
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
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Serviços para API Explorer e Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// Serviços essenciais da aplicação
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// REGISTRO DOS SERVIÇOS DA APLICAÇÃO
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IClientService, ClientService>();

// Configuração do DbContext com PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// ✨✨✨ A CORREÇÃO MÁGICA ESTÁ AQUI ✨✨✨
// Limpa o mapeamento de claims padrão do .NET.
// Isso impede que o middleware de autenticação renomeie claims como "sub" para nomes longos da Microsoft,
// garantindo que `User.FindFirstValue(JwtRegisteredClaimNames.Sub)` funcione corretamente.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


// Configuração de Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new ArgumentException("Jwt:SecretKey não está configurado.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            // Define explicitamente qual claim representa a Role. Crucial para [Authorize(Roles = "...")]
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
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

app.UseRouting();
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication(); // 1º: Identifica quem é o usuário (valida o token)
app.UseAuthorization();  // 2º: Verifica se o usuário identificado tem permissão para acessar o recurso

app.MapControllers();

app.Run();