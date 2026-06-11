# Servico da Palavra Backend

Backend V2 da plataforma Servico da Palavra, uma base de formacao biblica e espiritual com foco inicial em conteudos, trilhas e autenticacao. Este repositorio contem somente o backend.

## Stack

- ASP.NET Core
- Entity Framework Core
- SQLite local / PostgreSQL em producao
- ASP.NET Core Identity
- Cookie auth HttpOnly + CSRF
- Swagger/OpenAPI

## Estrutura

```text
src/
  ServicoPalavra.Api
  ServicoPalavra.Application
  ServicoPalavra.Domain
  ServicoPalavra.Infrastructure
tests/
  ServicoPalavra.UnitTests
  ServicoPalavra.IntegrationTests
```

## Como rodar

```bash
dotnet restore
dotnet build ServicoPalavra.sln
dotnet user-secrets --project src/ServicoPalavra.Api set "INITIAL_ADMIN_EMAIL" "admin@servicopalavra.local"
dotnet user-secrets --project src/ServicoPalavra.Api set "INITIAL_ADMIN_PASSWORD" "troque-esta-senha-local"
dotnet user-secrets --project src/ServicoPalavra.Api set "INITIAL_ADMIN_NAME" "Admin Local"
dotnet run --project src/ServicoPalavra.Api/ServicoPalavra.Api.csproj
```

Swagger em desenvolvimento:

```text
http://localhost:<porta>/swagger
```

Health check:

```text
GET /health
```

## Migrations

Criar migration:

```bash
dotnet ef migrations add NomeDaMigration --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Api --output-dir Persistence/Migrations
```

Aplicar migration:

```bash
dotnet ef database update --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Api
```

A API tambem aplica migrations na inicializacao.

## Banco de dados

O provider e escolhido por variavel:

```bash
DATABASE_PROVIDER=sqlite
ConnectionStrings__DefaultConnection="Data Source=servico-palavra-dev.db"
```

Em producao use PostgreSQL remoto persistente, por exemplo Neon:

```bash
DATABASE_PROVIDER=postgresql
ConnectionStrings__DefaultConnection="<configurada no provedor, fora do repositorio>"
```

## Autenticacao

A autenticacao usa ASP.NET Core Identity com roles `Usuario`, `Admin` e `Coordenador`. O navegador recebe cookie HttpOnly; a API nao retorna token. Antes de qualquer escrita, o cliente deve chamar `GET /api/auth/csrf` e enviar o token no header `X-CSRF-TOKEN`.

## Admin inicial

Em producao, nenhum admin padrao e criado sem variaveis de bootstrap:

```bash
INITIAL_ADMIN_EMAIL=<email>
INITIAL_ADMIN_PASSWORD=<senha-temporaria-forte>
INITIAL_ADMIN_NAME=<nome>
```

Use uma senha unica, troque apos o primeiro acesso e remova/rotacione as variaveis de bootstrap.

## Render Free

Para deploy futuro no Render:

- Build command: `dotnet publish src/ServicoPalavra.Api/ServicoPalavra.Api.csproj -c Release -o out`
- Start command: `dotnet out/ServicoPalavra.Api.dll`
- Configure `DATABASE_PROVIDER=postgresql`.
- Configure `ConnectionStrings__DefaultConnection` com a URL do Neon/PostgreSQL fora do repositorio.
- Configure `ALLOWED_ORIGINS` com os dominios reais do frontend.

Mais detalhes em [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), [docs/DATABASE.md](docs/DATABASE.md), [docs/API.md](docs/API.md) e [docs/ROADMAP.md](docs/ROADMAP.md).
