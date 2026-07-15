using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TesteFeriados.Domain.Abstractions;
using TesteFeriados.Infrastructure.ExternalApi;
using TesteFeriados.Infrastructure.Persistence;
using TesteFeriados.Infrastructure.Sync;

namespace TesteFeriados.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string feriadosApiBaseUrl)
    {
        services.AddDbContext<FeriadosDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IFeriadoRepository, FeriadoRepository>();

        services.AddHttpClient<IFeriadosApiClient, FeriadosApiClient>(client =>
        {
            client.BaseAddress = new Uri(feriadosApiBaseUrl);
        });

        services.AddScoped<IFeriadoSyncService, FeriadoSyncService>();

        return services;
    }
}
