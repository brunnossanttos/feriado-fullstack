using Microsoft.EntityFrameworkCore;
using TesteFeriados.Domain.Abstractions;
using TesteFeriados.Domain.Entities;

namespace TesteFeriados.Infrastructure.Persistence;

public class FeriadoRepository : IFeriadoRepository
{
    private readonly FeriadosDbContext _dbContext;

    public FeriadoRepository(FeriadosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Feriado?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Feriados.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<Feriado?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
        => _dbContext.Feriados.FirstOrDefaultAsync(f => f.Title == title, cancellationToken);

    public async Task<IReadOnlyList<Feriado>> ListAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Feriados.AsNoTracking()
            .OrderBy(f => f.Title)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Feriado> Items, int TotalCount)> ListPagedAsync(
        int page,
        int pageSize,
        string? title,
        string? date,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Feriados.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(f => EF.Functions.ILike(f.Title, $"%{title}%"));
        }

        if (!string.IsNullOrWhiteSpace(date))
        {
            query = query.Where(f => f.Date == date);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(f => f.Date == null)
            .ThenBy(f => f.Date!.Substring(3, 2))
            .ThenBy(f => f.Date!.Substring(0, 2))
            .ThenBy(f => f.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Feriado feriado) => _dbContext.Feriados.Add(feriado);

    public void AddRange(IEnumerable<Feriado> feriados) => _dbContext.Feriados.AddRange(feriados);

    public void Update(Feriado feriado) => _dbContext.Feriados.Update(feriado);

    public void Remove(Feriado feriado) => _dbContext.Feriados.Remove(feriado);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
