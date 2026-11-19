using CatalogService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CatalogService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Properties");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Address)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(p => p.Price)
                  .HasColumnType("decimal(18,2)");
        });
    }
}