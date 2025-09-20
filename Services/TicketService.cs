using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    // --- Interface ---
    // A interface é o "contrato" que a classe de serviço deve seguir.
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket?> UpdateTicketStatusAsync(int id, TicketStatus newStatus);

        // ✨ 1. AQUI ESTÁ A DEFINIÇÃO QUE FALTAVA NA INTERFACE ✨
        Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(int userId);
    }

    // --- Implementação ---
    // A classe que contém a lógica de negócio real.
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;

        public TicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets.FindAsync(id);
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            ticket.Status = TicketStatus.Pending;
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket?> UpdateTicketStatusAsync(int id, TicketStatus newStatus)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return null;
            }

            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ticket;
        }

        // ✨ 2. AQUI ESTÁ A IMPLEMENTAÇÃO DO NOVO MÉTODO ✨
        public async Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(int userId)
        {
            // Este comando LINQ vai ao banco de dados e seleciona
            // apenas os tickets onde a coluna 'CreatedByUserId'
            // corresponde ao ID do usuário que foi passado como parâmetro.
            return await _context.Tickets
                .Where(t => t.CreatedByUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}