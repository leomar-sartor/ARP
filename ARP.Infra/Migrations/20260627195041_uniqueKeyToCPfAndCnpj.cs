using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class uniqueKeyToCPfAndCnpj : Migration
    {
        /// <inheritdoc />
        //protected override void Up(MigrationBuilder migrationBuilder)
        //{
        //    migrationBuilder.CreateIndex(
        //        name: "IX_Empresas_Cnpj",
        //        table: "Empresas",
        //        column: "Cnpj",
        //        unique: true);

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Colaboradores_Cpf",
        //        table: "Colaboradores",
        //        column: "Cpf",
        //        unique: true);

        //    migrationBuilder.CreateIndex(
        //        name: "IX_arp_user_Cpf",
        //        table: "arp_user",
        //        column: "Cpf",
        //        unique: true);
        //}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalize CNPJ (remove non-digits) and remove duplicates keeping lowest id
            migrationBuilder.Sql(@"
            UPDATE ""Empresas"" SET ""Cnpj"" = regexp_replace(""Cnpj"", '\D', '', 'g') WHERE ""Cnpj"" IS NOT NULL;
            DELETE FROM ""Empresas"" e
            USING (
                SELECT MIN(""Id"") AS keepid, ""Cnpj""
                FROM ""Empresas""
                WHERE ""Cnpj"" IS NOT NULL
                GROUP BY ""Cnpj""
                HAVING COUNT(*) > 1
            ) d
            WHERE e.""Cnpj"" = d.""Cnpj"" AND e.""Id"" <> d.keepid;
        ");

            // Normalize CPF for Colaboradores and remove duplicates
            migrationBuilder.Sql(@"
            UPDATE ""Colaboradores"" SET ""Cpf"" = regexp_replace(""Cpf"", '\D', '', 'g') WHERE ""Cpf"" IS NOT NULL;
            DELETE FROM ""Colaboradores"" c
            USING (
                SELECT MIN(""Id"") AS keepid, ""Cpf""
                FROM ""Colaboradores""
                WHERE ""Cpf"" IS NOT NULL
                GROUP BY ""Cpf""
                HAVING COUNT(*) > 1
            ) d
            WHERE c.""Cpf"" = d.""Cpf"" AND c.""Id"" <> d.keepid;
        ");

            // Normalize CPF for users (table mapped to arp_user) and remove duplicates
            migrationBuilder.Sql(@"
            UPDATE ""arp_user"" SET ""Cpf"" = regexp_replace(""Cpf"", '\D', '', 'g') WHERE ""Cpf"" IS NOT NULL;
            DELETE FROM ""arp_user"" u
            USING (
                SELECT MIN(""Id"") AS keepid, ""Cpf""
                FROM ""arp_user""
                WHERE ""Cpf"" IS NOT NULL
                GROUP BY ""Cpf""
                HAVING COUNT(*) > 1
            ) d
            WHERE u.""Cpf"" = d.""Cpf"" AND u.""Id"" <> d.keepid;
        ");

            // Create unique indexes
            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Cnpj",
                table: "Empresas",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_Cpf",
                table: "Colaboradores",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_arp_user_Cpf",
                table: "arp_user",
                column: "Cpf",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_arp_user_Cpf", table: "arp_user");
            migrationBuilder.DropIndex(name: "IX_Colaboradores_Cpf", table: "Colaboradores");
            migrationBuilder.DropIndex(name: "IX_Empresas_Cnpj", table: "Empresas");
        }

        /// <inheritdoc />
        //protected override void Down(MigrationBuilder migrationBuilder)
        //{
        //    migrationBuilder.DropIndex(
        //        name: "IX_Empresas_Cnpj",
        //        table: "Empresas");

        //    migrationBuilder.DropIndex(
        //        name: "IX_Colaboradores_Cpf",
        //        table: "Colaboradores");

        //    migrationBuilder.DropIndex(
        //        name: "IX_arp_user_Cpf",
        //        table: "arp_user");
        //}
    }
}
