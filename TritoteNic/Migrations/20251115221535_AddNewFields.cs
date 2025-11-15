using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TritoteNic.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ultimo_acceso",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "descuento",
                table: "Pedidos",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "subtotal",
                table: "Pedidos",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoriaCliente",
                table: "Clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_ultimo_pedido",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_gastado",
                table: "Clientes",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ultimo_acceso",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "descuento",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "subtotal",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "CategoriaCliente",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "fecha_ultimo_pedido",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "total_gastado",
                table: "Clientes");
        }
    }
}
