# Banco de Dados

O banco local e SQLite por custo zero, usando EF Core e migrations. Producao deve usar PostgreSQL remoto persistente por `Npgsql.EntityFrameworkCore.PostgreSQL`.

## Migrations

Migration baseline atual:

```text
src/ServicoPalavra.Infrastructure/Persistence/Migrations/*_InitialIdentityBaseline.cs
```

Comandos:

```bash
dotnet ef migrations add NomeDaMigration --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Api
```

## Indices unicos

- `Usuarios.NormalizedEmail`
- `Usuarios.NormalizedUserName`
- `AspNetRoles.NormalizedName`
- `Perfis.Nome`
- `CategoriasConteudo.Slug`
- `Conteudos.Slug`
- `TrilhasFormacao.Slug`
- `BaseBiblica.Ordem`
- `TrilhasConteudos(TrilhaFormacaoId, ConteudoId)`
- `ProgressosConteudo(UsuarioId, ConteudoId)`
- `Favoritos(UsuarioId, ConteudoId)`
- `ProgressosLeitura(UsuarioId, PlanoBiblicoDiaId)`
- `PosicoesBiblicasUsuario(UsuarioId)`

## Troca futura de banco

Para trocar o banco, use:

- `DATABASE_PROVIDER=sqlite` ou `DATABASE_PROVIDER=postgresql`;
- `ConnectionStrings__DefaultConnection` fora do repositorio;
- migrations revisadas para o provider alvo.

As camadas Domain e Application nao dependem de SQLite.
