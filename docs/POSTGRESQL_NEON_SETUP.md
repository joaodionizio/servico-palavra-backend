# PostgreSQL / Neon Setup

O backend esta preparado para alternar provider por configuracao:

```bash
DATABASE_PROVIDER=postgresql
ConnectionStrings__DefaultConnection="<connection-string-neon>"
```

Para SQLite local:

```bash
DATABASE_PROVIDER=sqlite
ConnectionStrings__DefaultConnection="Data Source=servico-palavra-dev.db"
```

Passos para validar Neon:

1. Criar banco Neon/PostgreSQL.
2. Configurar `DATABASE_PROVIDER=postgresql`.
3. Configurar `ConnectionStrings__DefaultConnection` fora do repositorio.
4. Rodar `dotnet ef database update --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Api`.
5. Subir a API em `ASPNETCORE_ENVIRONMENT=Production`.
6. Validar `GET /health`, register/login/logout, plano biblico e uma escrita admin.
7. Rodar suite opcional quando `TEST_POSTGRES_CONNECTION_STRING` existir.

Status: nao validado contra Neon real nesta execucao porque nenhuma connection string real foi fornecida.
