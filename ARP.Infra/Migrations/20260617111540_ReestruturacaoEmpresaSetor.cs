using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class ReestruturacaoEmpresaSetor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "RazaoSocial",
                table: "Empresas");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "arp_usertoken");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "arp_userrole");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "arp_userclaim");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "arp_role");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "arp_roleclaim");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "arp_user");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "arp_userrole",
                newName: "IX_arp_userrole_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "arp_userclaim",
                newName: "IX_arp_userclaim_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "arp_roleclaim",
                newName: "IX_arp_roleclaim_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Setores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<long>(
                name: "EmpresaId",
                table: "Setores",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql("""
                UPDATE "Setores" s
                SET "EmpresaId" = es."EmpresaId"
                FROM (
                    SELECT "SetorId", MIN("EmpresaId") AS "EmpresaId"
                    FROM "EmpresaSetores"
                    GROUP BY "SetorId"
                ) es
                WHERE s."Id" = es."SetorId";
                """);

            migrationBuilder.DropTable(
                name: "EmpresaSetores");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Empresas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "Empresas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomeFantasia",
                table: "Empresas",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_arp_usertoken",
                table: "arp_usertoken",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_arp_userrole",
                table: "arp_userrole",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_arp_userclaim",
                table: "arp_userclaim",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_arp_role",
                table: "arp_role",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_arp_roleclaim",
                table: "arp_roleclaim",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Setores_EmpresaId",
                table: "Setores",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_arp_roleclaim_arp_role_RoleId",
                table: "arp_roleclaim",
                column: "RoleId",
                principalTable: "arp_role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_arp_userclaim_arp_user_UserId",
                table: "arp_userclaim",
                column: "UserId",
                principalTable: "arp_user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_arp_userrole_arp_role_RoleId",
                table: "arp_userrole",
                column: "RoleId",
                principalTable: "arp_role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_arp_userrole_arp_user_UserId",
                table: "arp_userrole",
                column: "UserId",
                principalTable: "arp_user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_arp_usertoken_arp_user_UserId",
                table: "arp_usertoken",
                column: "UserId",
                principalTable: "arp_user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_arp_user_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "arp_user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_arp_user_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "arp_user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Setores_Empresas_EmpresaId",
                table: "Setores",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_arp_roleclaim_arp_role_RoleId",
                table: "arp_roleclaim");

            migrationBuilder.DropForeignKey(
                name: "FK_arp_userclaim_arp_user_UserId",
                table: "arp_userclaim");

            migrationBuilder.DropForeignKey(
                name: "FK_arp_userrole_arp_role_RoleId",
                table: "arp_userrole");

            migrationBuilder.DropForeignKey(
                name: "FK_arp_userrole_arp_user_UserId",
                table: "arp_userrole");

            migrationBuilder.DropForeignKey(
                name: "FK_arp_usertoken_arp_user_UserId",
                table: "arp_usertoken");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_arp_user_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_arp_user_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Setores_Empresas_EmpresaId",
                table: "Setores");

            migrationBuilder.DropIndex(
                name: "IX_Setores_EmpresaId",
                table: "Setores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_arp_usertoken",
                table: "arp_usertoken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_arp_userrole",
                table: "arp_userrole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_arp_userclaim",
                table: "arp_userclaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_arp_roleclaim",
                table: "arp_roleclaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_arp_role",
                table: "arp_role");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Setores");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "NomeFantasia",
                table: "Empresas");

            migrationBuilder.RenameTable(
                name: "arp_usertoken",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "arp_userrole",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "arp_userclaim",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "arp_roleclaim",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "arp_role",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "arp_user",
                newName: "AspNetUsers");

            migrationBuilder.RenameIndex(
                name: "IX_arp_userrole_RoleId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_arp_userclaim_UserId",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_arp_roleclaim_RoleId",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Setores",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Empresas",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazaoSocial",
                table: "Empresas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmpresaSetores",
                columns: table => new
                {
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    SetorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaSetores", x => new { x.EmpresaId, x.SetorId });
                    table.ForeignKey(
                        name: "FK_EmpresaSetores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpresaSetores_Setores_SetorId",
                        column: x => x.SetorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaSetores_SetorId",
                table: "EmpresaSetores",
                column: "SetorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
