# BaseBiblica V2 Import

A `BaseBiblica` V2 revisada continua sendo uma base operacional sensivel: ela pode ser importada localmente para validacao, mas ainda nao deve ser importada no Neon sem revisao final e janela operacional definida.

## Arquivos Usados

- `docs/examples/base-biblica-capitulos-com-versiculos.v2.json`: fonte principal da importacao.
- `docs/examples/base-biblica-livros-com-versiculos.v2.json`: totais por livro para conferencia.
- `docs/examples/base-biblica-versiculos.v2.json`: contagens por capitulo e URLs de referencia.
- `docs/BIBLE_BASE_V2_VERSE_COUNTS_REPORT.md`: relatorio tecnico de fontes, divergencias e decisoes.
- `tools/validar-base-biblica-versiculos.py`: validacao offline dos artefatos revisados.

Nenhum arquivo contem texto biblico; os JSONs guardam apenas metadados e contagens.

## Validar Os JSONs

```bash
python3 tools/validar-base-biblica-versiculos.py
dotnet test ServicoPalavra.sln
```

Validacoes esperadas:

- 73 livros.
- 46 livros no Antigo Testamento.
- 27 livros no Novo Testamento.
- 1334 capitulos.
- `quantidadeVersiculos > 0`.
- `pesoLeitura > 0`.
- `tempoEstimadoMinutos > 0`.
- `Ordem` sequencial e unica.
- Soma por livro igual a `totalVersiculos`.

## Migration

A entidade `BaseBiblica` agora possui:

- `QuantidadeVersiculos`.
- `PesoLeitura`.

A migration `AddBaseBiblicaReadingWeights` adiciona apenas essas duas colunas inteiras, com valor default `0` para registros existentes. O importador atualiza os valores reais a partir do JSON revisado.

## Importar Localmente

Use SQLite local e o modo operacional explicito:

```bash
env DATABASE_PROVIDER=sqlite \
  ConnectionStrings__DefaultConnection='Data Source=servico-palavra-dev.db' \
  dotnet run --project src/ServicoPalavra.Api -- \
  --run-mode=import-base-biblica-v2
```

Para informar outro arquivo:

```bash
env DATABASE_PROVIDER=sqlite \
  ConnectionStrings__DefaultConnection='Data Source=servico-palavra-dev.db' \
  dotnet run --project src/ServicoPalavra.Api -- \
  --run-mode=import-base-biblica-v2 \
  --base-biblica-v2-path=docs/examples/base-biblica-capitulos-com-versiculos.v2.json
```

O modo `import-base-biblica-v2` nao inicia o servidor HTTP. Ele executa bootstrap de banco, roles/admin existentes e, em seguida, roda a importacao uma unica vez.

## Como O Importador Funciona

- Le `docs/examples/base-biblica-capitulos-com-versiculos.v2.json`.
- Valida 1334 registros.
- Valida `Ordem` unica e sequencial.
- Valida `Livro + Capitulo` sem duplicidade.
- Valida `QuantidadeVersiculos`, `PesoLeitura` e `TempoEstimadoMinutos` maiores que zero.
- Usa transacao.
- Insere registros novos.
- Atualiza registros existentes por `Ordem`, com fallback por `Livro + Capitulo`.
- Nao duplica dados em reexecucao.
- Registra apenas resumo: processados, inseridos e atualizados.

## Conferir No Banco Local

Exemplo com SQLite:

```bash
sqlite3 servico-palavra-dev.db \
  'select count(*), sum(QuantidadeVersiculos), sum(PesoLeitura) from BaseBiblica;'

sqlite3 servico-palavra-dev.db \
  'select Ordem, Livro, Capitulo, QuantidadeVersiculos, PesoLeitura from BaseBiblica order by Ordem limit 10;'
```

Resultado esperado:

- `count(*) = 1334`.
- `sum(QuantidadeVersiculos) = 35527`.
- `sum(PesoLeitura) = 35527`.

## Importar No Neon Com Seguranca

Antes de Neon:

1. Confirmar backup/snapshot do branch/database.
2. Confirmar migration aplicada em ambiente de staging ou branch de teste.
3. Rodar `dotnet test ServicoPalavra.sln`.
4. Validar os JSONs com `python3 tools/validar-base-biblica-versiculos.py`.
5. Usar connection string apenas por variavel de ambiente ou secret manager.

Exemplo seguro, sem gravar connection string em arquivo:

```bash
env DATABASE_PROVIDER=postgresql \
  ConnectionStrings__DefaultConnection="$NEON_CONNECTION_STRING" \
  dotnet run --project src/ServicoPalavra.Api -- \
  --run-mode=import-base-biblica-v2
```

Nao use connection string real em `appsettings*.json`, README, arquivos `.http`, scripts versionados ou logs.

## Reversao

Se a importacao foi feita no banco errado ou antes da revisao final:

1. Preferir restaurar snapshot/backup do banco.
2. Se ainda nao houver dados dependentes de planos, remover registros da tabela `BaseBiblica` em uma transacao controlada.
3. Se ja houver planos usando `BaseBiblica`, nao apagar em massa; avaliar impacto das FKs em `PlanosBiblicosDiasCapitulos` e `PosicoesBiblicasUsuario`.
4. Nunca rodar limpeza em producao sem backup e revisao manual.

## Observacoes Pastorais Pendentes

As contagens revisadas foram conferidas contra o pacote validado. Ainda permanecem como pontos de atencao pastoral antes de uso publico:

- Salmos 2.
- Ester.
- Daniel.
- Joel.
- Baruc.
- Malaquias.
- Atos 10.
- Apocalipse 12.
- 2 Corintios 13.
- 3 Joao.
