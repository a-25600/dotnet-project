using RealEstateApi.Models;

namespace RealEstateApi.Data;

public static class MockDatabase
{
    public static List<Deal> Deals = new()
    {
        new Deal { Id = 1, PropertyId = 101, ClientId = 1, RealtorId = 1, Price = 5000, Date = DateTime.UtcNow, Status = "Completed" },
        new Deal {Id = 2, PropertyId = 102, ClientId = 2, RealtorId = 2, Price = 1000, Date = DateTime.UtcNow, Status = "Pending"}
    };
}