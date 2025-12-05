import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Label } from "./ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Input } from "./ui/input";
import { toast } from "sonner";
import type { EstadoPedidoDto, MetodoPagoDto } from "../types/api";

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

interface EditOrderDialogProps {
  order: Order | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onOrderUpdated: (order: Order) => void;
  estadosPedido?: EstadoPedidoDto[];
  metodosPago?: MetodoPagoDto[];
}

const statusConfig = {
  pending: { label: 'Pendiente', className: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
  'in-progress': { label: 'En Proceso', className: 'bg-blue-100 text-blue-800 border-blue-200' },
  completed: { label: 'Completado', className: 'bg-green-100 text-green-800 border-green-200' },
  cancelled: { label: 'Cancelado', className: 'bg-gray-100 text-gray-800 border-gray-200' },
  delayed: { label: 'Retrasado', className: 'bg-red-100 text-red-800 border-red-200' },
};

// Función para mapear ID de estado a OrderStatus
const mapEstadoIdToStatus = (estadoNombre: string | null | undefined): OrderStatus => {
  if (!estadoNombre) return 'pending';
  const estado = estadoNombre.toLowerCase();
  if (estado.includes('pendiente')) return 'pending';
  if (estado.includes('proceso')) return 'in-progress';
  if (estado.includes('completado')) return 'completed';
  if (estado.includes('cancelado')) return 'cancelled';
  if (estado.includes('retrasado')) return 'delayed';
  return 'pending';
};

export function EditOrderDialog({ order, open, onOpenChange, onOrderUpdated, estadosPedido = [], metodosPago = [] }: EditOrderDialogProps) {
  const [formData, setFormData] = useState<Order>({
    id: '',
    idPedido: 0,
    customer: '',
    date: '',
    status: 'pending',
    total: 0,
    paymentMethod: 'Efectivo',
    idEstadoPedido: 0,
    idMetodoPago: 0,
    idCliente: 0
  });

  // Estado temporal para campos numéricos (permite borrar el 0)
  const [totalInput, setTotalInput] = useState<string>('');
  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  useEffect(() => {
    if (order) {
      // Preservar el idPedido del order original
      setFormData({
        ...order,
        idPedido: order.idPedido // Asegurar que el ID se mantiene
      });
      // Inicializar el campo de total (permite borrar y escribir desde cero)
      // Si el total es 0 o no existe, mostrar campo vacío para facilitar escritura desde cero
      setTotalInput(order.total !== null && order.total !== undefined && order.total > 0 ? order.total.toString() : '');
      setErrors({});
    }
  }, [order]);

  const handleTotalChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    
    // Permitir borrar todo el contenido (permite escribir desde cero)
    if (value === '') {
      setTotalInput('');
      setErrors(prev => ({ ...prev, total: '' }));
      return;
    }

    // Solo permitir números y punto decimal (permite múltiples dígitos, incluye 0 al inicio)
    if (!/^\d*\.?\d{0,2}$/.test(value)) {
      return;
    }

    setTotalInput(value);
    setErrors(prev => ({ ...prev, total: '' }));

    // No actualizar formData aquí, solo cuando haya blur o submit
  };

  const handleTotalBlur = () => {
    // Validar cuando el usuario sale del campo
    if (totalInput === '' || totalInput.trim() === '') {
      // Permitir que quede vacío temporalmente (validación final en submit)
      setErrors(prev => ({ ...prev, total: '' }));
      return;
    }

    const numValue = parseFloat(totalInput);
    if (isNaN(numValue) || numValue < 0) {
      setErrors(prev => ({ ...prev, total: 'El total debe ser un número válido mayor o igual a 0' }));
      // No restaurar automáticamente, dejar que el usuario corrija
    } else {
      setFormData({ ...formData, total: numValue });
      setErrors(prev => ({ ...prev, total: '' }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: { [key: string]: string } = {};

    if (!formData.idEstadoPedido || formData.idEstadoPedido === 0) {
      newErrors.estado = 'El estado del pedido es requerido';
    }

    if (!formData.idMetodoPago || formData.idMetodoPago === 0) {
      newErrors.metodoPago = 'El método de pago es requerido';
    }

    if (totalInput === '' || totalInput.trim() === '') {
      newErrors.total = 'El total es requerido';
    } else {
      const numValue = parseFloat(totalInput);
      if (isNaN(numValue) || numValue < 0) {
        newErrors.total = 'El total debe ser un número válido mayor o igual a 0';
      }
    }

    if (!formData.date || formData.date.trim() === '') {
      newErrors.date = 'La fecha del pedido es requerida';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      toast.error('Por favor completa todos los campos requeridos');
      return;
    }

    // Asegurar que el total esté actualizado
    const finalTotal = parseFloat(totalInput);
    if (isNaN(finalTotal) || finalTotal < 0) {
      toast.error('El total debe ser un número válido');
      return;
    }

    // Asegurar que idPedido se mantiene del order original
    if (!order || !order.idPedido || order.idPedido === 0) {
      toast.error('Error: No se pudo identificar el pedido');
      console.error('Error: order o idPedido inválido', { order, idPedido: order?.idPedido });
      return;
    }

    // Construir el objeto actualizado preservando todos los campos necesarios
    const updatedOrder: Order = {
      ...formData,
      idPedido: order.idPedido, // SIEMPRE usar el ID del order original
      total: finalTotal,
      idEstadoPedido: formData.idEstadoPedido,
      idMetodoPago: formData.idMetodoPago
    };

    console.log('Enviando pedido actualizado:', {
      idPedido: updatedOrder.idPedido,
      idEstadoPedido: updatedOrder.idEstadoPedido,
      idMetodoPago: updatedOrder.idMetodoPago,
      total: updatedOrder.total
    });

    onOrderUpdated(updatedOrder);
  };

  if (!order) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Editar Pedido {order.id}</DialogTitle>
          <DialogDescription>
            Modifica el estado y la información del pedido
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="edit-order-id">Número de Pedido</Label>
              <Input
                id="edit-order-id"
                value={formData.id}
                disabled
                className="bg-muted"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="edit-order-customer">Cliente</Label>
              <Input
                id="edit-order-customer"
                value={formData.customer}
                disabled
                className="bg-muted"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="edit-order-date">Fecha del Pedido *</Label>
              <Input
                id="edit-order-date"
                type="date"
                value={formData.date}
                onChange={(e) => {
                  setFormData({ ...formData, date: e.target.value });
                  setErrors(prev => ({ ...prev, date: '' }));
                }}
                className={errors.date ? 'border-red-500' : ''}
              />
              {errors.date && (
                <p className="text-sm text-red-500">{errors.date}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="edit-order-status">Estado del Pedido *</Label>
              <Select
                value={formData.idEstadoPedido.toString()}
                onValueChange={(value) => {
                  setFormData({ ...formData, idEstadoPedido: parseInt(value) });
                  setErrors(prev => ({ ...prev, estado: '' }));
                }}
              >
                <SelectTrigger id="edit-order-status" className={errors.estado ? 'border-red-500' : ''}>
                  <SelectValue placeholder="Selecciona un estado" />
                </SelectTrigger>
                <SelectContent>
                  {estadosPedido.map((estado) => (
                    <SelectItem key={estado.idEstadoPedido} value={estado.idEstadoPedido.toString()}>
                      {estado.nombreEstadoPedido}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.estado && (
                <p className="text-sm text-red-500">{errors.estado}</p>
              )}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="edit-order-payment">Método de Pago *</Label>
              <Select
                value={formData.idMetodoPago.toString()}
                onValueChange={(value) => {
                  setFormData({ ...formData, idMetodoPago: parseInt(value) });
                  setErrors(prev => ({ ...prev, metodoPago: '' }));
                }}
              >
                <SelectTrigger id="edit-order-payment" className={errors.metodoPago ? 'border-red-500' : ''}>
                  <SelectValue placeholder="Selecciona un método de pago" />
                </SelectTrigger>
                <SelectContent>
                  {metodosPago.map((metodo) => (
                    <SelectItem key={metodo.idMetodoPago} value={metodo.idMetodoPago.toString()}>
                      {metodo.nombreMetodoPago}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.metodoPago && (
                <p className="text-sm text-red-500">{errors.metodoPago}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="edit-order-total">Total (C$) *</Label>
              <Input
                id="edit-order-total"
                type="text"
                inputMode="decimal"
                value={totalInput}
                onChange={handleTotalChange}
                onBlur={handleTotalBlur}
                placeholder="0.00"
                className={errors.total ? 'border-red-500' : ''}
              />
              {errors.total && (
                <p className="text-sm text-red-500">{errors.total}</p>
              )}
            </div>
          </div>

          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
            <p className="text-sm text-blue-800">
              <strong>Nota:</strong> Los cambios en el pedido se reflejarán inmediatamente en el sistema.
            </p>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancelar
            </Button>
            <Button type="submit" className="bg-[#C9A664] hover:bg-[#B8965A]">
              Guardar Cambios
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
