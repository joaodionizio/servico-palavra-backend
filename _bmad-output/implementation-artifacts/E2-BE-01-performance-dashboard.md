# Story E2-BE-01 — Reduzir round-trips do dashboard

Status: done  
Prioridade: P0

## Objetivo
Reduzir a latência do `GET /api/dashboard/me` sem adicionar cache externo, serviço pago ou inconsistência de dados.

## Baseline
Antes da alteração, a composição do dashboard executava consultas independentes em sequência para:
1. favoritos;
2. conteúdos concluídos;
3. formações recentes;
4. favoritos recentes;
5. conteúdos com progresso;
6. plano ativo;
7. leitura de hoje;
8. fallback para próxima pendente quando não havia leitura exatamente na data.

Isso equivalia a:
- **6 comandos SQL** no cenário sem plano ativo;
- **7 comandos SQL** com plano e leitura exatamente no dia;
- **8 comandos SQL** com plano e necessidade de fallback.

## Implementação
- criada projeção `GetOverviewAsync` que consolida em uma única query:
  - total de favoritos;
  - total de concluídos;
  - plano ativo;
  - total de dias;
  - dias concluídos;
- formações, favoritos recentes e progresso permanecem consultas específicas, mantendo ordenação/limite e projeções atuais;
- leitura de hoje e fallback para próxima pendente foram consolidados em uma única query, priorizando `DataPrevista == hoje` e depois `DiaNumero`;
- não foi usado paralelismo no mesmo `DbContext`;
- leituras continuam com `AsNoTracking`;
- nenhum Redis, cache externo ou serviço pago foi adicionado.

## Medição automatizada
Foi adicionado `DashboardCommandCounterInterceptor`, registrado nos testes via `IInterceptor`, para contar comandos reais executados pelo EF Core somente durante o GET do dashboard.

Resultados protegidos por teste:
- cenário sem plano ativo: **6 → exatamente 4 comandos SQL**;
- cenário com plano ativo + leitura de hoje: **7 → exatamente 5 comandos SQL**;
- o antigo cenário de fallback deixa de executar a query adicional, portanto **8 → 5 comandos SQL**.

Redução:
- sem plano: **-33%** de round-trips;
- plano/leitura do dia: **-29%**;
- plano/fallback: **-37,5%**.

O ganho de tempo real depende da latência entre aplicação e banco; por isso milissegundos não foram usados como gate instável de CI.

## Testes
- usuário autenticado único por teste;
- cenário sem plano ativo exige exatamente 4 comandos;
- cenário com plano ativo e leitura prevista para hoje exige exatamente 5 comandos;
- contrato retorna o plano correto e `leituraHoje.diaNumero == 1`;
- contador é resetado imediatamente antes do GET, não contando cadastro/setup.

## Verificação
`Backend Verify` run `40971254712`: **success** após correção do enum usado apenas no fixture do teste.

O gate executou restore, build Release e toda a suíte de testes.

## Code review adversarial
Resultado: **APPROVED**.

Achados bloqueantes: nenhum.

Revisão confirmou:
- nenhum N+1 introduzido;
- nenhuma operação concorrente no mesmo `DbContext`;
- `AsNoTracking` preservado;
- ordenações e `Take(5)` preservados nas listas;
- comportamento anterior de uma leitura exatamente para hoje ter prioridade foi preservado;
- quando não existe leitura hoje, a próxima pendente continua sendo escolhida, agora na mesma query;
- a decisão de usar UTC como data civil não foi misturada nesta otimização e permanece explicitamente reservada para E3-BE-01.

## Critérios de aceitação
1. contador reproduzível de SQL — **atendido**;
2. redução mínima de 2 round-trips — **atendido**;
3. contadores preservados — **atendido pela projeção/testes existentes**;
4. listas/ordenação/limites preservados — **atendido**;
5. contrato de plano/leitura preservado — **atendido/testado**;
6. sem N+1 — **atendido**;
7. `AsNoTracking` preservado — **atendido**;
8. sem custo externo — **atendido**;
9. antes/depois documentado — **atendido**;
10. verify verde — **atendido**.

## Definition of Done
Concluída e aprovada.
