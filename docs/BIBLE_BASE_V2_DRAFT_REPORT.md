# Base Biblica V2 - Draft Report

Este relatorio acompanha os arquivos `docs/examples/base-biblica-livros.v2.json` e `docs/examples/base-biblica-capitulos.v2.json`.

## Totais

- Total de livros: 73
- Total de livros do Antigo Testamento: 46
- Total de livros do Novo Testamento: 27
- Total de capitulos gerados: 1334
- Transcricao de capitulos ou versiculos incluida: nao

## Livros Por Grupo Pastoral

- Cartas Joaninas: 3 livros, 7 capitulos (1 Joao, 2 Joao, 3 Joao)
- Evangelhos: 4 livros, 89 capitulos (Joao, Mateus, Marcos, Lucas)
- Cartas Apostolicas: 20 livros, 164 capitulos (Atos, Romanos, 1 Corintios, 2 Corintios, Galatas, Efesios, Filipenses, Colossenses, 1 Tessalonicenses, 2 Tessalonicenses, 1 Timoteo, 2 Timoteo, Tito, Filemon, Hebreus, Tiago, 1 Pedro, 2 Pedro, Judas, Apocalipse)
- Pentateuco: 5 livros, 187 capitulos (Genesis, Exodo, Levitico, Numeros, Deuteronomio)
- Historicos: 12 livros, 255 capitulos (Josue, Juizes, Rute, 1 Samuel, 2 Samuel, 1 Reis, 2 Reis, 1 Cronicas, 2 Cronicas, Esdras, Neemias, Ester)
- Profetas: 17 livros, 252 capitulos (Isaias, Jeremias, Lamentacoes, Ezequiel, Daniel, Oseias, Joel, Amos, Abdias, Jonas, Miqueias, Naum, Habacuc, Sofonias, Ageu, Zacarias, Malaquias)
- Sapienciais: 5 livros, 243 capitulos (Jo, Salmos, Proverbios, Eclesiastes, Cantico dos Canticos)
- Deuterocanonicos: 7 livros, 137 capitulos (Tobias, Judite, Sabedoria, Eclesiastico, Baruc, 1 Macabeus, 2 Macabeus)

## Fontes Internas Usadas

- `src/ServicoPalavra.Domain/Entities/BaseBiblica.cs`, para confirmar os campos usados pela entidade persistida.
- `docs/BIBLE_BASE_IMPORT.md`, para confirmar a ordem pastoral V2.
- `docs/examples/base-biblica.sample.json`, para preservar o formato esperado para capitulos.
- Fixtures de testes de integracao, para confirmar os nomes de grupos ja exercitados pelo backend.

O arquivo antigo da V1 com plano livre de 600 dias nao foi localizado neste repositorio. Por isso, a comparacao pastoral contra esse material deve ser feita manualmente quando a fonte for fornecida.

## Decisoes Sobre Atos E Apocalipse

- `Atos` foi classificado em `Cartas Apostolicas`, subgrupo `Historia da Igreja Primitiva`, por funcionar pastoralmente como ponte entre Evangelhos e vida apostolica. Esta decisao precisa de validacao manual porque Atos nao e uma carta.
- `Apocalipse` foi classificado em `Cartas Apostolicas`, subgrupo `Literatura Apocaliptica`, conforme a revisao rigorosa da BaseBiblica V2.

## Decisoes Sobre Deuterocanonicos

Foram tratados como grupo pastoral proprio os sete livros deuterocanonicos tradicionalmente listados na Biblia Catolica:

- Tobias
- Judite
- Sabedoria
- Eclesiastico
- Baruc
- 1 Macabeus
- 2 Macabeus

As adicoes de Ester e Daniel nao foram separadas como livros proprios. Elas foram consideradas dentro de `Ester` e `Daniel`, conforme a numeracao de capitulos escolhida neste draft.

## Problemas De Numeracao Biblica Que Precisam De Revisao

- `Ester` foi gerado com 16 capitulos para contemplar a tradicao catolica com adicoes gregas. Algumas edicoes modernas organizam as adicoes por letras/secoes ou mantem 10 capitulos; revisar antes de importar.
- `Daniel` foi gerado com 14 capitulos, incluindo as tradicoes catolicas de Susana e Bel e o Dragao. Confirmar se a fonte pastoral usara capitulos 13 e 14 ou outra organizacao.
- `Joel` foi gerado com 4 capitulos, conforme a fonte canonica adotada na revisao rigorosa.
- `Salmos` foi gerado com 150 capitulos. Confirmar se a numeracao liturgica/pastoral adotada seguira a numeracao hebraica comum ou outra referencia.
- `Baruc` foi gerado com 6 capitulos, incluindo a Carta de Jeremias como capitulo 6. Confirmar a convencao da fonte pastoral.
- `Malaquias` foi gerado com 3 capitulos, conforme a fonte canonica adotada na revisao rigorosa.

## Validacao Rigorosa

O arquivo `docs/BIBLE_BASE_V2_STRICT_VALIDATION_REPORT.md` e a referencia tecnica mais recente. Ele registra a comparacao livro por livro e o teste automatizado que valida a BaseBiblica V2.

## Itens Para Validacao Manual Antes De Importar No Neon

- Confirmar a fonte biblica catolica oficial usada pelo projeto para nomes e numeracao.
- Comparar contra o plano antigo de 600 dias quando o arquivo V1 estiver disponivel.
- Revisar a classificacao pastoral de `Atos` e `Apocalipse`.
- Revisar se `Sabedoria`, `Eclesiastico` e `Baruc` devem permanecer em `Deuterocanonicos` ou aparecer tambem em percursos sapienciais/profeticos.
- Validar se o campo `tempoEstimadoMinutos` deve continuar como estimativa simples ou ser recalculado por extensao real dos capitulos.
- Definir estrategia de importacao idempotente para `BaseBiblica` antes de tocar no banco.
- Executar importacao primeiro em ambiente local ou staging, nunca diretamente no Neon de producao.

## Garantias Deste Draft

- Nao contem transcricao de capitulos ou versiculos.
- Nao contem credenciais sensiveis.
- Nao cria migration.
- Nao importa dados no banco.
