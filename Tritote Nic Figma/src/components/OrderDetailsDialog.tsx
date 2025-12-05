import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Badge } from "./ui/badge";
import { Button } from "./ui/button";
import { Card } from "./ui/card";
import { Download } from "lucide-react";
import { apiService } from "../services/apiService";
import type { PedidoDto, DetallePedidoDto } from "../types/api";

interface Order {
  id: string;
  idPedido: number;
  customer: string;
  date: string;
  status: 'pending' | 'in-progress' | 'completed' | 'cancelled' | 'delayed';
  total: number;
  paymentMethod: string;
}

interface OrderDetailsDialogProps {
  order: Order;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onGeneratePDF?: () => void;
}

const statusConfig = {
  pending: { label: 'Pendiente', className: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
  'in-progress': { label: 'En Proceso', className: 'bg-blue-100 text-blue-800 border-blue-200' },
  completed: { label: 'Completado', className: 'bg-green-100 text-green-800 border-green-200' },
  cancelled: { label: 'Cancelado', className: 'bg-gray-100 text-gray-800 border-gray-200' },
  delayed: { label: 'Retrasado', className: 'bg-red-100 text-red-800 border-red-200' },
};

export function OrderDetailsDialog({ order, open, onOpenChange, onGeneratePDF }: OrderDetailsDialogProps) {
  const [pedidoCompleto, setPedidoCompleto] = useState<PedidoDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (open && order && order.idPedido) {
      loadOrderDetails();
    }
  }, [open, order?.idPedido]);

  const loadOrderDetails = async () => {
    if (!order || !order.idPedido) return;
    
    try {
      setIsLoading(true);
      const pedido = await apiService.getPedido(order.idPedido);
      setPedidoCompleto(pedido);
    } catch (error) {
      console.error("Error al cargar detalles del pedido:", error);
      setPedidoCompleto(null);
    } finally {
      setIsLoading(false);
    }
  };

  if (!order || !order.id) {
    return null;
  }

  const detalles = pedidoCompleto?.detalles || pedidoCompleto?.detallesPedido || [];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">{/* Aumentado de max-w-2xl a max-w-3xl */}
        <DialogHeader>
          <div className="flex items-center justify-between">
            <div>
              <DialogTitle>Pedido {order.id}</DialogTitle>
              <DialogDescription>Detalles completos del pedido</DialogDescription>
            </div>
            <Badge variant="outline" className={statusConfig[order.status].className}>
              {statusConfig[order.status].label}
            </Badge>
          </div>
        </DialogHeader>

        <div className="space-y-4">
          <Card className="p-4">
            <h4 className="mb-3">Información del Cliente</h4>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-muted-foreground">Nombre</p>
                <p>{order.customer}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Fecha del Pedido</p>
                <p>{order.date}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Método de Pago</p>
                <p>{order.paymentMethod}</p>
              </div>
              <div>
                <p className="text-muted-foreground">Total</p>
                <p className="text-lg font-medium">${order.total.toLocaleString()}</p>
              </div>
            </div>
          </Card>

          <Card className="p-4">
            <h4 className="mb-3">Productos</h4>
            {isLoading ? (
              <div className="text-center py-4">
                <p className="text-sm text-muted-foreground">Cargando productos...</p>
              </div>
            ) : detalles.length > 0 ? (
              <div className="space-y-3">
                {detalles.map((detalle: DetallePedidoDto, index: number) => {
                  const nombreProducto = detalle.nombreProducto || 'Producto sin nombre';
                  const cantidad = detalle.cantidadProducto || detalle.cantidadDetallePedido || 0;
                  const precioUnitario = detalle.precioUnitarioProducto || detalle.precioUnitarioDetallePedido || 0;
                  const subtotal = detalle.subtotalProducto || detalle.subtotalDetallePedido || 0;
                  
                  return (
                    <div key={detalle.idDetalle || detalle.idDetallePedido || index} className="flex items-center justify-between py-2 border-b last:border-0">
                      <div>
                        <p className="text-sm font-medium">{nombreProducto}</p>
                        <p className="text-xs text-muted-foreground">
                          Cantidad: {cantidad} × C${precioUnitario.toFixed(2)}
                        </p>
                      </div>
                      <p className="text-sm font-medium">C${subtotal.toFixed(2)}</p>
                    </div>
                  );
                })}
              </div>
            ) : (
              <div className="text-center py-4">
                <p className="text-sm text-muted-foreground">No hay productos en este pedido</p>
              </div>
            )}
          </Card>

          <div className="flex justify-end gap-2">
            <Button 
              className="bg-[#C9A664] hover:bg-[#B8965A]"
              onClick={() => {
                if (onGeneratePDF) {
                  onGeneratePDF();
                }
              }}
            >
              <Download className="h-4 w-4 mr-2" />
              Generar PDF
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}