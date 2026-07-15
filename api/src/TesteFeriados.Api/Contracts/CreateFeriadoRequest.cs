using System.ComponentModel.DataAnnotations;

namespace TesteFeriados.Api.Contracts;

public sealed class CreateFeriadoRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [RegularExpression(
        @"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])$",
        ErrorMessage = "A data deve estar no formato dd/MM, com mês entre 01 e 12.")]
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
