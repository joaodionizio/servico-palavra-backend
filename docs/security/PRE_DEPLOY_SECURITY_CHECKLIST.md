# Pre-Deploy Security Checklist

- [ ] SQLite nao e usado como banco persistente no Render Free
- [ ] PostgreSQL remoto persistente configurado
- [ ] Connection string fora do repositorio
- [ ] Secrets fora do repositorio
- [x] JWT removido da autenticacao
- [x] Decisao registrada: ASP.NET Core Identity
- [x] Cookies HttpOnly configurados
- [x] Cookies Secure em producao
- [x] SameSite documentado
- [x] CSRF implementado para escritas
- [ ] CORS restritivo com dominios reais
- [ ] HTTPS obrigatorio
- [ ] HSTS ativo
- [ ] CSP configurada no frontend
- [ ] `X-Content-Type-Options` configurado
- [ ] `Referrer-Policy` configurada
- [ ] `Permissions-Policy` configurada
- [ ] Sem token no localStorage no frontend
- [ ] Sem secrets `NEXT_PUBLIC_`
- [ ] Sem senha padrao de admin em producao
- [ ] Swagger revisado para producao
- [ ] Stack traces ocultos
- [ ] Logs sem dados sensiveis
- [ ] Rate limit ativo
- [ ] Endpoints admin protegidos
- [x] Ownership validado parcialmente na suite local: usuario nao le plano biblico de outro usuario
- [ ] Testes UsuarioA x UsuarioB completos contra PostgreSQL/Neon real
- [x] Migrations revisadas localmente: baseline PostgreSQL EF Core 8 `20260616004919_InitialIdentityBaseline` gerada
- [ ] Migrations aplicadas em PostgreSQL/Neon real
- [x] Dependencias auditadas por solucao em 2026-06-16
- [ ] Backup/exportacao documentado
- [ ] Ambiente Production configurado
- [ ] Google Drive revisado
- [ ] URLs externas validadas
- [ ] Documentacao atualizada

## Registro 2026-06-15

- `DATABASE_PROVIDER` e `ConnectionStrings__DefaultConnection` nao estavam visiveis nesta sessao, entao a validacao Neon real nao foi executada.
- `dotnet restore`, `dotnet build` e `dotnet test` passaram.
- `dotnet ef migrations list --no-build` listou a baseline disponivel naquele momento.
- A factory design-time do EF foi ajustada para respeitar PostgreSQL quando configurado por ambiente.
- Nenhum secret, host sensivel completo, senha, token, cookie ou connection string real foi registrado neste documento.

## Registro 2026-06-16

- Solucao migrada de .NET 10 / EF Core 10 para .NET 8 LTS / EF Core 8.
- Baseline PostgreSQL/Npgsql EF Core 8 regenerada como `20260616004919_InitialIdentityBaseline`.
- Campos `DateTime`/`DateTime?` mapeados como `timestamp with time zone`.
- `dotnet restore ServicoPalavra.sln`, `dotnet build ServicoPalavra.sln` e `dotnet test ServicoPalavra.sln` passaram.
- `dotnet list ServicoPalavra.sln package --vulnerable --include-transitive` nao encontrou pacotes vulneraveis conhecidos.
- Se o Neon tiver schema parcial de tentativa anterior e nao houver dados reais, resetar manualmente o branch/database no painel Neon antes de aplicar a baseline corrigida.
