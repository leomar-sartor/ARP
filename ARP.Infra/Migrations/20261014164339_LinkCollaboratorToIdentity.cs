using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class LinkCollaboratorToIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UsuarioId",
                table: "Colaboradores",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_UsuarioId",
                table: "Colaboradores",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Colaboradores_arp_user_UsuarioId",
                table: "Colaboradores",
                column: "UsuarioId",
                principalTable: "arp_user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                WITH usuarios_normalizados AS
                (
                    SELECT
                        "Id",
                        regexp_replace(COALESCE("Cpf", ''), '[^0-9]', '', 'g') AS "CpfNormalizado"
                    FROM "arp_user"
                ),
                usuarios_unicos AS
                (
                    SELECT "CpfNormalizado", MIN("Id") AS "UsuarioId"
                    FROM usuarios_normalizados
                    WHERE length("CpfNormalizado") = 11
                    GROUP BY "CpfNormalizado"
                    HAVING COUNT(*) = 1
                ),
                colaboradores_normalizados AS
                (
                    SELECT
                        "Id",
                        regexp_replace(COALESCE("Cpf", ''), '[^0-9]', '', 'g') AS "CpfNormalizado"
                    FROM "Colaboradores"
                ),
                colaboradores_unicos AS
                (
                    SELECT "CpfNormalizado", MIN("Id") AS "ColaboradorId"
                    FROM colaboradores_normalizados
                    WHERE length("CpfNormalizado") = 11
                    GROUP BY "CpfNormalizado"
                    HAVING COUNT(*) = 1
                )
                UPDATE "Colaboradores" AS colaborador
                SET "UsuarioId" = usuario."UsuarioId"
                FROM colaboradores_unicos AS perfil
                INNER JOIN usuarios_unicos AS usuario
                    ON usuario."CpfNormalizado" = perfil."CpfNormalizado"
                WHERE colaborador."Id" = perfil."ColaboradorId"
                    AND colaborador."UsuarioId" IS NULL;

                INSERT INTO "UsuariosEmpresas"
                    ("UsuarioId", "EmpresaId", "Ativo", "UserId", "CreatedAt", "UpdatedAt", "DeletedAt")
                SELECT DISTINCT
                    colaborador."UsuarioId", vinculo."EmpresaId", TRUE, 0,
                    NOW(), NOW(), NULL::timestamp with time zone
                FROM "Colaboradores" AS colaborador
                INNER JOIN "ColaboradorEmpresas" AS vinculo
                    ON vinculo."ColaboradorId" = colaborador."Id"
                WHERE colaborador."UsuarioId" IS NOT NULL
                    AND vinculo."Ativo"
                ON CONFLICT ("UsuarioId", "EmpresaId") DO NOTHING;

                INSERT INTO "UsuariosTenants"
                    ("UsuarioId", "TenantId", "Ativo", "UserId", "CreatedAt", "UpdatedAt", "DeletedAt")
                SELECT DISTINCT
                    usuario_empresa."UsuarioId", empresa."TenantId", TRUE, 0,
                    NOW(), NOW(), NULL::timestamp with time zone
                FROM "UsuariosEmpresas" AS usuario_empresa
                INNER JOIN "Empresas" AS empresa
                    ON empresa."Id" = usuario_empresa."EmpresaId"
                INNER JOIN "Colaboradores" AS colaborador
                    ON colaborador."UsuarioId" = usuario_empresa."UsuarioId"
                WHERE usuario_empresa."Ativo"
                ON CONFLICT ("UsuarioId", "TenantId") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colaboradores_arp_user_UsuarioId",
                table: "Colaboradores");

            migrationBuilder.DropIndex(
                name: "IX_Colaboradores_UsuarioId",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Colaboradores");
        }
    }
}
