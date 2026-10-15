using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddContextualPersonasAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Empresas_TenantId",
                table: "Empresas");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Empresas_TenantId_Id",
                table: "Empresas",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "Permissoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonasPermissoes",
                columns: table => new
                {
                    PersonaId = table.Column<long>(type: "bigint", nullable: false),
                    PermissaoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonasPermissoes", x => new { x.PersonaId, x.PermissaoId });
                    table.ForeignKey(
                        name: "FK_PersonasPermissoes_Permissoes_PermissaoId",
                        column: x => x.PermissaoId,
                        principalTable: "Permissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonasPermissoes_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosPersonas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    PersonaId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPersonas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosPersonas_Empresas_TenantId_EmpresaId",
                        columns: x => new { x.TenantId, x.EmpresaId },
                        principalTable: "Empresas",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosPersonas_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosPersonas_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosPersonas_arp_user_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "arp_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Permissoes",
                columns: new[] { "Id", "Ativo", "Codigo", "CreatedAt", "DeletedAt", "Descricao", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1L, true, "empresa.visualizar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Visualizar empresas autorizadas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2L, true, "empresa.criar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cadastrar empresas no tenant ativo", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3L, true, "empresa.editar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Editar empresas autorizadas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4L, true, "empresa.gerenciar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gerenciar empresas autorizadas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5L, true, "colaborador.visualizar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Visualizar colaboradores autorizados", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 6L, true, "colaborador.gerenciar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gerenciar colaboradores autorizados", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 7L, true, "pesquisa_padronizada.criar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Criar pesquisas padronizadas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 8L, true, "pesquisa_padronizada.editar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Editar pesquisas padronizadas não publicadas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 9L, true, "pesquisa_padronizada.publicar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Publicar pesquisas padronizadas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 10L, true, "pesquisa_padronizada.gerenciar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gerenciar pesquisas padronizadas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 11L, true, "pesquisa.responder", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Responder pesquisas atribuídas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 12L, true, "pesquisa.acompanhar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Acompanhar indicadores operacionais", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 13L, true, "tenant.alternar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Alternar entre tenants autorizados", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 14L, true, "permissao.gerenciar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gerenciar personas e permissões", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 15L, true, "setor.gerenciar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gerenciar setores autorizados", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 16L, true, "categoria.gerenciar", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gerenciar categorias de pesquisas padronizadas", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "Personas",
                columns: new[] { "Id", "Ativo", "Codigo", "CreatedAt", "DeletedAt", "Nome", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1L, true, "operador", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Operador", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2L, true, "colaborador", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Colaborador", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3L, true, "empresa", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Empresa", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4L, true, "admin", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null, "Admin", new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "PersonasPermissoes",
                columns: new[] { "PermissaoId", "PersonaId" },
                values: new object[,]
                {
                    { 1L, 1L },
                    { 2L, 1L },
                    { 3L, 1L },
                    { 4L, 1L },
                    { 5L, 1L },
                    { 6L, 1L },
                    { 7L, 1L },
                    { 8L, 1L },
                    { 9L, 1L },
                    { 10L, 1L },
                    { 12L, 1L },
                    { 13L, 1L },
                    { 15L, 1L },
                    { 16L, 1L },
                    { 11L, 2L },
                    { 13L, 2L },
                    { 1L, 3L },
                    { 12L, 3L },
                    { 13L, 3L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissoes_Codigo",
                table: "Permissoes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Codigo",
                table: "Personas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonasPermissoes_PermissaoId",
                table: "PersonasPermissoes",
                column: "PermissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPersonas_PersonaId",
                table: "UsuariosPersonas",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPersonas_TenantId_EmpresaId",
                table: "UsuariosPersonas",
                columns: new[] { "TenantId", "EmpresaId" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPersonas_UsuarioId_PersonaId_TenantId",
                table: "UsuariosPersonas",
                columns: new[] { "UsuarioId", "PersonaId", "TenantId" },
                unique: true,
                filter: "\"EmpresaId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPersonas_UsuarioId_PersonaId_TenantId_EmpresaId",
                table: "UsuariosPersonas",
                columns: new[] { "UsuarioId", "PersonaId", "TenantId", "EmpresaId" },
                unique: true,
                filter: "\"EmpresaId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonasPermissoes");

            migrationBuilder.DropTable(
                name: "UsuariosPersonas");

            migrationBuilder.DropTable(
                name: "Permissoes");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Empresas_TenantId_Id",
                table: "Empresas");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_TenantId",
                table: "Empresas",
                column: "TenantId");
        }
    }
}
