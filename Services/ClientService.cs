using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public interface IClientService
    {
        Task<Client> CreateClientAsync(Client client);
        Task<string?> AuthenticateAsync(string email, string password);
    }

    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEncryptionService _encryptionService;

        public ClientService(ApplicationDbContext context, IConfiguration configuration, IEncryptionService encryptionService)
        {
            _context = context;
            _configuration = configuration;
            _encryptionService = encryptionService;
        }

        // ... (CreateClientAsync não muda)
        public async Task<Client> CreateClientAsync(Client client)
        {
            if (await _context.Clients.AnyAsync(c => c.Email == client.Email))
            {
                throw new ArgumentException("Um cliente com este email já existe.");
            }
            client.Password = _encryptionService.Encrypt(client.Password);
            client.DateCreated = DateTime.UtcNow;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            var client = await _context.Clients.SingleOrDefaultAsync(c => c.Email == email);
            if (client == null) return null;
            var decryptedPassword = _encryptionService.Decrypt(client.Password);
            if (decryptedPassword != password) return null;
            return GenerateJwtToken(client);
        }

        private string GenerateJwtToken(Client client)
        {
            var secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Chave secreta JWT não configurada.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString()),           // Padrão JWT
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),         // Padrão Microsoft/ASP.NET Core
                new Claim(JwtRegisteredClaimNames.Email, client.Email),
                new Claim(ClaimTypes.Role, "Support")
            };


            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}