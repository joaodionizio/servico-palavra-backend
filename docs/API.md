# API

Resposta padrao:

```json
{
  "success": true,
  "data": {},
  "message": null,
  "errors": null
}
```

## Health

- `GET /health`

## Auth

- `GET /api/auth/csrf`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `PUT /api/auth/me`

Auth usa cookie HttpOnly. A API nao retorna JWT/token no login. Para `POST`, `PUT`, `PATCH` e `DELETE`, envie `X-CSRF-TOKEN` obtido em `GET /api/auth/csrf`.

Atualizacao de perfil autenticado:

```json
{
  "nome": "Joao Luis",
  "email": "novo@email.com"
}
```

Somente `nome` e `email` podem ser atualizados nesta fase. Roles, tipo de acesso e senha nao sao aceitos neste contrato.

## Categorias

- `GET /api/categorias`
- `GET /api/admin/categorias` Admin
- `GET /api/admin/categorias/{id}` Admin
- `POST /api/admin/categorias` Admin
- `PUT /api/admin/categorias/{id}` Admin
- `PATCH /api/admin/categorias/{id}/status` Admin
- `DELETE /api/admin/categorias/{id}` Admin

`GET /api/categorias` retorna apenas categorias ativas. Os endpoints admin retornam categorias ativas e inativas. Categorias em uso por conteudos nao podem ser excluidas fisicamente; desative a categoria ou remova os vinculos antes de excluir.

## Conteudos

- `GET /api/conteudos`
- `GET /api/conteudos/{id}`
- `GET /api/conteudos/{slug}`
- `POST /api/admin/conteudos` Admin
- `PUT /api/admin/conteudos/{id}` Admin
- `DELETE /api/admin/conteudos/{id}` Admin
- `PATCH /api/admin/conteudos/{id}/publicacao` Admin
- `PATCH /api/admin/conteudos/{id}/publicar` Admin
- `PATCH /api/admin/conteudos/{id}/despublicar` Admin

Conteudos de video/audio/material nao enviam arquivo binario para a API nesta fase. O cadastro usa metadados e `url` externa, por exemplo YouTube ou Google Drive, conforme `origem`. Materiais de apoio tambem usam links externos `http/https`; nao ha upload. A estrategia completa esta em `MEDIA_STRATEGY.md`.

O contrato detalhado de listagem, detalhe e enums de conteudos/formacoes esta em `CONTENT_CONTRACT.md`.

## Favoritos

- `GET /api/favoritos`
- `POST /api/conteudos/{id}/favoritar`
- `DELETE /api/conteudos/{id}/favoritar`
- `POST /api/favoritos/{conteudoId}`
- `DELETE /api/favoritos/{conteudoId}`

## Progresso

- `POST /api/conteudos/{id}/concluir`
- `POST /api/progresso/conteudos/{conteudoId}/concluir`
- `DELETE /api/progresso/conteudos/{conteudoId}/concluir`

## Plano Biblico

- `GET /api/planos-biblicos/ativo`
- `GET /api/planos-biblicos/me/ativo`
- `GET /api/planos-biblicos/me/historico`
- `POST /api/planos-biblicos`
- `POST /api/planos-biblicos/alterar`
- `GET /api/planos-biblicos/{id}`
- `GET /api/planos-biblicos/{id}/dias`
- `GET /api/planos-biblicos/progresso/posicao-atual`
- `POST /api/planos-biblicos/dias/{diaId}/concluir`
- `POST /api/planos-biblicos/dias/{diaId}/desmarcar`

Criacao de plano:

```json
{
  "nome": "Plano Pastoral",
  "duracaoAnos": 1,
  "duracaoMeses": 0,
  "dataInicio": "2026-06-11"
}
```

Alteracao de plano:

```json
{
  "duracaoAnos": 0,
  "duracaoMeses": 6,
  "modo": "ContinuarDeOndeParei"
}
```

Detalhes de BaseBiblica, sequencia pastoral, importacao e peso de leitura estao em `BIBLE_PLAN.md`.

## Dashboard

- `GET /api/dashboard/me`

## Biblioteca

Biblioteca foi removida da V2 inicial. Nesta fase, o backend nao possui endpoints, entidades, tabelas ou servicos especificos de Biblioteca.

Conteudos/Formacoes continuam sendo o nucleo do produto e seguem expostos pelos endpoints de `Conteudos`, com categorias, favoritos, progresso, dashboard, admin de conteudos e plano biblico.

## Trilhas

Trilhas foram removidas da V2 inicial. A plataforma nesta fase trabalha com conteudos/formacoes individuais, categorias, favoritos, progresso de conteudo, dashboard, admin de conteudos e plano biblico.

As tabelas historicas `TrilhasFormacao` e `TrilhasConteudos` podem existir em bancos criados por migrations antigas, mas nao ha endpoints ou codigo ativo para trilhas nesta fase.
