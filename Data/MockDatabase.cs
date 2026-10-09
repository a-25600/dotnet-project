using RealEstateApi.Models;

namespace RealEstateApi.Data;

public static class MockDatabase
{
    public static List<Deal> Deals = new()
    {
        new Deal { Id = 1, PropertyId = 101, ClientId = 1, RealtorId = 1, Price = 5000, Date = DateTime.UtcNow, Status = "Completed" },
        new Deal {Id = 2, PropertyId = 102, ClientId = 2, RealtorId = 2, Price = 1000, Date = DateTime.UtcNow, Status = "Pending"}
    };
    
    public static List<Agent> Agents = new()
    {
        new Agent { Id = 1, FullName = "Олена Ковальчук", Email = "kovalchuk@realty.ua", PhoneNumber = "+380501112233", LicenseNumber = "AG-001", IsActive = true },
        new Agent { Id = 2, FullName = "Андрій Петренко", Email = "petrenko@realty.ua", PhoneNumber = "+380502223344", LicenseNumber = "AG-002", IsActive = true }
    };

    public static List<Client> Clients = new()
    {
        new Client { Id = 1, FullName = "Ірина Мельник", Email = "melnyk@example.com", PhoneNumber = "+380671234567" },
        new Client { Id = 2, FullName = "Максим Бондар", Email = "bondar@example.com", PhoneNumber = "+380931234567" }
    };
}