# Deploy

Este documento resume configuracao operacional para desenvolvimento, Render e PostgreSQL/Neon.

## Providers De Banco

SQLite local:

```bash
DATABASE_PROVIDER=sqlite
ConnectionStrings__DefaultConnection="Data Source=servico-palavra-dev.db"
```

PostgreSQL/Neon:

```bash
DATABASE_PROVIDER=postgresql
ConnectionStrings__DefaultConnection="<configurada no provedor, fora do repositorio>"
```

Nunca versionar connection string real.

## Migrations

Criar migration:

```bash
dotnet ef migrations add NomeDaMigration \
  --project src/ServicoPalavra.Infrastructure \
  --startup-project src/ServicoPalavra.Infrastructure \
  --output-dir Persistence/Migrations
```

Aplicar migration:

```bash
dotnet ef database update \
  --project src/ServicoPalavra.Infrastructure \
  --startup-project src/ServicoPalavra.Infrastructure
```

Use `ServicoPalavra.Infrastructure` como startup project para que o EF use a factory design-time sem inicializar a API.

## Render

Para Docker Web Service, use o `Dockerfile` do repositorio.

Variaveis esperadas:

- `ASPNETCORE_ENVIRONMENT=Production`
- `DATABASE_PROVIDER=postgresql`
- `ConnectionStrings__DefaultConnection=<secret do provedor>`
- `ALLOWED_ORIGINS=<dominios reais do frontend separados por virgula>`
- `INITIAL_ADMIN_EMAIL`, `INITIAL_ADMIN_PASSWORD`, `INITIAL_ADMIN_NAME` apenas para bootstrap controlado.

A API aceita `PORT`, `ASPNETCORE_URLS` ou `urls` e deve escutar em `0.0.0.0:{PORT}` em producao.

## Admin Inicial

Em producao, nenhum admin padrao deve ser criado sem variaveis de bootstrap.

Use senha unica, troque apos o primeiro acesso e remova/rotacione as variaveis.

```bash
INITIAL_ADMIN_EMAIL=<email>
INITIAL_ADMIN_PASSWORD=<senha-temporaria-forte>
INITIAL_ADMIN_NAME=<nome>
```

## Validacao De Deploy

Antes de publicar:

```bash
dotnet restore ServicoPalavra.sln
dotnet build ServicoPalavra.sln
dotnet test ServicoPalavra.sln
```

Smoke tests esperados:

- `GET /health`
- `GET /`
- `GET /api/auth/csrf`
- register/login/logout
- uma escrita admin
- criacao/listagem de plano biblico, se a BaseBiblica estiver importada.

## BaseBiblica Em Producao

A BaseBiblica nao e importada automaticamente no startup normal da API.

Para importar no Neon com seguranca:

1. Fazer backup/snapshot.
2. Confirmar migrations aplicadas.
3. Rodar testes localmente.
4. Validar JSONs.
5. Executar modo `import-base-biblica-v2` com connection string via secret/env var.

Exemplo:

```bash
env DATABASE_PROVIDER=postgresql \
  ConnectionStrings__DefaultConnection="$NEON_CONNECTION_STRING" \
  dotnet run --project src/ServicoPalavra.Api -- \
  --run-mode=import-base-biblica-v2
```

## Seguranca Operacional

- Nao usar SQLite como banco persistente em producao.
- Nao gravar secrets em `appsettings*.json`, README, `.http`, scripts ou logs.
- CORS deve usar allowlist real; nao usar `*` com credenciais.
- Cookies devem permanecer HttpOnly.
- Escritas devem manter validacao CSRF.
- Logs nao devem expor connection strings, tokens, cookies ou senhas.
