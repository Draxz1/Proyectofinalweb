using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace megadeliciasapi.Migrations
{
    /// <inheritdoc />
    public partial class bdactualizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "EsBancario",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "RequiereReferencia",
                table: "MetodosPago");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsBancario",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereReferencia",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
