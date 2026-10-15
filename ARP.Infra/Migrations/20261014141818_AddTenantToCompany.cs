using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "Empresas",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    company_record RECORD;
                    new_tenant_id bigint;
                BEGIN
                    FOR company_record IN
                        SELECT "Id", "NomeFantasia", "Cnpj", "Ativo", "UserId",
                               "CreatedAt", "UpdatedAt", "DeletedAt"
                        FROM "Empresas"
                    LOOP
                        INSERT INTO "Tenants"
                            ("Nome", "Ativo", "UserId", "CreatedAt", "UpdatedAt", "DeletedAt")
                        VALUES
                            (
                                COALESCE(
                                    NULLIF(BTRIM(company_record."NomeFantasia"), ''),
                                    NULLIF(BTRIM(company_record."Cnpj"), ''),
                                    'Empresa ' || company_record."Id"::text
                                ),
                                company_record."Ativo",
                                company_record."UserId",
                                company_record."CreatedAt",
                                company_record."UpdatedAt",
                                company_record."DeletedAt"
                            )
                        RETURNING "Id" INTO new_tenant_id;

                        UPDATE "Empresas"
                        SET "TenantId" = new_tenant_id
                        WHERE "Id" = company_record."Id";
                    END LOOP;
                END $$;
                """);

            migrationBuilder.AlterColumn<long>(
                name: "TenantId",
                table: "Empresas",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_TenantId",
                table: "Empresas",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empresas_Tenants_TenantId",
                table: "Empresas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empresas_Tenants_TenantId",
                table: "Empresas");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Empresas_TenantId",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Empresas");
        }
    }
}
