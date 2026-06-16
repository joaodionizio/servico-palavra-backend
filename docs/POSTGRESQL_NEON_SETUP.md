# PostgreSQL / Neon Setup

O backend esta preparado para alternar provider por configuracao:

```bash
DATABASE_PROVIDER=postgresql
ConnectionStrings__DefaultConnection="<connection-string-neon>"
```

Para SQLite local:

```bash
DATABASE_PROVIDER=sqlite
ConnectionStrings__DefaultConnection="Data Source=servico-palavra-dev.db"
```

Passos para validar Neon:

1. Criar banco Neon/PostgreSQL.
2. Configurar `DATABASE_PROVIDER=postgresql`.
3. Configurar `ConnectionStrings__DefaultConnection` fora do repositorio.
4. Rodar `dotnet ef database update --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Infrastructure`.
5. Subir a API em `ASPNETCORE_ENVIRONMENT=Production`.
6. Validar `GET /health`, register/login/logout, plano biblico e uma escrita admin.
7. Rodar suite opcional quando `TEST_POSTGRES_CONNECTION_STRING` existir.

## Estrategia de datas

O projeto usa uma estrategia unica para campos `DateTime` e `DateTime?` persistidos:

- no C#, datas de auditoria e progresso devem ser gravadas com `DateTime.UtcNow`;
- no PostgreSQL, esses campos sao mapeados como `timestamp with time zone`;
- exemplos: `CriadoEm`, `AtualizadoEm`, `UltimoAcessoEm`, `PublicadoEm`, `ConcluidoEm` e `IniciadoEm`.

## Baseline atual

A baseline PostgreSQL atual e:

```text
20260616004919_InitialIdentityBaseline
```

Ela substitui a baseline inicial gerada com tipos SQLite/TEXT e evita sincronizacoes posteriores tentando converter colunas como `UltimoAcessoEm` para `timestamp with time zone`.

Se a validacao inicial no Neon aplicou uma migration parcial ou criou a tabela `__EFMigrationsHistory` com uma migration antiga, e o banco ainda nao tem dados reais, faca o reset manual do branch/database pelo painel do Neon antes de aplicar a baseline corrigida. Nao misture a baseline nova com schema parcial antigo.

## Validacao em 2026-06-15

Status: **nao validado contra Neon real nesta execucao**.

Motivo: nesta sessao local, `DATABASE_PROVIDER` nao estava definido como `postgresql` e `ConnectionStrings__DefaultConnection` nao estava visivel no ambiente. Por seguranca, `dotnet ef database update` nao foi executado para evitar aplicar migrations no provider errado.

Confirmacoes realizadas:

- O pacote `Npgsql.EntityFrameworkCore.PostgreSQL` esta referenciado em `ServicoPalavra.Infrastructure`.
- O runtime seleciona `UseNpgsql` quando `DATABASE_PROVIDER=postgresql`.
- A factory design-time do EF tambem passou a respeitar `DATABASE_PROVIDER` e `ConnectionStrings__DefaultConnection`.
- `dotnet ef migrations list --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Infrastructure --no-build` listou a baseline disponivel naquele momento.
- `DATABASE_PROVIDER=postgresql` sem connection string falha antes de criar o `DbContext`, sem tocar no banco.

Comandos executados com sucesso nesta validacao:

```bash
dotnet restore ServicoPalavra.sln
dotnet build ServicoPalavra.sln --no-restore
dotnet test ServicoPalavra.sln --no-build
dotnet ef migrations list --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Infrastructure --no-build
dotnet list src/ServicoPalavra.Api/ServicoPalavra.Api.csproj package --vulnerable --include-transitive
dotnet list src/ServicoPalavra.Application/ServicoPalavra.Application.csproj package --vulnerable --include-transitive
dotnet list src/ServicoPalavra.Domain/ServicoPalavra.Domain.csproj package --vulnerable --include-transitive
dotnet list src/ServicoPalavra.Infrastructure/ServicoPalavra.Infrastructure.csproj package --vulnerable --include-transitive
```

Pendencias para validar Neon real:

- Exportar `DATABASE_PROVIDER=postgresql` na mesma sessao em que os comandos serao executados.
- Exportar `ConnectionStrings__DefaultConnection` por secret/env var, sem gravar em arquivo.
- Executar `dotnet ef database update` contra Neon.
- Confirmar tabelas Identity, constraints e indices diretamente no PostgreSQL.
- Subir a API contra PostgreSQL e validar `GET /health`.
- Executar smoke tests reais com usuarios de teste e confirmar isolamento UsuarioA x UsuarioB.

## Atualizacao em 2026-06-16

- A baseline foi regenerada para PostgreSQL/Npgsql como `20260616003806_InitialIdentityBaseline` antes da migração para .NET 8.
- Todos os `DateTime`/`DateTime?` do modelo passam a ser configurados como `timestamp with time zone`.
- A migration de sincronizacao problemática nao deve ser usada.
- Se houver schema parcial no Neon de uma tentativa anterior e nao houver dados reais, resete o branch/database no painel Neon antes de rodar `dotnet ef database update`.
- `dotnet build ServicoPalavra.sln` e `dotnet test ServicoPalavra.sln` passaram apos a regeneracao.

## Atualizacao .NET 8 LTS em 2026-06-16

- A solucao foi migrada de .NET 10 / EF Core 10 para .NET 8 LTS / EF Core 8.
- A baseline PostgreSQL/Npgsql atual e `20260616004919_InitialIdentityBaseline`.
- A baseline EF 10 anterior foi removida porque o banco Neon ainda nao possui dados reais.
- Para comandos `dotnet ef`, use `ServicoPalavra.Infrastructure` como startup project para evitar inicializar a API durante design-time.
