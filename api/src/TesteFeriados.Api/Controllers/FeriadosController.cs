using Microsoft.AspNetCore.Mvc;
using TesteFeriados.Api.Contracts;
using TesteFeriados.Domain.Abstractions;

namespace TesteFeriados.Api.Controllers;

[ApiController]
[Route("api/feriados")]
[Produces("application/json")]
public sealed class FeriadosController : ControllerBase
{
    private const int DefaultPageSize = 5;
    private const int MaxPageSize = 50;

    private readonly IFeriadoRepository _repository;

    public FeriadosController(IFeriadoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FeriadoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FeriadoResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? title = null,
        [FromQuery] string? date = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }
        else if (pageSize > MaxPageSize)
        {
            pageSize = MaxPageSize;
        }

        var (items, totalCount) = await _repository.ListPagedAsync(page, pageSize, title, date, cancellationToken);

        var response = new PagedResult<FeriadoResponse>(
            items.Select(f => f.ToResponse()).ToList(),
            page,
            pageSize,
            totalCount);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FeriadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeriadoResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var feriado = await _repository.GetByIdAsync(id, cancellationToken);
        if (feriado is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(feriado.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(FeriadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeriadoResponse>> Create(
        CreateFeriadoRequest request,
        CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        var existing = await _repository.GetByTitleAsync(title, cancellationToken);
        if (existing is not null)
        {
            return ConflictProblem(title);
        }

        var feriado = request.ToEntity();
        _repository.Add(feriado);
        await _repository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = feriado.Id }, feriado.ToResponse());
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FeriadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeriadoResponse>> Update(
        Guid id,
        UpdateFeriadoRequest request,
        CancellationToken cancellationToken)
    {
        var feriado = await _repository.GetByIdAsync(id, cancellationToken);
        if (feriado is null)
        {
            return NotFoundProblem(id);
        }

        var newTitle = request.Title.Trim();
        if (!string.Equals(feriado.Title, newTitle, StringComparison.Ordinal))
        {
            var other = await _repository.GetByTitleAsync(newTitle, cancellationToken);
            if (other is not null && other.Id != id)
            {
                return ConflictProblem(newTitle);
            }
        }

        request.ApplyTo(feriado);
        _repository.Update(feriado);
        await _repository.SaveChangesAsync(cancellationToken);

        return Ok(feriado.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var feriado = await _repository.GetByIdAsync(id, cancellationToken);
        if (feriado is null)
        {
            return NotFoundProblem(id);
        }

        _repository.Remove(feriado);
        await _repository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private ObjectResult NotFoundProblem(Guid id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Feriado não encontrado",
        detail: $"Não existe feriado com Id {id}.");

    private ObjectResult ConflictProblem(string title) => Problem(
        statusCode: StatusCodes.Status409Conflict,
        title: "Título já cadastrado",
        detail: $"Já existe um feriado com o título '{title}'.");
}
