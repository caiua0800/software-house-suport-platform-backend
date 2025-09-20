// backend/Controllers/ClientController.cs
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public record CreateClientDto(string Name, string Email, string Password, string PhoneNumber);
    // Usaremos o mesmo LoginDto e LoginResponseDto do AdminController

    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateClientDto createClientDto)
        {
            try
            {
                var newClient = new Client
                {
                    Name = createClientDto.Name,
                    Email = createClientDto.Email,
                    Password = createClientDto.Password,
                    PhoneNumber = createClientDto.PhoneNumber
                };
                var createdClient = await _clientService.CreateClientAsync(newClient);
                return Ok(new { createdClient.Id, createdClient.Name, createdClient.Email });
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
            var token = await _clientService.AuthenticateAsync(loginDto.Email, loginDto.Password);
            if (token == null)
            {
                return Unauthorized(new { message = "Email ou senha de cliente inválidos." });
            }
            return Ok(new LoginResponseDto(token));
        }
    }
}