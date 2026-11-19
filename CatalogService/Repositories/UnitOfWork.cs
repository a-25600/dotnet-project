using CatalogService.Data;
using CatalogService.Models;

namespace CatalogService.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRepository<Property> Properties { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Properties = new Repository<Property>(_context);
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