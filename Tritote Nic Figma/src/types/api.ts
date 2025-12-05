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
  categoriaCliente?: string | null;
  totalGastado: number;
  fechaUltimoPedido?: string | null;
  totalPedidos?: number | null;
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
  estadoProducto: string;
  imagenProducto?: string;
}

export interface ProductoUpdateDto {
  idProducto: number;
  nombreProducto?: string;
  descripcionProducto?: string;
  precioProducto?: number;
  stockProducto?: number;
  idCategoria?: number;
  estadoProducto: string;
}

export interface PedidoDto {
  idPedido: number;
  idCliente: number;
  nombreCliente?: string | null;
  idUsuario: number;
  nombreUsuario?: string | null;
  fechaPedido: string;
  totalPedido: number;
  idEstadoPedido: number;
  nombreEstadoPedido?: string | null;
  idMetodoPago: number;
  nombreMetodoPago?: string | null;
  detallesPedido?: DetallePedidoDto[];
  detalles?: DetallePedidoDto[]; // Propiedad alternativa del backend
}

export interface PedidoCreateDto {
  idCliente: number;
  idUsuario: number;
  idEstadoPedido: number;
  idMetodoPago: number;
  subtotalPedido?: number;
  descuento?: number;
  totalPedido: number;
  detalles: DetallePedidoCreateDto[];
}

export interface PedidoUpdateDto {
  idPedido: number;
  idEstadoPedido: number;
  idMetodoPago: number;
  totalPedido: number;
}

export interface DetallePedidoDto {
  idDetallePedido?: number;
  idDetalle?: number;
  idPedido: number;
  idProducto: number;
  nombreProducto?: string | null;
  cantidadDetallePedido?: number;
  cantidadProducto?: number;
  precioUnitarioDetallePedido?: number;
  precioUnitarioProducto?: number;
  subtotalDetallePedido?: number;
  subtotalProducto?: number;
}

export interface DetallePedidoCreateDto {
  idProducto: number;
  cantidadProducto: number;
  precioUnitarioProducto: number;
  subtotalProducto?: number;
}

export interface DashboardDto {
  ventasKpi?: VentasKpiDto;
  pedidosKpi?: PedidosKpiDto;
  alertas?: AlertaDto[];
  ventasDiarias?: VentasDiariasDto[];
  productosMasVendidos?: ProductoVendidoDto[];
}

export interface VentasKpiDto {
  ventasDia: number;
  ventasSemana: number;
  ventasMes: number;
  porcentajeCambioDia: number;
  porcentajeCambioSemana: number;
  porcentajeCambioMes: number;
}

export interface PedidosKpiDto {
  pedidosActivos: number;
  pedidosPendientes: number;
  pedidosEnProceso: number;
  totalPedidos: number;
}

export interface AlertaDto {
  tipo?: string;
  mensaje?: string;
  idProducto?: number;
  nombreProducto?: string;
  stockActual?: number;
  idPedido?: number;
  diasRetraso?: number;
}

export interface VentasDiariasDto {
  fecha: string;
  totalVentas: number;
  cantidadPedidos: number;
}

export interface ProductoVendidoDto {
  idProducto: number;
  nombreProducto?: string;
  cantidadVendida: number;
  totalVentas: number;
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
  estadoUsuario: string;
}

export interface UsuarioUpdateDto {
  nombreUsuario?: string;
  emailUsuario?: string;
  contrasenaUsuario?: string;
  idRol?: number;
  estadoUsuario?: string;
}

export interface AnalisisCompletoDto {
  comparativaVentas?: ComparativaVentasDto;
  tendenciasColor?: TendenciaColorDto[];
  tendenciasTemporada?: TendenciaTemporadaDto[];
  productosRotacion?: RotacionProductoDto[];
}

export interface ComparativaVentasDto {
  ventasSemanales?: ReporteVentasDto[];
  totalPeriodoActual: number;
  totalPeriodoAnterior: number;
  porcentajeCambio: number;
}

export interface ReporteVentasDto {
  periodo: string;
  ventasActuales: number;
  ventasAnteriores: number;
  porcentajeCambio: number;
  fechaInicio: string;
  fechaFin: string;
}

export interface TendenciaColorDto {
  color: string;
  cantidadVendida: number;
  totalVentas: number;
  porcentajeVentas: number;
  cantidadProductos: number;
}

export interface TendenciaTemporadaDto {
  mes: number;
  nombreMes: string;
  totalVentas: number;
  cantidadPedidos: number;
  promedioVenta: number;
}

export interface RotacionProductoDto {
  idProducto: number;
  nombreProducto: string;
  categoria: string;
  cantidadVendida: number;
  totalVentas: number;
  rotacion: number;
  tipoRotacion: string;
}

