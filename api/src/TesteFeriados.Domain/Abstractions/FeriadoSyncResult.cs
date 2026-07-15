namespace TesteFeriados.Domain.Abstractions;

public sealed record FeriadoSyncResult(int TotalReceived, int Inserted, int Updated, int Unchanged);
