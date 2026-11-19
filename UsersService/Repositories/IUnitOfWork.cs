using UsersService.Models;

namespace UsersService.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Client> Clients { get; }
    IRepository<Agent> Agents { get; }

    Task<int> SaveAsync();
}