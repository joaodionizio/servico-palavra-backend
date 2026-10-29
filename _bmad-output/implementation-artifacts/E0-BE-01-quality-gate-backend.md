# Story E0-BE-01 — Quality gate do backend

Status: review  
Prioridade: P1

## Objetivo
Criar um gate único e repetível para validar o backend antes de qualquer story ser movida para review, sem adicionar custo de infraestrutura.

## Contexto
O repositório já possui testes unitários, de integração e de segurança, e a documentação recomenda `restore`, `build` e `test`. Faltava consolidar isso em um comando simples e em CI para impedir regressões silenciosas.

## Escopo
- adicionar um script de verificação local, preferencialmente `tools/verify.sh`;
- executar restore, build e toda a suíte de testes;
- adicionar workflow de GitHub Actions para o mesmo gate em push/PR;
- usar versão do .NET compatível com o projeto;
- não depender de serviços pagos ou secrets de produção;
- documentar o comando no workflow BMAD.

## Fora de escopo
- alterar lógica de negócio;
- otimizar queries do dashboard;
- recuperação de senha;
- alterar infraestrutura de produção.

## Critérios de aceitação
1. Existe um único comando local documentado que falha com exit code não-zero se restore, build ou testes falharem.
2. O gate executa `dotnet restore`, `dotnet build` e `dotnet test` sobre `ServicoPalavra.sln` sem ignorar os projetos de teste existentes.
3. Existe CI no GitHub executando o gate em pull requests e pushes na `main`.
4. CI não exige connection string, senha admin ou qualquer secret de produção.
5. O fluxo de testes mantém suporte ao ambiente `Testing` existente.
6. Nenhuma regra de autenticação, migrations ou domínio é alterada por esta story.
7. A documentação aponta para o novo comando de verify.

## Implementação
Arquivos adicionados/alterados:
- `tools/verify.sh` — restore, build Release e test Release com `set -euo pipefail`;
- `.github/workflows/verify.yml` — GitHub Actions em push/PR da `main`, com .NET 8 e permissões somente de leitura;
- `docs/BMAD-WORKFLOW.md` — documenta `bash tools/verify.sh` como gate oficial.

## Verificação
- GitHub Actions run `40964117076`: `completed / success`.
- Job `verify` concluiu com sucesso: checkout, setup .NET 8 e `Verify backend` verdes.
- O workflow não contém secrets nem credenciais de produção.
- O script interrompe em qualquer comando com erro por `set -euo pipefail`.

## Code review adversarial
Resultado: **APPROVED**.

Achados bloqueantes: nenhum.

Observações:
- `tools/verify.sh` é invocado explicitamente com `bash`, portanto não depende do bit executável do arquivo criado pela Contents API.
- `dotnet test --no-build --configuration Release` corresponde ao build Release imediatamente anterior.
- O workflow usa `permissions: contents: read`, reduzindo privilégios desnecessários.
- Nenhuma lógica de aplicação, autenticação, banco ou migrations foi tocada.

## Definition of Done
- critérios 1–7 atendidos;
- verify verde em CI;
- arquivos alterados registrados;
- story em `review`;
- code review independente concluído com `APPROVED`.

A story está pronta para ser marcada `done` após aprovação do fluxo/usuário.
