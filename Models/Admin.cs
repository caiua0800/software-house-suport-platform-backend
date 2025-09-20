// Admin.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Admin
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public required string Name { get; set; }

    [Column("name_normalized")]
    public string? NameNormalized { get; set; }

    [Required]
    [Column("email")]
    public required string Email { get; set; }

    [Required]
    [Column("password")]
    public required string Password { get; set; }

    [Column("phone_number")]
    public required string PhoneNumber { get; set; }

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }
}
