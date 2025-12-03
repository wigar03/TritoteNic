import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Badge } from "./ui/badge";
import { Button } from "./ui/button";
import { Card } from "./ui/card";
import { Download, Edit } from "lucide-react";

interface Order {
  id: string;
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
  onEditClick?: () => void;
}

const statusConfig = {
  pending: { label: 'Pendiente', className: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
  'in-progress': { label: 'En Proceso', className: 'bg-blue-100 text-blue-800 border-blue-200' },
  completed: { label: 'Completado', className: 'bg-green-100 text-green-800 border-green-200' },
  cancelled: { label: 'Cancelado', className: 'bg-gray-100 text-gray-800 border-gray-200' },
  delayed: { label: 'Retrasado', className: 'bg-red-100 text-red-800 border-red-200' },
};

const mockItems = [
  { name: 'Bolso Clásico Negro', quantity: 2, price: 8500 },
  { name: 'Cartera Mini Rosa', quantity: 1, price: 4500 },
];

export function OrderDetailsDialog({ order, open, onOpenChange, onEditClick }: OrderDetailsDialogProps) {
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
            <div className="space-y-3">
              {mockItems.map((item, index) => (
                <div key={index} className="flex items-center justify-between py-2 border-b last:border-0">
                  <div>
                    <p className="text-sm">{item.name}</p>
                    <p className="text-xs text-muted-foreground">Cantidad: {item.quantity}</p>
                  </div>
                  <p className="text-sm font-medium">${(item.price * item.quantity).toLocaleString()}</p>
                </div>
              ))}
            </div>
          </Card>

          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={onEditClick}>
              <Edit className="h-4 w-4 mr-2" />
              Editar Estado
            </Button>
            <Button className="bg-[#C9A664] hover:bg-[#B8965A]">
              <Download className="h-4 w-4 mr-2" />
              Descargar PDF
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}