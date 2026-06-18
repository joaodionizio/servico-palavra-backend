# Matriz De Modulos Do Backend

O backend V2 deve evoluir como plataforma de formacao biblica, espiritual e pastoral. Esta matriz descreve o estado atual sem propor novas funcionalidades.

| Modulo | Responsabilidade | Entidades principais | Services principais | Controllers principais | Status atual |
|---|---|---|---|---|---|
| Auth | Cadastro, login, logout, sessao e usuario autenticado. | `ApplicationUser` | `AuthService`, `CurrentUser` | `AuthController` | Implementado. |
| Users | Representar usuarios da plataforma via Identity. | `ApplicationUser` | `UserManager`, `IUsuarioRepository` | Indireto via Auth/Admin futuro | Parcial; sem endpoints administrativos dedicados de usuarios. |
| Roles | Permissoes por papel no Identity. | `IdentityRole<Guid>`, roles `Usuario`, `Admin`, `Coordenador` | `DatabaseSeeder`, Identity | Controllers com `[Authorize(Roles = "Admin")]` | Implementado para Admin; Coordenador pendente de regras especificas. |
| Dashboard | Resumo do usuario autenticado. | Depende de progresso/favoritos/plano | `DashboardService` | `DashboardController` | Inicial; possui TODO para agregacoes mais ricas. |
| Conteudos/Formacoes | Catalogo de videos, audios, textos e materiais formativos publicados. Midias sao cadastradas por metadados e URL externa. | `Conteudo`, `MaterialApoio`, `CategoriaConteudo` | `ConteudoService` | `ConteudosController` | CRUD admin inicial e listagem publica implementados. Upload direto nao implementado. |
| Categorias | Organizacao de conteudos/formacoes. | `CategoriaConteudo` | `CategoriaService` | `CategoriasController` | Implementado de forma inicial. |
| Trilhas | Sequencias formativas compostas por conteudos. | `TrilhaFormacao`, `TrilhaConteudo`, `Conteudo` | `TrilhaService` | `TrilhasController` | CRUD admin inicial, associacao e reordenacao implementados. |
| Favoritos | Conteudos salvos pelo usuario. | `Favorito`, `Conteudo` | `FavoritoService` | `FavoritosController` | Implementado. Cobertura cruzada dedicada ainda pendente. |
| ProgressoConteudo | Conclusao/progresso de conteudos formativos. | `ProgressoConteudo`, `Conteudo` | `ProgressoService` | `ProgressoController` | Implementado de forma inicial. Cobertura cruzada dedicada ainda pendente. |
| BibleBase | Catalogo unico de livros/capitulos biblicos com peso de leitura. | `BaseBiblica` | `BaseBiblicaV2Importer` | Modo operacional de importacao | Importador controlado implementado; nao roda no startup normal. |
| BiblePlans | Geracao e alteracao de planos biblicos do usuario. | `PlanoBiblicoUsuario`, `PlanoBiblicoDia`, `PlanoBiblicoDiaCapitulo` | `BiblePlanGeneratorService`, `PlanoBiblicoService` | `PlanosBiblicosController` | Implementado com BaseBiblica real, sequencia pastoral e peso de leitura. |
| ReadingProgress | Conclusao/desmarcacao de dias e posicao biblica do usuario. | `ProgressoLeitura`, `PosicaoBiblicaUsuario` | `PlanoBiblicoService` | `PlanosBiblicosController` | Implementado com isolamento por usuario e transacao. |
| Admin | Operacoes administrativas de catalogo. | Conteudos, categorias, trilhas e roles | Services dos respectivos modulos | Rotas `api/admin/*` | Parcial; cobre conteudos, categorias e trilhas. Usuarios/admin avancado pendente. |

## Observacoes Arquiteturais

- O Plano Biblico nao deve ser tratado como nucleo unico da plataforma.
- Conteudos, trilhas, favoritos e progresso sao modulos de produto de primeira classe, ainda que menos maduros que o Plano Biblico.
- Videos e audios nao devem ser armazenados no banco; a fase atual usa URLs externas e metadados.
- Novos modulos devem seguir o padrao atual: controller fino, regra na Application, persistencia na Infrastructure, entidades no Domain.
- Evitar criar abstracoes novas ate haver repeticao real entre modulos.
