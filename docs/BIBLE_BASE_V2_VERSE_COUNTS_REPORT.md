# Relatório — Contagem de Versículos por Capítulo — BaseBiblica V2

## Status

Esta base é um **draft técnico de validação**. Ela **não deve ser importada no Neon** até revisão final pastoral e técnica.

## Fontes usadas

1. USCCB/NABRE — referência canônica e páginas por capítulo: https://bible.usccb.org/bible
2. Catholic Resources / Felix Just, S.J. — estatísticas por capítulo do Antigo Testamento baseadas na New American Bible: https://www.catholic-resources.org/Bible/OT-Statistics-NAB.htm
3. Catholic Resources / Felix Just, S.J. — estatísticas do Novo Testamento e tabela de divergências, com ajustes pela coluna NAB quando aplicável: https://www.catholic-resources.org/Bible/NT-Statistics-Greek.htm
4. Vatican/New American Bible — referência católica adicional: https://www.vatican.va/archive/ENG0839/_INDEX.HTM

Observação: os arquivos gerados **não contêm texto bíblico**, apenas metadados e contagens.

## Totais gerados

| Métrica | Total |
|---|---:|
| Livros | 73 |
| Livros do Antigo Testamento | 46 |
| Livros do Novo Testamento | 27 |
| Capítulos | 1334 |
| Versículos totais | 35527 |
| Versículos do Antigo Testamento | 27569 |
| Versículos do Novo Testamento | 7958 |

## Observação crítica sobre total de versículos

O total ficou em **35527 versículos**.

O ponto que mais afeta o total é **Salmos 2**:
- A página atual da USCCB/NABRE exibe 11 versículos numerados em Salmos 2.
- Catholic Resources registra uma nota sobre edições/tradições com 12 versículos em Salmos 2.
- Por preferência da fonte principal USCCB/NABRE, este draft adotou **Salmos 2 = 11**.
- Se a decisão pastoral for usar Salmos 2 = 12, o total geral passará de **35527** para **35528**.

## Top 10 capítulos mais longos

| # | Capítulo | Versículos |
|---:|---|---:|
| 1 | Salmos 119 | 176 |
| 2 | Daniel 3 | 100 |
| 3 | Números 7 | 89 |
| 4 | 1 Macabeus 10 | 89 |
| 5 | Lucas 1 | 80 |
| 6 | Mateus 26 | 75 |
| 7 | 1 Macabeus 11 | 74 |
| 8 | 1 Macabeus 9 | 73 |
| 9 | Neemias 7 | 72 |
| 10 | Salmos 78 | 72 |

## Top 10 capítulos mais curtos

| # | Capítulo | Versículos |
|---:|---|---:|
| 1 | Salmos 117 | 2 |
| 2 | Ester 10 | 3 |
| 3 | Salmos 131 | 3 |
| 4 | Salmos 133 | 3 |
| 5 | Salmos 134 | 3 |
| 6 | Salmos 123 | 4 |
| 7 | Jeremias 45 | 5 |
| 8 | Joel 3 | 5 |
| 9 | Oseias 3 | 5 |
| 10 | Salmos 15 | 5 |

## Top 10 livros com maior quantidade de versículos

| # | Livro | Capítulos | Versículos | Média |
|---:|---|---:|---:|---:|
| 1 | Salmos | 150 | 2525 | 16.83 |
| 2 | Gênesis | 50 | 1533 | 30.66 |
| 3 | Eclesiástico | 51 | 1372 | 26.9 |
| 4 | Jeremias | 52 | 1364 | 26.23 |
| 5 | Isaías | 66 | 1291 | 19.56 |
| 6 | Números | 36 | 1289 | 35.81 |
| 7 | Ezequiel | 48 | 1273 | 26.52 |
| 8 | Êxodo | 40 | 1213 | 30.32 |
| 9 | Lucas | 24 | 1151 | 47.96 |
| 10 | Mateus | 28 | 1071 | 38.25 |

## Pontos críticos e divergências

| Item | Decisão/observação |
|---|---|
| Salmos 2 | Adotado 11 versículos conforme página atual da USCCB/NABRE; Catholic Resources registra nota sobre tradição/edições com 12, gerando divergência de 1 no total de Salmos/OT. |
| Números 25 | Adotado 19 versículos conforme NAB; outras tradições contam 18 e deslocam palavras para Números 26:1. |
| 1 Reis 20 | Adotado 43 versículos; Catholic Resources observa erro em algumas edições impressas da NAB que omitiram a referência v.43. |
| Ester | Adotado 16 capítulos conforme estrutura católica/NAB; capítulos/acréscimos precisam de revisão pastoral antes de importação. |
| Daniel | Adotado 14 capítulos conforme estrutura católica/NAB; capítulos/acréscimos precisam de revisão pastoral antes de importação. |
| Ezequiel | A soma por capítulo usada é 1273; a linha textual extraída apresentou possível inconsistência com o total do livro, mas o total de grupo de Catholic Resources sustenta a soma por capítulo. |
| Atos 10 | Adotado 49 versículos para NAB/NABRE; Grego e várias traduções contam 48. |
| Atos 19 | Adotado 40 versículos; algumas traduções contam 41. |
| 2 Coríntios 13 | Adotado 13 versículos; algumas traduções contam 14. |
| 3 João | Adotado 15 versículos; algumas traduções contam 14. |
| Apocalipse 12 | Adotado 18 versículos; algumas tradições mapeiam o trecho final como 12:17b ou 13:1. |
| Joel | Adotado 4 capítulos no padrão NAB; outras tradições podem usar 3. |
| Baruc | Adotado 6 capítulos; ponto sensível dos deuterocanônicos. |
| Malaquias | Adotado 3 capítulos no padrão NAB; outras tradições podem usar 4. |

## Critério de validação executado

- 73 livros.
- 46 livros do Antigo Testamento.
- 27 livros do Novo Testamento.
- 1334 capítulos.
- Todos os capítulos possuem `quantidadeVersiculos > 0`.
- Todos os capítulos possuem `pesoLeitura > 0`.
- Todos os capítulos possuem `tempoEstimadoMinutos > 0`.
- Soma de versículos por livro bate com `totalVersiculos`.
- Ordem dos capítulos é sequencial de 1 a 1334.
- Nenhum texto bíblico foi salvo.
- Nenhum secret foi salvo.
- Nenhum acesso a banco foi feito.
- Nenhuma migration foi criada.

## Recomendação

Antes de importar no banco:
1. Validar manualmente Salmos 2.
2. Validar Ester e Daniel conforme a fonte católica oficial escolhida.
3. Validar Atos 10 e Apocalipse 12.
4. Decidir se o algoritmo de plano vai permitir dividir capítulos longos, como Salmos 119, Daniel 3, Números 7 e 1 Macabeus 10.
5. Só depois enriquecer a entidade `BaseBiblica` no backend e criar importação idempotente.
