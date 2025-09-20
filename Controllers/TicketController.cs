using System.Security.Claims; // Garanta que este using está aqui
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    // DTOs não mudam
    public record CreateTicketDto(string Title, string Description, int? ClientId, int? ContractId, int? WithdrawalId);
    public record UpdateStatusDto(TicketStatus Status);

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Support")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // ✨ AQUI ESTÁ A CORREÇÃO FINAL, SEM DESCULPAS ✨
            // Vamos usar ClaimTypes.NameIdentifier. É a forma que o ASP.NET Core
            // prioriza para identificar o usuário.
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                // Se isso falhar agora, o problema é fundamental na sua configuração de autenticação.
                return Unauthorized("Token inválido ou não contém um ID de usuário válido.");
            }

            IEnumerable<Ticket> tickets;

            if (userRole == "Admin")
            {
                tickets = await _ticketService.GetAllTicketsAsync();
            }
            else
            {
                tickets = await _ticketService.GetTicketsByUserIdAsync(userId);
            }

            return Ok(tickets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); // ✨ MESMA CORREÇÃO AQUI
            int.TryParse(userIdClaim, out int userId);

            if (userRole != "Admin" && ticket.CreatedByUserId != userId)
            {
                return Forbid();
            }

            return Ok(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto createTicketDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); // ✨ E AQUI TAMBÉM
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("ID do usuário não encontrado no token.");
            }

            var ticket = new Ticket
            {
                Title = createTicketDto.Title,
                Description = createTicketDto.Description,
                ClientId = createTicketDto.ClientId,
                ContractId = createTicketDto.ContractId,
                WithdrawalId = createTicketDto.WithdrawalId,
                CreatedByUserId = userId,
            };

            var createdTicket = await _ticketService.CreateTicketAsync(ticket);
            return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, createdTicket);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateStatusDto updateStatusDto)
        {
            var updatedTicket = await _ticketService.UpdateTicketStatusAsync(id, updateStatusDto.Status);
            if (updatedTicket == null)
            {
                return NotFound();
            }
            return Ok(updatedTicket);
        }
    }
}