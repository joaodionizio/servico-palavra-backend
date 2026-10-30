# Story E4-BE-01 — Observabilidade de performance sem custo adicional

Status: done  
Prioridade: P1

## Objetivo
Dar visibilidade prática sobre cold start, bootstrap do banco e requisições lentas usando apenas recursos nativos da aplicação e do provedor atual, sem contratar APM, filas externas ou serviços adicionais.

## Implementação
- `RequestTimingMiddleware` mede duração de cada requisição;
- respostas expõem `Server-Timing: app;dur=...` para inspeção no navegador e smoke tests;
- requisições acima do limiar configurável geram log estruturado de warning;
- o limiar padrão é 1000 ms e pode ser ajustado por `Observability__SlowRequestThresholdMs`;
- bootstrap/migrations/seed do banco agora têm duração registrada em log;
- o tempo total até a aplicação ficar pronta também é registrado;
- nenhum query string, token ou credencial é escrito pelos novos logs;
- nenhuma dependência externa foi adicionada.

## Evidência
O teste `ObservabilityTests.Health_response_exposes_server_timing_without_external_service` valida o header em `/health` usando a factory de integração real.

GitHub Actions `Backend Verify` run `40983367395`: `success` após restore, build e toda a suíte de testes.

## Review
APPROVED.

Não há alteração de contrato funcional nem custo recorrente. O middleware envolve o tratamento global de exceções, permitindo registrar também a duração de respostas de erro tratadas.

## Definition of Done
Atendida. A aplicação agora possui baseline de cold-start/request timing auditável usando somente logs e headers nativos.
