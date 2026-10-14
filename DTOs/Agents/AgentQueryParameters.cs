namespace RealEstateApi.DTOs.Agents;

public class AgentQueryParameters
{
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "asc";
}