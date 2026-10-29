# Story E1-BE-01 — Recuperação segura de senha

Status: done  
Prioridade: P0

## Objetivo
Permitir que uma pessoa recupere o acesso à conta com um link temporário enviado por e-mail, sem expor se um endereço está cadastrado e sem adicionar custo recorrente obrigatório.

## Implementação
- adicionados `ForgotPasswordRequest` e `ResetPasswordRequest`;
- `IAuthService`/`AuthService` agora solicitam e concluem redefinição;
- token gerado por `UserManager.GeneratePasswordResetTokenAsync`;
- token transportado em Base64Url, sem persistência própria;
- reset realizado exclusivamente por `UserManager.ResetPasswordAsync`;
- endpoints públicos:
  - `POST /api/auth/password/forgot`;
  - `POST /api/auth/password/reset`;
- ambos permanecem protegidos pelo antiforgery global de writes e pelo limiter `auth`;
- token provider configurado com validade de 1 hora;
- abstração `IPasswordResetEmailSender` na Application;
- implementação SMTP configurável na Infrastructure, sem biblioteca/serviço pago obrigatório;
- `PasswordResetLinkBuilder` gera e valida de forma testável o link para `/redefinir-senha`;
- nenhuma migration criada;
- documentação de API, deploy e `.env.example` atualizada sem secrets reais.

## Segurança
- resposta de solicitação é idêntica para e-mail existente e inexistente;
- conta inativa/inexistente não recebe e-mail, sem diferença pública na resposta;
- userId inexistente e token inválido usam o mesmo erro público de link inválido/expirado;
- token não aparece em logs, snapshots ou resposta da API;
- senha nova fora da política retorna erro de política, sem consumir um token ainda válido;
- token reutilizado após reset bem-sucedido falha;
- senha antiga deixa de autenticar depois do reset;
- senha nova autentica normalmente;
- cadastro deixou de impor o teto manual inseguro de 8 caracteres e passou a seguir a mesma política do Identity.

## Configuração operacional
Variáveis documentadas:
- `FRONTEND_BASE_URL`;
- `SMTP_HOST`;
- `SMTP_PORT`;
- `SMTP_USERNAME`;
- `SMTP_PASSWORD`;
- `SMTP_FROM_ADDRESS`;
- `SMTP_FROM_NAME`;
- `SMTP_ENABLE_SSL`.

Sem configuração SMTP/base URL, a API preserva resposta pública genérica e registra apenas aviso operacional; publicação da feature exige configurar o transporte de e-mail.

## Testes automatizados
Suíte dedicada cobre:
1. resposta indistinguível para conta existente/inexistente;
2. captura de exatamente um payload de reset para conta válida;
3. token URL-safe;
4. reset válido;
5. senha antiga inválida após reset;
6. senha nova válida;
7. token não reutilizável;
8. token inválido e userId inexistente com mesmo erro público;
9. senha fraca com erro correto e token ainda utilizável;
10. cadastro com senha forte acima de 8 caracteres;
11. geração da URL final do frontend;
12. escaping dos parâmetros da URL.

## Verificação
- `Backend Verify` run `40968122400`: success após introdução da suíte principal.
- `Backend Verify` run `40968516130`: success após hardening dos erros.
- `Backend Verify` run `40968946267`: **success** após teste final do link gerado.

O gate executou restore, build Release e toda a suíte de testes.

## Code review adversarial
Resultado: **APPROVED**.

Achados encontrados e corrigidos durante review:
- decoder Base64Url precisava de parênteses no `switch` — corrigido e capturado pelo CI;
- userId inexistente tinha mensagem diferente de token inválido — uniformizado;
- falha de política de senha era confundida com link expirado — corrigida;
- geração do link SMTP não estava diretamente testável — extraída para `PasswordResetLinkBuilder` e coberta por testes.

Achados bloqueantes remanescentes: nenhum.

## Critérios de aceitação
1. resposta indistinguível para e-mail existente/inexistente — **atendido/testado**;
2. sender recebe um único reset para conta válida — **atendido/testado**;
3. link contém userId + token URL-safe e aponta ao frontend — **atendido/testado**;
4. token válido troca senha — **atendido/testado**;
5. senha antiga deixa de autenticar — **atendido/testado**;
6. token inválido/alterado não redefine — **atendido/testado**;
7. token reutilizado falha — **atendido/testado**;
8. rate limiting usa política `auth`, mais restritiva que general — **atendido**;
9. nenhum secret/token em logs/documentação — **atendido/revisado**;
10. `bash tools/verify.sh` verde — **atendido em CI**.

## Definition of Done
Concluída e aprovada. O contrato está pronto para o frontend implementar `/esqueci-senha` e `/redefinir-senha`.
