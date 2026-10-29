---
name: bmad-code-review
description: Revisa adversarialmente uma story implementada, sem corrigir automaticamente e sem marcar done por conta própria.
---

# BMad Code Review — Serviço da Palavra

## Objetivo
Atuar como revisor independente após build + verify, procurando regressões, violações de critérios, riscos de segurança, problemas de performance e lacunas de teste.

## Contexto
Execute preferencialmente em uma nova sessão/contexto. Leia:
1. story completa e critérios de aceitação;
2. diff/arquivos alterados;
3. documentação arquitetural e contratos relevantes;
4. resultados de testes/verificação registrados na story.

## Checklist
- Cada critério de aceitação está realmente atendido?
- Há comportamento quebrado fora do happy path?
- Há regressão de autenticação, autorização, cookie/CSRF ou validação?
- Consultas ao banco podem gerar round-trips/N+1 desnecessários?
- Datas civis respeitam timezone do projeto?
- Erros/estados de loading são tratados?
- UI permanece utilizável em 320/390/768/1024/1440, teclado e dark mode quando aplicável?
- Há cobertura automatizada suficiente para a mudança?
- O código adicionou complexidade ou duplicação desnecessária?

## Severidade
Classifique achados como `CRITICAL`, `HIGH`, `MEDIUM` ou `LOW`, com arquivo/área, evidência e impacto.

## Resultado
Entregue uma destas conclusões:
- `APPROVED` — sem achados bloqueantes;
- `CHANGES_REQUESTED` — há correções necessárias;
- `BLOCKED` — não foi possível validar por falta de evidência/dependência.

A story permanece em `review` durante a análise. Só mova para `done` quando o usuário aprovar a conclusão ou quando o fluxo do projeto explicitamente determinar isso.

## Restrições
- Não corrija código automaticamente durante review, salvo pedido explícito posterior.
- Não faça commit, push, merge ou branch.
- Não invente falhas: cada achado deve ser sustentado pelo código, teste ou critério da story.
- Diferencie defeito real de melhoria opcional.