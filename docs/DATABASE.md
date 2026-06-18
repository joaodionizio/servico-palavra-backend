# Banco de Dados

O banco local e SQLite por custo zero, usando EF Core e migrations. Producao deve usar PostgreSQL remoto persistente por `Npgsql.EntityFrameworkCore.PostgreSQL`.

## Migrations

Migration baseline atual:

```text
src/ServicoPalavra.Infrastructure/Persistence/Migrations/*_InitialIdentityBaseline.cs
```

Comandos:

```bash
dotnet ef migrations add NomeDaMigration --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Infrastructure --output-dir Persistence/Migrations
dotnet ef database update --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Infrastructure
```

## Indices unicos

- `Usuarios.NormalizedEmail`
- `Usuarios.NormalizedUserName`
- `AspNetRoles.NormalizedName`
- `Perfis.Nome`
- `CategoriasConteudo.Slug`
- `Conteudos.Slug`
- `BaseBiblica.Ordem`
- `ProgressosConteudo(UsuarioId, ConteudoId)`
- `Favoritos(UsuarioId, ConteudoId)`
- `ProgressosLeitura(UsuarioId, PlanoBiblicoDiaId)`
- `PosicoesBiblicasUsuario(UsuarioId)`

## Tabelas Historicas Inativas

As migrations antigas podem criar `TrilhasFormacao` e `TrilhasConteudos`. O modulo de Trilhas foi removido da V2 inicial e essas tabelas nao fazem parte do modelo ativo da aplicacao nesta fase. Nao remova tabelas de banco de producao sem uma migration explicita e revisada para esse objetivo.

## Midia E Arquivos

O banco deve armazenar apenas metadados de conteudos e materiais:

- titulo, descricao, resumo e slug;
- tipo e origem;
- URL externa;
- duracao;
- thumbnail;
- categoria;
- status de publicacao.

Videos, audios, imagens e documentos nao devem ser salvos como binario ou base64 no banco. Na V2 inicial, videos e audios devem apontar para origem externa, como YouTube ou Google Drive. Upload direto para storage externo fica pendente de decisao futura e nao esta implementado.

## Troca futura de banco

Para trocar o banco, use:

- `DATABASE_PROVIDER=sqlite` ou `DATABASE_PROVIDER=postgresql`;
- `ConnectionStrings__DefaultConnection` fora do repositorio;
- migrations revisadas para o provider alvo.

As camadas Domain e Application nao dependem de SQLite.
