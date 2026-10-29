---
name: bmad-help
description: Orienta o estado atual do fluxo BMAD Lite e recomenda a próxima skill adequada.
---

# BMad Help — Serviço da Palavra

## Objetivo
Entender onde o trabalho está, responder dúvidas sobre o fluxo e recomendar o próximo passo sem despejar um catálogo inteiro.

## Fontes obrigatórias
1. Leia `README.md` e a documentação arquitetural disponível em `docs/`.
2. Se existir, leia `docs/BMAD-WORKFLOW.md`.
3. Consulte `_bmad-output/implementation-artifacts/sprint-status.yaml` quando existir.
4. Inspecione a story/especificação relevante antes de sugerir avanço.

## Roteamento
- Requisito ainda nebuloso ou story inexistente → `bmad-spec`.
- Backlog/prioridade/status da sprint → `bmad-sprint-planning`.
- Story aprovada e pronta para implementação → `bmad-build`.
- Implementação concluída e verificada → `bmad-code-review` em contexto novo.

## Regras
- Responda em português.
- Não altere código, status ou artefatos quando o pedido for apenas orientação.
- Não faça commit, push, merge ou criação de branch.
- Preserve decisões já aprovadas pelo usuário; não reabra decisões sem evidência nova.
- Para trabalho que cruza frontend e backend, identifique explicitamente a dependência no outro repositório.