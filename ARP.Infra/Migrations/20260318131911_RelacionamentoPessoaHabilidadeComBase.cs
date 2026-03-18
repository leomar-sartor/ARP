using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARP.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RelacionamentoPessoaHabilidadeComBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PessoaHabilidades",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PessoaHabilidades",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "PessoaHabilidades",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PessoaHabilidades",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "PessoaHabilidades",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PessoaHabilidades");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PessoaHabilidades");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PessoaHabilidades");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PessoaHabilidades");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PessoaHabilidades");
        }
    }
}
