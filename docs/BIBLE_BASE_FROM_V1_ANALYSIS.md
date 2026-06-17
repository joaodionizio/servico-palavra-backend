# Análise da Base Bíblica a partir do plano V1

## Origem e escopo

Este relatório trata o array antigo `leituras` apenas como fonte textual de apoio para a futura `BaseBiblica` da V2. Nenhuma conexão com serviços externos, Neon, PostgreSQL ou backend foi realizada.

A busca no repositório não encontrou um arquivo local com `const leituras = [[...]]` idêntico ao material fornecido nesta tarefa. O frontend legado consulta a tabela `plano_leitura_dias`, mas a lista completa usada nesta conversão veio do array informado na solicitação.

## JSON intermediário criado

Arquivo: `docs/examples/plano-biblico-v1-600-dias.json`

Formato por item:

```json
{
  "mesNumero": 1,
  "diaNumero": 1,
  "leituraTexto": "1 João 1",
  "salmoNumero": 1
}
```

Observação: `salmoNumero` foi inferido como ciclo de 1 a 150 repetido a cada 150 dias, resultando em quatro ciclos nos 600 dias. Esse ponto deve ser validado contra a regra pastoral original antes de importar dados oficiais.

## Contagens

- Total de meses/blocos: 20
- Dias por bloco: 30
- Total de dias extraídos: 600
- Leituras "Meditação livre": 35
- Leituras simples: 271
- Leituras com múltiplos livros: 24
- Leituras com intervalo de capítulos: 81
- Leituras com exceções explícitas: 2
- Casos recomendados para revisão manual: 63
- Total de livros da Bíblia católica: 73
- Livros católicos identificados neste plano V1: 68
- Livros católicos não encontrados no plano V1: 5

## Livros católicos identificados no plano V1

- 1 Coríntios
- 1 Crônicas
- 1 João
- 1 Macabeus
- 1 Pedro
- 1 Reis
- 1 Samuel
- 1 Tessalonicenses
- 1 Timóteo
- 2 Coríntios
- 2 Crônicas
- 2 João
- 2 Macabeus
- 2 Pedro
- 2 Reis
- 2 Samuel
- 2 Tessalonicenses
- 2 Timóteo
- 3 João
- Abdias
- Ageu
- Amós
- Apocalipse
- Atos
- Baruc
- Colossenses
- Daniel
- Deuteronômio
- Eclesiastes
- Efésios
- Esdras
- Ester
- Êxodo
- Ezequiel
- Filemon
- Filipenses
- Gálatas
- Gênesis
- Habacuc
- Hebreus
- Isaías
- Jeremias
- Jó
- João
- Joel
- Jonas
- Josué
- Judas
- Judite
- Juízes
- Lamentações
- Levítico
- Lucas
- Malaquias
- Marcos
- Mateus
- Miqueias
- Naum
- Neemias
- Números
- Oseias
- Romanos
- Rute
- Sabedoria
- Tiago
- Tito
- Tobias
- Zacarias

## Livros católicos não encontrados no plano V1

- Salmos
- Provérbios
- Cântico dos Cânticos
- Eclesiástico
- Sofonias

## Abreviações encontradas

- 1 Crôn -> 1 Crônicas
- 1 Mac -> 1 Macabeus
- 1 Sm -> 1 Samuel
- 2 Crôn -> 2 Crônicas
- 2 Mac -> 2 Macabeus
- 2 Pe -> 2 Pedro
- 2 Sm -> 2 Samuel
- Deut -> Deuteronômio
- Ecl -> Eclesiastes
- Est -> Ester
- Hab -> Habacuc
- Jer -> Jeremias
- Jon -> Jonas
- Jud -> Judite
- Lam -> Lamentações
- Lev -> Levítico
- Mal -> Malaquias
- Sab -> Sabedoria
- Zac -> Zacarias

## Exemplos de leituras simples

- Mês 1, dia 1: 1 João 1
- Mês 1, dia 2: 1 João 2
- Mês 1, dia 3: 1 João 3
- Mês 1, dia 4: 1 João 4
- Mês 1, dia 5: 1 João 5
- Mês 1, dia 6: João 1
- Mês 1, dia 7: João 2
- Mês 1, dia 8: João 3
- Mês 1, dia 9: João 4
- Mês 1, dia 10: João 5
- Mês 1, dia 11: João 6
- Mês 1, dia 12: João 7

## Exemplos de leituras com múltiplos livros

- Mês 9, dia 3: 1 Pedro 5, 2 Pe 1
- Mês 14, dia 16: 1 Sm 31, 2 Sm 1
- Mês 16, dia 4: Amós 9, Oseias 1
- Mês 16, dia 11: Oseias 14, Isaías 1
- Mês 17, dia 3: Miqueias 7, Naum 1-2
- Mês 17, dia 4: Naum 3, Hab 1-2
- Mês 17, dia 5: Hab 3, Jer 1-2
- Mês 17, dia 28: Jeremias 52, Lam 1
- Mês 18, dia 24: Ezequiel 48, Abdias 1
- Mês 19, dia 10: 1 Crôn 28-29, 2 Crôn 1
- Mês 19, dia 22: 2 Crôn 35-36, Esdras 1
- Mês 20, dia 1: Ageu 1-2, Zac 1-7

## Exemplos de leituras com intervalo de capítulos

- Mês 11, dia 1: Gênesis 1-3
- Mês 11, dia 2: Gênesis 4-6
- Mês 11, dia 3: Gênesis 7-9
- Mês 11, dia 4: Gênesis 10-12
- Mês 11, dia 5: Gênesis 13-15
- Mês 11, dia 6: Gênesis 16-18
- Mês 11, dia 7: Gênesis 19-21
- Mês 11, dia 8: Gênesis 22-24
- Mês 11, dia 9: Gênesis 25-27
- Mês 11, dia 10: Gênesis 28-30
- Mês 16, dia 1: Amós 1-3
- Mês 16, dia 2: Amós 4-6

## Leituras com exceções

- Mês 20, dia 9: Jó 6-14 (exceto 10)
- Mês 20, dia 10: Jó 15-23 (exceto 18)

## Casos que precisam de revisão manual

1. `Meditação livre` não representa livro/capítulo e não deve virar capítulo da `BaseBiblica` sem uma regra própria.
2. Leituras com múltiplos livros no mesmo dia precisam ser quebradas em múltiplos registros ou em uma entidade de agrupamento de leitura diária.
3. Leituras com abreviações devem ser normalizadas antes de qualquer seed oficial.
4. Leituras com intervalo, como `Gênesis 1-3`, precisam ser expandidas para capítulo a capítulo se a entidade alvo for um registro por capítulo.
5. Leituras com exceção, como `Jó 6-14 (exceto 10)`, exigem expansão cuidadosa removendo o capítulo excluído.
6. O plano V1 não cobre todos os 73 livros da Bíblia católica; os livros ausentes devem ser adicionados por uma fonte canônica revisada.
7. Há leituras deuterocanônicas e abreviações ambíguas, especialmente `Jud`, que ora representa Judite no plano antigo, enquanto `Judas` aparece escrito por extenso.
8. A ordem V1 é uma ordem de plano de leitura de 600 dias, não a ordem pastoral oficial da V2.

## Relação com a ordem pastoral V2

A ordem oficial da V2 deve ser construída a partir dos grupos pastorais definidos:

1. Cartas Joaninas
2. Evangelhos
3. Cartas Apostólicas
4. Pentateuco
5. Históricos
6. Profetas
7. Sapienciais
8. Deuterocanônicos

O plano V1 ajuda como fonte de nomes, capítulos, abreviações e casos de parsing, mas não deve ser importado como `Ordem` oficial da `BaseBiblica` sem reorganização.

## Recomendação técnica

Para transformar esta base em `BaseBiblica` oficial da V2, o caminho mais seguro é:

1. Criar um catálogo canônico dos 73 livros da Bíblia católica com nome oficial, abreviações aceitas, testamento, grupo e subgrupo.
2. Expandir cada `leituraTexto` para unidades normalizadas `Livro + Capitulo`.
3. Adicionar os livros ausentes do plano V1 a partir do catálogo canônico: Salmos, Provérbios, Cântico dos Cânticos, Eclesiástico, Sofonias.
4. Marcar entradas que vieram de `Meditação livre`, múltiplos livros ou exceções como pendentes de revisão.
5. Aplicar a ordem pastoral V2 manualmente, usando a V1 apenas como apoio.
6. Gerar um seed revisável em JSON/CSV antes de criar migration ou importar no banco.
