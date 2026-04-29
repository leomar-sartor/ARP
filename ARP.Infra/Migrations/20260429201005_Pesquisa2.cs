using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Pesquisa2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convite_Pesquisa_PesquisaId",
                table: "Convite");

            migrationBuilder.DropForeignKey(
                name: "FK_Questao_Pesquisa_PesquisaId",
                table: "Questao");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestaoOpcao_Questao_QuestaoId",
                table: "QuestaoOpcao");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestaoResposta_QuestaoOpcao_QuestaoOpcaoId",
                table: "QuestaoResposta");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestaoResposta_Questao_QuestaoId",
                table: "QuestaoResposta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestaoResposta",
                table: "QuestaoResposta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestaoOpcao",
                table: "QuestaoOpcao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Questao",
                table: "Questao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pesquisa",
                table: "Pesquisa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Convite",
                table: "Convite");

            migrationBuilder.RenameTable(
                name: "QuestaoResposta",
                newName: "QuestaoRespostas");

            migrationBuilder.RenameTable(
                name: "QuestaoOpcao",
                newName: "QuestaoOpcoes");

            migrationBuilder.RenameTable(
                name: "Questao",
                newName: "Questoes");

            migrationBuilder.RenameTable(
                name: "Pesquisa",
                newName: "Pesquisas");

            migrationBuilder.RenameTable(
                name: "Convite",
                newName: "Convites");

            migrationBuilder.RenameIndex(
                name: "IX_QuestaoResposta_QuestaoOpcaoId",
                table: "QuestaoRespostas",
                newName: "IX_QuestaoRespostas_QuestaoOpcaoId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestaoResposta_QuestaoId",
                table: "QuestaoRespostas",
                newName: "IX_QuestaoRespostas_QuestaoId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestaoOpcao_QuestaoId",
                table: "QuestaoOpcoes",
                newName: "IX_QuestaoOpcoes_QuestaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Questao_PesquisaId",
                table: "Questoes",
                newName: "IX_Questoes_PesquisaId");

            migrationBuilder.RenameIndex(
                name: "IX_Convite_Token",
                table: "Convites",
                newName: "IX_Convites_Token");

            migrationBuilder.RenameIndex(
                name: "IX_Convite_PesquisaId",
                table: "Convites",
                newName: "IX_Convites_PesquisaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestaoRespostas",
                table: "QuestaoRespostas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestaoOpcoes",
                table: "QuestaoOpcoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Questoes",
                table: "Questoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pesquisas",
                table: "Pesquisas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Convites",
                table: "Convites",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Pesquisas_PesquisaId",
                table: "Convites",
                column: "PesquisaId",
                principalTable: "Pesquisas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestaoOpcoes_Questoes_QuestaoId",
                table: "QuestaoOpcoes",
                column: "QuestaoId",
                principalTable: "Questoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestaoRespostas_QuestaoOpcoes_QuestaoOpcaoId",
                table: "QuestaoRespostas",
                column: "QuestaoOpcaoId",
                principalTable: "QuestaoOpcoes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestaoRespostas_Questoes_QuestaoId",
                table: "QuestaoRespostas",
                column: "QuestaoId",
                principalTable: "Questoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questoes_Pesquisas_PesquisaId",
                table: "Questoes",
                column: "PesquisaId",
                principalTable: "Pesquisas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convites_Pesquisas_PesquisaId",
                table: "Convites");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestaoOpcoes_Questoes_QuestaoId",
                table: "QuestaoOpcoes");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestaoRespostas_QuestaoOpcoes_QuestaoOpcaoId",
                table: "QuestaoRespostas");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestaoRespostas_Questoes_QuestaoId",
                table: "QuestaoRespostas");

            migrationBuilder.DropForeignKey(
                name: "FK_Questoes_Pesquisas_PesquisaId",
                table: "Questoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Questoes",
                table: "Questoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestaoRespostas",
                table: "QuestaoRespostas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestaoOpcoes",
                table: "QuestaoOpcoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pesquisas",
                table: "Pesquisas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Convites",
                table: "Convites");

            migrationBuilder.RenameTable(
                name: "Questoes",
                newName: "Questao");

            migrationBuilder.RenameTable(
                name: "QuestaoRespostas",
                newName: "QuestaoResposta");

            migrationBuilder.RenameTable(
                name: "QuestaoOpcoes",
                newName: "QuestaoOpcao");

            migrationBuilder.RenameTable(
                name: "Pesquisas",
                newName: "Pesquisa");

            migrationBuilder.RenameTable(
                name: "Convites",
                newName: "Convite");

            migrationBuilder.RenameIndex(
                name: "IX_Questoes_PesquisaId",
                table: "Questao",
                newName: "IX_Questao_PesquisaId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestaoRespostas_QuestaoOpcaoId",
                table: "QuestaoResposta",
                newName: "IX_QuestaoResposta_QuestaoOpcaoId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestaoRespostas_QuestaoId",
                table: "QuestaoResposta",
                newName: "IX_QuestaoResposta_QuestaoId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestaoOpcoes_QuestaoId",
                table: "QuestaoOpcao",
                newName: "IX_QuestaoOpcao_QuestaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Convites_Token",
                table: "Convite",
                newName: "IX_Convite_Token");

            migrationBuilder.RenameIndex(
                name: "IX_Convites_PesquisaId",
                table: "Convite",
                newName: "IX_Convite_PesquisaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Questao",
                table: "Questao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestaoResposta",
                table: "QuestaoResposta",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestaoOpcao",
                table: "QuestaoOpcao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pesquisa",
                table: "Pesquisa",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Convite",
                table: "Convite",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Convite_Pesquisa_PesquisaId",
                table: "Convite",
                column: "PesquisaId",
                principalTable: "Pesquisa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questao_Pesquisa_PesquisaId",
                table: "Questao",
                column: "PesquisaId",
                principalTable: "Pesquisa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestaoOpcao_Questao_QuestaoId",
                table: "QuestaoOpcao",
                column: "QuestaoId",
                principalTable: "Questao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestaoResposta_QuestaoOpcao_QuestaoOpcaoId",
                table: "QuestaoResposta",
                column: "QuestaoOpcaoId",
                principalTable: "QuestaoOpcao",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestaoResposta_Questao_QuestaoId",
                table: "QuestaoResposta",
                column: "QuestaoId",
                principalTable: "Questao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
