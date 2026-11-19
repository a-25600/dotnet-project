using CatalogService.Models;

namespace CatalogService.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Property> Properties { get; }
    Task<int> SaveAsync();
}