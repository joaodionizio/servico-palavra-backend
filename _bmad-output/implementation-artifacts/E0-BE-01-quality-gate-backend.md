# Story E0-BE-01 — Quality gate do backend

Status: ready-for-dev  
Prioridade: P1

## Objetivo
Criar um gate único e repetível para validar o backend antes de qualquer story ser movida para review, sem adicionar custo de infraestrutura.

## Contexto
O repositório já possui testes unitários, de integração e de segurança, e a documentação recomenda `restore`, `build` e `test`. Falta consolidar isso em um comando simples e em CI para impedir regressões silenciosas.

## Escopo
- adicionar um script de verificação local, preferencialmente `tools/verify.sh`;
- executar restore, build e toda a suíte de testes;
- adicionar workflow de GitHub Actions para o mesmo gate em push/PR;
- usar versão do .NET compatível com o projeto;
- não depender de serviços pagos ou secrets de produção;
- documentar o comando no README/deploy quando necessário.

## Fora de escopo
- alterar lógica de negócio;
- otimizar queries do dashboard;
- recuperação de senha;
- alterar infraestrutura de produção.

## Critérios de aceitação
1. Existe um único comando local documentado que falha com exit code não-zero se restore, build ou testes falharem.
2. O gate executa `dotnet restore`, `dotnet build` e `dotnet test` sobre `ServicoPalavra.sln` sem ignorar os projetos de teste existentes.
3. Existe CI no GitHub executando o gate em pull requests e/ou pushes relevantes.
4. CI não exige connection string, senha admin ou qualquer secret de produção.
5. O fluxo de testes mantém suporte ao ambiente `Testing` existente.
6. Nenhuma regra de autenticação, migrations ou domínio é alterada por esta story.
7. A documentação aponta para o novo comando de verify.

## Verificação
- executar o novo verify localmente;
- confirmar que todos os testes atuais passam;
- revisar o workflow para ausência de secrets hardcoded;
- confirmar que uma falha de teste interrompe o script/CI.

## Riscos
- configuração de CI divergente da execução local;
- duplicação de comandos entre script e workflow.

## Definition of Done
- critérios 1–7 atendidos;
- verify verde;
- arquivos alterados registrados nesta story;
- story movida para `review`, não diretamente para `done`;
- `bmad-code-review` executado em contexto independente.
