using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace megadeliciasapi.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriaToPlato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Platos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Platos");
        }
    }
}
