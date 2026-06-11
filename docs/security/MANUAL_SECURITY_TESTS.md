# Manual Security Tests

Execute somente localmente ou em ambiente explicitamente autorizado.

- Trocar GUID em `/api/planos-biblicos/{id}` de outro usuario e confirmar 404/403.
- Trocar GUID em `/api/planos-biblicos/{id}/dias`.
- Trocar `diaId` em conclusao de leitura.
- Acessar endpoints privados sem cookie.
- Usar sessao de Usuario em endpoint `/api/admin/*`.
- Enviar `PerfilId`, `UsuarioId` ou role em payloads publicos e confirmar que sao ignorados.
- Enviar `javascript:alert(1)` em URL de conteudo.
- Enviar `https://evil.example/watch` com origem YouTube.
- Enviar HTML/script em titulo, descricao e resumo; confirmar escape no frontend quando existir.
- Repetir login invalido ate 429.
- Chamar API com origem CORS nao permitida.
- Confirmar Swagger indisponivel em Production.
- Forcar erro e confirmar ausencia de stack trace.
- Conferir headers `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`, `X-Frame-Options`.
- Validar no deploy que HTTPS e HSTS estao ativos.
- Revisar permissoes reais dos arquivos do Google Drive.
