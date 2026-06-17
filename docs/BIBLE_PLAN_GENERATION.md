# Bible Plan Generation

O plano biblico personalizado da V2 usa a tabela `BaseBiblica` real do banco. O frontend nao envia `UsuarioId`, `OrdemInicio`, `OrdemFim`, `UltimaOrdemConcluida` nem `PlanoOrigemId`; esses valores sao calculados pelo backend a partir do usuario autenticado e do estado salvo.

## Fonte

O gerador consulta `BaseBiblica` com:

- `Ativo = true`.
- `Ordem >= ordemInicio`.
- Ordenacao crescente por `Ordem`.

A ordem pastoral e controlada exclusivamente por `BaseBiblica.Ordem`.

## Peso De Leitura

A distribuicao diaria usa peso, nao quantidade bruta de capitulos:

1. Calcula `pesoTotal = sum(PesoLeitura)`.
2. Calcula `pesoMedioDia = pesoTotal / duracaoDias`.
3. Percorre os capitulos em ordem pastoral.
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
3. Comeca em `Ordem = 1`.
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
- Conclusao de dia tambem valida propriedade do usuario.
- O frontend nao controla IDs internos nem ordem biblica.
- A BaseBiblica nao e importada automaticamente no startup normal.

## Validacoes Tecnicas

Os testes cobrem:

- Geracao com BaseBiblica real importada.
- Ordem crescente e sem duplicidade.
- Soma de `PesoLeitura` igual ao esperado.
- Duracoes de 6 meses, 1 ano e 2 anos.
- Isolamento entre usuarios.
- Continuar e recomecar.
- Bloqueio de dois planos ativos.
- Erro claro quando `BaseBiblica` esta vazia.
