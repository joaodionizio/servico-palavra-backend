# Release Blockers

| ID | Severidade | Area | Bloqueador | Status | Observacao |
|----|------------|------|------------|--------|------------|
| BLK-001 | CRITICAL | Secrets | Segredo JWT versionado em `appsettings.json` | CORRIGIDO | JWT removido da autenticacao. |
| BLK-002 | HIGH | Admin | Senha padrao do admin hardcoded | CORRIGIDO | Nenhum admin e criado sem `INITIAL_ADMIN_*`. |
| BLK-003 | HIGH | Banco | SQLite como banco de producao no Render Free | CORRIGIDO | `DATABASE_PROVIDER=postgresql` e `ConnectionStrings__DefaultConnection` suportados via Npgsql. |
| BLK-004 | HIGH | Admin | Endpoints admin sem role Admin | CORRIGIDO | Endpoints admin usam `[Authorize(Roles = "Admin")]`; testes cobrem anonimo/usuario/admin parcialmente. |
| BLK-005 | HIGH | Ownership | Plano biblico acessivel por GUID alheio | CORRIGIDO | Repository filtra por `usuarioId`; teste A x B adicionado. |
| BLK-006 | HIGH | Externos | URL `javascript:` e host indevido aceitos em conteudos | CORRIGIDO | Validador backend de URL/host adicionado. |
| BLK-007 | MEDIUM | Swagger | Swagger publico em producao sem decisao explicita | CORRIGIDO | Swagger habilitado apenas em Development. |
| BLK-008 | MEDIUM | HTTPS | Ausencia de HSTS/headers basicos | CORRIGIDO | HSTS fora de Development/Testing e headers basicos adicionados. |
| BLK-009 | MEDIUM | Rate limit | Login/cadastro sem rate limit | CORRIGIDO | Politica `auth` adicionada. |
| BLK-010 | HIGH | Auth | Ausencia de ASP.NET Core Identity, lockout persistente, security stamp e revogacao | CORRIGIDO | Identity cookie auth implementado. |
| BLK-011 | HIGH | Frontend | Frontend/cookies/CSP/localStorage nao auditaveis neste repo | PENDENTE | Repo atual e somente backend. |

Produzir release publico deve aguardar BLK-011, validacao PostgreSQL/Neon real e dominios finais de CORS/cookies.
