using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace megadeliciasapi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CierresCaja_Usuarios_UsuarioId",
                table: "CierresCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Pagos_PagoId",
                table: "Facturas");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Pagos_PagoId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_Ordenes_Usuarios_UsuarioId",
                table: "Ordenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Usuarios_UsuarioId",
                table: "Ventas");

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Categoria",
                table: "Productos",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre",
                table: "Productos",
                column: "Nombre");

            migrationBuilder.AddForeignKey(
                name: "FK_CierresCaja_Usuarios_UsuarioId",
                table: "CierresCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Pagos_PagoId",
                table: "Facturas",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Pagos_PagoId",
                table: "MovimientosCaja",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ordenes_Usuarios_UsuarioId",
                table: "Ordenes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Usuarios_UsuarioId",
                table: "Ventas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CierresCaja_Usuarios_UsuarioId",
                table: "CierresCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Pagos_PagoId",
                table: "Facturas");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Pagos_PagoId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_Ordenes_Usuarios_UsuarioId",
                table: "Ordenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Usuarios_UsuarioId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.AddForeignKey(
                name: "FK_CierresCaja_Usuarios_UsuarioId",
                table: "CierresCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Pagos_PagoId",
                table: "Facturas",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Pagos_PagoId",
                table: "MovimientosCaja",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ordenes_Usuarios_UsuarioId",
                table: "Ordenes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Usuarios_UsuarioId",
                table: "Ventas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
