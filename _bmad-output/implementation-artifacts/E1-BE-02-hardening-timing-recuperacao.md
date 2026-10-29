# Story E1-BE-02 — Hardening de timing na recuperação de senha

Status: backlog  
Prioridade: P0

## Objetivo
Eliminar a diferença de tempo observável entre solicitar recuperação para uma conta existente e para um e-mail inexistente, sem adicionar serviço externo ou custo recorrente.

## Contexto
A resposta pública de `POST /api/auth/password/forgot` já é indistinguível em conteúdo/status. Porém, para conta válida, o fluxo atual gera token e aguarda envio SMTP dentro da própria requisição; para conta inexistente retorna imediatamente. Um atacante com muitas medições pode tentar inferir existência de conta por latência.

## Estratégia preferida
Separar o envio do e-mail da resposta HTTP usando fila **in-process**:
- `Channel<T>` bounded em memória;
- `BackgroundService` consumidor;
- endpoint gera token/payload e enfileira rapidamente;
- resposta HTTP não aguarda conexão SMTP;
- nenhum Redis, RabbitMQ, serviço de fila ou infraestrutura paga.

A fila é best-effort: como não há persistência externa nesta etapa, uma reinicialização entre enqueue e envio pode perder uma mensagem. O comportamento deve ser documentado e o usuário sempre pode solicitar outro link.

## Segurança
- payload enfileirado pode conter token somente em memória; nunca persistir/logar token;
- fila deve ser bounded para impedir consumo ilimitado de memória;
- quando cheia, política deve ser segura e não revelar existência de conta;
- falha SMTP deve ser logada sem token/link completo/credenciais;
- endpoint deve manter exatamente a mesma resposta genérica.

## Critérios de aceitação
1. `POST /api/auth/password/forgot` não aguarda `SmtpClient.SendMailAsync`.
2. Solicitação para conta válida e inexistente segue o mesmo caminho HTTP público e retorna antes do envio SMTP.
3. Existe fila bounded com capacidade explícita e comportamento definido quando cheia.
4. Background worker consome e-mails e chama o sender real fora do request.
5. Token/URL completa não são registrados em logs.
6. Teste prova que o endpoint retorna mesmo quando o sender consumidor está bloqueado/lento.
7. Teste prova que conta inexistente continua sem enfileirar e-mail, mas resposta pública permanece idêntica.
8. Falha no worker não derruba a API nem altera resposta já enviada.
9. Nenhum serviço externo/custo recorrente é introduzido.
10. `bash tools/verify.sh` permanece verde.

## Fora de escopo
- fila persistente/durável;
- retries sofisticados com banco;
- provedor transacional pago;
- telemetria externa;
- mudar o contrato do frontend.

## Definition of Done
- critérios 1–10 atendidos;
- risco residual de perda em restart documentado;
- verify verde;
- review independente aprovado.
