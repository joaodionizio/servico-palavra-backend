# Documentacao Tecnica Do Backend

Este diretorio documenta o backend V2 do Servico da Palavra.

A V2 deve ser entendida como uma plataforma catolica de formacao biblica, espiritual e pastoral. O Plano Biblico e um modulo central e importante, mas nao deve acoplar toda a arquitetura da plataforma.

## Stack Atual

- ASP.NET Core / .NET 8 LTS.
- Entity Framework Core 8.
- PostgreSQL/Neon em producao.
- SQLite local para desenvolvimento/testes, quando configurado.
- ASP.NET Core Identity.
- Autenticacao por cookie HttpOnly.
- CSRF para requisicoes de escrita.
- CORS restritivo por origem configurada.
- Arquitetura em camadas: Api, Application, Domain e Infrastructure.

## Modulos Oficiais

| Modulo | Documento principal |
|---|---|
| Auth, Users e Roles | `AUTHENTICATION_FLOW.md`, `security/SECURITY_INVENTORY.md` |
| Dashboard | `MODULES.md` |
| Conteudos/Formacoes | `API.md`, `MODULES.md` |
| Categorias | `API.md`, `MODULES.md` |
| Trilhas | `API.md`, `MODULES.md` |
| Favoritos | `API.md`, `MODULES.md` |
| ProgressoConteudo | `API.md`, `MODULES.md` |
| BibleBase | `BIBLE_BASE_IMPORT.md`, `examples/README.md` |
| BiblePlans | `BIBLE_PLAN_GENERATION.md` |
| ReadingProgress | `BIBLE_PLAN_GENERATION.md`, `SECURITY_MATRIX.md` |
| Admin | `API.md`, `SECURITY_MATRIX.md` |

## Mapa Dos Documentos

| Arquivo | Papel |
|---|---|
| `ARCHITECTURE.md` | Visao da arquitetura atual e limites entre camadas. |
| `API.md` | Lista resumida dos endpoints disponiveis. |
| `DATABASE.md` | Persistencia, providers e indices relevantes. |
| `MEDIA_STRATEGY.md` | Decisoes sobre videos, audios, links externos e storage futuro. |
| `BIBLE_PLAN_GENERATION.md` | Regra tecnica de geracao do plano biblico, peso de leitura e sequencia pastoral. |
| `BIBLE_BASE_IMPORT.md` | Como validar e importar a BaseBiblica de forma controlada. |
| `MODULES.md` | Matriz dos modulos oficiais do backend. |
| `SECURITY_MATRIX.md` | Matriz simples de acesso por endpoint. |
| `examples/README.md` | Classificacao dos JSONs de BaseBiblica, sequencia e material historico. |
| `security/*` | Inventario, relatorios e checklists de seguranca. |
| `sql/neon-baseline.sql` | Script documental de baseline PostgreSQL/Neon. |
| `tools/*` | Ferramentas offline de validacao/geracao documental. |

## Documentos Historicos Ou De Apoio

Alguns documentos registram etapas anteriores de revisao, migracao e validacao. Eles nao devem ser apagados sem justificativa, mas podem conter status historico:

- `BIBLE_BASE_FROM_V1_ANALYSIS.md`.
- `BIBLE_BASE_V2_DRAFT_REPORT.md`.
- `BIBLE_BASE_V2_STRICT_VALIDATION_REPORT.md`.
- `BIBLE_BASE_V2_VERSE_COUNTS_REPORT.md`.
- `POSTGRESQL_NEON_SETUP.md`.
- `security/IDENTITY_MIGRATION.md`.
- `security/SECURITY_REPORT.md`.
- `security/RELEASE_BLOCKERS.md`.

Quando houver conflito entre um documento historico e a documentacao operacional atual, considere atuais:

1. `ARCHITECTURE.md`.
2. `API.md`.
3. `DATABASE.md`.
4. `BIBLE_PLAN_GENERATION.md`.
5. `BIBLE_BASE_IMPORT.md`.
6. `MODULES.md`.
7. `SECURITY_MATRIX.md`.

## Regras De Manutencao

- Nao versionar secrets, connection strings reais, senhas, tokens, cookies ou bancos locais.
- Nao usar JWT/localStorage/sessionStorage para autenticacao.
- Nao alterar a regra pastoral do Plano Biblico sem justificativa documentada.
- Nao importar BaseBiblica automaticamente no startup normal da API.
- Nao armazenar videos, audios ou arquivos binarios diretamente no banco.
- Cadastrar midias por metadados e URL externa nesta fase.
- Marcar como `pendente de validacao` qualquer status que nao tenha evidencia em codigo, teste ou ambiente real.

## Pendencias Conhecidas

- Validacoes reais em PostgreSQL/Neon dependem de ambiente autorizado e secrets fora do repositorio.
- Testes de acesso cruzado existem para Plano Biblico; outros modulos de usuario ainda precisam de cobertura dedicada mais ampla.
- A documentacao de seguranca contem relatorios historicos que podem preservar status antigos para rastreabilidade.
