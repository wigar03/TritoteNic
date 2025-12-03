import { useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Card } from "./ui/card";
import { Badge } from "./ui/badge";
import { Minus, Plus, ShoppingCart, FileText } from "lucide-react";
import { toast } from "sonner";

interface Product {
  id: number;
  name: string;
  price: number;
  stock: number;
  category: string;
  image: string;
}

const mockProducts: Product[] = [
  { id: 1, name: 'Tote Bag Clásico Beige', price: 450, stock: 25, category: 'Canvas', image: 'https://images.unsplash.com/photo-1574365569389-a10d488ca3fb?w=200&h=200&fit=crop' },
  { id: 2, name: 'Tote Bag Eco Natural', price: 520, stock: 18, category: 'Ecológico', image: 'https://images.unsplash.com/photo-1758708536099-9f46dc81fffc?w=200&h=200&fit=crop' },
  { id: 3, name: 'Tote Bag Shopping Negro', price: 480, stock: 30, category: 'Canvas', image: 'https://images.unsplash.com/photo-1759463408569-c81a969a6c3d?w=200&h=200&fit=crop' },
  { id: 4, name: 'Tote Bag Playero Azul', price: 580, stock: 15, category: 'Playa', image: 'https://images.unsplash.com/photo-1465742744535-bac796b80f3a?w=200&h=200&fit=crop' },
  { id: 5, name: 'Tote Bag Minimalista Blanco', price: 550, stock: 22, category: 'Minimalista', image: 'https://images.unsplash.com/photo-1647426112650-6c96ce7ab5f0?w=200&h=200&fit=crop' },
  { id: 6, name: 'Tote Bag Market Kraft', price: 420, stock: 28, category: 'Market', image: 'https://images.unsplash.com/photo-1663154438413-244fae34c9a7?w=200&h=200&fit=crop' },
];

interface CartItem extends Product {
  quantity: number;
}

interface NewOrderDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function NewOrderDialog({ open, onOpenChange }: NewOrderDialogProps) {
  const [step, setStep] = useState(1);
  const [customer, setCustomer] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('');
  const [cart, setCart] = useState<CartItem[]>([]);
  const [discount, setDiscount] = useState(0);

  const addToCart = (product: Product) => {
    const existing = cart.find(item => item.id === product.id);
    if (existing) {
      if (existing.quantity < product.stock) {
        setCart(cart.map(item =>
          item.id === product.id ? { ...item, quantity: item.quantity + 1 } : item
        ));
      } else {
        toast.error('Stock insuficiente');
      }
    } else {
      setCart([...cart, { ...product, quantity: 1 }]);
    }
  };

  const updateQuantity = (productId: number, delta: number) => {
    setCart(cart.map(item => {
      if (item.id === productId) {
        const newQuantity = item.quantity + delta;
        if (newQuantity <= 0) return item;
        if (newQuantity > item.stock) {
          toast.error('Stock insuficiente');
          return item;
        }
        return { ...item, quantity: newQuantity };
      }
      return item;
    }).filter(item => item.quantity > 0));
  };

  const removeFromCart = (productId: number) => {
    setCart(cart.filter(item => item.id !== productId));
  };

  const subtotal = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  const discountAmount = (subtotal * discount) / 100;
  const subtotalAfterDiscount = subtotal - discountAmount;
  const iva = subtotalAfterDiscount * 0.15; // IVA del 15%
  const total = subtotalAfterDiscount + iva;

  const handleConfirm = () => {
    toast.success('Pedido creado exitosamente', {
      description: `Pedido para ${customer} por $${total.toLocaleString()}`,
    });
    onOpenChange(false);
    resetForm();
  };

  const resetForm = () => {
    setStep(1);
    setCustomer('');
    setPaymentMethod('');
    setCart([]);
    setDiscount(0);
  };

  return (
    <Dialog open={open} onOpenChange={(open) => {
      if (!open) resetForm();
      onOpenChange(open);
    }}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Nuevo Pedido</DialogTitle>
          <DialogDescription>
            {step === 1 && 'Paso 1: Seleccionar cliente y método de pago'}
            {step === 2 && 'Paso 2: Elegir productos del catálogo'}
            {step === 3 && 'Paso 3: Confirmar pedido'}
          </DialogDescription>
        </DialogHeader>

        {/* Paso 1: Cliente */}
        {step === 1 && (
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Cliente</Label>
              <Input
                placeholder="Nombre del cliente"
                value={customer}
                onChange={(e) => setCustomer(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Método de Pago</Label>
              <Select value={paymentMethod} onValueChange={setPaymentMethod}>
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar método" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="cash">Efectivo</SelectItem>
                  <SelectItem value="transfer">Transferencia</SelectItem>
                  <SelectItem value="card">Tarjeta</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="flex justify-end">
              <Button
                onClick={() => setStep(2)}
                disabled={!customer || !paymentMethod}
                className="bg-[#C9A664] hover:bg-[#B8965A]"
              >
                Siguiente
              </Button>
            </div>
          </div>
        )}

        {/* Paso 2: Productos */}
        {step === 2 && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
              {mockProducts.map((product) => (
                <Card key={product.id} className="overflow-hidden">
                  <img
                    src={product.image}
                    alt={product.name}
                    className="w-full h-32 object-cover"
                  />
                  <div className="p-3 space-y-2">
                    <h4 className="text-sm">{product.name}</h4>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">${product.price.toLocaleString()}</span>
                      <Badge variant="outline" className="text-xs">
                        Stock: {product.stock}
                      </Badge>
                    </div>
                    <Button
                      size="sm"
                      onClick={() => addToCart(product)}
                      className="w-full bg-[#C9A664] hover:bg-[#B8965A]"
                    >
                      <Plus className="h-3 w-3 mr-1" />
                      Agregar
                    </Button>
                  </div>
                </Card>
              ))}
            </div>

            {/* Carrito */}
            {cart.length > 0 && (
              <Card className="p-4 bg-muted/50">
                <h4 className="mb-3 flex items-center gap-2">
                  <ShoppingCart className="h-4 w-4" />
                  Carrito ({cart.length} productos)
                </h4>
                <div className="space-y-2">
                  {cart.map((item) => (
                    <div key={item.id} className="flex items-center justify-between bg-background p-2 rounded">
                      <div className="flex-1">
                        <p className="text-sm">{item.name}</p>
                        <p className="text-xs text-muted-foreground">${item.price.toLocaleString()} × {item.quantity}</p>
                      </div>
                      <div className="flex items-center gap-2">
                        <Button size="sm" variant="outline" onClick={() => updateQuantity(item.id, -1)}>
                          <Minus className="h-3 w-3" />
                        </Button>
                        <span className="text-sm w-8 text-center">{item.quantity}</span>
                        <Button size="sm" variant="outline" onClick={() => updateQuantity(item.id, 1)}>
                          <Plus className="h-3 w-3" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              </Card>
            )}

            <div className="flex justify-between">
              <Button variant="outline" onClick={() => setStep(1)}>
                Atrás
              </Button>
              <Button
                onClick={() => setStep(3)}
                disabled={cart.length === 0}
                className="bg-[#C9A664] hover:bg-[#B8965A]"
              >
                Siguiente
              </Button>
            </div>
          </div>
        )}

        {/* Paso 3: Confirmar */}
        {step === 3 && (
          <div className="space-y-4">
            <Card className="p-4">
              <h4 className="mb-3">Resumen del Pedido</h4>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Cliente:</span>
                  <span>{customer}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Método de pago:</span>
                  <span className="capitalize">{paymentMethod === 'cash' ? 'Efectivo' : paymentMethod === 'transfer' ? 'Transferencia' : 'Tarjeta'}</span>
                </div>
              </div>
            </Card>

            <Card className="p-4">
              <h4 className="mb-3">Productos</h4>
              <div className="space-y-2">
                {cart.map((item) => (
                  <div key={item.id} className="flex justify-between text-sm">
                    <span>{item.name} × {item.quantity}</span>
                    <span>${(item.price * item.quantity).toLocaleString()}</span>
                  </div>
                ))}
              </div>
            </Card>

            <div className="space-y-2">
              <Label>Descuento (%)</Label>
              <Input
                type="number"
                min="0"
                max="100"
                value={discount}
                onChange={(e) => setDiscount(Math.min(100, Math.max(0, Number(e.target.value))))}
              />
            </div>

            <Card className="p-4 bg-[#C9A664]/10">
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span>Subtotal:</span>
                  <span>C${subtotal.toFixed(2)}</span>
                </div>
                {discount > 0 && (
                  <div className="flex justify-between text-sm text-green-600">
                    <span>Descuento ({discount}%):</span>
                    <span>-C${discountAmount.toFixed(2)}</span>
                  </div>
                )}
                <div className="flex justify-between text-sm border-t pt-2">
                  <span>Subtotal después de descuento:</span>
                  <span>C${subtotalAfterDiscount.toFixed(2)}</span>
                </div>
                <div className="flex justify-between text-sm text-blue-600">
                  <span>IVA (15%):</span>
                  <span>+C${iva.toFixed(2)}</span>
                </div>
                <div className="flex justify-between pt-2 border-t">
                  <span className="font-medium">Total:</span>
                  <span className="font-medium text-lg text-[#C9A664]">C${total.toFixed(2)}</span>
                </div>
              </div>
            </Card>

            <div className="flex justify-between">
              <Button variant="outline" onClick={() => setStep(2)}>
                Atrás
              </Button>
              <div className="flex gap-2">
                <Button variant="outline" onClick={handleConfirm}>
                  <FileText className="h-4 w-4 mr-2" />
                  Generar PDF
                </Button>
                <Button onClick={handleConfirm} className="bg-[#C9A664] hover:bg-[#B8965A]">
                  Confirmar Pedido
                </Button>
              </div>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}