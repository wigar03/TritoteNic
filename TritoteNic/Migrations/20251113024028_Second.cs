using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TritoteNic.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rol",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "categoria",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "metodo_pago",
                table: "Pedidos");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Usuarios",
                newName: "NombreUsuario");

            migrationBuilder.RenameColumn(
                name: "estado",
                table: "Usuarios",
                newName: "EstadoUsuario");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Usuarios",
                newName: "EmailUsuario");

            migrationBuilder.RenameColumn(
                name: "contraseña",
                table: "Usuarios",
                newName: "ContrasenaUsuario");

            migrationBuilder.RenameColumn(
                name: "stock",
                table: "Productos",
                newName: "id_categoria");

            migrationBuilder.RenameColumn(
                name: "precio",
                table: "Productos",
                newName: "PrecioProducto");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Productos",
                newName: "NombreProducto");

            migrationBuilder.RenameColumn(
                name: "estado",
                table: "Productos",
                newName: "EstadoProducto");

            migrationBuilder.RenameColumn(
                name: "descripcion",
                table: "Productos",
                newName: "ImagenProducto");

            migrationBuilder.RenameColumn(
                name: "cantidad",
                table: "DetallesPedido",
                newName: "CantidadProducto");

            migrationBuilder.RenameColumn(
                name: "telefono",
                table: "Clientes",
                newName: "TelefonoCliente");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Clientes",
                newName: "NombreCliente");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Clientes",
                newName: "EmailCliente");

            migrationBuilder.RenameColumn(
                name: "direccion",
                table: "Clientes",
                newName: "DireccionCliente");

            migrationBuilder.AddColumn<int>(
                name: "id_rol",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DescripcionProducto",
                table: "Productos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StockProducto",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_estado_pedido",
                table: "Pedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_metodo_pago",
                table: "Pedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCategoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescripcionCategoria = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "EstadosPedido",
                columns: table => new
                {
                    id_estado_pedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstadoPedido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescripcionEstadoPedido = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosPedido", x => x.id_estado_pedido);
                });

            migrationBuilder.CreateTable(
                name: "MetodosPago",
                columns: table => new
                {
                    id_metodo_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreMetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescripcionMetodoPago = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPago", x => x.id_metodo_pago);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreRol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescripcionRol = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id_rol);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_id_rol",
                table: "Usuarios",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_categoria",
                table: "Productos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_estado_pedido",
                table: "Pedidos",
                column: "id_estado_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_id_metodo_pago",
                table: "Pedidos",
                column: "id_metodo_pago");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_EstadosPedido_id_estado_pedido",
                table: "Pedidos",
                column: "id_estado_pedido",
                principalTable: "EstadosPedido",
                principalColumn: "id_estado_pedido",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_MetodosPago_id_metodo_pago",
                table: "Pedidos",
                column: "id_metodo_pago",
                principalTable: "MetodosPago",
                principalColumn: "id_metodo_pago",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_id_categoria",
                table: "Productos",
                column: "id_categoria",
                principalTable: "Categorias",
                principalColumn: "id_categoria",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_id_rol",
                table: "Usuarios",
                column: "id_rol",
                principalTable: "Roles",
                principalColumn: "id_rol",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_EstadosPedido_id_estado_pedido",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_MetodosPago_id_metodo_pago",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_id_categoria",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Roles_id_rol",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "EstadosPedido");

            migrationBuilder.DropTable(
                name: "MetodosPago");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_id_rol",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Productos_id_categoria",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_id_estado_pedido",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_id_metodo_pago",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "id_rol",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DescripcionProducto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockProducto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "id_estado_pedido",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "id_metodo_pago",
                table: "Pedidos");

            migrationBuilder.RenameColumn(
                name: "NombreUsuario",
                table: "Usuarios",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "EstadoUsuario",
                table: "Usuarios",
                newName: "estado");

            migrationBuilder.RenameColumn(
                name: "EmailUsuario",
                table: "Usuarios",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "ContrasenaUsuario",
                table: "Usuarios",
                newName: "contraseña");

            migrationBuilder.RenameColumn(
                name: "id_categoria",
                table: "Productos",
                newName: "stock");

            migrationBuilder.RenameColumn(
                name: "PrecioProducto",
                table: "Productos",
                newName: "precio");

            migrationBuilder.RenameColumn(
                name: "NombreProducto",
                table: "Productos",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "ImagenProducto",
                table: "Productos",
                newName: "descripcion");

            migrationBuilder.RenameColumn(
                name: "EstadoProducto",
                table: "Productos",
                newName: "estado");

            migrationBuilder.RenameColumn(
                name: "CantidadProducto",
                table: "DetallesPedido",
                newName: "cantidad");

            migrationBuilder.RenameColumn(
                name: "TelefonoCliente",
                table: "Clientes",
                newName: "telefono");

            migrationBuilder.RenameColumn(
                name: "NombreCliente",
                table: "Clientes",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "EmailCliente",
                table: "Clientes",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "DireccionCliente",
                table: "Clientes",
                newName: "direccion");

            migrationBuilder.AddColumn<string>(
                name: "rol",
                table: "Usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "categoria",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "Pedidos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "metodo_pago",
                table: "Pedidos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
