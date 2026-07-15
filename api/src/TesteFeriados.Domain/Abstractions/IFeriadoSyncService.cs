namespace TesteFeriados.Domain.Abstractions;

public interface IFeriadoSyncService
{
    Task<FeriadoSyncResult> SynchronizeAsync(CancellationToken cancellationToken = default);
}
