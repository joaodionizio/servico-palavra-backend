---
name: bmad-build
description: Implementa uma story aprovada do Serviço da Palavra com escopo controlado e verificação obrigatória.
---

# BMad Build — Serviço da Palavra

## Objetivo
Implementar exatamente a story aprovada, preservando arquitetura, segurança e decisões já tomadas.

## Antes de editar
1. Leia a story completa em `_bmad-output/implementation-artifacts/`.
2. Leia `docs/BMAD-WORKFLOW.md`, README, arquitetura e documentação específica do domínio.
3. Inspecione os arquivos reais afetados antes de planejar.
4. Faça um plano curto, com no máximo 8 linhas.
5. Confirme dependências cross-repo no próprio plano; não altere contratos silenciosamente.

## Implementação
- Faça a menor mudança coerente que satisfaça os critérios de aceitação.
- Não faça rewrite sem necessidade demonstrada.
- Preserve autenticação por cookie HttpOnly, CSRF, autorização, rate limiting e contratos públicos existentes, salvo quando a story exigir mudança explícita.
- Para calendário/data civil, use a política de timezone definida pelo projeto; não introduza `UtcNow` como substituto de data local.
- Para performance, evite N+1/round-trips desnecessários e registre a evidência de medição quando aplicável.
- Para UI, preserve responsividade, teclado, dark mode e reduced motion quando a story tocar frontend.

## Verificação obrigatória
Use o gate mais forte disponível no repositório. No backend, no mínimo:
`dotnet restore ServicoPalavra.sln && dotnet build ServicoPalavra.sln --no-restore && dotnet test ServicoPalavra.sln --no-build`

Se existir script `verify`, prefira-o.

## Atualização da story
Registre:
- arquivos alterados;
- testes executados e resultado;
- decisões/limitações relevantes;
- itens deliberadamente fora de escopo.
Mova para `review` apenas quando a implementação e a verificação estiverem concluídas.

## Restrições
- Não faça commit, push, merge ou criação de branch.
- Não marque como `done`; `done` vem após review aprovado.
- Não implemente melhorias extras só porque foram encontradas; registre-as como débito/backlog.