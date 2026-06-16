# Security Report

## 1. Resumo executivo

Foi realizada auditoria no backend ASP.NET Core. A base foi endurecida em pontos criticos: Identity/cookie auth, CSRF, lockout, roles, admin bootstrap protegido, PostgreSQL habilitado por provider, rate limiting, headers, HSTS, validacao de URL externa, transacoes no plano biblico e testes de autorizacao/ownership.

## 2. Status geral

Nao declarar como pronto para producao ate validar PostgreSQL/Neon real, CORS com dominios finais e frontend.

## 3. Arquitetura analisada

Clean architecture com Api, Application, Domain e Infrastructure.
Runtime alvo atual: .NET 8 LTS com EF Core 8.

## 4. Dados pessoais tratados

Nome, e-mail, hash de senha, perfil, ultimo acesso, favoritos, progresso, plano biblico e posicao pastoral.

## 5-8. Riscos

| ID | Severidade | Area | Problema | Impacto | Correcao | Status |
|----|------------|------|----------|---------|---------|--------|
| R-001 | CRITICAL | Secrets | JWT secret versionado | Tokens falsificaveis | JWT removido; cookie Identity usado | CORRIGIDO |
| R-002 | CRITICAL | Admin | Senha admin padrao | Tomada de conta admin | Bootstrap somente por env; sem senha padrao | CORRIGIDO |
| R-003 | HIGH | Auth | Sem Identity/lockout/security stamp | Revogacao/lockout fracos | Identity + cookie + lockout | CORRIGIDO |
| R-004 | HIGH | Banco | SQLite em producao | Perda de dados no Render Free | Provider PostgreSQL por env | CORRIGIDO |
| R-005 | HIGH | Ownership | GUID alheio em plano | Vazamento de plano | Filtro por owner + teste | CORRIGIDO |
| R-006 | HIGH | Conteudos | URLs maliciosas | XSS/abuso externo | Validador de URL/host | CORRIGIDO |
| R-007 | MEDIUM | Rate limit | Login sem limite | Brute force | Rate limiting | CORRIGIDO |
| R-008 | MEDIUM | Frontend | Sem auditoria de localStorage/CSP/cookies | Risco desconhecido | Nao aplicavel ao repo atual | PENDENTE |
| R-009 | MEDIUM | Plano | Geracao pastoral incompleta | Estado funcional parcial | Algoritmo/fixture implementados; seed completa depende de fonte | PARCIAL |

## 9. Correcoes aplicadas

- Remocao de secrets versionados.
- Configuracao PostgreSQL/SQLite por ambiente.
- Admin bootstrap seguro em producao.
- Rate limiting.
- HTTPS/HSTS/headers.
- Validacao de URL externa.
- Identity cookie auth, roles, logout, CSRF e lockout.
- Transacoes em criacao, alteracao e conclusao de plano.
- Testes unitarios e de integracao de seguranca.

## 10. Correcoes pendentes

- Auditar frontend.
- Importar base biblica pastoral completa a partir de fonte revisada.
- Ampliar testes A x B para favoritos, progresso, dias e dashboard.
- Executar validacao real PostgreSQL/Neon com `DATABASE_PROVIDER=postgresql` e `ConnectionStrings__DefaultConnection` disponiveis no ambiente local.

## 11-13. Testes

Validacao executada em 2026-06-16:

- Migracao controlada de .NET 10 / EF Core 10 para .NET 8 LTS / EF Core 8.
- `dotnet restore ServicoPalavra.sln`: passou.
- `dotnet build ServicoPalavra.sln`: passou, 0 warnings, 0 erros.
- `dotnet test ServicoPalavra.sln`: passou, 13 testes no total.
- Baseline PostgreSQL/Npgsql atual: `20260616004919_InitialIdentityBaseline`.
- Todos os campos `DateTime`/`DateTime?` do modelo estao mapeados como `timestamp with time zone`.
- Nao ha uso de `DateTime.Now` nas regras de persistencia.
- `dotnet list ServicoPalavra.sln package --vulnerable --include-transitive`: nenhum pacote vulneravel conhecido nos projetos da solucao.
- Validacao PostgreSQL/Neon real: pendente de reset manual do schema parcial no Neon e execucao manual de `dotnet ef database update`.

Validacao executada em 2026-06-15:

- `dotnet restore ServicoPalavra.sln`: passou.
- `dotnet build ServicoPalavra.sln --no-restore`: passou, 0 warnings, 0 erros.
- `dotnet test ServicoPalavra.sln --no-build`: passou, 13 testes no total.
- `dotnet ef migrations list --project src/ServicoPalavra.Infrastructure --startup-project src/ServicoPalavra.Infrastructure --no-build`: listou a baseline disponivel naquele momento.
- Primeiro `dotnet ef migrations list` sem `--no-build` foi interrompido apos ficar sem saida util alem de `Build started...`; a repeticao com `--no-build` passou.
- Validacao PostgreSQL/Neon real: nao executada porque `DATABASE_PROVIDER` e `ConnectionStrings__DefaultConnection` nao estavam visiveis nesta sessao.
- `DATABASE_PROVIDER=postgresql` sem connection string foi validado como falha segura: o `DbContext` nao e criado.

Smoke tests cobertos pela suite de integracao atual:

- Login retorna cookie `__Host-ServicoPalavra` sem token nem hash de senha no body.
- Login invalido usa resposta generica.
- Lockout apos tentativas invalidas.
- Rota protegida exige autenticacao.
- Usuario comum recebe 403 em endpoint admin e admin consegue criar categoria.
- Usuario nao le plano biblico de outro usuario mesmo com GUID real.
- Plano biblico gera ordem pastoral, conclui dia, continua de onde parou, recomeca do inicio e preserva historico.
- Falha de continuacao preserva plano ativo.
- URL de conteudo maliciosa e rejeitada.
- Resposta de erro nao expoe stack trace nem connection string.

Smoke tests reais contra PostgreSQL/Neon: pendentes ate as variaveis reais estarem disponiveis na sessao.

Auditoria de dependencias executada por projeto:

- `ServicoPalavra.Api`: nenhum pacote vulneravel conhecido.
- `ServicoPalavra.Application`: nenhum pacote vulneravel conhecido.
- `ServicoPalavra.Domain`: nenhum pacote vulneravel conhecido.
- `ServicoPalavra.Infrastructure`: nenhum pacote vulneravel conhecido.

## 14. Bloqueadores de deploy

- Frontend nao auditado.
- Banco PostgreSQL/Neon ainda nao validado em ambiente real.
- Smoke tests UsuarioA x UsuarioB contra IDs reais do Neon ainda nao executados.

## 15. Decisoes humanas necessarias

- Dominios reais de frontend/API.
- Processo de criacao do primeiro admin.

## 16. Checklist antes de producao

Ver `PRE_DEPLOY_SECURITY_CHECKLIST.md`.

## 17. Conclusao honesta

O backend ficou mais seguro, mas nao esta automaticamente seguro para producao. As protecoes principais de backend foram fortalecidas e testadas; ainda ha decisoes arquiteturais e auditoria de frontend pendentes.
