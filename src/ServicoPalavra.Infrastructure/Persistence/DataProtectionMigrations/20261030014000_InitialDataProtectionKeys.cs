using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicoPalavra.Infrastructure.Persistence.DataProtectionMigrations;

[DbContext(typeof(DataProtectionDbContext))]
[Migration("20261030014000_InitialDataProtectionKeys")]
public sealed class InitialDataProtectionKeys : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DataProtectionKeys",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .Annotation("Sqlite:Autoincrement", true),
                FriendlyName = table.Column<string>(type: "TEXT", nullable: true),
                Xml = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DataProtectionKeys");
    }
}
