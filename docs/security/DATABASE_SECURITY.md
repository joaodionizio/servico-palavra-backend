# Database Security

## Providers

- Desenvolvimento/testes: `DATABASE_PROVIDER=sqlite`.
- Producao: `DATABASE_PROVIDER=postgresql`.
- Connection string vem de `ConnectionStrings__DefaultConnection` ou de provider seguro do ambiente.

SQLite nao deve ser usado como banco persistente no Render Free.

## PostgreSQL/Neon

Use Neon Free inicialmente com:

```text
DATABASE_PROVIDER=postgresql
ConnectionStrings__DefaultConnection=<configurar no provedor, nunca no repositorio>
```

## Controles

- EF Core/LINQ; nao foi encontrado uso de `FromSqlRaw` ou `ExecuteSqlRaw`.
- Indices unicos definidos para e-mail, slugs e pares usuario/recurso.
- Ownership aplicado nos repositories de favoritos, progresso e plano biblico.
- Migrations versionadas.

## Riscos pendentes

- Cascades relacionados a dados historicos devem ser revisados antes de producao.
- Backups/exportacao do Neon precisam ser definidos operacionalmente.
- Migrations PostgreSQL devem ser testadas em uma instancia Neon autorizada.

## Regras

- Nunca concatenar input de usuario em SQL.
- Limitar paginas em endpoints de listagem quando houver volume.
- Nao expor erros do banco ao usuario final.
