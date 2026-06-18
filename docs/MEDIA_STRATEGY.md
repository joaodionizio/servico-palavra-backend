# Estrategia De Midia

A V2 do Servico da Palavra e formacao-first. Videos, audios e materiais de apoio existem para servir a experiencia formativa, mas o backend nao deve virar um servidor de arquivos nesta fase.

## Decisao Atual

- Videos nao serao armazenados diretamente no servidor da API.
- Videos nao serao armazenados diretamente no banco.
- Videos devem ser cadastrados por link externo, inicialmente YouTube ou Google Drive.
- Audios sao necessarios na plataforma.
- Audios nao devem ser armazenados no banco.
- Na V2 inicial, audios podem ser cadastrados por link externo, preferencialmente Google Drive.
- O banco deve armazenar apenas metadados.

## O Que O Banco Armazena

Para conteudos e materiais, o banco deve guardar metadados como:

- titulo;
- slug;
- descricao;
- resumo;
- tipo;
- origem;
- URL externa;
- duracao;
- thumbnail;
- categoria;
- autor/criador;
- status publicado;
- destaque;
- ordem;
- datas de criacao, atualizacao e publicacao.

O banco nao deve guardar binarios de video, audio, imagem ou documento.

## Origem Dos Conteudos

Na fase atual, a plataforma trabalha com links externos:

- YouTube para videos publicos ou incorporaveis.
- Google Drive para videos, audios ou arquivos quando for adequado.
- Outras origens externas somente quando houver validacao de seguranca e decisao documentada.

Links externos precisam ser tratados como entrada nao confiavel. A API deve continuar validando URL, protocolo HTTPS e origem permitida.

## Audios

Audios fazem parte da estrategia da plataforma, especialmente para formacoes, meditacoes, aulas e materiais pastorais.

Na V2 inicial:

- audio deve ser cadastrado como conteudo ou material por URL externa;
- Google Drive e a opcao preferencial enquanto nao houver storage dedicado;
- o banco armazena apenas metadados e link;
- permissoes reais do arquivo externo precisam ser revisadas manualmente.

## Futuro Storage Externo

Em uma fase futura, a plataforma podera implementar upload direto para storage externo, como:

- Cloudflare R2;
- Azure Blob Storage;
- Amazon S3;
- servico equivalente.

Essa decisao futura deve ser implementada sem gravar arquivos no banco e sem depender do filesystem local do servidor. O banco continuaria armazenando metadados, URL publica/assinada, provider, tamanho, mime type e campos operacionais necessarios.

## Regras Arquiteturais

- A API nao deve depender de armazenamento local persistente para midia.
- O Render/servidor da API nao deve ser fonte primaria de arquivos.
- O banco nao deve receber base64 de audio, video ou imagem.
- Upload direto, quando existir, deve ser tratado como integracao de infraestrutura, nao como regra de dominio.
- Secrets de providers de storage devem ficar fora do repositorio.
- Nao criar migration ou entidade nova ate existir requisito operacional claro para upload.

## Pendencias De Validacao

- Definir politica pastoral/editorial para uso de Google Drive.
- Definir se audios serao `Conteudo` principal, `MaterialApoio` ou ambos conforme o caso.
- Definir regras de privacidade/compartilhamento dos arquivos no Drive.
- Definir CSP e estrategia de embed no frontend.
- Definir provider de storage externo apenas quando upload direto entrar no escopo.
