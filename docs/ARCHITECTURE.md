# Arquitetura

O backend V2 do Servico da Palavra e uma plataforma catolica de formacao biblica, espiritual e pastoral. O Plano Biblico e um modulo importante, mas a arquitetura deve continuar preparada para conteudos, trilhas, progresso, favoritos, administracao e permissoes.

A solucao segue uma arquitetura em camadas simples:

- `ServicoPalavra.Domain`: entidades e enums. Nao referencia nenhum projeto.
- `ServicoPalavra.Application`: contratos, DTOs e casos de uso. Referencia apenas Domain.
- `ServicoPalavra.Infrastructure`: EF Core, SQLite/PostgreSQL, repositories, Identity stores, importadores e seeds. Referencia Application e Domain.
- `ServicoPalavra.Api`: controllers, autenticacao, autorizacao, CORS, CSRF, Swagger, health check, rate limiting e middleware global.

## Referencias Entre Projetos

```text
Api -> Application, Infrastructure
Application -> Domain
Infrastructure -> Application, Domain
Domain -> nenhuma
```

As regras de fluxo ficam em services da Application, evitando concentrar comportamento nos controllers. A Infrastructure implementa os contratos de persistencia e servicos tecnicos.

## Stack

- ASP.NET Core / .NET 8 LTS.
- Entity Framework Core 8.
- PostgreSQL/Neon em producao.
- SQLite local para desenvolvimento/testes, quando configurado.
- ASP.NET Core Identity.
- Cookie HttpOnly.
- CSRF por antiforgery para escritas.

## Autenticacao E Perfis

A API usa ASP.NET Core Identity com cookie HttpOnly e antiforgery para escritas. Roles do Identity protegem endpoints administrativos.

Roles iniciais:

- Usuario
- Admin
- Coordenador

O `UsuarioId` de fluxos autenticados deve ser derivado da sessao no backend. O frontend nao deve enviar `UsuarioId`, roles internas, posicao biblica ou estado administrativo.

## Modulos Da Plataforma

- Auth
- Users
- Roles
- Dashboard
- Conteudos/Formacoes
- Categorias
- Trilhas
- Favoritos
- ProgressoConteudo
- BibleBase
- BiblePlans
- ReadingProgress
- Admin

## Plano Biblico E Base Biblica

`BaseBiblica` e o catalogo unico de capitulos biblicos. Ela guarda metadados como livro, capitulo, testamento, quantidade de versiculos, peso de leitura e status ativo.

`BaseBiblica` nao deve ser duplicada para representar repeticoes pastorais. A repeticao de livros/capitulos pertence a uma camada de sequencia pastoral do Plano Biblico.

A geracao do plano usa:

1. `BaseBiblica` ativa como fonte real de capitulos.
2. Sequencia pastoral V2 como ordem de leitura.
3. `PesoLeitura` para equilibrar a carga diaria.
4. `ProgressoLeitura` e `PosicaoBiblicaUsuario` para continuidade.

A regra pastoral nao deve ser alterada sem justificativa documentada e validacao manual.

## Persistencia

O banco de producao e PostgreSQL/Neon. SQLite pode ser usado localmente ou em testes quando configurado.

Migrations, connection strings reais e comandos operacionais estao documentados em `DATABASE.md` e `DEPLOY.md`.

Secrets e connection strings reais devem ficar em variaveis de ambiente ou no provedor de deploy, nunca versionados.

## Midia

A plataforma e formacao-first. Videos e audios sao conteudos formativos, mas a API nao deve armazenar arquivos de midia no servidor nem no banco nesta fase.

A estrategia atual e cadastrar metadados e URLs externas:

- videos por YouTube ou Google Drive;
- audios por link externo, preferencialmente Google Drive;
- thumbnails por URL;
- materiais de apoio por URL externa.

Upload direto para storage externo, como Cloudflare R2, Azure Blob, S3 ou equivalente, e uma possibilidade futura. Essa decisao nao esta implementada nesta fase e nao deve gerar migration sem requisito claro.

Detalhes estao em `MEDIA_STRATEGY.md`.

## Fronteiras Importantes

- Controllers devem ser finos.
- Services da Application devem conter fluxo de negocio.
- Infrastructure deve conter EF Core, repositories, seeds e importadores tecnicos.
- Domain deve continuar sem dependencia de ASP.NET Core, EF Core ou Infrastructure.
- Novos modulos devem crescer seguindo os padroes existentes antes de criar novas abstracoes.
