# Seguranca

Este documento consolida as decisoes de seguranca do backend V2.

## Autenticacao

O backend usa ASP.NET Core Identity com `ApplicationUser : IdentityUser<Guid>` e roles:

- `Usuario`
- `Admin`
- `Coordenador`

Fluxo web:

1. `GET /api/auth/csrf` retorna `data.token` e grava cookie antiforgery.
2. `POST /api/auth/login` ou `POST /api/auth/register` envia `X-CSRF-TOKEN`.
3. A API grava cookie HttpOnly `__Host-ServicoPalavra`.
4. Nenhum JWT/token de autenticacao e retornado no corpo.
5. Apos login/register/logout, o frontend deve buscar CSRF novamente.
6. `GET /api/auth/me` identifica a sessao atual.
7. `POST /api/auth/logout` encerra a sessao.

Cookies:

- HttpOnly sempre.
- Secure sempre fora de Development/Testing.
- SameSite `None`, para frontend em dominio separado com CORS allowlist e credenciais.
- Expiracao de 60 minutos com sliding expiration.

## CSRF

Todas as requisicoes `POST`, `PUT`, `PATCH` e `DELETE` passam por validacao antiforgery.

Header esperado:

```text
X-CSRF-TOKEN
```

Endpoint de token:

```text
GET /api/auth/csrf
```

## Autorizacao

Endpoints administrativos usam:

```csharp
[Authorize(Roles = "Admin")]
```

Usuario comum nao deve acessar rotas `api/admin/*`.

Regras especificas para `Coordenador` ainda dependem de validacao de produto.

## Isolamento Por Usuario

O frontend nao deve enviar `UsuarioId` para controlar acesso a dados privados.

O backend deriva o usuario da sessao autenticada por `CurrentUser`.

Modulos com dados por usuario:

- Favoritos
- ProgressoConteudo
- Dashboard
- BiblePlans
- ReadingProgress

Consultas e escritas desses modulos devem filtrar por `UsuarioId` do backend.

## Midia E URLs Externas

Conteudos de video/audio/material usam metadados e URL externa. Links externos devem ser tratados como entrada nao confiavel.

Validacoes esperadas:

- protocolo HTTPS;
- host permitido conforme origem;
- rejeicao de `javascript:` e URLs maliciosas.

Detalhes de midia estao em `MEDIA_STRATEGY.md`.

## Secrets

Nunca versionar:

- connection strings reais;
- senhas;
- tokens;
- cookies;
- arquivos `.env`;
- bancos locais;
- secrets de providers externos.

Variaveis sensiveis:

- `ConnectionStrings__DefaultConnection`
- `INITIAL_ADMIN_EMAIL`
- `INITIAL_ADMIN_PASSWORD`
- `INITIAL_ADMIN_NAME`

Em desenvolvimento, prefira `dotnet user-secrets`.

## Matriz Resumida De Endpoints

| Area | Acesso |
|---|---|
| `/health` e `/` | Publico |
| `/api/auth/csrf`, login e register | Publico com CSRF para escrita |
| `/api/auth/me`, logout | Autenticado |
| `/api/categorias`, `/api/conteudos`, `/api/trilhas` | Publico, somente dados publicados/ativos |
| `/api/admin/*` | Admin |
| `/api/favoritos` | Usuario autenticado |
| `/api/conteudos/{id}/favoritar` | Usuario autenticado + CSRF |
| `/api/conteudos/{id}/concluir` | Usuario autenticado + CSRF |
| `/api/dashboard/me` | Usuario autenticado |
| `/api/planos-biblicos/*` | Usuario autenticado; escrita com CSRF |

## Testes De Seguranca

A suite cobre:

- login valido sem retornar hash/token;
- login invalido com resposta generica;
- lockout por tentativas invalidas;
- rota protegida sem auth retorna `401`;
- usuario comum bloqueado em admin;
- admin aceito em admin;
- CSRF em escritas;
- erro sem stack trace/connection string;
- URL maliciosa de conteudo rejeitada;
- isolamento do Plano Biblico entre usuarios;
- marcar/desmarcar dia do plano respeitando dono;
- plano unico ativo por usuario;
- rollback em alteracao de plano.

Coberturas que devem continuar sendo reforcadas conforme o produto cresce:

- regras especificas para `Coordenador`;
- CORS com dominios finais;
- smoke tests contra PostgreSQL/Neon autorizado.
