using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public enum TicketStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public class Ticket
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public required string Title { get; set; }

        [Required]
        [Column("description")]
        public required string Description { get; set; }

        [Required]
        [Column("status")]
        public TicketStatus Status { get; set; }

        [Column("client_id")]
        public int? ClientId { get; set; }

        [Column("contract_id")]
        public int? ContractId { get; set; }

        [Column("withdrawal_id")]
        public int? WithdrawalId { get; set; }

        [Required]
        [Column("created_by_user_id")]
        public int CreatedByUserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}