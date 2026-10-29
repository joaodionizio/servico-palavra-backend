# BMAD Lite — Serviço da Palavra

Este repositório usa um fluxo BMAD Lite inspirado no projeto Casa de Leilões, adaptado ao Serviço da Palavra.

## Skills

- `bmad-help` — orientação e próximo passo.
- `bmad-spec` — cria/atualiza story executável.
- `bmad-sprint-planning` — organiza backlog e status.
- `bmad-build` — implementa story aprovada e executa verificação.
- `bmad-code-review` — revisão adversarial independente.

A fonte canônica das skills é `.agents/skills/`. `.claude/skills/` contém entradas que delegam para a versão canônica.

## Fluxo padrão

1. `bmad-spec`
2. aprovação da story pelo usuário
3. `bmad-build`
4. verify automatizado
5. teste manual quando aplicável
6. `bmad-code-review` em contexto novo
7. correções explícitas, se necessárias
8. aprovação e `done`

## Git

As skills não devem criar branch, commit, push ou merge automaticamente. O usuário mantém controle sobre o fluxo Git. Alterações via ferramentas externas só devem ocorrer quando o usuário pedir explicitamente a modificação do repositório.

## Artefatos

Stories e status ficam em `_bmad-output/implementation-artifacts/`.

Status válidos:

- `backlog`
- `ready-for-dev`
- `in-progress`
- `review`
- `done`
- `blocked`

## Verificação do backend

Gate local único:

```bash
bash tools/verify.sh
```

O script executa restore, build Release e toda a suíte de testes da solução. O mesmo gate roda em `.github/workflows/verify.yml` para `push` e `pull_request` na `main`, sem depender de secrets de produção.

## Regras de arquitetura

- Preservar camadas API / Application / Domain / Infrastructure.
- Preservar ASP.NET Core Identity, cookie HttpOnly, CSRF, autorização e rate limiting.
- Não versionar secrets ou connection strings reais.
- Preferir queries `AsNoTracking` em leitura e evitar round-trips desnecessários.
- Datas de calendário civil devem respeitar o timezone configurado do projeto; UTC continua apropriado para timestamps instantâneos.
- Mudanças de contrato com o frontend devem ser explícitas na story.

## Trabalho cross-repo

Backend e frontend permanecem repositórios separados. Use o mesmo identificador lógico de story quando uma feature exigir alterações nos dois lados, e registre dependências/contratos na story de cada repositório.
