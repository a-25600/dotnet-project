using UsersService.Data;
using UsersService.Models;

namespace UsersService.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRepository<Client> Clients { get; private set; }
    public IRepository<Agent> Agents { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Clients = new Repository<Client>(_context);
        Agents = new Repository<Agent>(_context);
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}