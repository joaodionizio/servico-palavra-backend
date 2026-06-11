# Dependency Audit

## Executado

- `dotnet build ServicoPalavra.sln`: passou.
- `dotnet test ServicoPalavra.sln`: passou.

## Tentado sem conclusao no ambiente atual

- `dotnet list ServicoPalavra.sln package --vulnerable --include-transitive`: travou sem saida por mais de 60s.

Esse comando deve ser repetido antes de release em ambiente com CLI/NuGet estavel.

## Dependencias criticas

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Swashbuckle.AspNetCore`
- `Microsoft.AspNetCore.Mvc.Testing`

Nao houve frontend para executar `npm audit`, `npm run build` ou `npm run lint`.
