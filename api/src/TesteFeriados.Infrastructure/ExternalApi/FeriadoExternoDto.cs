using System.Text.Json.Serialization;

namespace TesteFeriados.Infrastructure.ExternalApi;

internal sealed class FeriadoExternoDto
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("legislation")]
    public string? Legislation { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("variableDates")]
    public Dictionary<string, string>? VariableDates { get; set; }
}
