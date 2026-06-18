# Contrato De Conteudos E Formacoes

Este documento registra o contrato oficial do modulo de Conteudos/Formacoes consumido pelo frontend.

## Decisao De Serializacao

Os DTOs de conteudo usam enums C# diretamente e a API nao configura `JsonStringEnumConverter` em `Program.cs`. Portanto, no contrato JSON atual, os enums sao serializados como numeros.

Decisao da Fase 10A:

- manter os enums como numeros para nao quebrar o contrato atual;
- documentar os valores oficiais abaixo;
- nao trocar serializacao global para string, porque isso afetaria outros DTOs e requests;
- nao adicionar campos auxiliares nesta fase, para manter a mudanca pequena e sem impacto em records/consumidores existentes.

Se uma fase futura quiser facilitar consumo no frontend, a opcao compativel e adicionar campos somente leitura como `tipoNome`, `origemNome` e `materialTipoNome`, preservando os campos numericos atuais.

## Endpoints Publicos Usados Pelo Frontend

- `GET /api/conteudos`
- `GET /api/conteudos/{slug}`
- `POST /api/favoritos/{conteudoId}`
- `DELETE /api/favoritos/{conteudoId}`
- `POST /api/progresso/conteudos/{conteudoId}/concluir`
- `DELETE /api/progresso/conteudos/{conteudoId}/concluir`

Aliases legados tambem existem para favoritos e progresso:

- `POST /api/conteudos/{id}/favoritar`
- `DELETE /api/conteudos/{id}/favoritar`
- `POST /api/conteudos/{id}/concluir`

## Enums Oficiais

### TipoConteudo

Fonte: `src/ServicoPalavra.Domain/Enums/TipoConteudo.cs`.

| Nome | Valor | Significado |
| --- | ---: | --- |
| `Video` | 1 | Conteudo principal em video. |
| `Audio` | 2 | Conteudo principal em audio. |
| `Documento` | 3 | Conteudo principal em documento ou arquivo textual. |
| `Link` | 4 | Conteudo principal representado por link externo. |
| `Texto` | 5 | Conteudo textual mantido como formacao/artigo. |

### OrigemConteudo

Fonte: `src/ServicoPalavra.Domain/Enums/OrigemConteudo.cs`.

| Nome | Valor | Significado |
| --- | ---: | --- |
| `YouTube` | 1 | Conteudo hospedado no YouTube. A URL deve usar dominio `youtube.com` ou `youtu.be`. |
| `GoogleDrive` | 2 | Conteudo hospedado no Google Drive ou Google Docs. |
| `Externo` | 3 | Conteudo hospedado em outro provedor externo via HTTPS. |
| `Interno` | 4 | Conteudo tratado como interno pela plataforma. |

### TipoMaterialApoio

Fonte: `src/ServicoPalavra.Domain/Enums/TipoMaterialApoio.cs`.

| Nome | Valor | Significado |
| --- | ---: | --- |
| `PDF` | 1 | Material de apoio em PDF. |
| `Slide` | 2 | Apresentacao ou conjunto de slides. |
| `Imagem` | 3 | Imagem de apoio. |
| `Documento` | 4 | Documento de apoio em formato textual/editavel. |
| `Link` | 5 | Link externo de apoio. |
| `Outro` | 6 | Material de apoio que nao se encaixa nos tipos anteriores. |

## Campos De Categoria

Categoria nao e enum no backend. Os DTOs retornam:

- `categoriaConteudoId`: `Guid` da categoria, ou `null` quando o conteudo/formacao estiver sem categoria.
- `categoria`: nome textual da categoria, ou `null` quando nao houver categoria.

## Listagem

`GET /api/conteudos` retorna uma resposta paginada dentro do envelope padrao:

```json
{
  "success": true,
  "data": {
    "itens": [
      {
        "id": "11111111-1111-1111-1111-111111111111",
        "titulo": "Formacao sobre Liturgia",
        "slug": "formacao-sobre-liturgia",
        "resumo": "Introducao pastoral ao tema.",
        "tipo": 1,
        "origem": 1,
        "urlThumbnail": "https://img.youtube.com/vi/exemplo/hqdefault.jpg",
        "duracaoMinutos": 42,
        "categoriaConteudoId": null,
        "categoria": null,
        "destaque": true,
        "publicadoEm": "2026-06-18T12:00:00Z"
      }
    ],
    "pagina": 1,
    "tamanhoPagina": 12,
    "totalItens": 1,
    "totalPaginas": 1
  },
  "message": null,
  "errors": null
}
```

Filtros aceitos:

- `busca`: texto livre.
- `categoriaSlug`: slug da categoria.
- `tipo`: valor numerico de `TipoConteudo`.
- `pagina`: pagina iniciando em `1`.
- `tamanhoPagina`: entre `1` e `50`.

## Detalhe

`GET /api/conteudos/{slug}` retorna:

```json
{
  "success": true,
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "titulo": "Formacao sobre Liturgia",
    "slug": "formacao-sobre-liturgia",
    "descricao": "Descricao completa da formacao.",
    "resumo": "Introducao pastoral ao tema.",
    "tipo": 1,
    "origem": 1,
    "url": "https://www.youtube.com/watch?v=exemplo",
    "urlThumbnail": "https://img.youtube.com/vi/exemplo/hqdefault.jpg",
    "duracaoMinutos": 42,
    "categoriaConteudoId": null,
    "categoria": null,
    "publicado": true,
    "destaque": true,
    "ordem": 1,
    "publicadoEm": "2026-06-18T12:00:00Z",
    "materiaisApoio": [
      {
        "id": "33333333-3333-3333-3333-333333333333",
        "titulo": "Roteiro de estudo",
        "descricao": "PDF para acompanhar a formacao.",
        "tipo": 1,
        "url": "https://drive.google.com/file/d/exemplo/view",
        "ordem": 1
      }
    ],
    "favorito": false,
    "concluido": false
  },
  "message": null,
  "errors": null
}
```

## Admin De Conteudos

Endpoints administrativos exigem role `Admin` e CSRF para escrita.

- `GET /api/admin/conteudos?busca=&categoriaSlug=&tipo=&publicado=&pagina=1&tamanhoPagina=20`
- `GET /api/admin/conteudos/{id}`
- `POST /api/admin/conteudos`
- `PUT /api/admin/conteudos/{id}`
- `PATCH /api/admin/conteudos/{id}/publicacao`
- `DELETE /api/admin/conteudos/{id}`

Campos obrigatorios para criar/editar:

- `titulo`
- `tipo`
- `origem`
- `url`

Campos opcionais:

- `descricao`
- `resumo`
- `urlThumbnail`
- `duracaoMinutos`
- `categoriaConteudoId`
- `publicado`
- `destaque`
- `ordem`
- `materiaisApoio`

Regras do `POST /api/admin/conteudos`:

- se `publicado` for omitido, o conteudo nasce publicado (`true`);
- se `publicado: false` for enviado explicitamente, o conteudo nasce como rascunho;
- `categoriaConteudoId` pode ser `null` ou omitido;
- materiais de apoio sao apenas links externos `http/https`; nao ha upload;
- se `materiaisApoio[].ordem` for omitido, a API atribui a sequencia automaticamente;
- se `materiaisApoio[].ativo` for omitido, a API considera `true`.

Exemplo minimo:

```json
{
  "titulo": "Pregacao",
  "tipo": 1,
  "origem": 1,
  "url": "https://youtu.be/exemplo"
}
```

Exemplo com material externo:

```json
{
  "titulo": "Formacao sobre vida de oracao",
  "tipo": 1,
  "origem": 1,
  "url": "https://www.youtube.com/watch?v=exemplo",
  "categoriaConteudoId": null,
  "materiaisApoio": [
    {
      "titulo": "Roteiro",
      "tipo": 1,
      "url": "https://drive.google.com/file/d/exemplo/view"
    }
  ]
}
```

## Observacoes Para O Frontend

- `tipo`, `origem` e `materiaisApoio[].tipo` devem ser interpretados como numeros.
- `categoria` ja e textual e pode ser `null`; nao ha `categoriaNome` separado.
- Campos futuros auxiliares de nome, se adicionados, devem ser tratados como complementares e nao substitutos dos campos numericos.
