using Microsoft.Extensions.Logging;
using TesteFeriados.Domain.Abstractions;
using TesteFeriados.Domain.Entities;

namespace TesteFeriados.Infrastructure.Sync;

internal sealed class FeriadoSyncService : IFeriadoSyncService
{
    private readonly IFeriadosApiClient _apiClient;
    private readonly IFeriadoRepository _repository;
    private readonly ILogger<FeriadoSyncService> _logger;

    public FeriadoSyncService(
        IFeriadosApiClient apiClient,
        IFeriadoRepository repository,
        ILogger<FeriadoSyncService> logger)
    {
        _apiClient = apiClient;
        _repository = repository;
        _logger = logger;
    }

    public async Task<FeriadoSyncResult> SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando sincronização de feriados com a API pública.");

        IReadOnlyList<Feriado> externos;
        try
        {
            externos = await _apiClient.GetNationalHolidaysAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao obter feriados da API pública. Sincronização abortada.");
            throw;
        }

        var inserted = 0;
        var updated = 0;
        var unchanged = 0;

        var processedTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var incoming in externos)
        {
            if (string.IsNullOrWhiteSpace(incoming.Title))
            {
                _logger.LogWarning("Feriado recebido sem título; ignorado.");
                continue;
            }

            if (!processedTitles.Add(incoming.Title))
            {
                _logger.LogWarning("Título duplicado no payload da API: {Title}. Ignorado.", incoming.Title);
                continue;
            }

            var existing = await _repository.GetByTitleAsync(incoming.Title, cancellationToken);

            if (existing is null)
            {
                _repository.Add(incoming);
                inserted++;
            }
            else if (ApplyChanges(existing, incoming))
            {
                _repository.Update(existing);
                updated++;
            }
            else
            {
                unchanged++;
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);

        var result = new FeriadoSyncResult(externos.Count, inserted, updated, unchanged);
        _logger.LogInformation(
            "Sincronização concluída. Recebidos={Total}, Inseridos={Inserted}, Atualizados={Updated}, Inalterados={Unchanged}.",
            result.TotalReceived, result.Inserted, result.Updated, result.Unchanged);

        return result;
    }

    private static bool ApplyChanges(Feriado existing, Feriado incoming)
    {
        if (!HasDifferences(existing, incoming))
        {
            return false;
        }

        existing.Date = incoming.Date;
        existing.Description = incoming.Description;
        existing.Legislation = incoming.Legislation;
        existing.Type = incoming.Type;
        existing.StartTime = incoming.StartTime;
        existing.EndTime = incoming.EndTime;
        existing.VariableDates = incoming.VariableDates;
        return true;
    }

    private static bool HasDifferences(Feriado a, Feriado b)
        => a.Date != b.Date
        || a.Description != b.Description
        || a.Legislation != b.Legislation
        || a.Type != b.Type
        || a.StartTime != b.StartTime
        || a.EndTime != b.EndTime
        || !VariableDatesEqual(a.VariableDates, b.VariableDates);

    private static bool VariableDatesEqual(
        IReadOnlyDictionary<string, string> a,
        IReadOnlyDictionary<string, string> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (var kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
            {
                return false;
            }
        }

        return true;
    }
}
