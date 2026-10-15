using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddStandardizedSurveyStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Valor",
                table: "QuestaoOpcoes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicadaEm",
                table: "Pesquisas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "Pesquisas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Pesquisas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "EmpresaId",
                table: "Convites",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PesquisaEmpresas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    PesquisaId = table.Column<long>(type: "bigint", nullable: false),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PesquisaEmpresas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PesquisaEmpresas_Empresas_TenantId_EmpresaId",
                        columns: x => new { x.TenantId, x.EmpresaId },
                        principalTable: "Empresas",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PesquisaEmpresas_Pesquisas_PesquisaId",
                        column: x => x.PesquisaId,
                        principalTable: "Pesquisas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pesquisas_TenantId",
                table: "Pesquisas",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Convites_EmpresaId",
                table: "Convites",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PesquisaEmpresas_PesquisaId_EmpresaId",
                table: "PesquisaEmpresas",
                columns: new[] { "PesquisaId", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PesquisaEmpresas_TenantId_EmpresaId",
                table: "PesquisaEmpresas",
                columns: new[] { "TenantId", "EmpresaId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Empresas_EmpresaId",
                table: "Convites",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pesquisas_Tenants_TenantId",
                table: "Pesquisas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convites_Empresas_EmpresaId",
                table: "Convites");

            migrationBuilder.DropForeignKey(
                name: "FK_Pesquisas_Tenants_TenantId",
                table: "Pesquisas");

            migrationBuilder.DropTable(
                name: "PesquisaEmpresas");

            migrationBuilder.DropIndex(
                name: "IX_Pesquisas_TenantId",
                table: "Pesquisas");

            migrationBuilder.DropIndex(
                name: "IX_Convites_EmpresaId",
                table: "Convites");

            migrationBuilder.DropColumn(
                name: "Valor",
                table: "QuestaoOpcoes");

            migrationBuilder.DropColumn(
                name: "PublicadaEm",
                table: "Pesquisas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Pesquisas");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Pesquisas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Convites");
        }
    }
}
