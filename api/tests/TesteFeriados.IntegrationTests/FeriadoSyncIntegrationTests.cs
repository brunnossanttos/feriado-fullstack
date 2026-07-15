using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TesteFeriados.Api.Contracts;
using TesteFeriados.Domain.Abstractions;

namespace TesteFeriados.IntegrationTests;

[Collection("postgres")]
public sealed class FeriadoSyncIntegrationTests
{
    private readonly PostgresApiFactory _factory;

    public FeriadoSyncIntegrationTests(PostgresApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Sync_ContraApiPublicaReal_GravaNoPostgres()
    {
        using var scope = _factory.Services.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<IFeriadoSyncService>();

        var result = await sync.SynchronizeAsync();

        Assert.True(result.TotalReceived > 0, "A API pública deve retornar feriados.");
        Assert.Equal(result.TotalReceived, result.Inserted + result.Updated + result.Unchanged);

        var client = _factory.CreateClient();

        var natal = await (await client.GetAsync("/api/feriados?title=Natal"))
            .Content.ReadFromJsonAsync<PagedResult<FeriadoResponse>>();
        Assert.True(natal!.TotalCount >= 1);

        var movel = await (await client.GetAsync("/api/feriados?title=Paix"))
            .Content.ReadFromJsonAsync<PagedResult<FeriadoResponse>>();
        Assert.True(movel!.TotalCount >= 1);
        Assert.NotEmpty(movel.Items[0].VariableDates);
    }

    [Fact]
    public async Task Sync_ExecutadoDuasVezes_EhIdempotente()
    {
        using var scope = _factory.Services.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<IFeriadoSyncService>();

        await sync.SynchronizeAsync();
        var segundo = await sync.SynchronizeAsync();

        Assert.Equal(0, segundo.Inserted);
        Assert.Equal(segundo.TotalReceived, segundo.Unchanged + segundo.Updated);
    }
}
