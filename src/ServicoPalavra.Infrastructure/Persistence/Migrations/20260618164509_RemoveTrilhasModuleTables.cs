using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicoPalavra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTrilhasModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrilhasConteudos");

            migrationBuilder.DropTable(
                name: "TrilhasFormacao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrilhasFormacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Destaque = table.Column<bool>(type: "boolean", nullable: false),
                    ImagemUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: true),
                    Publicado = table.Column<bool>(type: "boolean", nullable: false),
                    PublicadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resumo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false)
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
                name: "TrilhasConteudos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConteudoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrilhaFormacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
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
        }
    }
}
