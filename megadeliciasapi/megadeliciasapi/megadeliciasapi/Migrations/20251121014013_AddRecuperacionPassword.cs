using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace megadeliciasapi.Migrations
{
    /// <inheritdoc />
    public partial class AddRecuperacionPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordTemporalExpira",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereCambioPassword",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordTemporalExpira",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RequiereCambioPassword",
                table: "Usuarios");
        }
    }
}
