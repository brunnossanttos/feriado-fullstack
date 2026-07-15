using TesteFeriados.Domain.Entities;

namespace TesteFeriados.Domain.Abstractions;

public interface IFeriadoRepository
{
    Task<Feriado?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Feriado?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Feriado>> ListAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Feriado> Items, int TotalCount)> ListPagedAsync(
        int page,
        int pageSize,
        string? title,
        string? date,
        CancellationToken cancellationToken = default);

    void Add(Feriado feriado);

    void AddRange(IEnumerable<Feriado> feriados);

    void Update(Feriado feriado);

    void Remove(Feriado feriado);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
