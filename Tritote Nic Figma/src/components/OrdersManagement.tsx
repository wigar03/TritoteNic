import { useState } from "react";
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

type OrderStatus = 'pending' | 'in-progress' | 'completed' | 'cancelled' | 'delayed';

interface Order {
  id: string;
  customer: string;
  date: string;
  status: OrderStatus;
  total: number;
  paymentMethod: string;
}

const mockOrders: Order[] = [
  { id: '#001', customer: 'María González', date: '2025-10-06', status: 'pending', total: 8500, paymentMethod: 'Transferencia' },
  { id: '#002', customer: 'Ana Rodríguez', date: '2025-10-06', status: 'in-progress', total: 12300, paymentMethod: 'Efectivo' },
  { id: '#003', customer: 'Lucía Fernández', date: '2025-10-05', status: 'completed', total: 6700, paymentMethod: 'Tarjeta' },
  { id: '#004', customer: 'Carolina Silva', date: '2025-10-05', status: 'delayed', total: 9200, paymentMethod: 'Transferencia' },
  { id: '#005', customer: 'Sofía Martínez', date: '2025-10-04', status: 'completed', total: 15400, paymentMethod: 'Efectivo' },
  { id: '#006', customer: 'Valentina López', date: '2025-10-04', status: 'in-progress', total: 7800, paymentMethod: 'Tarjeta' },
  { id: '#007', customer: 'Camila Díaz', date: '2025-10-03', status: 'pending', total: 10500, paymentMethod: 'Transferencia' },
  { id: '#008', customer: 'Isabella Torres', date: '2025-10-03', status: 'completed', total: 13200, paymentMethod: 'Efectivo' },
];

const statusConfig = {
  pending: { label: 'Pendiente', className: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
  'in-progress': { label: 'En Proceso', className: 'bg-blue-100 text-blue-800 border-blue-200' },
  completed: { label: 'Completado', className: 'bg-green-100 text-green-800 border-green-200' },
  cancelled: { label: 'Cancelado', className: 'bg-gray-100 text-gray-800 border-gray-200' },
  delayed: { label: 'Retrasado', className: 'bg-red-100 text-red-800 border-red-200' },
};

export function OrdersManagement() {
  const [orders, setOrders] = useState(mockOrders);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [newOrderOpen, setNewOrderOpen] = useState(false);
  const [viewOrderOpen, setViewOrderOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [editOrderOpen, setEditOrderOpen] = useState(false);
  const [deleteOrderOpen, setDeleteOrderOpen] = useState(false);
  const { hasPermission } = usePermissions();

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

  const handleOrderUpdated = (updatedOrder: Order) => {
    setOrders(orders.map(o => o.id === updatedOrder.id ? updatedOrder : o));
  };

  const handleDeleteOrder = (order: Order) => {
    setSelectedOrder(order);
    setDeleteOrderOpen(true);
  };

  const handleConfirmDelete = () => {
    if (selectedOrder) {
      setOrders(orders.filter(order => order.id !== selectedOrder.id));
      toast.success('Pedido eliminado exitosamente');
      setDeleteOrderOpen(false);
      setSelectedOrder(null);
    }
  };

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
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 md:flex-row md:items-center">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Buscar por cliente o número de pedido..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9"
              />
            </div>
            <div className="flex items-center gap-2">
              <Filter className="h-4 w-4 text-muted-foreground" />
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder="Estado" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los estados</SelectItem>
                  <SelectItem value="pending">Pendiente</SelectItem>
                  <SelectItem value="in-progress">En Proceso</SelectItem>
                  <SelectItem value="completed">Completado</SelectItem>
                  <SelectItem value="delayed">Retrasado</SelectItem>
                  <SelectItem value="cancelled">Cancelado</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Tabla de pedidos */}
      <Card>
        <CardHeader>
          <CardTitle>Pedidos</CardTitle>
          <CardDescription>
            Mostrando {filteredOrders.length} de {orders.length} pedidos
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nº Pedido</TableHead>
                <TableHead>Cliente</TableHead>
                <TableHead>Fecha</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead>Total</TableHead>
                <TableHead>Método de Pago</TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredOrders.map((order) => (
                <TableRow key={order.id}>
                  <TableCell className="font-medium">{order.id}</TableCell>
                  <TableCell>{order.customer}</TableCell>
                  <TableCell>{order.date}</TableCell>
                  <TableCell>
                    <Badge variant="outline" className={statusConfig[order.status].className}>
                      {statusConfig[order.status].label}
                    </Badge>
                  </TableCell>
                  <TableCell>${order.total.toLocaleString()}</TableCell>
                  <TableCell>{order.paymentMethod}</TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        setSelectedOrder(order);
                        setViewOrderOpen(true);
                      }}
                    >
                      <Eye className="h-4 w-4 mr-1" />
                      Ver
                    </Button>
                    {hasPermission('editOrders') && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleEditOrder(order)}
                      >
                        <Pencil className="h-4 w-4 mr-1" />
                        Editar
                      </Button>
                    )}
                    {hasPermission('deleteOrders') && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDeleteOrder(order)}
                      >
                        <Trash2 className="h-4 w-4 mr-1" />
                        Eliminar
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <NewOrderDialog open={newOrderOpen} onOpenChange={setNewOrderOpen} />
      {selectedOrder && (
        <OrderDetailsDialog
          order={selectedOrder}
          open={viewOrderOpen}
          onOpenChange={setViewOrderOpen}
          onEditClick={() => {
            setViewOrderOpen(false);
            setEditOrderOpen(true);
          }}
        />
      )}
      {editOrderOpen && selectedOrder && (
        <EditOrderDialog
          order={selectedOrder}
          open={editOrderOpen}
          onOpenChange={setEditOrderOpen}
          onOrderUpdated={handleOrderUpdated}
        />
      )}
      {deleteOrderOpen && selectedOrder && (
        <ConfirmDialog
          open={deleteOrderOpen}
          onOpenChange={setDeleteOrderOpen}
          onConfirm={handleConfirmDelete}
          title="Eliminar Pedido"
          description={`¿Estás seguro de que quieres eliminar el pedido ${selectedOrder.id}?`}
        />
      )}
    </div>
  );
}