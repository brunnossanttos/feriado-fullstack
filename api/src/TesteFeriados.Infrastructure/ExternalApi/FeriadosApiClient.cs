using System.Net.Http.Json;
using TesteFeriados.Domain.Abstractions;
using TesteFeriados.Domain.Entities;

namespace TesteFeriados.Infrastructure.ExternalApi;

internal sealed class FeriadosApiClient : IFeriadosApiClient
{
    private const string ResourcePath = "feriados/nacionais.json";

    private readonly HttpClient _httpClient;

    public FeriadosApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Feriado>> GetNationalHolidaysAsync(CancellationToken cancellationToken = default)
    {
        var externos = await _httpClient.GetFromJsonAsync<List<FeriadoExternoDto>>(
            ResourcePath, cancellationToken);

        if (externos is null)
        {
            return Array.Empty<Feriado>();
        }

        return externos.Select(MapToDomain).ToList();
    }

    private static Feriado MapToDomain(FeriadoExternoDto dto) => new()
    {
        Date = NormalizeToNull(dto.Date),
        Title = dto.Title?.Trim() ?? string.Empty,
        Description = NormalizeToNull(dto.Description),
        Legislation = NormalizeToNull(dto.Legislation),
        Type = NormalizeToNull(dto.Type),
        StartTime = NormalizeToNull(dto.StartTime),
        EndTime = NormalizeToNull(dto.EndTime),
        VariableDates = dto.VariableDates is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(dto.VariableDates)
    };

    private static string? NormalizeToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
