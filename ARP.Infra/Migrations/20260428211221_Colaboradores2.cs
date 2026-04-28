using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Colaboradores2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colaborador_Empresas_EmpresaId",
                table: "Colaborador");

            migrationBuilder.DropForeignKey(
                name: "FK_Colaborador_Setores_SetorId",
                table: "Colaborador");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Colaborador",
                table: "Colaborador");

            migrationBuilder.RenameTable(
                name: "Colaborador",
                newName: "Colaboradores");

            migrationBuilder.RenameIndex(
                name: "IX_Colaborador_SetorId",
                table: "Colaboradores",
                newName: "IX_Colaboradores_SetorId");

            migrationBuilder.RenameIndex(
                name: "IX_Colaborador_EmpresaId",
                table: "Colaboradores",
                newName: "IX_Colaboradores_EmpresaId");

            //migrationBuilder.AlterColumn<bool>(
            //    name: "Ativo",
            //    table: "Colaboradores",
            //    type: "boolean",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "text");

            migrationBuilder.Sql(
            @"ALTER TABLE ""Colaboradores""
              ALTER COLUMN ""Ativo"" TYPE boolean
              USING ""Ativo""::boolean;");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Colaboradores",
                table: "Colaboradores",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Colaboradores_Empresas_EmpresaId",
                table: "Colaboradores",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Colaboradores_Setores_SetorId",
                table: "Colaboradores",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colaboradores_Empresas_EmpresaId",
                table: "Colaboradores");

            migrationBuilder.DropForeignKey(
                name: "FK_Colaboradores_Setores_SetorId",
                table: "Colaboradores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Colaboradores",
                table: "Colaboradores");

            migrationBuilder.RenameTable(
                name: "Colaboradores",
                newName: "Colaborador");

            migrationBuilder.RenameIndex(
                name: "IX_Colaboradores_SetorId",
                table: "Colaborador",
                newName: "IX_Colaborador_SetorId");

            migrationBuilder.RenameIndex(
                name: "IX_Colaboradores_EmpresaId",
                table: "Colaborador",
                newName: "IX_Colaborador_EmpresaId");

            migrationBuilder.AlterColumn<string>(
                name: "Ativo",
                table: "Colaborador",
                type: "text",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Colaborador",
                table: "Colaborador",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Colaborador_Empresas_EmpresaId",
                table: "Colaborador",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Colaborador_Setores_SetorId",
                table: "Colaborador",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
