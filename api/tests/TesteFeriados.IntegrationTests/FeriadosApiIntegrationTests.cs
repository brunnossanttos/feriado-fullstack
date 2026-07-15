using System.Net;
using System.Net.Http.Json;
using TesteFeriados.Api.Contracts;

namespace TesteFeriados.IntegrationTests;

[Collection("postgres")]
public sealed class FeriadosApiIntegrationTests
{
    private readonly PostgresApiFactory _factory;

    public FeriadosApiIntegrationTests(PostgresApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Crud_Completo_PorHttp()
    {
        var client = _factory.CreateClient();
        var titulo = $"CRUD {Guid.NewGuid()}";

        var createResp = await client.PostAsJsonAsync(
            "/api/feriados",
            new CreateFeriadoRequest { Title = titulo, Date = "09/07", Description = "criado" });
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        Assert.NotNull(createResp.Headers.Location);
        var created = await createResp.Content.ReadFromJsonAsync<FeriadoResponse>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal(titulo, created.Title);

        var getResp = await client.GetAsync($"/api/feriados/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        var fetched = await getResp.Content.ReadFromJsonAsync<FeriadoResponse>();
        Assert.Equal("09/07", fetched!.Date);
        Assert.Equal("criado", fetched.Description);

        var putResp = await client.PutAsJsonAsync(
            $"/api/feriados/{created.Id}",
            new UpdateFeriadoRequest { Title = titulo, Date = "10/07" });
        Assert.Equal(HttpStatusCode.OK, putResp.StatusCode);
        var updated = await putResp.Content.ReadFromJsonAsync<FeriadoResponse>();
        Assert.Equal("10/07", updated!.Date);
        Assert.Null(updated.Description);
        Assert.Equal(created.Id, updated.Id);

        var delResp = await client.DeleteAsync($"/api/feriados/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);

        var getAfter = await client.GetAsync($"/api/feriados/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }

    [Fact]
    public async Task Post_TituloDuplicado_Retorna409()
    {
        var client = _factory.CreateClient();
        var req = new CreateFeriadoRequest { Title = $"DUP {Guid.NewGuid()}" };

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/feriados", req)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/feriados", req)).StatusCode);
    }

    [Fact]
    public async Task Post_SemTitulo_Retorna400()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/feriados", new CreateFeriadoRequest { Title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Jsonb_VariableDates_FazRoundTripNoBanco()
    {
        var client = _factory.CreateClient();
        var titulo = $"JSONB {Guid.NewGuid()}";
        var variableDates = new Dictionary<string, string> { ["2025"] = "03/04", ["2026"] = "25/03" };

        var created = await (await client.PostAsJsonAsync(
            "/api/feriados",
            new CreateFeriadoRequest { Title = titulo, VariableDates = variableDates }))
            .Content.ReadFromJsonAsync<FeriadoResponse>();

        var fetched = await (await client.GetAsync($"/api/feriados/{created!.Id}"))
            .Content.ReadFromJsonAsync<FeriadoResponse>();

        Assert.Equal(2, fetched!.VariableDates.Count);
        Assert.Equal("03/04", fetched.VariableDates["2025"]);
        Assert.Equal("25/03", fetched.VariableDates["2026"]);
    }

    [Fact]
    public async Task Lista_PaginaEFiltraPorTitulo()
    {
        var client = _factory.CreateClient();
        var prefixo = $"PAG{Guid.NewGuid():N}";
        for (var i = 0; i < 7; i++)
        {
            var resp = await client.PostAsJsonAsync(
                "/api/feriados",
                new CreateFeriadoRequest { Title = $"{prefixo}-{i:D2}" });
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        }

        var page1 = await (await client.GetAsync($"/api/feriados?title={prefixo}&page=1&pageSize=5"))
            .Content.ReadFromJsonAsync<PagedResult<FeriadoResponse>>();
        Assert.Equal(7, page1!.TotalCount);
        Assert.Equal(2, page1.TotalPages);
        Assert.Equal(5, page1.Items.Count);

        var page2 = await (await client.GetAsync($"/api/feriados?title={prefixo}&page=2&pageSize=5"))
            .Content.ReadFromJsonAsync<PagedResult<FeriadoResponse>>();
        Assert.Equal(2, page2!.Items.Count);
    }

    [Fact]
    public async Task Lista_FiltroPorTitulo_EhCaseInsensitive()
    {
        var client = _factory.CreateClient();
        var marca = Guid.NewGuid().ToString("N");
        await client.PostAsJsonAsync("/api/feriados", new CreateFeriadoRequest { Title = $"HOLIDAY-{marca}" });

        var res = await (await client.GetAsync($"/api/feriados?title=holiday-{marca}"))
            .Content.ReadFromJsonAsync<PagedResult<FeriadoResponse>>();

        Assert.Equal(1, res!.TotalCount);
    }

    [Fact]
    public async Task Lista_FiltroPorData_CombinaComTitulo()
    {
        var client = _factory.CreateClient();
        var prefixo = $"DATE{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/api/feriados", new CreateFeriadoRequest { Title = $"{prefixo}-A", Date = "31/12" });
        await client.PostAsJsonAsync("/api/feriados", new CreateFeriadoRequest { Title = $"{prefixo}-B", Date = "01/01" });

        var res = await (await client.GetAsync($"/api/feriados?title={prefixo}&date=31%2F12"))
            .Content.ReadFromJsonAsync<PagedResult<FeriadoResponse>>();

        Assert.Equal(1, res!.TotalCount);
        Assert.EndsWith("-A", res.Items[0].Title);
    }
}
