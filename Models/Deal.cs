namespace RealEstateApi.Models;

public class Deal
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public int RealtorId { get; set; }
    public Agent? Realtor { get; set; }

    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    // ^ з можливих статусів - Pending, Completed, Cancelled.
}
