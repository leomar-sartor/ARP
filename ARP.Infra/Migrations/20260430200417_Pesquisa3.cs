using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Pesquisa3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "Questoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataInicial",
                table: "Pesquisas",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataFinal",
                table: "Pesquisas",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<long>(
                name: "ColaboradorId",
                table: "Convites",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Convites",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PesquisaRascunhos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    PesquisaId = table.Column<long>(type: "bigint", nullable: false),
                    UltimaQuestaoRespondidaId = table.Column<long>(type: "bigint", nullable: false),
                    RespostasParciais = table.Column<string>(type: "text", nullable: true),
                    UltimaAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PesquisaRascunhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PesquisaRascunhos_Pesquisas_PesquisaId",
                        column: x => x.PesquisaId,
                        principalTable: "Pesquisas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Convites_ColaboradorId",
                table: "Convites",
                column: "ColaboradorId");

            migrationBuilder.CreateIndex(
                name: "IX_PesquisaRascunhos_PesquisaId",
                table: "PesquisaRascunhos",
                column: "PesquisaId");

            migrationBuilder.CreateIndex(
                name: "IX_PesquisaRascunhos_Token",
                table: "PesquisaRascunhos",
                column: "Token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Colaboradores_ColaboradorId",
                table: "Convites",
                column: "ColaboradorId",
                principalTable: "Colaboradores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convites_Colaboradores_ColaboradorId",
                table: "Convites");

            migrationBuilder.DropTable(
                name: "PesquisaRascunhos");

            migrationBuilder.DropIndex(
                name: "IX_Convites_ColaboradorId",
                table: "Convites");

            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "Questoes");

            migrationBuilder.DropColumn(
                name: "ColaboradorId",
                table: "Convites");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Convites");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataInicial",
                table: "Pesquisas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataFinal",
                table: "Pesquisas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
