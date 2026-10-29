# Story E2-BE-01 — Reduzir round-trips do dashboard

Status: ready-for-dev  
Prioridade: P0

## Objetivo
Reduzir a latência do `GET /api/dashboard/me` sem adicionar cache externo, serviço pago ou inconsistência de dados.

## Baseline observado
`DashboardReadRepository.GetMeAsync` atualmente monta uma única resposta HTTP, mas executa em sequência consultas independentes para:
1. total de favoritos;
2. total de conteúdos concluídos;
3. formações recentes;
4. favoritos recentes;
5. conteúdos com progresso;
6. plano bíblico ativo e seus contadores;
7. leitura do dia/próxima pendente.

Em banco remoto, cada round-trip adiciona latência mesmo quando cada SQL individual é rápido.

## Estratégia
- medir a quantidade real de comandos SQL do endpoint antes da alteração;
- consolidar consultas compatíveis, especialmente métricas escalares/contadores;
- avaliar se plano ativo + leitura atual podem ser projetados com menos round-trips sem gerar SQL excessivamente complexo;
- manter `AsNoTracking` em leitura;
- não paralelizar operações usando o mesmo `DbContext`, pois EF Core não suporta múltiplas operações concorrentes na mesma instância;
- não introduzir Redis, cache distribuído ou serviço externo;
- evitar cache de dados personalizados nesta story para não criar invalidação prematura.

## Critérios de aceitação
1. Existe teste/medição automatizada ou diagnóstico reproduzível que conte os comandos SQL usados por `GET /api/dashboard/me`.
2. A implementação reduz o número de round-trips em pelo menos 2 em relação ao baseline medido, sem aumentar o número de requisições HTTP do frontend.
3. Contadores de favoritos/concluídos permanecem corretos.
4. Formações recentes, favoritos recentes e conteúdos em andamento preservam ordenação e limite atuais.
5. Resumo do plano bíblico e leitura atual preservam o contrato público existente.
6. Nenhuma query N+1 é introduzida.
7. Todas as consultas de leitura relevantes continuam `AsNoTracking`.
8. Nenhum cache externo ou custo recorrente é adicionado.
9. O ganho é registrado na story como `comandos SQL antes → depois`; tempo em milissegundos pode ser observado, mas não deve ser critério rígido de CI.
10. `bash tools/verify.sh` permanece verde.

## Testes
- integração do endpoint com usuário autenticado;
- cenário sem plano ativo;
- cenário com plano ativo e leitura atual;
- favoritos/progresso/formações com dados suficientes para validar ordenação e limites;
- contador/interceptor de comandos reiniciado imediatamente antes do GET do dashboard para não contar setup/login.

## Fora de escopo
- alterar layout do dashboard;
- cache Redis/memória de resposta personalizada;
- trocar Neon/Render;
- corrigir timezone da leitura de hoje (story E3-BE-01);
- refatoração ampla de repositories sem impacto mensurável.

## Arquivos prováveis
- `src/ServicoPalavra.Infrastructure/Persistence/Repositories/DashboardReadRepository.cs`
- DTO/repository abstractions apenas se necessário;
- testes de integração e interceptor de comandos SQL.

## Definition of Done
- baseline e resultado documentados;
- redução de pelo menos 2 comandos SQL demonstrada;
- contrato funcional preservado por testes;
- verify verde;
- story em `review` e `bmad-code-review` independente aprovado.
