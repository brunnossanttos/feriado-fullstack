namespace TesteFeriados.Domain.Entities;

public class Feriado
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Date { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Legislation { get; set; }

    public string? Type { get; set; }

    public string? StartTime { get; set; }

    public string? EndTime { get; set; }

    public Dictionary<string, string> VariableDates { get; set; } = new();
}
