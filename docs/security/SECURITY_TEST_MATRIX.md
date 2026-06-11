# Security Test Matrix

| Area | Teste | Status |
|------|-------|--------|
| Auth | Login valido gera cookie e nao retorna hash/token | IMPLEMENTADO |
| Auth | Login invalido usa resposta generica | IMPLEMENTADO |
| Auth | Rota protegida sem auth retorna 401 | IMPLEMENTADO |
| Auth | Rate limit login/cadastro | IMPLEMENTADO no runtime; teste dedicado pendente |
| Auth | Lockout persistente | PENDENTE Identity |
| Authorization | Usuario comum bloqueado em admin | IMPLEMENTADO |
| Authorization | Admin aceito em admin | IMPLEMENTADO |
| Authorization | Coordenador bloqueado onde nao ha permissao | PENDENTE |
| Ownership | Usuario A nao le plano de B por GUID | IMPLEMENTADO |
| Conteudos | URL `javascript:` rejeitada | IMPLEMENTADO |
| Conteudos | Host indevido para YouTube rejeitado | IMPLEMENTADO |
| Favoritos | A nao remove favorito de B | PENDENTE |
| Progresso | A nao conclui conteudo por B | PENDENTE |
| Plano biblico | A nao conclui dia de B | PENDENTE |
| Plano biblico | Continuidade usa ultima ordem + 1 | PENDENTE geracao completa |
| Plano biblico | Troca transacional | IMPLEMENTADO parcialmente; teste de falha pendente |
| CORS | Origem indevida falha | PENDENTE |
| Erros | Sem stack trace/connection string para usuario | IMPLEMENTADO |
| CSRF | POST sem antiforgery falha | IMPLEMENTADO |
