# Threat Model STRIDE

| Area | Ativo | Ameaca | Impacto | Prob. | Sev. | Mitigacao existente | Mitigacao necessaria | Teste |
|------|-------|--------|---------|-------|------|---------------------|----------------------|-------|
| Login | Conta | Brute force/credential stuffing | Conta comprometida | Media | HIGH | Identity lockout, rate limit | MFA futura | Login invalido/rate limit |
| Cadastro | Conta | Enumeracao/abuso | Spam/privacidade | Media | MEDIUM | Resposta simples, rate limit | Validacao forte, captcha se abuso | Cadastro repetido |
| Recuperacao senha | Conta | Token roubado | Conta comprometida | Baixa | HIGH | Nao implementado | Identity tokens curtos | Pendente |
| Sessao | Cookie | Roubo/replay | Acesso indevido | Media | HIGH | HttpOnly, Secure prod, CSRF, logout | MFA e device/session management futuro | 401/403 |
| Cookies | Sessao | CSRF/cookie theft | Escrita indevida | Media | HIGH | HttpOnly/Secure/CSRF | Validar dominios reais | CSRF tests |
| Endpoints privados | Dados usuario | Acesso sem auth | Vazamento | Media | HIGH | `[Authorize]` | Testes por endpoint | Sem auth 401 |
| Endpoints admin | Admin | Usuario comum chama admin | Alteracao indevida | Media | HIGH | Role Admin | Auditoria admin | Usuario 403 |
| Conteudos | Catalogo | URL maliciosa/XSS | XSS/tabnabbing | Media | HIGH | DTOs, URL validator | Sanitizacao frontend | URL maliciosa |
| Favoritos | Dados usuario | GUID de outro usuario | Vazamento/alteracao | Media | HIGH | UsuarioId da sessao | Mais testes CRUD cruzado | Pendente parcial |
| Progresso conteudo | Dados usuario | Concluir por outro usuario | Integridade | Media | HIGH | UsuarioId da sessao | Teste cruzado dedicado | Pendente parcial |
| Trilhas | Catalogo | Admin indevido | Integridade | Media | HIGH | Admin role | Auditoria | Admin tests |
| Plano biblico | Plano usuario | Ler plano alheio | Vazamento espiritual/pessoal | Media | HIGH | Repository filtra owner | Mais testes historico/dias | A x B |
| Historico planos | Historico | Vazamento entre usuarios | Privacidade | Media | HIGH | Filtro por owner | Endpoints historico dedicados | Pendente |
| Continuidade | Posicao | Manipular ordem | Integridade pastoral | Media | HIGH | Ordem derivada do backend | Geracao completa + testes | Pendente parcial |
| Alteracao duracao | Plano | Estado parcial | Corrupcao | Media | HIGH | Transacao na troca | Teste falha transacional | Pendente |
| Recomeço | Plano | Plano origem alheio | Vazamento | Baixa | MEDIUM | PlanoOrigemId nao vem do client | Teste dedicado | Pendente |
| Dashboard | Resumo | Agregado alheio | Vazamento | Media | HIGH | UsuarioId da sessao | Teste A x B | Pendente |
| Google Drive | Links | Expor privado | Vazamento | Media | HIGH | Host allowlist | Permissoes Drive revisadas | Manual |
| YouTube | Embed/link | Iframe arbitrario | XSS/clickjacking | Media | MEDIUM | Host allowlist | CSP frontend | Manual |
| Logs | PII/secrets | Senha/token/cookie em log | Vazamento | Media | HIGH | Middleware generico | Politica logging + masking | Manual |
| Deploy | Ambiente | Dev config em prod | Vazamento | Media | HIGH | Secret externo requerido | Checklist Render/Neon | Pre-deploy |
| Banco remoto | Dados | Connection string vazada | DB comprometido | Media | CRITICAL | Env vars | Rotacao/least privilege | Secrets audit |
| SQLite local | Dados dev | Commit de DB | Vazamento | Media | MEDIUM | `.gitignore` | Remover DBs locais | Git status |
| Variaveis ambiente | Secrets | Exposicao no repo/log | Vazamento | Media | HIGH | `.env.example` | User-secrets/Render env | Git grep |
| Migrations | Schema | Cascade indevido | Perda historico | Baixa | MEDIUM | Restrict em origem/trilhas | Revisar cascades usuario | Pendente |
| Seed admin | Conta admin | Senha conhecida | Compromisso total | Media | CRITICAL | Env-only em producao | Rotina manual e troca senha | Config test |
