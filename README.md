# Serviço da Palavra — Backend

**API REST em C# / ASP.NET Core (.NET 8) para uma plataforma de formação bíblica e espiritual.**

O Serviço da Palavra reúne conteúdos formativos, categorias, favoritos, acompanhamento de progresso e planos de leitura bíblica. Este repositório contém o backend V2, responsável pelas regras de negócio, autenticação, autorização e persistência.

[Frontend da plataforma](https://github.com/joaodionizio/servico-palavra-frontend) · [Contrato da API](docs/API.md) · [Arquitetura](docs/ARCHITECTURE.md) · [Segurança](docs/SECURITY.md)

## Funcionalidades

- Cadastro, login, logout e atualização do perfil.
- Gestão e publicação de conteúdos e categorias.
- Favoritos e progresso individual de conteúdos.
- Dashboard com informações do usuário.
- Geração de planos bíblicos com sequência pastoral, distribuição da carga de leitura e continuidade do progresso.
- Administração com permissões por perfil: `Usuario`, `Admin` e `Coordenador`.

## Decisões de engenharia

| Decisão | Aplicação no projeto |
| --- | --- |
| Arquitetura em camadas | Controllers na API, casos de uso na Application, entidades na Domain e persistência na Infrastructure. |
| Sessão com Identity e cookies HttpOnly | Autenticação sem retornar token de sessão no corpo da resposta. Escritas exigem token CSRF. |
| Autorização no backend | Permissões administrativas verificadas na API; identificação do usuário obtida da sessão. |
| EF Core com dois providers | SQLite para desenvolvimento/testes e PostgreSQL para produção. |
| Plano bíblico baseado em catálogo | Capítulos cadastrados e sequência pastoral orientam a geração; testes verificam ordem, duração e continuidade. |
| Mídia por URLs externas | A API armazena metadados e referências, evitando hospedar arquivos de áudio e vídeo nesta fase. |

Consulte os detalhes e limites em [Arquitetura](docs/ARCHITECTURE.md), [Plano bíblico](docs/BIBLE_PLAN.md) e [Estratégia de mídia](docs/MEDIA_STRATEGY.md).

## Stack

- **Backend:** C#, .NET 8, ASP.NET Core.
- **Persistência:** Entity Framework Core 8, PostgreSQL e SQLite.
- **Autenticação:** ASP.NET Core Identity, cookies HttpOnly e antiforgery.
- **API e operação:** Swagger/OpenAPI em desenvolvimento, middleware global de exceções, CORS, rate limiting e endpoint de health check.
- **Empacotamento:** Dockerfile.

## Organização da solução

| Projeto | Responsabilidade |
| --- | --- |
| [ServicoPalavra.Api](src/ServicoPalavra.Api) | Controllers, configuração HTTP, autenticação/autorização e middleware. |
| [ServicoPalavra.Application](src/ServicoPalavra.Application) | Contratos, DTOs e serviços com os fluxos de negócio. |
| [ServicoPalavra.Domain](src/ServicoPalavra.Domain) | Entidades e enums, sem referência aos demais projetos. |
| [ServicoPalavra.Infrastructure](src/ServicoPalavra.Infrastructure) | EF Core, repositórios, Identity, importação e seed. |
| [Testes unitários](tests/ServicoPalavra.UnitTests) | Validações e consistência dos dados de entrada. |
| [Testes de integração](tests/ServicoPalavra.IntegrationTests) | API, segurança, persistência, importação e geração de planos. |

## Executar localmente

Pré-requisitos: **SDK .NET 8** e Git. Execute os comandos a partir da raiz do repositório.

```bash
git clone https://github.com/joaodionizio/servico-palavra-backend.git
cd servico-palavra-backend
dotnet restore ServicoPalavra.sln
dotnet build ServicoPalavra.sln
dotnet dev-certs https --trust
```

Inicie com o perfil HTTPS:

```bash
dotnet run --project src/ServicoPalavra.Api/ServicoPalavra.Api.csproj --launch-profile https
```

- Swagger: https://127.0.0.1:5051/swagger
- Health check: https://127.0.0.1:5051/health
- O provider padrão é SQLite. Fora do ambiente `Testing`, SQLite usa `EnsureCreated`; PostgreSQL usa migrations no startup.
- O certificado de desenvolvimento pode exigir autorização do sistema e configuração adicional em Linux.

### Administrador local (opcional)

O seed cria as roles. Um administrador só é criado quando as três configurações abaixo estão preenchidas. Em Bash, forneça-as por variáveis de ambiente:

```bash
export INITIAL_ADMIN_EMAIL="admin@servicopalavra.local"
export INITIAL_ADMIN_NAME="Admin Local"
read -r -s -p "Senha do administrador local: " INITIAL_ADMIN_PASSWORD
printf '\n'
export INITIAL_ADMIN_PASSWORD
dotnet run --project src/ServicoPalavra.Api/ServicoPalavra.Api.csproj --launch-profile https
```

Para outros shells, configure as mesmas três variáveis de ambiente antes de iniciar a API.

Após encerrar a API, remova as variáveis da sessão com `unset INITIAL_ADMIN_EMAIL INITIAL_ADMIN_NAME INITIAL_ADMIN_PASSWORD`. Se o e-mail já existir, o seed não altera o usuário.

A inicialização atual carrega JSON, variáveis de ambiente e argumentos; não carrega `user-secrets` nem arquivos `.env` automaticamente.

### Fluxo de autenticação

1. Obtenha um token com `GET /api/auth/csrf` e preserve os cookies.
2. Envie o token no header `X-CSRF-TOKEN` nas operações de escrita, inclusive login, cadastro e logout.
3. Após autenticar, preserve o cookie de sessão nas requisições seguintes.

Consulte [API](docs/API.md) e [Segurança](docs/SECURITY.md) para os contratos e configurações.

## Testes automatizados

```bash
dotnet test ServicoPalavra.sln
```

Exemplos de comportamentos verificados na suíte:

- [Segurança](tests/ServicoPalavra.IntegrationTests/SecurityTests.cs): login, bloqueio por tentativas inválidas, CSRF e restrição de operações administrativas.
- [Geração de planos](tests/ServicoPalavra.IntegrationTests/BiblePlanGenerationTests.cs): ordem pastoral, duração, capítulos existentes e continuidade/reinício.
- [URLs externas](tests/ServicoPalavra.UnitTests/ExternalUrlValidatorTests.cs): rejeição de esquemas inadequados e validação de origem.

## Banco de dados e migrations

Para PostgreSQL, configure por variáveis de ambiente:

```bash
export DATABASE_PROVIDER=postgresql
export ConnectionStrings__DefaultConnection="<connection string do seu ambiente>"
```

Para criar uma migration, com a ferramenta `dotnet-ef` da linha 8.x instalada:

```bash
dotnet ef migrations add NomeDaMigration \
  --project src/ServicoPalavra.Infrastructure \
  --startup-project src/ServicoPalavra.Infrastructure \
  --output-dir Persistence/Migrations
```

Para aplicar:

```bash
dotnet ef database update \
  --project src/ServicoPalavra.Infrastructure \
  --startup-project src/ServicoPalavra.Infrastructure
```

A factory design-time permite usar Infrastructure como startup sem inicializar a API. Não versione connection strings reais. Mais detalhes em [Banco de dados](docs/DATABASE.md).

A base bíblica não é importada no startup normal. O modo de importação e seus requisitos estão em [Deploy](docs/DEPLOY.md) e [Exemplos](docs/examples/README.md).

## Deploy

O repositório possui [Dockerfile](Dockerfile) e instruções para Render com PostgreSQL/Neon.

Configure `ASPNETCORE_ENVIRONMENT`, `DATABASE_PROVIDER`, `ConnectionStrings__DefaultConnection` e `ALLOWED_ORIGINS` no provedor. As variáveis de administrador são destinadas ao bootstrap controlado. A API aceita `PORT`, `ASPNETCORE_URLS` ou `urls`.

Consulte [Deploy](docs/DEPLOY.md) para configuração, importação e validação operacional.

## Documentação

| Guia | Conteúdo |
| --- | --- |
| [Arquitetura](docs/ARCHITECTURE.md) | Camadas, dependências e fronteiras dos módulos. |
| [API](docs/API.md) | Endpoints e contratos. |
| [Segurança](docs/SECURITY.md) | Sessão, CSRF, autorização e configurações. |
| [Plano bíblico](docs/BIBLE_PLAN.md) | Sequência pastoral e regras de geração. |
| [Banco de dados](docs/DATABASE.md) | Providers e migrations. |
| [Deploy](docs/DEPLOY.md) | Configuração operacional e importação. |
| [Contrato de conteúdo](docs/CONTENT_CONTRACT.md) | Campos e regras dos conteúdos formativos. |
| [Mídia](docs/MEDIA_STRATEGY.md) | Metadados e URLs externas. |

