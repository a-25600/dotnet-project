using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApi.Models;

public class Deal
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Property))]
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    [Required]
    [ForeignKey(nameof(Client))]
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    [Required]
    [ForeignKey(nameof(Realtor))]
    public int RealtorId { get; set; }
    public Agent? Realtor { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;
    // ^ з можливих статусів - Pending, Completed, Cancelled.
}
