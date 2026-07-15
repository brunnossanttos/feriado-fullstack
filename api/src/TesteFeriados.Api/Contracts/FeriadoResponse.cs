namespace TesteFeriados.Api.Contracts;

public sealed record FeriadoResponse(
    Guid Id,
    string? Date,
    string Title,
    string? Description,
    string? Legislation,
    string? Type,
    string? StartTime,
    string? EndTime,
    IReadOnlyDictionary<string, string> VariableDates);
