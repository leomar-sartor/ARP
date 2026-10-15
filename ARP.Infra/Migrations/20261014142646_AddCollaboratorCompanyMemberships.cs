using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddCollaboratorCompanyMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Setores_EmpresaId",
                table: "Setores");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Setores_EmpresaId_Id",
                table: "Setores",
                columns: new[] { "EmpresaId", "Id" });

            migrationBuilder.CreateTable(
                name: "ColaboradorEmpresas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColaboradorId = table.Column<long>(type: "bigint", nullable: false),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    SetorId = table.Column<long>(type: "bigint", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColaboradorEmpresas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColaboradorEmpresas_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ColaboradorEmpresas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ColaboradorEmpresas_Setores_EmpresaId_SetorId",
                        columns: x => new { x.EmpresaId, x.SetorId },
                        principalTable: "Setores",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO "ColaboradorEmpresas"
                    ("ColaboradorId", "EmpresaId", "SetorId", "Ativo", "UserId", "CreatedAt", "UpdatedAt", "DeletedAt")
                SELECT
                    colaborador."Id", colaborador."EmpresaId", colaborador."SetorId",
                    colaborador."Ativo", colaborador."UserId", colaborador."CreatedAt",
                    colaborador."UpdatedAt", colaborador."DeletedAt"
                FROM "Colaboradores" AS colaborador;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ColaboradorEmpresas_ColaboradorId_EmpresaId",
                table: "ColaboradorEmpresas",
                columns: new[] { "ColaboradorId", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ColaboradorEmpresas_EmpresaId_SetorId",
                table: "ColaboradorEmpresas",
                columns: new[] { "EmpresaId", "SetorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColaboradorEmpresas");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Setores_EmpresaId_Id",
                table: "Setores");

            migrationBuilder.CreateIndex(
                name: "IX_Setores_EmpresaId",
                table: "Setores",
                column: "EmpresaId");
        }
    }
}
