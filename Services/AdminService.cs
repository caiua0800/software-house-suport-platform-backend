using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public interface IAdminService
    {
        Task<Admin> CreateAdminAsync(Admin admin);
        Task<string?> AuthenticateAsync(string email, string password);
        Task<Admin?> GetAdminByIdAsync(int id);
    }

    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEncryptionService _encryptionService; // ✨ INJETADO

        public AdminService(ApplicationDbContext context, IConfiguration configuration, IEncryptionService encryptionService)
        {
            _context = context;
            _configuration = configuration;
            _encryptionService = encryptionService; // ✨ ARMAZENADO
        }

        public async Task<Admin> CreateAdminAsync(Admin admin)
        {
            if (await _context.Admins.AnyAsync(a => a.Email == admin.Email))
            {
                throw new ArgumentException("Um admin com este email já existe.");
            }

            // ✨ CRIPTOGRAFA a senha antes de salvar
            admin.Password = _encryptionService.Encrypt(admin.Password);
            admin.DateCreated = DateTime.UtcNow;
            admin.NameNormalized = admin.Name.ToUpperInvariant();

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            var admin = await _context.Admins.SingleOrDefaultAsync(a => a.Email == email);

            if (admin == null) return null;

            // ✨ DESCRIPTOGRAFA a senha do banco para comparar
            var decryptedPassword = _encryptionService.Decrypt(admin.Password);

            if (decryptedPassword != password)
            {
                return null; // Senha incorreta
            }

            return GenerateJwtToken(admin);
        }

        public async Task<Admin?> GetAdminByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id);
        }

        private string GenerateJwtToken(Admin admin)
        {
            var secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Chave secreta JWT não configurada.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, admin.Email),
                new Claim(ClaimTypes.Role, "Admin") // ✨ Role adicionada ao token
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