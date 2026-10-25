using RealEstateApi.Models;

namespace RealEstateApi.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Property> Properties { get; }
    IRepository<Client> Clients { get; }
    IRepository<Agent> Agents { get; }
    IRepository<Deal> Deals { get; }

    Task<int> SaveAsync();
}