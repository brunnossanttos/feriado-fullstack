# Feriados — Full Stack (.NET 8 + Angular 21)

Aplicação de gestão de **feriados nacionais**. Uma Web API em .NET 8 sincroniza
periodicamente os feriados a partir da fonte pública
[`dadosbr.github.io/feriados/nacionais.json`](https://dadosbr.github.io/feriados/nacionais.json),
persiste em PostgreSQL e os expõe via CRUD; um front-end Angular consome essa API com grid
paginado, filtros, edição e exclusão.

## Estrutura do repositório

```
feriado/
├── api/     → Web API .NET 8 (CRUD + BackgroundService de sincronização)
├── app/     → Front-end Angular 21
└── .github/ → Pipeline de CI (build + testes)
```

## Stack

| Camada | Tecnologia |
|---|---|
| Back-end | .NET 8 (Web API + Controllers), EF Core 8, Npgsql |
| Banco | PostgreSQL 16 (coluna `jsonb` para datas variáveis) |
| Sincronização | `BackgroundService` dentro da própria Web API |
| Front-end | Angular 21 (standalone, signals, zoneless), SCSS |
| Testes | xUnit + Testcontainers (back) · vitest (front) — reais, sem mocks |
| Docs da API | Swagger / OpenAPI |

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download) (versão fixada em `api/global.json`)
- [Docker](https://www.docker.com/) (PostgreSQL local e Testcontainers)
- [Node.js 22+](https://nodejs.org/) e npm (front-end)

## Como rodar

### 1. Back-end (API + PostgreSQL)

```bash
cd api
docker compose up -d                        # sobe o PostgreSQL
dotnet run --project src/TesteFeriados.Api   # aplica migrations e sobe a API
```

- API: `http://localhost:5010`
- Swagger: `http://localhost:5010/swagger`

A API aplica as migrations no startup e o `BackgroundService` já sincroniza os feriados na
primeira execução. Detalhes em [`api/README.md`](api/README.md).

### 2. Front-end (Angular)

```bash
cd app
npm install
npm start        # http://localhost:4200
```

O dev-server usa `proxy.conf.json` para encaminhar `/api` → `http://localhost:5010`
(sem CORS em desenvolvimento). **Suba a API antes** para a aplicação funcionar.

## Testes

```bash
# Back-end (unit + integração com Testcontainers — precisa de Docker)
cd api && dotnet test

# Front-end (unit + integração real — precisa da API no ar em :5010)
cd app && npm test
```

Os testes de integração são **reais, sem mocks**: o back usa PostgreSQL efêmero via
Testcontainers e bate na API pública; o front consome a API real rodando.

## CI

O workflow [`.github/workflows/ci.yml`](.github/workflows/ci.yml) roda a cada push/PR na `main`:

- **backend**: `restore` → `build` → `test` (integração via Testcontainers).
- **frontend**: sobe PostgreSQL + a API, faz `build` do app e roda os testes contra a API real.

## Funcionalidades

- CRUD completo de feriados (criar, listar, editar, excluir).
- Grid com **paginação de 5 por página**, ordenado por data.
- Filtros por **nome** (contém, sem diferenciar maiúsculas) e por **data**.
- Sincronização periódica automática com a fonte pública.
- Validação (ex.: data no formato `dd/MM`, mês 01–12) e erros padronizados (`ProblemDetails`).
