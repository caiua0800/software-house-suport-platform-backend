using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public record CreateAdminDto(string Name, string Email, string Password, string PhoneNumber);
    public record LoginDto(string Email, string Password);
    public record LoginResponseDto(string Token);

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateAdminDto createAdminDto)
        {
            try
            {
                var newAdmin = new Admin
                {
                    Name = createAdminDto.Name,
                    Email = createAdminDto.Email,
                    Password = createAdminDto.Password,
                    PhoneNumber = createAdminDto.PhoneNumber
                };
                var createdAdmin = await _adminService.CreateAdminAsync(newAdmin);
                return CreatedAtAction(nameof(GetAdminById), new { id = createdAdmin.Id }, new { createdAdmin.Id, createdAdmin.Name, createdAdmin.Email });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _adminService.AuthenticateAsync(loginDto.Email, loginDto.Password);
            if (token == null)
            {
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }
            return Ok(new LoginResponseDto(token));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminById(int id)
        {
            var admin = await _adminService.GetAdminByIdAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            // Retorna o admin sem a senha
            return Ok(new { admin.Id, admin.Name, admin.Email, admin.PhoneNumber });
        }
    }
}