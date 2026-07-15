using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TesteFeriados.Infrastructure.Persistence;

public class FeriadosDbContextFactory : IDesignTimeDbContextFactory<FeriadosDbContext>
{
    public FeriadosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FeriadosDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=feriados;Username=postgres;Password=postgres")
            .Options;

        return new FeriadosDbContext(options);
    }
}
