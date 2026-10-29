---
name: bmad-sprint-planning
description: Planeja e mantém o status da sprint do Serviço da Palavra sem implementar código.
---

# BMad Sprint Planning — Serviço da Palavra

## Objetivo
Organizar backlog, prioridades e prontidão das stories mantendo um estado simples e auditável.

## Fonte de status
Use `_bmad-output/implementation-artifacts/sprint-status.yaml` quando existir. Se não existir e o usuário pedir planejamento, crie-o.

## Status permitidos
- `backlog`
- `ready-for-dev`
- `in-progress`
- `review`
- `done`
- `blocked`

## Procedimento
1. Leia `docs/BMAD-WORKFLOW.md`, README e arquitetura relevante.
2. Leia as stories existentes em `_bmad-output/implementation-artifacts/`.
3. Priorize primeiro correção funcional/segurança/performance, depois qualidade estrutural, depois acabamento visual.
4. Marque `ready-for-dev` somente quando critérios de aceitação e dependências estiverem claros.
5. Registre dependências cross-repo explicitamente.
6. Ao apresentar o plano, destaque no máximo o próximo conjunto útil de trabalho; não sobrecarregue o usuário.

## Regras do projeto
- P0: bugs funcionais, autenticação, dados, timezone, segurança e gargalos perceptíveis.
- P1: testes, responsividade, acessibilidade, design system e arquitetura de frontend.
- P2: refactors não bloqueantes e melhorias de manutenção.

## Restrições
- Não implemente código nesta skill.
- Não faça commit, push, merge ou branch.
- Não marque story como `done` sem evidência de verificação e review concluídos.