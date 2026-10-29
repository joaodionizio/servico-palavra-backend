# Story E3-BE-01 — Unificar timezone e calendário adaptativo do plano bíblico

Status: backlog  
Prioridade: P0

## Objetivo
Garantir que todas as decisões de calendário civil do plano bíblico usem o mesmo “hoje” no timezone configurado, preservando timestamps instantâneos em UTC, e concluir o comportamento adaptativo sem alterar o schema.

## Problema atual
Há usos diretos de `DateTime.UtcNow` convertidos para `DateOnly`, inclusive na leitura de hoje e na alteração de plano. Para usuários em UTC-3, por exemplo, UTC pode virar o dia enquanto localmente ainda é noite, fazendo a aplicação tratar “amanhã” como “hoje”.

## Decisões
- timezone de calendário configurável por `Calendar:TimeZone`, padrão `America/Sao_Paulo`;
- fonte de tempo injetável (`TimeProvider` ou abstração equivalente), permitindo testes determinísticos;
- `DateOnly` representa calendário civil e deve ser calculado após conversão para o timezone configurado;
- `ConcluidoEm`, `CriadoEm`, `AtualizadoEm` e demais timestamps instantâneos continuam UTC;
- sem migrations/schema novo;
- campos já existentes: `DataInicio`, `DataFimPrevista`, `PlanoBiblicoDia.DataPrevista`, `ProgressoLeitura.ConcluidoEm`.

## Regras do calendário adaptativo
1. Dias concluídos são histórico imutável: seu `DiaNumero`, leitura e data prevista histórica não são reescritos pelo realinhamento.
2. Apenas dias pendentes podem ser redistribuídos.
3. Se ao consultar um plano houver dia pendente com `DataPrevista < hoje`, os pendentes devem ser realinhados a partir de `hoje`, mantendo a ordem e intervalo diário.
4. Ao concluir uma leitura, o histórico concluído permanece intacto e os demais pendentes são redistribuídos a partir de **amanhã** no calendário local.
5. Ao desmarcar uma conclusão, o sistema deve aplicar regra determinística documentada para reentrada do dia na sequência, sem duplicar datas ou corromper histórico.
6. `DataFimPrevista` do plano acompanha a última data prevista dos dias após redistribuição.
7. O dashboard e os endpoints de plano devem usar a mesma fonte de “hoje”.
8. A leitura de hoje deve priorizar o dia previsto para hoje; se inexistente, o comportamento de fallback para próxima pendente deve ser explícito e testado.

## Critérios de aceitação
1. Às `00:30 UTC` de um instante em que ainda é o dia anterior em `America/Sao_Paulo`, dashboard/plano usam a data local correta.
2. Fonte de tempo pode ser congelada em testes sem depender do relógio da máquina.
3. Consultar plano atrasado realinha apenas pendentes e não altera concluídos.
4. Concluir um dia registra `ConcluidoEm` em UTC e redistribui os pendentes a partir do amanhã local.
5. Dias concluídos mantêm a data histórica mesmo após novas consultas/conclusões.
6. Não existem duas leituras pendentes com a mesma `DataPrevista` após realinhamento.
7. `DataFimPrevista` é recalculada de acordo com a última leitura pendente/histórica aplicável.
8. Dashboard e endpoint do plano concordam sobre “leitura de hoje”.
9. Nenhuma migration é criada.
10. Testes cobrem virada UTC/local, atraso de múltiplos dias, conclusão, desmarcação, plano totalmente concluído e idempotência de consultas repetidas.
11. `bash tools/verify.sh` permanece verde.

## Arquitetura sugerida
- criar uma abstração única de relógio/calendário na Application, por exemplo `ICalendarClock`, expondo `UtcNow` e `Today`;
- implementação baseada em `TimeProvider` + `TimeZoneInfo`, registrada por DI;
- remover decisões de calendário baseadas em `DateTime.UtcNow` direto;
- centralizar realinhamento em serviço de domínio/aplicação reutilizado por dashboard e plano, evitando duas regras paralelas.

## Fora de escopo
- alterar a sequência pastoral/conteúdo das leituras;
- adicionar tabela de calendário;
- notificações/reminders;
- mudar o timezone por usuário nesta etapa;
- frontend visual do calendário, salvo ajustes de contrato indispensáveis.

## Definition of Done
- critérios 1–11 atendidos;
- nenhuma decisão de calendário relevante usa UTC como data civil diretamente;
- testes determinísticos com relógio injetado;
- verify verde;
- story em `review` e code review independente aprovado.
