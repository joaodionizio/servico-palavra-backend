# Arquitetura

O backend segue uma arquitetura limpa simples:

- `ServicoPalavra.Domain`: entidades e enums. Nao referencia nenhum projeto.
- `ServicoPalavra.Application`: contratos, DTOs e casos de uso. Referencia apenas Domain.
- `ServicoPalavra.Infrastructure`: EF Core, SQLite/PostgreSQL, repositories, Identity stores e seeds. Referencia Application e Domain.
- `ServicoPalavra.Api`: controllers, autenticacao, autorizacao, CORS, Swagger, health check e middleware global.

Referencias:

```text
Api -> Application, Infrastructure
Application -> Domain
Infrastructure -> Application, Domain
Domain -> nenhuma
```

As regras de fluxo ficam em services da Application, evitando concentrar comportamento nos controllers. A Infrastructure implementa os contratos de persistencia e servicos tecnicos.

## Autenticacao e perfis

A API usa ASP.NET Core Identity com cookie HttpOnly e antiforgery para escritas. Roles do Identity protegem endpoints administrativos.

Perfis iniciais:

- Usuario
- Admin
- Coordenador

## Plano biblico

O modulo de plano biblico gera dias/capitulos a partir de `BaseBiblica` ordenada por `Ordem`. A base completa deve ser importada de fonte pastoral revisada; o repositorio inclui somente fixture representativa em testes e documentacao de importacao.
