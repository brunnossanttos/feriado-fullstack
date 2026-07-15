using TesteFeriados.Domain.Entities;

namespace TesteFeriados.Domain.Abstractions;

public interface IFeriadosApiClient
{
    Task<IReadOnlyList<Feriado>> GetNationalHolidaysAsync(CancellationToken cancellationToken = default);
}
