# Plano Biblico E BaseBiblica

Este documento cobre a BaseBiblica V2, a sequencia pastoral e a geracao do Plano Biblico.

## BaseBiblica

`BaseBiblica` e o catalogo unico de capitulos biblicos. Ela nao representa repeticoes de leitura; cada registro representa um capitulo unico com metadados:

- ordem;
- livro;
- capitulo;
- grupo/subgrupo;
- testamento;
- quantidade de versiculos;
- peso de leitura;
- tempo estimado;
- status ativo.

Arquivo canonico para importacao:

```text
docs/examples/base-biblica-capitulos-com-versiculos.v2.json
```

Arquivos de conferencia mantidos porque sao usados por testes e validacao:

- `docs/examples/base-biblica-livros-com-versiculos.v2.json`
- `docs/examples/base-biblica-versiculos.v2.json`
- `docs/examples/base-biblica-livros.v2.json`
- `docs/examples/base-biblica-capitulos.v2.json`

Nenhum JSON deve conter texto biblico completo, secrets ou connection string.

## Sequencia Pastoral

A ordem real do plano e definida pela sequencia pastoral V2, documentada em:

```text
docs/examples/sequencia-leitura-biblica-v2.json
```

Essa camada permite:

- repetir livros e capitulos;
- ler faixas de capitulos;
- dividir Isaias em blocos;
- anexar Salmos como leitura diaria paralela.

`BaseBiblica` continua sem duplicidade, enquanto o plano pode ler `1 Joao` tres vezes e `Joao` duas vezes.

## Ordem De Leitura

A sequencia comeca pelo Novo Testamento. `Genesis` nao deve ser a primeira leitura.

Resumo atual:

1. `1 Joao` duas vezes.
2. `Joao`.
3. Demais livros do Novo Testamento na ordem pastoral.
4. `1 Joao` pela terceira vez.
5. `Joao` pela segunda vez.
6. Antigo Testamento na ordem pastoral, com `Isaias` dividido em `1-39`, `40-55` e `56-66`.

A regra pastoral nao deve ser alterada sem justificativa documentada e validacao manual.

## Salmos

Salmos nao e tratado como bloco comum na sequencia principal. A regra atual e anexar um salmo por dia como leitura paralela ate `Salmos 150`.

- Plano com 150 dias ou mais: le `Salmos 1` ate `Salmos 150`, um por dia.
- Plano com mais de 150 dias: depois do `Salmos 150`, nao repete Salmos nesta fase.
- Plano com menos de 150 dias: nao agrupa Salmos automaticamente nesta fase; os Salmos restantes ficam pendentes de revisao pastoral/manual.

## Peso De Leitura

A distribuicao diaria usa peso, nao quantidade bruta de capitulos:

1. Calcula `pesoTotal = sum(PesoLeitura)`.
2. Calcula `pesoMedioDia = pesoTotal / duracaoDias`.
3. Percorre as ocorrencias da sequencia pastoral.
4. Agrupa capitulos inteiros buscando aproximar cada dia do peso medio.
5. Nao divide capitulos por versiculo nesta fase.

Se `PesoLeitura` estiver ausente ou invalido para algum legado, o gerador tenta usar `QuantidadeVersiculos` e, por ultimo, `TempoEstimadoMinutos`. Se a base inteira nao tiver peso valido, retorna erro `422`.

## Criacao E Alteracao De Plano

O frontend nao envia `UsuarioId`, `OrdemInicio`, `OrdemFim`, `UltimaOrdemConcluida` nem `PlanoOrigemId`. Esses valores sao calculados pelo backend a partir do usuario autenticado e do estado salvo.

Ao criar plano:

- o backend usa o usuario autenticado;
- verifica se ja existe plano ativo;
- comeca na primeira ocorrencia da sequencia pastoral;
- cria `PlanoBiblicoUsuario`, `PlanoBiblicoDia` e `PlanoBiblicoDiaCapitulo`;
- executa em transacao.

Ao alterar plano:

- `ContinuarDeOndeParei`: inicia em `UltimaOrdemConcluida + 1`;
- `RecomecarDoInicio`: inicia em `Ordem = 1`;
- o plano ativo anterior e marcado como `Substituido`;
- um novo plano ativo e criado;
- o historico e preservado;
- a operacao roda em transacao.

Existe apenas um plano ativo por usuario.

## Importacao Da BaseBiblica

O importador nao roda no startup normal da API. Ele precisa ser chamado em modo operacional explicito:

```bash
env DATABASE_PROVIDER=sqlite \
  ConnectionStrings__DefaultConnection='Data Source=servico-palavra-dev.db' \
  dotnet run --project src/ServicoPalavra.Api -- \
  --run-mode=import-base-biblica-v2
```

Para indicar o arquivo:

```bash
env DATABASE_PROVIDER=sqlite \
  ConnectionStrings__DefaultConnection='Data Source=servico-palavra-dev.db' \
  dotnet run --project src/ServicoPalavra.Api -- \
  --run-mode=import-base-biblica-v2 \
  --base-biblica-v2-path=docs/examples/base-biblica-capitulos-com-versiculos.v2.json
```

O importador:

- le `docs/examples/base-biblica-capitulos-com-versiculos.v2.json`;
- valida 1334 registros;
- valida `Ordem` unica e sequencial;
- valida `Livro + Capitulo` sem duplicidade;
- valida `QuantidadeVersiculos`, `PesoLeitura` e `TempoEstimadoMinutos` maiores que zero;
- usa transacao;
- insere registros novos;
- atualiza registros existentes por `Ordem`, com fallback por `Livro + Capitulo`;
- nao duplica dados em reexecucao.

Para Neon, use connection string apenas por variavel de ambiente ou secret manager e faca backup/snapshot antes.

## Validacao

```bash
python3 tools/validar-base-biblica-versiculos.py
dotnet test ServicoPalavra.sln
```

Resultado esperado da base:

- 73 livros.
- 46 livros no Antigo Testamento.
- 27 livros no Novo Testamento.
- 1334 capitulos.
- `sum(QuantidadeVersiculos) = 35527`.
- `sum(PesoLeitura) = 35527`.

## Endpoints Principais

- `GET /api/planos-biblicos/ativo`
- `GET /api/planos-biblicos/me/ativo`
- `GET /api/planos-biblicos/me/historico`
- `POST /api/planos-biblicos`
- `POST /api/planos-biblicos/alterar`
- `GET /api/planos-biblicos/{id}`
- `GET /api/planos-biblicos/{id}/dias`
- `GET /api/planos-biblicos/progresso/posicao-atual`
- `POST /api/planos-biblicos/dias/{diaId}/concluir`
- `POST /api/planos-biblicos/dias/{diaId}/desmarcar`

Endpoints `POST` exigem cookie autenticado e CSRF.
