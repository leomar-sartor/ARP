using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotStandardizedSurveyCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriaNomeSnapshot",
                table: "Questoes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaNomeSnapshot",
                table: "Questoes");
        }
    }
}
