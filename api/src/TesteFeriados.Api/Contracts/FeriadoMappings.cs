using TesteFeriados.Domain.Entities;

namespace TesteFeriados.Api.Contracts;

public static class FeriadoMappings
{
    public static FeriadoResponse ToResponse(this Feriado feriado) => new(
        feriado.Id,
        feriado.Date,
        feriado.Title,
        feriado.Description,
        feriado.Legislation,
        feriado.Type,
        feriado.StartTime,
        feriado.EndTime,
        new Dictionary<string, string>(feriado.VariableDates));

    public static Feriado ToEntity(this CreateFeriadoRequest request) => new()
    {
        Date = NormalizeToNull(request.Date),
        Title = request.Title.Trim(),
        Description = NormalizeToNull(request.Description),
        Legislation = NormalizeToNull(request.Legislation),
        Type = NormalizeToNull(request.Type),
        StartTime = NormalizeToNull(request.StartTime),
        EndTime = NormalizeToNull(request.EndTime),
        VariableDates = request.VariableDates is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(request.VariableDates)
    };

    public static void ApplyTo(this UpdateFeriadoRequest request, Feriado feriado)
    {
        feriado.Date = NormalizeToNull(request.Date);
        feriado.Title = request.Title.Trim();
        feriado.Description = NormalizeToNull(request.Description);
        feriado.Legislation = NormalizeToNull(request.Legislation);
        feriado.Type = NormalizeToNull(request.Type);
        feriado.StartTime = NormalizeToNull(request.StartTime);
        feriado.EndTime = NormalizeToNull(request.EndTime);
        feriado.VariableDates = request.VariableDates is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(request.VariableDates);
    }

    private static string? NormalizeToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
