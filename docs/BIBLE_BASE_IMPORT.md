# Bible Base Import

A geracao do plano biblico depende da tabela `BaseBiblica`, ordenada pelo campo `Ordem`. A ordem pastoral esperada e:

1. Cartas Joaninas
2. Evangelhos
3. Cartas Apostolicas
4. Pentateuco
5. Historicos
6. Profetas
7. Sapienciais
8. Deuterocanonicos

Este repositorio nao inclui uma base pastoral completa porque a fonte oficial ainda nao foi fornecida. Use importacao estruturada revisada pastoralmente antes de producao.

Formato JSON recomendado:

```json
[
  {
    "ordem": 1,
    "livro": "1 Joao",
    "capitulo": 1,
    "grupo": "Cartas Joaninas",
    "subgrupo": null,
    "testamento": "Novo",
    "tempoEstimadoMinutos": 4,
    "ativo": true
  }
]
```

Arquivo de exemplo: [docs/examples/base-biblica.sample.json](examples/base-biblica.sample.json).

Regras:

- `Ordem` e unica e controla toda a geracao.
- O cliente nao envia ordem de livros/capitulos.
- O plano distribui capitulos ativos entre os dias calculados por duracao.
- Se a duracao for maior que a quantidade de capitulos disponiveis, a API gera apenas dias com leitura.
- Para continuar, a API inicia em `UltimaOrdemConcluida + 1`.
- Para recomecar, a API inicia em `Ordem = 1`.

Os testes usam uma fixture representativa dos oito grupos; isso nao equivale a seed pastoral completo.
