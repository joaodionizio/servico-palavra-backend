# Bible Plan Generation

O plano biblico personalizado da V2 usa a tabela `BaseBiblica` real do banco como catalogo unico de capitulos. O frontend nao envia `UsuarioId`, `OrdemInicio`, `OrdemFim`, `UltimaOrdemConcluida` nem `PlanoOrigemId`; esses valores sao calculados pelo backend a partir do usuario autenticado e do estado salvo.

## BaseBiblica E Sequencia Pastoral

`BaseBiblica` nao representa a ordem de leitura do plano. Ela representa capitulos unicos da Biblia, com metadados como livro, capitulo, quantidade de versiculos e peso de leitura.

A ordem real do plano e definida pela sequencia pastoral V2, documentada em `docs/examples/sequencia-leitura-biblica-v2.json` e implementada no gerador. Essa camada permite:

- repetir livros e capitulos;
- ler faixas de capitulos;
- dividir Isaias em blocos;
- anexar Salmos como leitura diaria paralela.

O gerador consulta `BaseBiblica` com:

- `Ativo = true`.
- todos os capitulos ativos disponiveis.

Depois, expande a sequencia pastoral contra a base real. Assim, `BaseBiblica` continua sem duplicidade, enquanto o plano pode ler `1 João` tres vezes e `João` duas vezes.

## Ordem Pastoral

A sequencia começa pelo Novo Testamento, conforme o metodo pastoral baseado na Canção Nova:

1. `1 João` duas vezes.
2. `João`.
3. Demais livros do Novo Testamento na ordem pastoral.
4. `1 João` pela terceira vez.
5. `João` pela segunda vez.
6. Antigo Testamento na ordem pastoral, com `Isaias` dividido em `1-39`, `40-55` e `56-66`.

`Gênesis` nao deve ser a primeira leitura do plano.

## Salmos

Salmos nao e tratado como bloco comum na sequencia principal. A regra atual e anexar `1` salmo por dia como leitura paralela ate `Salmos 150`.

- Plano com 150 dias ou mais: le `Salmos 1` ate `Salmos 150`, um por dia.
- Plano com mais de 150 dias: depois do `Salmos 150`, nao repete Salmos nesta fase.
- Plano com menos de 150 dias: nao agrupa Salmos automaticamente nesta fase; os Salmos restantes ficam pendentes de revisao pastoral/manual antes de qualquer regra de agrupamento.

Os Salmos entram no calculo de peso diario para equilibrar a carga de leitura, mas continuam marcados como leitura paralela no texto diario.

## Peso De Leitura

A distribuicao diaria usa peso, nao quantidade bruta de capitulos:

1. Calcula `pesoTotal = sum(PesoLeitura)`.
2. Calcula `pesoMedioDia = pesoTotal / duracaoDias`.
3. Percorre as ocorrencias da sequencia pastoral.
4. Agrupa capitulos inteiros buscando aproximar cada dia do peso medio.
5. Nao divide capitulos por versiculo nesta fase.
6. Capitulos muito longos podem ficar sozinhos em um dia.

Se `PesoLeitura` estiver ausente ou invalido para algum legado, o gerador tenta usar `QuantidadeVersiculos` e, por ultimo, `TempoEstimadoMinutos`. Se a base inteira nao tiver peso valido, retorna erro `422`.

## Duracao

- 1 mes = 30 dias.
- 1 ano = 365 dias.
- Duracao maxima = 10 anos.
- Duracao zerada ou negativa retorna erro claro.

O plano gera exatamente `duracaoDias` dias quando ha base biblica disponivel. Se houver menos capitulos restantes que dias, os dias sem capitulo ficam como `Meditação livre`, sem registro em `PlanosBiblicosDiasCapitulos`. Isso preserva a duracao escolhida sem duplicar nem pular capitulos.

## Criacao

Ao criar novo plano:

1. O backend usa o usuario autenticado.
2. Verifica se ja existe plano ativo.
3. Comeca na primeira ocorrencia da sequencia pastoral.
4. Cria `PlanoBiblicoUsuario`, `PlanoBiblicoDia` e `PlanoBiblicoDiaCapitulo`.
5. Executa em transacao.

Existe apenas um plano ativo por usuario.

## Alteracao

Ao alterar plano:

- `ContinuarDeOndeParei`: consulta `PosicaoBiblicaUsuario` e inicia em `UltimaOrdemConcluida + 1`.
- `RecomecarDoInicio`: inicia em `Ordem = 1`.

Nos dois casos:

1. O plano ativo anterior e marcado como `Substituido`.
2. Um novo plano ativo e criado.
3. O historico e preservado.
4. A operacao roda em transacao.

## Seguranca

- Usuario comum cria e acessa apenas o proprio plano.
- Admin nao e necessario para criar plano do proprio usuario.
- Conclusao e desmarcacao de dia tambem validam propriedade do usuario.
- O frontend nao controla IDs internos nem ordem biblica.
- A BaseBiblica nao e importada automaticamente no startup normal.

## Endpoints Do Plano

- `GET /api/planos-biblicos/ativo`: retorna o plano ativo do usuario autenticado.
- `GET /api/planos-biblicos/{id}/dias`: retorna o cronograma real com `concluido` por dia.
- `POST /api/planos-biblicos/dias/{diaId}/concluir`: marca o dia como concluido e retorna o dia atualizado.
- `POST /api/planos-biblicos/dias/{diaId}/desmarcar`: desmarca o dia e retorna o dia atualizado.
- `GET /api/planos-biblicos/progresso/posicao-atual`: retorna a ultima ordem concluida recalculada.

Os endpoints `POST` exigem cookie de autenticacao e validacao CSRF.

## Performance

O cronograma era sensivel a lentidao porque a listagem de dias consultava o progresso dia a dia. A listagem agora projeta `PlanoBiblicoDiaResponse` em uma unica consulta, com `AsNoTracking`, retornando apenas os campos usados pela tela: identificador, dia, mes, data prevista, texto da leitura, salmo e status `concluido`.

A acao de marcar/desmarcar nao carrega capitulos ou plano inteiro para validar propriedade do dia. Ela altera apenas o registro de progresso, recalcula a posicao biblica do plano ativo e retorna o dia atualizado. Isso permite que o frontend use atualizacao otimista e evite refetch completo quando for seguro.

## Validacoes Tecnicas

Os testes cobrem:

- Geracao com BaseBiblica real importada.
- Plano começando por `1 João`, nao por `Gênesis`.
- Repeticao de `1 João` tres vezes e `João` duas vezes.
- Salmos como leitura paralela diaria.
- Blocos de `Isaias` em `1-39`, `40-55` e `56-66`.
- Capitulos gerados resolvidos sempre contra capitulos existentes em `BaseBiblica`.
- Duracoes de 6 meses, 1 ano e 2 anos.
- Isolamento entre usuarios.
- Marcar, desmarcar e marcar novamente um dia concluido.
- Recalculo de posicao biblica ao desmarcar.
- Continuar e recomecar.
- Bloqueio de dois planos ativos.
- Erro claro quando `BaseBiblica` esta vazia.
