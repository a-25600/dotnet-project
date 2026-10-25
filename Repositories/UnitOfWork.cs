using RealEstateApi.Data;
using RealEstateApi.Models;

namespace RealEstateApi.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRepository<Property> Properties { get; private set; }
    public IRepository<Client> Clients { get; private set; }
    public IRepository<Agent> Agents { get; private set; }
    public IRepository<Deal> Deals { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Properties = new Repository<Property>(_context);
        Clients = new Repository<Client>(_context);
        Agents = new Repository<Agent>(_context);
        Deals = new Repository<Deal>(_context);
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