using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TritoteNic.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImagenProducto",
                table: "Productos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "DescripcionProducto",
                table: "Productos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateTable(
                name: "Bitacora",
                columns: table => new
                {
                    id_bitacora = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_accion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    nombre_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tabla_afectada = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    id_registro = table.Column<int>(type: "integer", nullable: true),
                    descripcion_registro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    datos_anteriores = table.Column<string>(type: "text", nullable: true),
                    datos_nuevos = table.Column<string>(type: "text", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bitacora", x => x.id_bitacora);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bitacora");

            migrationBuilder.AlterColumn<string>(
                name: "ImagenProducto",
                table: "Productos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescripcionProducto",
                table: "Productos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
