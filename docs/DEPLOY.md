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

A recuperacao de senha usa os token providers do ASP.NET Core Identity e nao exige migration propria.

## Render

Para Docker Web Service, use o `Dockerfile` do repositorio.

Variaveis esperadas:

- `ASPNETCORE_ENVIRONMENT=Production`
- `DATABASE_PROVIDER=postgresql`
- `ConnectionStrings__DefaultConnection=<secret do provedor>`
- `ALLOWED_ORIGINS=<dominios reais do frontend separados por virgula>`
- `INITIAL_ADMIN_EMAIL`, `INITIAL_ADMIN_PASSWORD`, `INITIAL_ADMIN_NAME` apenas para bootstrap controlado.

A API aceita `PORT`, `ASPNETCORE_URLS` ou `urls` e deve escutar em `0.0.0.0:{PORT}` em producao.

## Recuperacao De Senha / SMTP

O backend nao depende de um provedor pago especifico. O envio usa SMTP configuravel e os segredos ficam somente no provedor de deploy.

Variaveis:

```bash
FRONTEND_BASE_URL=https://seu-frontend.example
SMTP_HOST=<host-do-provedor>
SMTP_PORT=587
SMTP_USERNAME=<secret>
SMTP_PASSWORD=<secret>
SMTP_FROM_ADDRESS=no-reply@seu-dominio.example
SMTP_FROM_NAME="Servico da Palavra"
SMTP_ENABLE_SSL=true
```

O link gerado aponta para:

```text
/redefinir-senha?userId=<guid>&token=<token-url-safe>
```

O token de redefinicao expira em 1 hora. Nunca registrar em logs o token, a URL completa de reset, senha SMTP ou senha do usuario.

Se SMTP/`FRONTEND_BASE_URL` nao estiverem configurados, a API continua retornando a resposta publica generica para nao permitir enumeracao de contas, mas nenhum e-mail e enviado; trate isso como erro de configuracao operacional antes de publicar a feature.

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
bash tools/verify.sh
```

Smoke tests esperados:

- `GET /health`
- `GET /`
- `GET /api/auth/csrf`
- register/login/logout
- solicitar recuperacao de senha com conta existente e inexistente, confirmando resposta publica indistinguivel
- redefinir senha com token valido em ambiente de teste controlado
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
- Logs nao devem expor connection strings, tokens, cookies, links completos de recuperacao ou senhas.
