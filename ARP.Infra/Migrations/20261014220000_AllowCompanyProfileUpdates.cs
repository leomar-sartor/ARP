using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ARP.Infra.Migrations;

[DbContext(typeof(Context))]
[Migration("20261014220000_AllowCompanyProfileUpdates")]
public sealed class AllowCompanyProfileUpdates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        "INSERT INTO \"PersonasPermissoes\" (\"PersonaId\", \"PermissaoId\") VALUES (3, 3) ON CONFLICT DO NOTHING;");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        "DELETE FROM \"PersonasPermissoes\" WHERE \"PersonaId\" = 3 AND \"PermissaoId\" = 3;");
}
