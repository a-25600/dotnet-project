using DealsService.Models;

namespace DealsService.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Deal> Deals { get; }

    Task<int> SaveAsync();
}