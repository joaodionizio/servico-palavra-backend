# Story E1-BE-02 — Hardening de timing na recuperação de senha

Status: done  
Prioridade: P0

## Objetivo
Reduzir o side-channel de timing da recuperação de senha, retirando o envio SMTP da requisição HTTP sem adicionar serviço externo ou custo recorrente.

## Implementação
- criada `IPasswordResetEmailQueue` na Application;
- criado `PasswordResetEmailJob`, contendo apenas o payload necessário em memória;
- criada `PasswordResetEmailQueue` com `Channel<T>` bounded de capacidade 100;
- `FullMode = Wait` combinado com `TryWrite` faz a fila recusar imediatamente quando cheia, sem crescimento ilimitado de memória;
- fila usa um único reader e múltiplos writers;
- criado `PasswordResetEmailWorker : BackgroundService`;
- worker resolve `IPasswordResetEmailSender` em scope separado e envia fora do request;
- `AuthService.RequestPasswordResetAsync` gera token e apenas tenta enfileirar; não aguarda SMTP;
- quando a fila está cheia, o evento é logado sem e-mail, token ou URL, e a resposta pública continua genérica.

## Comportamento operacional
A fila é intencionalmente **best-effort e in-process**. Uma reinicialização da aplicação entre enqueue e envio pode perder a mensagem pendente; não há persistência externa nesta etapa. O usuário pode solicitar um novo link, e nenhuma infraestrutura paga/externa foi introduzida.

## Testes
- testes existentes de recuperação foram adaptados para aguardar a entrega assíncrona sem assumir execução imediata do worker;
- `PasswordRecoveryTimingTests` bloqueia deliberadamente o sender e comprova que `POST /api/auth/password/forgot` responde enquanto o sender continua bloqueado;
- `PasswordResetEmailQueueTests` preenche 100 itens e comprova que o 101º é recusado;
- conta inexistente continua sem produzir e-mail e mantém a mesma resposta pública;
- toda a suíte de recuperação original permanece verde.

## Verificação
`Backend Verify` run `40971818401`: **success**.

O gate executou restore, build Release e toda a suíte de testes, incluindo os testes de timing e capacidade da fila.

## Code review adversarial
Resultado: **APPROVED**.

Achados bloqueantes: nenhum.

Revisão confirmou:
- SMTP não participa mais do tempo de resposta do endpoint;
- fila é bounded e não cresce indefinidamente;
- token permanece apenas em memória até o sender consumi-lo;
- logs de fila cheia/worker não incluem token ou URL completa;
- exceção no worker é capturada e não derruba a API;
- o contrato HTTP do frontend não mudou;
- não foi adicionado Redis, RabbitMQ, banco de fila ou serviço pago.

Risco residual aceito e documentado:
- por ser in-process e não persistente, uma reinicialização pode perder um e-mail pendente. Isso é compatível com a restrição atual de zero custo adicional e pode ser revisitado caso o produto exija garantia de entrega mais forte.

## Critérios de aceitação
1. endpoint não aguarda SMTP — **atendido/testado**;
2. resposta ocorre antes do envio terminar — **atendido/testado**;
3. fila bounded com comportamento definido — **atendido/testado**;
4. worker em background — **atendido**;
5. sem token/URL em logs — **atendido/revisado**;
6. sender bloqueado não bloqueia endpoint — **atendido/testado**;
7. conta inexistente preserva resposta genérica — **atendido/testado**;
8. falha do worker não derruba API — **atendido por isolamento/catch**;
9. sem custo externo — **atendido**;
10. verify verde — **atendido**.

## Definition of Done
Concluída e aprovada.
