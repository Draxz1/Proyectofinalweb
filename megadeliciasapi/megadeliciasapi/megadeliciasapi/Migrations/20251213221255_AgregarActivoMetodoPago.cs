using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace megadeliciasapi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarActivoMetodoPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "MetodosPago");
        }
    }
}
