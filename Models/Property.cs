namespace RealEstateApi.Models;

public class Property
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Type { get; set; } = string.Empty;
    public int RoomsCount { get; set; }
    public bool IsAvailable { get; set; } = true;
}