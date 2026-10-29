---
name: bmad-spec
description: Cria ou atualiza uma especificação executável para uma story antes da implementação.
---

# BMad Spec — Serviço da Palavra

## Objetivo
Transformar uma demanda em uma story implementável, testável e pequena o bastante para revisão independente.

## Antes de escrever
1. Leia `README.md`, `docs/ARCHITECTURE.md` e os documentos de domínio relevantes.
2. Leia `docs/BMAD-WORKFLOW.md` quando existir.
3. Inspecione o código real relacionado; documentação não substitui o código atual.
4. Consulte `_bmad-output/implementation-artifacts/sprint-status.yaml` se existir.
5. Para features cross-repo, registre claramente mudanças e contratos esperados no repositório irmão.

## Artefato
Crie/atualize `_bmad-output/implementation-artifacts/<story-id>-<slug>.md` com:
- objetivo e contexto;
- escopo e fora de escopo;
- decisões já aprovadas;
- critérios de aceitação numerados;
- contrato API/dados quando aplicável;
- arquivos/áreas provavelmente afetados;
- plano de testes e verificação;
- riscos, regressões e acessibilidade/responsividade quando houver UI;
- Definition of Done.

## Qualidade
- Critérios devem ser verificáveis, não vagos.
- Prefira mudanças incrementais a rewrites.
- Performance: medir antes/depois quando a story alegar ganho.
- UI: considerar 320/390/768/1024/1440, teclado, dark mode e reduced motion quando relevante.
- Segurança: preservar cookie HttpOnly, CSRF, autorização e rate limiting.

## Restrições
- Não implemente a story nesta skill.
- Não faça commit, push, merge ou branch.
- Não invente requisitos ausentes; marque hipóteses explicitamente.
- Ao finalizar, deixe a story pronta para aprovação e, depois de aprovada, para `bmad-build`.