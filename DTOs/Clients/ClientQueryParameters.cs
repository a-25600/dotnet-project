namespace RealEstateApi.DTOs.Clients;

public class ClientQueryParameters
{
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "asc";
}
