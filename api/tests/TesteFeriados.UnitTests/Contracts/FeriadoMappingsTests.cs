using TesteFeriados.Api.Contracts;
using TesteFeriados.Domain.Entities;

namespace TesteFeriados.UnitTests.Contracts;

public class FeriadoMappingsTests
{
    [Fact]
    public void ToEntity_MapeiaTodosOsCampos()
    {
        var request = new CreateFeriadoRequest
        {
            Title = "Natal",
            Date = "25/12",
            Description = "Celebração",
            Legislation = "Lei X",
            Type = "feriado",
            StartTime = "00:00",
            EndTime = "23:59",
            VariableDates = new Dictionary<string, string> { ["2026"] = "25/12" }
        };

        var feriado = request.ToEntity();

        Assert.Equal("Natal", feriado.Title);
        Assert.Equal("25/12", feriado.Date);
        Assert.Equal("Celebração", feriado.Description);
        Assert.Equal("Lei X", feriado.Legislation);
        Assert.Equal("feriado", feriado.Type);
        Assert.Equal("00:00", feriado.StartTime);
        Assert.Equal("23:59", feriado.EndTime);
        Assert.Equal("25/12", feriado.VariableDates["2026"]);
        Assert.NotEqual(Guid.Empty, feriado.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ToEntity_NormalizaCamposVaziosOuBrancoParaNull(string? valor)
    {
        var request = new CreateFeriadoRequest
        {
            Title = "Feriado",
            Date = valor,
            Description = valor,
            Legislation = valor,
            Type = valor,
            StartTime = valor,
            EndTime = valor
        };

        var feriado = request.ToEntity();

        Assert.Null(feriado.Date);
        Assert.Null(feriado.Description);
        Assert.Null(feriado.Legislation);
        Assert.Null(feriado.Type);
        Assert.Null(feriado.StartTime);
        Assert.Null(feriado.EndTime);
    }

    [Fact]
    public void ToEntity_AplicaTrimNoTitulo()
    {
        var request = new CreateFeriadoRequest { Title = "  Tiradentes  " };

        var feriado = request.ToEntity();

        Assert.Equal("Tiradentes", feriado.Title);
    }

    [Fact]
    public void ToEntity_VariableDatesNull_ViraDicionarioVazio()
    {
        var request = new CreateFeriadoRequest { Title = "Feriado", VariableDates = null };

        var feriado = request.ToEntity();

        Assert.NotNull(feriado.VariableDates);
        Assert.Empty(feriado.VariableDates);
    }

    [Fact]
    public void ToEntity_CopiaVariableDates_SemCompartilharReferencia()
    {
        var original = new Dictionary<string, string> { ["2026"] = "25/12" };
        var request = new CreateFeriadoRequest { Title = "Feriado", VariableDates = original };

        var feriado = request.ToEntity();
        original["2026"] = "ALTERADO";

        Assert.Equal("25/12", feriado.VariableDates["2026"]);
    }

    [Fact]
    public void ToResponse_MapeiaTodosOsCampos()
    {
        var id = Guid.NewGuid();
        var feriado = new Feriado
        {
            Id = id,
            Title = "Natal",
            Date = "25/12",
            Description = "Celebração",
            Legislation = "Lei X",
            Type = "feriado",
            StartTime = null,
            EndTime = null,
            VariableDates = new Dictionary<string, string> { ["2026"] = "25/12" }
        };

        var response = feriado.ToResponse();

        Assert.Equal(id, response.Id);
        Assert.Equal("Natal", response.Title);
        Assert.Equal("25/12", response.Date);
        Assert.Equal("Celebração", response.Description);
        Assert.Equal("Lei X", response.Legislation);
        Assert.Equal("feriado", response.Type);
        Assert.Null(response.StartTime);
        Assert.Null(response.EndTime);
        Assert.Equal("25/12", response.VariableDates["2026"]);
    }

    [Fact]
    public void ToResponse_CopiaVariableDates_SemCompartilharReferencia()
    {
        var feriado = new Feriado
        {
            Title = "Feriado",
            VariableDates = new Dictionary<string, string> { ["2026"] = "25/12" }
        };

        var response = feriado.ToResponse();
        feriado.VariableDates["2026"] = "ALTERADO";

        Assert.Equal("25/12", response.VariableDates["2026"]);
    }

    [Fact]
    public void ApplyTo_SubstituiCamposEPreservaId()
    {
        var id = Guid.NewGuid();
        var existente = new Feriado
        {
            Id = id,
            Title = "Antigo",
            Date = "01/01",
            Description = "desc antiga",
            Type = "feriado",
            VariableDates = new Dictionary<string, string> { ["2025"] = "01/01" }
        };

        var request = new UpdateFeriadoRequest
        {
            Title = "  Novo  ",
            Date = "02/02",
            Description = "",
            Type = null,
            VariableDates = null
        };

        request.ApplyTo(existente);

        Assert.Equal(id, existente.Id);
        Assert.Equal("Novo", existente.Title);
        Assert.Equal("02/02", existente.Date);
        Assert.Null(existente.Description);
        Assert.Null(existente.Type);
        Assert.Empty(existente.VariableDates);
    }
}
