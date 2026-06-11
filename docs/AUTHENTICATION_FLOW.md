# Authentication Flow

O backend usa ASP.NET Core Identity com `ApplicationUser : IdentityUser<Guid>` e roles `Usuario`, `Admin` e `Coordenador`.

Fluxo para navegador:

1. `GET /api/auth/csrf` retorna `data.token` e grava o cookie antiforgery.
2. `POST /api/auth/login` ou `POST /api/auth/register` envia `X-CSRF-TOKEN`.
3. A API grava cookie HttpOnly `__Host-ServicoPalavra`; nenhum token e retornado no corpo.
4. Apos login/register/logout, o frontend deve chamar `GET /api/auth/csrf` novamente porque o token antiforgery e vinculado ao usuario autenticado.
5. `GET /api/auth/me` identifica a sessao atual.
6. `POST /api/auth/logout` encerra a sessao.

Cookies:

- HttpOnly: sempre.
- Secure: sempre fora de Development/Testing.
- SameSite: `None`, para permitir frontend em dominio separado com CORS allowlist e credenciais.
- Expiracao: 60 minutos com sliding expiration.

CORS deve usar `ALLOWED_ORIGINS` com os dominios reais do frontend e `credentials: true` no cliente.
