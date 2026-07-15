using System.ComponentModel.DataAnnotations;

namespace TesteFeriados.Api.Contracts;

public sealed class CreateFeriadoRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "A data deve ter no máximo 10 caracteres (ex.: \"dd/MM\").")]
    public string? Date { get; set; }

    public string? Description { get; set; }

    public string? Legislation { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    [StringLength(10)]
    public string? StartTime { get; set; }

    [StringLength(10)]
    public string? EndTime { get; set; }

    public Dictionary<string, string>? VariableDates { get; set; }
}
