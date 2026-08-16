using BikeStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BikeStore.Infrastructure.Data;

public sealed class BikeStoreDbContext(
    DbContextOptions<BikeStoreDbContext> options)
    : DbContext(options)
{
    public DbSet<Bicycle> Bicycles => Set<Bicycle>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(BikeStoreDbContext).Assembly);
    }
}