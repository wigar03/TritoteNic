// Tipos TypeScript que coinciden con los DTOs de C#

export interface LoginDto {
  email: string;
  password: string;
}

export interface UsuarioDto {
  idUsuario: number;
  nombreUsuario: string | null;
  emailUsuario: string | null;
  estadoUsuario: string | null;
  fechaCreacionUsuario: string;
  ultimoAcceso: string | null;
  idRol: number;
  nombreRol: string | null;
}

export interface LoginResponseDto {
  token: string;
  usuario: UsuarioDto;
  expiresAt: string;
}

export interface ClienteDto {
  idCliente: number;
  nombreCliente: string | null;
  emailCliente: string | null;
  telefonoCliente: string | null;
  direccionCliente: string | null;
  fechaCreacionCliente: string;
  estadoCliente: string | null;
}

export interface ClienteCreateDto {
  nombreCliente: string;
  emailCliente: string;
  telefonoCliente: string;
  direccionCliente?: string;
}

export interface ClienteUpdateDto {
  nombreCliente?: string;
  emailCliente?: string;
  telefonoCliente?: string;
  direccionCliente?: string;
  estadoCliente?: string;
}

export interface ProductoDto {
  idProducto: number;
  nombreProducto: string | null;
  descripcionProducto: string | null;
  precioProducto: number;
  stockProducto: number;
  idCategoria: number;
  nombreCategoria?: string | null;
  estadoProducto: string | null;
  fechaCreacionProducto: string;
}

export interface ProductoCreateDto {
  nombreProducto: string;
  descripcionProducto?: string;
  precioProducto: number;
  stockProducto: number;
  idCategoria: number;
}

export interface ProductoUpdateDto {
  nombreProducto?: string;
  descripcionProducto?: string;
  precioProducto?: number;
  stockProducto?: number;
  idCategoria?: number;
  estadoProducto?: string;
}

export interface PedidoDto {
  idPedido: number;
  idCliente: number;
  nombreCliente?: string | null;
  fechaPedido: string;
  totalPedido: number;
  idEstadoPedido: number;
  nombreEstadoPedido?: string | null;
  idMetodoPago: number;
  nombreMetodoPago?: string | null;
  detallesPedido?: DetallePedidoDto[];
}

export interface PedidoCreateDto {
  idCliente: number;
  idEstadoPedido: number;
  idMetodoPago: number;
  detallesPedido: DetallePedidoCreateDto[];
}

export interface PedidoUpdateDto {
  idCliente?: number;
  idEstadoPedido?: number;
  idMetodoPago?: number;
}

export interface DetallePedidoDto {
  idDetallePedido: number;
  idPedido: number;
  idProducto: number;
  nombreProducto?: string | null;
  cantidadDetallePedido: number;
  precioUnitarioDetallePedido: number;
  subtotalDetallePedido: number;
}

export interface DetallePedidoCreateDto {
  idProducto: number;
  cantidadDetallePedido: number;
  precioUnitarioDetallePedido: number;
}

export interface DashboardDto {
  totalVentas: number;
  totalPedidos: number;
  totalClientes: number;
  totalProductos: number;
  pedidosPendientes: number;
  pedidosCompletados: number;
  ventasMensuales: number;
  ventasSemanal: number;
}

export interface CategoriaDto {
  idCategoria: number;
  nombreCategoria: string | null;
  descripcionCategoria: string | null;
  estadoCategoria: string | null;
}

export interface EstadoPedidoDto {
  idEstadoPedido: number;
  nombreEstadoPedido: string | null;
  descripcionEstadoPedido: string | null;
}

export interface MetodoPagoDto {
  idMetodoPago: number;
  nombreMetodoPago: string | null;
  descripcionMetodoPago: string | null;
}

export interface RolDto {
  idRol: number;
  nombreRol: string | null;
  descripcionRol: string | null;
}

export interface UsuarioCreateDto {
  nombreUsuario: string;
  emailUsuario: string;
  contrasenaUsuario: string;
  idRol: number;
}

export interface UsuarioUpdateDto {
  nombreUsuario?: string;
  emailUsuario?: string;
  contrasenaUsuario?: string;
  idRol?: number;
  estadoUsuario?: string;
}

export interface AnalisisCompletoDto {
  ventasPorMes: Array<{ mes: string; ventas: number }>;
  productosMasVendidos: Array<{ nombreProducto: string; cantidad: number }>;
  clientesMasFrecuentes: Array<{ nombreCliente: string; totalPedidos: number }>;
  categoriaMasVendida: string | null;
}

