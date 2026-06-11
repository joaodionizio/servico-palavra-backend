# Security Inventory

Data: 2026-06-11

## Escopo

Repositorio analisado: backend ASP.NET Core. Nao ha frontend Next.js, React, Tailwind ou rotas de cliente neste repositorio.

## Projetos .NET

- `src/ServicoPalavra.Api`
- `src/ServicoPalavra.Application`
- `src/ServicoPalavra.Domain`
- `src/ServicoPalavra.Infrastructure`
- `tests/ServicoPalavra.UnitTests`
- `tests/ServicoPalavra.IntegrationTests`

## Superficie de ataque

- API REST JSON.
- Swagger em ambiente Development.
- Cookie HttpOnly do ASP.NET Core Identity.
- SQLite local em desenvolvimento/testes.
- PostgreSQL habilitado por configuracao para producao.
- Links externos para YouTube/Google Drive/metadados de midia.

## Controllers e endpoints

Publicos:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/categorias`
- `GET /api/conteudos`
- `GET /api/conteudos/{id}`
- `GET /api/trilhas`
- `GET /api/trilhas/{id}`
- `GET /health`

Privados:

- `GET /api/auth/me`
- `GET /api/favoritos`
- `POST /api/conteudos/{id}/favoritar`
- `DELETE /api/conteudos/{id}/favoritar`
- `POST /api/conteudos/{id}/concluir`
- `GET /api/planos-biblicos/ativo`
- `POST /api/planos-biblicos`
- `POST /api/planos-biblicos/alterar`
- `GET /api/planos-biblicos/{id}`
- `GET /api/planos-biblicos/{id}/dias`
- `GET /api/planos-biblicos/progresso/posicao-atual`
- `POST /api/planos-biblicos/dias/{diaId}/concluir`
- `GET /api/dashboard/me`

Administrativos:

- `POST /api/admin/categorias`
- `PUT /api/admin/categorias/{id}`
- `POST /api/admin/conteudos`
- `PUT /api/admin/conteudos/{id}`
- `PATCH /api/admin/conteudos/{id}/publicar`
- `PATCH /api/admin/conteudos/{id}/despublicar`
- `POST /api/admin/trilhas`
- `PUT /api/admin/trilhas/{id}`
- `POST /api/admin/trilhas/{id}/conteudos`
- `PUT /api/admin/trilhas/{id}/conteudos/ordem`
- `DELETE /api/admin/trilhas/{id}/conteudos/{conteudoId}`

## Dados pessoais tratados

- Nome.
- E-mail.
- Hash de senha.
- FotoUrl.
- Perfil/role.
- Ultimo acesso.
- Favoritos.
- Progresso de conteudo.
- Plano biblico, dias e posicao pastoral.

## IDs recebidos pela API

- `conteudoId` por rota.
- `categoriaId` por rota/body admin.
- `trilhaId` por rota.
- `diaId` por rota.
- `planoBiblicoId` por rota.

O `UsuarioId` e derivado da sessao no backend, nao do frontend.

## Dependencias criticas

- ASP.NET Core.
- EF Core.
- SQLite provider.
- Npgsql provider.
- Microsoft.AspNetCore.Identity.EntityFrameworkCore.
- Swashbuckle.AspNetCore.
- Microsoft.AspNetCore.Mvc.Testing.

## Configuracoes sensiveis

- `DATABASE_PROVIDER`.
- `ConnectionStrings__DefaultConnection`.
- `INITIAL_ADMIN_EMAIL`.
- `INITIAL_ADMIN_PASSWORD`.
- `INITIAL_ADMIN_NAME`.
- `Cors:AllowedOrigins`.

## Riscos encontrados

- Cookies/CSRF exigem frontend com `credentials: true` e CORS allowlist.
- Sem frontend no repositorio para auditar localStorage, CSP ou cookies.
- Base biblica pastoral completa ainda depende de fonte revisada.
- Swagger aberto em Development; producao depende de ambiente correto.

## Pontos corrigidos nesta rodada

- Remocao de segredo JWT versionado e substituicao por Identity cookie auth.
- Bootstrap admin somente por variavel de ambiente; senha padrao nao e criada.
- Provider PostgreSQL por variavel de ambiente.
- Rate limiting.
- HSTS/HTTPS/headers basicos.
- Validacao de URLs externas.
- Testes de isolamento e autorizacao.
- CSRF para escritas autenticadas por cookie.
- Plano biblico pastoral com fixture representativa e historico.

## Decisoes humanas necessarias

- Definir dominios reais de frontend/API para CORS.
- Definir CSP do frontend quando o repo de frontend existir.
- Definir processo operacional de bootstrap do primeiro admin.
