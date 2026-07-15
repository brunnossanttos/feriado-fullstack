using Microsoft.EntityFrameworkCore;
using TesteFeriados.Domain.Entities;

namespace TesteFeriados.Infrastructure.Persistence;

public class FeriadosDbContext : DbContext
{
    public FeriadosDbContext(DbContextOptions<FeriadosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Feriado> Feriados => Set<Feriado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeriadosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
