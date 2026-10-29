# Story E3-BE-01 — Unificar timezone e calendário adaptativo do plano bíblico

Status: done  
Prioridade: P0

## Objetivo
Garantir que todas as decisões de calendário civil do plano bíblico usem o mesmo “hoje” no timezone configurado, preservando timestamps instantâneos em UTC, e concluir o comportamento adaptativo sem alterar o schema.

## Implementação
- criado `ICalendarClock`, com `UtcNow` e `Today`;
- implementação `CalendarClock` usa `TimeProvider` + `TimeZoneInfo`;
- `Calendar:TimeZone` configurável, com `America/Sao_Paulo` explícito em `appsettings.json` e fallback no código;
- timestamps instantâneos continuam UTC;
- criado `IPlanoBiblicoCalendarService` / `PlanoBiblicoCalendarService` para centralizar realinhamento;
- endpoints/serviços de plano e dashboard usam a mesma fonte de data civil;
- nenhum schema/migration foi adicionado.

## Regras implementadas
1. Dias concluídos permanecem históricos e não têm `DataPrevista` reescrita.
2. Apenas dias pendentes são redistribuídos.
3. Consulta de plano atrasado realinha os pendentes a partir de `Today` local.
4. Ao concluir, o restante dos pendentes começa em `Today + 1` local.
5. Ao desmarcar, o dia volta à sequência pendente a partir de `Today` local.
6. Datas já ocupadas por históricos concluídos são puladas, evitando colisão.
7. Agenda com datas pendentes duplicadas ou sobreposição com histórico é reparada mesmo que nenhuma data esteja atrasada.
8. `DataFimPrevista` é reconciliada com a última data efetiva do cronograma.
9. Consultas repetidas sobre agenda já válida são idempotentes.
10. Dashboard detecta atraso dentro da query de overview existente, sem adicionar round-trip no caminho normal.
11. Leitura do dashboard continua priorizando a data local de hoje e preserva o fallback para a próxima pendente.

## Critérios de aceitação — resultado
1. Virada UTC/local — atendido: teste congela `2026-10-30 00:30 UTC` e obtém `2026-10-29` em `America/Sao_Paulo`.
2. Relógio congelável — atendido por `TimeProvider`/`ICalendarClock` fake nos testes.
3. Plano atrasado altera somente pendentes — atendido.
4. Conclusão grava `ConcluidoEm` UTC e reageenda a partir do amanhã local — atendido.
5. Histórico concluído permanece imutável — atendido.
6. Pendentes não ficam com datas duplicadas após realinhamento — atendido, inclusive edge case de duplicidade futura.
7. `DataFimPrevista` acompanha o cronograma efetivo — atendido, inclusive plano totalmente concluído.
8. Dashboard e plano compartilham `ICalendarClock.Today` — atendido.
9. Nenhuma migration criada — atendido.
10. Testes cobrem UTC/local, atraso, conclusão, desmarcação, plano concluído, idempotência, colisão e edge case do dashboard — atendido.
11. `bash tools/verify.sh`/Backend Verify permanece verde — atendido.

## Testes adicionados/reforçados
- `BiblePlanCalendarTests`:
  - limite UTC/local;
  - atraso de múltiplos dias;
  - idempotência;
  - conclusão e timestamp UTC;
  - desmarcação;
  - colisão com histórico concluído;
  - plano totalmente concluído.
- `AdaptiveCalendarEdgeCaseTests`:
  - duplicidade futura entre pendentes;
  - dashboard com leitura concluída hoje e outro pendente atrasado.
- `DashboardPerformanceTests` passou a usar o mesmo `ICalendarClock` do produto.

## Performance
A sinalização `NeedsCalendarRealignment` é projetada dentro da query de overview já existente. O caminho normal do dashboard continua protegido pelos testes de performance de E2-BE-01: 4 comandos SQL sem plano e 5 com plano/leitura, sem cache externo ou serviço adicional.

## Review adversarial
Resultado: **APPROVED**.

Pontos revisados:
- sem migration/schema novo;
- nenhuma data histórica concluída é reescrita;
- `ConcluidoEm` permanece UTC;
- nenhuma decisão de calendário relevante usa UTC diretamente como data civil;
- duplicidade/colisão de datas é reparada;
- `DataFimPrevista` não fica obsoleta;
- dashboard não ganha query extra no caminho normal;
- nenhum serviço pago ou infraestrutura recorrente foi introduzido.

## Evidência de CI
- Backend Verify `40974132693`: success com a suíte principal do calendário adaptativo.
- Backend Verify `40974505697`: success com edge cases de duplicidade/dashboard.
- Backend Verify `40974558833`: success após tornar `Calendar:TimeZone` explícito na configuração.

## Fora de escopo
- alterar sequência pastoral/conteúdo das leituras;
- tabela de calendário;
- notificações/reminders;
- timezone por usuário;
- redesign visual do calendário.

## Definition of Done
Critérios atendidos, CI verde e review independente aprovado.
