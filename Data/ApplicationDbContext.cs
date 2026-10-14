using Microsoft.EntityFrameworkCore;
using RealEstateApi.Models;

namespace RealEstateApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<Deal> Deals { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<Client> Clients { get; set; }

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

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.FullName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.Email)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.PhoneNumber)
                  .IsRequired()
                  .HasMaxLength(30);
        });

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.FullName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(a => a.Email)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(a => a.PhoneNumber)
                  .IsRequired()
                  .HasMaxLength(30);

            entity.Property(a => a.LicenseNumber)
                  .IsRequired()
                  .HasMaxLength(50);
        });
    }
}