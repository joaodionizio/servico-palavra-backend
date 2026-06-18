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

Auth usa cookie HttpOnly. A API nao retorna JWT/token no login. Para `POST`, `PUT`, `PATCH` e `DELETE`, envie `X-CSRF-TOKEN` obtido em `GET /api/auth/csrf`.

## Categorias

- `GET /api/categorias`
- `POST /api/admin/categorias` Admin
- `PUT /api/admin/categorias/{id}` Admin

## Conteudos

- `GET /api/conteudos`
- `GET /api/conteudos/{id}`
- `POST /api/admin/conteudos` Admin
- `PUT /api/admin/conteudos/{id}` Admin
- `PATCH /api/admin/conteudos/{id}/publicar` Admin
- `PATCH /api/admin/conteudos/{id}/despublicar` Admin

Conteudos de video/audio/material nao enviam arquivo binario para a API nesta fase. O cadastro usa metadados e `url` externa, por exemplo YouTube ou Google Drive, conforme `origem`. A estrategia completa esta em `MEDIA_STRATEGY.md`.

## Favoritos

- `GET /api/favoritos`
- `POST /api/conteudos/{id}/favoritar`
- `DELETE /api/conteudos/{id}/favoritar`

## Progresso

- `POST /api/conteudos/{id}/concluir`

## Trilhas

- `GET /api/trilhas`
- `GET /api/trilhas/{id}`
- `POST /api/admin/trilhas` Admin
- `PUT /api/admin/trilhas/{id}` Admin
- `POST /api/admin/trilhas/{id}/conteudos` Admin
- `PUT /api/admin/trilhas/{id}/conteudos/ordem` Admin
- `DELETE /api/admin/trilhas/{id}/conteudos/{conteudoId}` Admin

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
