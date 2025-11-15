using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TritoteNic.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCategoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DescripcionCategoria = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TelefonoCliente = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    DireccionCliente = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EmailCliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.id_cliente);
                });

            migrationBuilder.CreateTable(
                name: "EstadosPedido",
                columns: table => new
                {
                    id_estado_pedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreEstadoPedido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DescripcionEstadoPedido = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosPedido", x => x.id_estado_pedido);
                });

            migrationBuilder.CreateTable(
                name: "MetodosPago",
                columns: table => new
                {
                    id_metodo_pago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreMetodoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DescripcionMetodoPago = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPago", x => x.id_metodo_pago);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreRol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DescripcionRol = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreProducto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DescripcionProducto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PrecioProducto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    StockProducto = table.Column<int>(type: "integer", nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    EstadoProducto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ImagenProducto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "Categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreUsuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailUsuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContrasenaUsuario = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    EstadoUsuario = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_id_rol",
                        column: x => x.id_rol,
                        principalTable: "Roles",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    id_pedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cliente = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_estado_pedido = table.Column<int>(type: "integer", nullable: false),
                    id_metodo_pago = table.Column<int>(type: "integer", nullable: false),
                    fecha_pedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.id_pedido);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedidos_EstadosPedido_id_estado_pedido",
                        column: x => x.id_estado_pedido,
                        principalTable: "EstadosPedido",
                        principalColumn: "id_estado_pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedidos_MetodosPago_id_metodo_pago",
                        column: x => x.id_metodo_pago,
                        principalTable: "MetodosPago",
                        principalColumn: "id_metodo_pago",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPedido",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_pedido = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    CantidadProducto = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPedido", x => x.id_detalle);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Pedidos_id_pedido",
                        column: x => x.id_pedido,
                        principalTable: "Pedidos",
                        principalColumn: "id_pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_id_pedido",
                table: "DetallesPedido",
                column: "id_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_id_producto",
                table: "DetallesPedido",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_cliente",
                table: "Pedidos",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_estado_pedido",
                table: "Pedidos",
                column: "id_estado_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_metodo_pago",
                table: "Pedidos",
                column: "id_metodo_pago");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_usuario",
                table: "Pedidos",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_categoria",
                table: "Productos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_id_rol",
                table: "Usuarios",
                column: "id_rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesPedido");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "EstadosPedido");

            migrationBuilder.DropTable(
                name: "MetodosPago");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
