using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    // DTOs para o Ticket
    public record CreateTicketDto(string Title, string Description, int? ClientId, int? ContractId, int? WithdrawalId, int CreatedByUserId);
    public record UpdateStatusDto(TicketStatus Status);

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Support")] // Protege todas as rotas de ticket para serem acessadas apenas por admins
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
            var tickets = await _ticketService.GetAllTicketsAsync();
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
            return Ok(ticket);
        }

        [HttpPost]
        // Futuramente, esta rota poderia ser [AllowAnonymous] ou ter uma role "Support"
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto createTicketDto)
        {
            var ticket = new Ticket
            {
                Title = createTicketDto.Title,
                Description = createTicketDto.Description,
                ClientId = createTicketDto.ClientId,
                ContractId = createTicketDto.ContractId,
                WithdrawalId = createTicketDto.WithdrawalId,
                CreatedByUserId = createTicketDto.CreatedByUserId,
            };

            var createdTicket = await _ticketService.CreateTicketAsync(ticket);
            return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, createdTicket);
        }

        [HttpPut("{id}/status")]
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