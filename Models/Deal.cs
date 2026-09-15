namespace RealEstateApi.Models;

public class Deal
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int ClientId { get; set; }
    public int RealtorId { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    // ^ з можливих статусів - Pending, Completed, Cancelled.
}
