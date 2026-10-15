using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTenantCompanyMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuariosEmpresas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosEmpresas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosEmpresas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosEmpresas_arp_user_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "arp_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosTenants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosTenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosTenants_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosTenants_arp_user_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "arp_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO "UsuariosEmpresas"
                    ("UsuarioId", "EmpresaId", "Ativo", "UserId", "CreatedAt", "UpdatedAt", "DeletedAt")
                SELECT
                    usuario."Id", empresa."Id", TRUE, 0, NOW(), NOW(), NULL::timestamp with time zone
                FROM "arp_user" AS usuario
                INNER JOIN "Empresas" AS empresa
                    ON empresa."Id" = usuario."EmpresaId"
                WHERE usuario."EmpresaId" IS NOT NULL;

                INSERT INTO "UsuariosTenants"
                    ("UsuarioId", "TenantId", "Ativo", "UserId", "CreatedAt", "UpdatedAt", "DeletedAt")
                SELECT DISTINCT
                    vinculo."UsuarioId", empresa."TenantId", TRUE, 0, NOW(), NOW(), NULL::timestamp with time zone
                FROM "UsuariosEmpresas" AS vinculo
                INNER JOIN "Empresas" AS empresa
                    ON empresa."Id" = vinculo."EmpresaId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosEmpresas_EmpresaId",
                table: "UsuariosEmpresas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosEmpresas_UsuarioId_EmpresaId",
                table: "UsuariosEmpresas",
                columns: new[] { "UsuarioId", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosTenants_TenantId",
                table: "UsuariosTenants",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosTenants_UsuarioId_TenantId",
                table: "UsuariosTenants",
                columns: new[] { "UsuarioId", "TenantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuariosEmpresas");

            migrationBuilder.DropTable(
                name: "UsuariosTenants");
        }
    }
}
