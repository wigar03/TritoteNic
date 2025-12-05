import { useState, useEffect } from "react";
import { Button } from "./ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Input } from "./ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "./ui/table";
import { Badge } from "./ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Plus, Search, Eye, Pencil, Trash2, Filter } from "lucide-react";
import { NewOrderDialog } from "./NewOrderDialog";
import { OrderDetailsDialog } from "./OrderDetailsDialog";
import { EditOrderDialog } from "./EditOrderDialog";
import { ConfirmDialog } from "./ConfirmDialog";
import { usePermissions } from "../contexts/AuthContext";
import { toast } from "sonner";
import { apiService } from "../services/apiService";
import type { PedidoDto, EstadoPedidoDto, MetodoPagoDto, ClienteDto } from "../types/api";

type OrderStatus = 'pending' | 'in-progress' | 'completed' | 'cancelled' | 'delayed';

interface Order {
  id: string;
  idPedido: number;
  customer: string;
  date: string;
  status: OrderStatus;
  total: number;
  paymentMethod: string;
  idEstadoPedido: number;
  idMetodoPago: number;
  idCliente: number;
}

const statusConfig = {
  pending: { label: 'Pendiente', className: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
  'in-progress': { label: 'En Proceso', className: 'bg-blue-100 text-blue-800 border-blue-200' },
  completed: { label: 'Completado', className: 'bg-green-100 text-green-800 border-green-200' },
  cancelled: { label: 'Cancelado', className: 'bg-gray-100 text-gray-800 border-gray-200' },
  delayed: { label: 'Retrasado', className: 'bg-red-100 text-red-800 border-red-200' },
};

// Función para mapear nombre de estado a OrderStatus
const mapEstadoToStatus = (estadoNombre: string | null | undefined): OrderStatus => {
  if (!estadoNombre) return 'pending';
  const estado = estadoNombre.toLowerCase();
  if (estado.includes('pendiente')) return 'pending';
  if (estado.includes('proceso')) return 'in-progress';
  if (estado.includes('completado')) return 'completed';
  if (estado.includes('cancelado')) return 'cancelled';
  if (estado.includes('retrasado')) return 'delayed';
  return 'pending';
};

export function OrdersManagement() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [estadosPedido, setEstadosPedido] = useState<EstadoPedidoDto[]>([]);
  const [metodosPago, setMetodosPago] = useState<MetodoPagoDto[]>([]);
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [newOrderOpen, setNewOrderOpen] = useState(false);
  const [viewOrderOpen, setViewOrderOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [editOrderOpen, setEditOrderOpen] = useState(false);
  const [deleteOrderOpen, setDeleteOrderOpen] = useState(false);
  const { hasPermission } = usePermissions();

  useEffect(() => {
    loadOrders();
    loadEstadosPedido();
    loadMetodosPago();
    loadClientes();
  }, []);

  const loadOrders = async () => {
    try {
      setIsLoading(true);
      console.log("Cargando pedidos...");
      const pedidos = await apiService.getPedidos();
      console.log("Pedidos recibidos:", pedidos);
      if (pedidos && Array.isArray(pedidos)) {
        const mappedOrders: Order[] = pedidos.map(p => ({
          id: `#${p.idPedido.toString().padStart(3, '0')}`,
          idPedido: p.idPedido,
          customer: p.nombreCliente || 'Sin cliente',
          date: p.fechaPedido ? new Date(p.fechaPedido).toISOString().split('T')[0] : new Date().toISOString().split('T')[0],
          status: mapEstadoToStatus(p.nombreEstadoPedido),
          total: Number(p.totalPedido || 0),
          paymentMethod: p.nombreMetodoPago || 'N/A',
          idEstadoPedido: p.idEstadoPedido || 0,
          idMetodoPago: p.idMetodoPago || 0,
          idCliente: p.idCliente || 0
        }));
        console.log("Pedidos mapeados:", mappedOrders);
        setOrders(mappedOrders);
      } else {
        console.warn("Pedidos devolvieron null o no es un array");
        setOrders([]);
      }
    } catch (error) {
      console.error("Error al cargar pedidos:", error);
      if (error instanceof Error) {
        console.error("Mensaje de error:", error.message);
        console.error("Stack:", error.stack);
      }
      const errorMessage = error instanceof Error ? error.message : "Error al cargar pedidos";
      toast.error("Error", {
        description: errorMessage,
      });
      setOrders([]); // Asegurar que orders no sea undefined
    } finally {
      setIsLoading(false);
    }
  };

  const loadEstadosPedido = async () => {
    try {
      const estados = await apiService.getEstadosPedido();
      if (estados) {
        setEstadosPedido(estados);
      }
    } catch (error) {
      console.error("Error al cargar estados de pedido:", error);
    }
  };

  const loadMetodosPago = async () => {
    try {
      const metodos = await apiService.getMetodosPago();
      if (metodos) {
        setMetodosPago(metodos);
      }
    } catch (error) {
      console.error("Error al cargar métodos de pago:", error);
    }
  };

  const loadClientes = async () => {
    try {
      const clientesData = await apiService.getClientes();
      if (clientesData) {
        setClientes(clientesData);
      }
    } catch (error) {
      console.error("Error al cargar clientes:", error);
    }
  };

  const filteredOrders = orders.filter(order => {
    const matchesSearch = order.customer.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          order.id.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || order.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleEditOrder = (order: Order) => {
    setSelectedOrder(order);
    setEditOrderOpen(true);
  };

  const handleOrderUpdated = async (updatedOrder: Order) => {
    try {
      // Validar que el idPedido existe
      if (!updatedOrder.idPedido || updatedOrder.idPedido === 0) {
        toast.error('Error: No se pudo identificar el pedido');
        return;
      }

      const updateDto = {
        idPedido: updatedOrder.idPedido,
        idEstadoPedido: updatedOrder.idEstadoPedido,
        idMetodoPago: updatedOrder.idMetodoPago,
        totalPedido: updatedOrder.total
      };

      console.log('Actualizando pedido:', updateDto);
      
      await apiService.updatePedido(updatedOrder.idPedido, updateDto);
      toast.success('Pedido actualizado exitosamente');
      await loadOrders(); // Recargar desde la API
      setEditOrderOpen(false);
    } catch (error) {
      console.error('Error al actualizar pedido:', error);
      const errorMessage = error instanceof Error ? error.message : "Error al actualizar pedido";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  const generatePDF = async (order: Order) => {
    try {
      // Importar jsPDF dinámicamente
      const { jsPDF } = await import('jspdf');
      
      // Obtener detalles completos del pedido desde la API
      const pedidoCompleto = await apiService.getPedido(order.idPedido);
      
      // Debug: ver qué está llegando
      console.log('Pedido completo recibido:', pedidoCompleto);
      console.log('Detalles del pedido:', pedidoCompleto.detallesPedido || pedidoCompleto.detalles);
      
      // Crear nuevo documento PDF
      const doc = new jsPDF();
      
      // Configuración de colores y estilos
      const primaryColor = [201, 166, 100]; // #C9A664
      const textColor = [51, 51, 51];
      const grayColor = [128, 128, 128];
      
      let yPos = 20;
      
      // Encabezado
      doc.setFillColor(...primaryColor);
      doc.rect(0, 0, 210, 30, 'F');
      doc.setTextColor(255, 255, 255);
      doc.setFontSize(20);
      doc.setFont('helvetica', 'bold');
      doc.text('TRITOTE NIC', 105, 15, { align: 'center' });
      doc.setFontSize(12);
      doc.text('Comprobante de Pedido', 105, 25, { align: 'center' });
      
      yPos = 40;
      doc.setTextColor(...textColor);
      
      // Información del pedido
      doc.setFontSize(16);
      doc.setFont('helvetica', 'bold');
      doc.text(`Pedido ${order.id}`, 20, yPos);
      
      yPos += 10;
      doc.setFontSize(10);
      doc.setFont('helvetica', 'normal');
      doc.setTextColor(...grayColor);
      doc.text(`Fecha: ${new Date(order.date).toLocaleDateString('es-NI')}`, 20, yPos);
      yPos += 5;
      doc.text(`Estado: ${statusConfig[order.status].label}`, 20, yPos);
      
      yPos += 10;
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(...textColor);
      doc.text('Información del Cliente', 20, yPos);
      
      yPos += 7;
      doc.setFont('helvetica', 'normal');
      doc.setTextColor(...grayColor);
      doc.text(`Nombre: ${order.customer}`, 20, yPos);
      yPos += 5;
      doc.text(`Método de Pago: ${order.paymentMethod}`, 20, yPos);
      
      yPos += 10;
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(...textColor);
      doc.text('Productos', 20, yPos);
      
      yPos += 7;
      
      // Tabla de productos - el backend ahora serializa como "detalles" (camelCase)
      const detalles = pedidoCompleto.detalles || pedidoCompleto.detallesPedido || [];
      
      if (detalles && detalles.length > 0) {
        // Encabezados de tabla
        doc.setFillColor(240, 240, 240);
        doc.rect(20, yPos - 5, 170, 7, 'F');
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(9);
        doc.setTextColor(...textColor);
        doc.text('Producto', 22, yPos);
        doc.text('Cantidad', 120, yPos);
        doc.text('Precio Unit.', 145, yPos);
        doc.text('Subtotal', 175, yPos);
        
        yPos += 7;
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(9);
        doc.setTextColor(...grayColor);
        
        detalles.forEach((detalle: any) => {
          if (yPos > 250) {
            doc.addPage();
            yPos = 20;
          }
          
          const nombreProducto = detalle.nombreProducto || 'Producto sin nombre';
          // Usar las propiedades correctas según el DTO (el backend usa CantidadProducto, PrecioUnitarioProducto, SubtotalProducto)
          const cantidad = detalle.cantidadDetallePedido || detalle.cantidadProducto || 0;
          const precioUnitario = detalle.precioUnitarioDetallePedido || detalle.precioUnitarioProducto || 0;
          const subtotal = detalle.subtotalDetallePedido || detalle.subtotalProducto || 0;
          
          doc.text(nombreProducto.substring(0, 40), 22, yPos);
          doc.text(cantidad.toString(), 120, yPos);
          doc.text(`C$${precioUnitario.toFixed(2)}`, 145, yPos);
          doc.text(`C$${subtotal.toFixed(2)}`, 175, yPos);
          yPos += 6;
        });
      } else {
        doc.setTextColor(...grayColor);
        doc.text('No hay productos en este pedido', 22, yPos);
        yPos += 6;
      }
      
      yPos += 5;
      
      // Línea separadora
      doc.setDrawColor(200, 200, 200);
      doc.line(20, yPos, 190, yPos);
      yPos += 8;
      
      // Totales
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(11);
      doc.setTextColor(...textColor);
      doc.text('Total:', 150, yPos);
      doc.text(`C$${order.total.toFixed(2)}`, 175, yPos);
      
      // Pie de página
      const pageHeight = doc.internal.pageSize.height;
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(8);
      doc.setTextColor(...grayColor);
      doc.text('Gracias por su compra', 105, pageHeight - 15, { align: 'center' });
      doc.text(`Generado el ${new Date().toLocaleDateString('es-NI')} ${new Date().toLocaleTimeString('es-NI')}`, 105, pageHeight - 10, { align: 'center' });
      
      // Guardar PDF
      doc.save(`Pedido_${order.id.replace('#', '')}.pdf`);
      
      toast.success('PDF generado exitosamente');
    } catch (error) {
      console.error('Error al generar PDF:', error);
      toast.error('Error al generar el PDF');
    }
  };

  const handleDeleteOrder = (order: Order) => {
    setSelectedOrder(order);
    setDeleteOrderOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!selectedOrder) return;
    
    try {
      await apiService.deletePedido(selectedOrder.idPedido);
      toast.success('Pedido eliminado exitosamente');
      await loadOrders(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al eliminar pedido";
      toast.error("Error", {
        description: errorMessage,
      });
    } finally {
      setDeleteOrderOpen(false);
      setSelectedOrder(null);
    }
  };

  if (isLoading) {
    return (
      <div className="p-6 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#C9A664] mx-auto mb-4"></div>
          <p className="text-muted-foreground">Cargando pedidos...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1>Gestión de Pedidos</h1>
          <p className="text-muted-foreground">Administra y monitorea todos los pedidos</p>
        </div>
        <Button onClick={() => setNewOrderOpen(true)} className="bg-[#C9A664] hover:bg-[#B8965A]">
          <Plus className="h-4 w-4 mr-2" />
          Nuevo Pedido
        </Button>
      </div>

      {/* Filtros */}
      <div className="flex gap-4">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Buscar pedidos..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-9"
          />
        </div>
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-[180px]">
            <Filter className="h-4 w-4 mr-2" />
            <SelectValue placeholder="Filtrar por estado" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todos los estados</SelectItem>
            <SelectItem value="pending">Pendiente</SelectItem>
            <SelectItem value="in-progress">En Proceso</SelectItem>
            <SelectItem value="completed">Completado</SelectItem>
            <SelectItem value="cancelled">Cancelado</SelectItem>
            <SelectItem value="delayed">Retrasado</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Tabla de pedidos */}
      <Card>
        <CardHeader>
          <CardTitle>Lista de Pedidos</CardTitle>
          <CardDescription>
            Mostrando {filteredOrders.length} de {orders.length} pedidos
          </CardDescription>
        </CardHeader>
        <CardContent>
          {filteredOrders.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              {orders.length === 0 ? "No hay pedidos registrados" : "No se encontraron pedidos"}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>ID</TableHead>
                  <TableHead>Cliente</TableHead>
                  <TableHead>Fecha</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead>Total</TableHead>
                  <TableHead>Método de Pago</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredOrders.map((order) => {
                  const status = statusConfig[order.status];
                  return (
                    <TableRow key={order.idPedido}>
                      <TableCell className="font-medium">{order.id}</TableCell>
                      <TableCell>{order.customer}</TableCell>
                      <TableCell>{order.date}</TableCell>
                      <TableCell>
                        <Badge variant="outline" className={status.className}>
                          {status.label}
                        </Badge>
                      </TableCell>
                      <TableCell className="font-medium">${order.total.toLocaleString()}</TableCell>
                      <TableCell>{order.paymentMethod}</TableCell>
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end gap-1">
                          <Button variant="ghost" size="sm" onClick={() => {
                            setSelectedOrder(order);
                            setViewOrderOpen(true);
                          }}>
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="sm" onClick={() => handleEditOrder(order)}>
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button 
                            variant="ghost" 
                            size="sm" 
                            onClick={() => handleDeleteOrder(order)}
                            disabled={!hasPermission('pedidos.delete')}
                            className="text-red-600 hover:text-red-700 hover:bg-red-50 disabled:opacity-30"
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <NewOrderDialog
        open={newOrderOpen}
        onOpenChange={setNewOrderOpen}
        clientes={clientes}
        productos={[]} // Se cargará dentro del diálogo
        estadosPedido={estadosPedido}
        metodosPago={metodosPago}
        onOrderCreated={loadOrders}
      />

      <OrderDetailsDialog
        open={viewOrderOpen}
        onOpenChange={setViewOrderOpen}
        order={selectedOrder || {
          id: '',
          idPedido: 0,
          customer: '',
          date: '',
          status: 'pending',
          total: 0,
          paymentMethod: '',
          idEstadoPedido: 0,
          idMetodoPago: 0,
          idCliente: 0
        }}
        onGeneratePDF={() => {
          if (selectedOrder) {
            generatePDF(selectedOrder);
          }
        }}
      />

      <EditOrderDialog
        open={editOrderOpen}
        onOpenChange={setEditOrderOpen}
        order={selectedOrder}
        estadosPedido={estadosPedido}
        metodosPago={metodosPago}
        onOrderUpdated={handleOrderUpdated}
      />

      <ConfirmDialog
        open={deleteOrderOpen}
        onOpenChange={setDeleteOrderOpen}
        title="Eliminar Pedido"
        description={`¿Estás seguro de que quieres eliminar el pedido "${selectedOrder?.id}"? Esta acción no se puede deshacer.`}
        onConfirm={handleConfirmDelete}
        confirmText="Eliminar"
        variant="destructive"
      />
    </div>
  );
}
