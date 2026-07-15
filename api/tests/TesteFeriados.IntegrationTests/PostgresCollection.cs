namespace TesteFeriados.IntegrationTests;

[CollectionDefinition("postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresApiFactory>;
