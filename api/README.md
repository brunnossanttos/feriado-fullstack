# TesteFeriados — API (.NET 8)

Web API de gestão de feriados nacionais. Sincroniza periodicamente os feriados da fonte pública
([`dadosbr.github.io/feriados/nacionais.json`](https://dadosbr.github.io/feriados/nacionais.json)),
persiste em PostgreSQL e expõe um CRUD com paginação e filtros.

## Arquitetura

Arquitetura em camadas (ports & adapters), com as dependências apontando para o domínio:

```
Api  →  Infrastructure  →  Domain
```

```
api/
├── src/
│   ├── TesteFeriados.Domain          → entidade Feriado + portas (interfaces)
│   ├── TesteFeriados.Infrastructure  → EF Core/Npgsql, repositório, HttpClient, sync
│   └── TesteFeriados.Api             → controllers, Program, BackgroundService, Swagger
├── tests/
│   ├── TesteFeriados.UnitTests        → testes unitários puros
│   └── TesteFeriados.IntegrationTests → Testcontainers + WebApplicationFactory
├── docker-compose.yml                 → PostgreSQL local
└── global.json                        → SDK .NET fixado
```

- **Domain**: `Feriado` e as portas `IFeriadoRepository`, `IFeriadosApiClient`, `IFeriadoSyncService`.
- **Infrastructure**: `FeriadosDbContext` + `FeriadoRepository` (EF Core/PostgreSQL, `variableDates`
  como `jsonb`), `FeriadosApiClient` (typed HttpClient), `FeriadoSyncService` (upsert por título).
- **Api**: `FeriadosController`, `FeriadosSyncBackgroundService`, handler global de erros e Swagger.

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download) (versão em `global.json`)
- [Docker](https://www.docker.com/) (PostgreSQL e Testcontainers)

## Como rodar

```bash
docker compose up -d                        # PostgreSQL em localhost:5432
dotnet run --project src/TesteFeriados.Api   # aplica migrations e sobe a API
```

- API: `http://localhost:5010`
- Swagger: `http://localhost:5010/swagger`

As migrations são aplicadas no startup (`Database.Migrate()`). O `BackgroundService` sincroniza
os feriados na primeira execução e depois a cada intervalo configurável.

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/feriados?page=1&pageSize=5&title=&date=` | Lista paginada (5/página), ordenada por data. Filtros opcionais por título (`ILIKE`) e data (`dd/MM`). |
| `GET` | `/api/feriados/{id}` | Obtém um feriado. |
| `POST` | `/api/feriados` | Cria um feriado. |
| `PUT` | `/api/feriados/{id}` | Substitui um feriado. |
| `DELETE` | `/api/feriados/{id}` | Remove um feriado. |

Erros seguem `ProblemDetails` (400 validação, 404 não encontrado, 409 título duplicado, 500
interno). A data, quando informada, deve estar em `dd/MM` com mês `01–12`.

## Configuração

`appsettings.json` (sobrescrevível por variável de ambiente com `__`):

| Chave | Padrão | Descrição |
|---|---|---|
| `ConnectionStrings:FeriadosDb` | `Host=localhost;Port=5432;Database=feriados;Username=postgres;Password=postgres` | Conexão com o PostgreSQL. |
| `FeriadosApi:BaseUrl` | `https://dadosbr.github.io/` | Base da API pública de feriados. |
| `FeriadosSync:IntervalHours` | `24` | Intervalo de sincronização do `BackgroundService`. |

Exemplo de override por ambiente:

```bash
export ConnectionStrings__FeriadosDb="Host=meu-host;Port=5432;Database=feriados;Username=user;Password=senha"
```

## Banco de dados / migrations

O schema é versionado por migrations do EF Core e aplicado automaticamente no startup. Para
gerar/aplicar manualmente, o `dotnet-ef` está fixado como ferramenta local:

```bash
dotnet tool restore
dotnet dotnet-ef migrations add <Nome> --project src/TesteFeriados.Infrastructure --startup-project src/TesteFeriados.Api
dotnet dotnet-ef database update      --project src/TesteFeriados.Infrastructure --startup-project src/TesteFeriados.Api
```

## Testes

```bash
dotnet test                 # unit + integração
```

- **Unit** (`TesteFeriados.UnitTests`): lógica pura (mapeamentos, paginação), sem dependências.
- **Integração** (`TesteFeriados.IntegrationTests`): **sem mocks** — sobe um PostgreSQL efêmero
  via **Testcontainers** e a aplicação real via `WebApplicationFactory`, cobrindo CRUD, round-trip
  do `jsonb`, paginação/filtros, ordenação, validação e o ciclo de sincronização contra a API
  pública real. Requer **Docker** em execução.

## Principais decisões

- `BackgroundService` (`IHostedService`) dentro da própria Web API para o consumo periódico.
- Repositório estilo unit-of-work; `variableDates` mapeado como `jsonb` (ValueConverter + ValueComparer).
- Typed `HttpClient` via `IHttpClientFactory`; upsert pela chave natural `Title` (índice único).
- Tratamento global de exceções via `IExceptionHandler` → `ProblemDetails`.
