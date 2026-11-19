using DealsService.Data;
using DealsService.Models;

namespace DealsService.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRepository<Deal> Deals { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
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