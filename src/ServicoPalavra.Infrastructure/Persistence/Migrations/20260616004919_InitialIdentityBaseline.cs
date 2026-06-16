using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicoPalavra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseBiblica",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Livro = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Capitulo = table.Column<int>(type: "integer", nullable: false),
                    Grupo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Subgrupo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Testamento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TempoEstimadoMinutos = table.Column<int>(type: "integer", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseBiblica", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasConteudo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    Cor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Icone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasConteudo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    FotoUrl = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UltimoAcessoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conteudos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Resumo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UrlThumbnail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "integer", nullable: true),
                    CategoriaConteudoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Publicado = table.Column<bool>(type: "boolean", nullable: false),
                    Destaque = table.Column<bool>(type: "boolean", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: true),
                    PublicadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conteudos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conteudos_CategoriasConteudo_CategoriaConteudoId",
                        column: x => x.CategoriaConteudoId,
                        principalTable: "CategoriasConteudo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conteudos_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanosBiblicosUsuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DuracaoDias = table.Column<int>(type: "integer", nullable: false),
                    DuracaoMeses = table.Column<int>(type: "integer", nullable: false),
                    DuracaoAnos = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFimPrevista = table.Column<DateOnly>(type: "date", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PlanoOrigemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModoCriacao = table.Column<int>(type: "integer", nullable: false),
                    OrdemInicio = table.Column<int>(type: "integer", nullable: false),
                    OrdemFim = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosBiblicosUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanosBiblicosUsuario_PlanosBiblicosUsuario_PlanoOrigemId",
                        column: x => x.PlanoOrigemId,
                        principalTable: "PlanosBiblicosUsuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanosBiblicosUsuario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosicoesBiblicasUsuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UltimaBaseBiblicaConcluidaId = table.Column<Guid>(type: "uuid", nullable: true),
                    UltimaOrdemConcluida = table.Column<int>(type: "integer", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosicoesBiblicasUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosicoesBiblicasUsuario_BaseBiblica_UltimaBaseBiblicaConclu~",
                        column: x => x.UltimaBaseBiblicaConcluidaId,
                        principalTable: "BaseBiblica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosicoesBiblicasUsuario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrilhasFormacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Resumo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImagemUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Publicado = table.Column<bool>(type: "boolean", nullable: false),
                    Destaque = table.Column<bool>(type: "boolean", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: true),
                    CriadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrilhasFormacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrilhasFormacao_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConteudoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favoritos_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MateriaisApoio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConteudoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MateriaisApoio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MateriaisApoio_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressosConteudo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConteudoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Percentual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    IniciadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcluidoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimoAcessoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressosConteudo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressosConteudo_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressosConteudo_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanosBiblicosDias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanoBiblicoUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaNumero = table.Column<int>(type: "integer", nullable: false),
                    MesNumero = table.Column<int>(type: "integer", nullable: false),
                    DataPrevista = table.Column<DateOnly>(type: "date", nullable: true),
                    LeiturasTexto = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    SalmoNumero = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosBiblicosDias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanosBiblicosDias_PlanosBiblicosUsuario_PlanoBiblicoUsuari~",
                        column: x => x.PlanoBiblicoUsuarioId,
                        principalTable: "PlanosBiblicosUsuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrilhasConteudos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrilhaFormacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConteudoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrilhasConteudos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrilhasConteudos_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrilhasConteudos_TrilhasFormacao_TrilhaFormacaoId",
                        column: x => x.TrilhaFormacaoId,
                        principalTable: "TrilhasFormacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanosBiblicosDiasCapitulos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanoBiblicoDiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseBiblicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosBiblicosDiasCapitulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanosBiblicosDiasCapitulos_BaseBiblica_BaseBiblicaId",
                        column: x => x.BaseBiblicaId,
                        principalTable: "BaseBiblica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanosBiblicosDiasCapitulos_PlanosBiblicosDias_PlanoBiblico~",
                        column: x => x.PlanoBiblicoDiaId,
                        principalTable: "PlanosBiblicosDias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressosLeitura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanoBiblicoDiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Concluido = table.Column<bool>(type: "boolean", nullable: false),
                    ConcluidoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressosLeitura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressosLeitura_PlanosBiblicosDias_PlanoBiblicoDiaId",
                        column: x => x.PlanoBiblicoDiaId,
                        principalTable: "PlanosBiblicosDias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressosLeitura_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseBiblica_Ordem",
                table: "BaseBiblica",
                column: "Ordem",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasConteudo_Slug",
                table: "CategoriasConteudo",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conteudos_CategoriaConteudoId",
                table: "Conteudos",
                column: "CategoriaConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_Conteudos_CriadoPorUsuarioId",
                table: "Conteudos",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Conteudos_Slug",
                table: "Conteudos",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_ConteudoId",
                table: "Favoritos",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_UsuarioId_ConteudoId",
                table: "Favoritos",
                columns: new[] { "UsuarioId", "ConteudoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MateriaisApoio_ConteudoId",
                table: "MateriaisApoio",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_Perfis_Nome",
                table: "Perfis",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanosBiblicosDias_PlanoBiblicoUsuarioId",
                table: "PlanosBiblicosDias",
                column: "PlanoBiblicoUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosBiblicosDiasCapitulos_BaseBiblicaId",
                table: "PlanosBiblicosDiasCapitulos",
                column: "BaseBiblicaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosBiblicosDiasCapitulos_PlanoBiblicoDiaId",
                table: "PlanosBiblicosDiasCapitulos",
                column: "PlanoBiblicoDiaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosBiblicosUsuario_PlanoOrigemId",
                table: "PlanosBiblicosUsuario",
                column: "PlanoOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosBiblicosUsuario_UsuarioId",
                table: "PlanosBiblicosUsuario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PosicoesBiblicasUsuario_UltimaBaseBiblicaConcluidaId",
                table: "PosicoesBiblicasUsuario",
                column: "UltimaBaseBiblicaConcluidaId");

            migrationBuilder.CreateIndex(
                name: "IX_PosicoesBiblicasUsuario_UsuarioId",
                table: "PosicoesBiblicasUsuario",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosConteudo_ConteudoId",
                table: "ProgressosConteudo",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosConteudo_UsuarioId_ConteudoId",
                table: "ProgressosConteudo",
                columns: new[] { "UsuarioId", "ConteudoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosLeitura_PlanoBiblicoDiaId",
                table: "ProgressosLeitura",
                column: "PlanoBiblicoDiaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosLeitura_UsuarioId_PlanoBiblicoDiaId",
                table: "ProgressosLeitura",
                columns: new[] { "UsuarioId", "PlanoBiblicoDiaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrilhasConteudos_ConteudoId",
                table: "TrilhasConteudos",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_TrilhasConteudos_TrilhaFormacaoId_ConteudoId",
                table: "TrilhasConteudos",
                columns: new[] { "TrilhaFormacaoId", "ConteudoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrilhasFormacao_CriadoPorUsuarioId",
                table: "TrilhasFormacao",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TrilhasFormacao_Slug",
                table: "TrilhasFormacao",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Usuarios",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "MateriaisApoio");

            migrationBuilder.DropTable(
                name: "Perfis");

            migrationBuilder.DropTable(
                name: "PlanosBiblicosDiasCapitulos");

            migrationBuilder.DropTable(
                name: "PosicoesBiblicasUsuario");

            migrationBuilder.DropTable(
                name: "ProgressosConteudo");

            migrationBuilder.DropTable(
                name: "ProgressosLeitura");

            migrationBuilder.DropTable(
                name: "TrilhasConteudos");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BaseBiblica");

            migrationBuilder.DropTable(
                name: "PlanosBiblicosDias");

            migrationBuilder.DropTable(
                name: "Conteudos");

            migrationBuilder.DropTable(
                name: "TrilhasFormacao");

            migrationBuilder.DropTable(
                name: "PlanosBiblicosUsuario");

            migrationBuilder.DropTable(
                name: "CategoriasConteudo");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
