using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicoPalavra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseBiblicaReadingWeights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PesoLeitura",
                table: "BaseBiblica",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeVersiculos",
                table: "BaseBiblica",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PesoLeitura",
                table: "BaseBiblica");

            migrationBuilder.DropColumn(
                name: "QuantidadeVersiculos",
                table: "BaseBiblica");
        }
    }
}
