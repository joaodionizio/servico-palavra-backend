# Exemplos E Artefatos De Base Biblica

Esta pasta contem JSONs usados para documentacao, validacao e importacao controlada da BaseBiblica V2.

Nenhum arquivo desta pasta deve conter texto biblico completo, secrets, connection string, token ou credencial.

## Classificacao Dos Arquivos

| Arquivo | Classificacao | Uso atual |
|---|---|---|
| `base-biblica-capitulos-com-versiculos.v2.json` | Canonico operacional | Fonte revisada para importacao controlada da tabela `BaseBiblica`. Usado por importador e testes. |
| `base-biblica-livros-com-versiculos.v2.json` | Canonico de conferencia | Resumo por livro com totais de capitulos/versiculos. Usado para auditoria e validacao humana. |
| `base-biblica-versiculos.v2.json` | Canonico de conferencia | Tabela de quantidade de versiculos por capitulo. Usado como apoio para validar pesos. |
| `sequencia-leitura-biblica-v2.json` | Canonico pastoral/documental | Documenta a sequencia pastoral do Plano Biblico V2. A `BaseBiblica` permanece catalogo unico de capitulos. |
| `base-biblica-capitulos.v2.json` | Draft historico | Versao anterior sem a mesma finalidade operacional dos arquivos com versiculos. Manter para rastreabilidade ate decisao de arquivamento. |
| `base-biblica-livros.v2.json` | Draft historico | Versao anterior por livros. Manter para rastreabilidade ate decisao de arquivamento. |
| `base-biblica.sample.json` | Exemplo pequeno | Exemplo reduzido para documentacao/testes manuais. Nao representa a base completa. |
| `plano-biblico-v1-600-dias.json` | Historico V1 | Material antigo convertido, usado apenas como referencia pastoral/historica. Nao e fonte final unica da V2. |

## Regras

- O arquivo canonico para importacao de capitulos e `base-biblica-capitulos-com-versiculos.v2.json`.
- A sequencia pastoral nao duplica registros na `BaseBiblica`; ela define ocorrencias de leitura do plano.
- Salmos sao tratados pelo gerador como leitura paralela diaria, conforme documentado em `BIBLE_PLAN_GENERATION.md`.
- Arquivos draft/historicos nao devem ser usados para importar dados sem validacao manual.
- Qualquer divergencia de versificacao catolica deve ser registrada no relatorio correspondente antes de alterar dados.

## Pendencias De Organizacao

- Avaliar, em uma fase futura, mover drafts historicos para uma subpasta `archive/`.
- Antes de mover qualquer JSON, verificar referencias em testes, ferramentas e documentacao.
