using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AnonymizeCompletedSurveyResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "QuestaoRespostas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.Sql("""
                UPDATE "QuestaoRespostas" AS resposta
                SET "Token" = NULL
                FROM "Convites" AS convite
                WHERE resposta."Token" = convite."Token"
                    AND convite."Status" = 3;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new InvalidOperationException(
                "This migration irreversibly anonymizes completed survey responses; restoring the association would violate the privacy requirement.");
        }
    }
}
