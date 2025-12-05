import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Card } from "./ui/card";
import { Badge } from "./ui/badge";
import { Minus, Plus, ShoppingCart, FileText, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { useAuth } from "../contexts/AuthContext";

interface Product {
  id: number;
  name: string;
  price: number;
  stock: number;
  category: string;
  image: string;
}

interface CartItem extends Product {
  quantity: number;
}

import { apiService } from "../services/apiService";
import type { ClienteDto, ProductoDto, EstadoPedidoDto, MetodoPagoDto } from "../types/api";

interface NewOrderDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  clientes?: ClienteDto[];
  productos?: ProductoDto[];
  estadosPedido?: EstadoPedidoDto[];
  metodosPago?: MetodoPagoDto[];
  onOrderCreated?: () => void;
}

export function NewOrderDialog({ 
  open, 
  onOpenChange, 
  clientes = [], 
  productos: productosProp = [], 
  estadosPedido = [], 
  metodosPago = [],
  onOrderCreated 
}: NewOrderDialogProps) {
  const { currentUser, isLoading: authLoading } = useAuth();
  const [step, setStep] = useState(1);
  const [selectedClienteId, setSelectedClienteId] = useState<string>('');
  const [selectedMetodoPagoId, setSelectedMetodoPagoId] = useState<string>('');
  const [selectedEstadoPedidoId, setSelectedEstadoPedidoId] = useState<string>('');
  const [productos, setProductos] = useState<ProductoDto[]>(productosProp);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [discount, setDiscount] = useState(0);
  const [discountInput, setDiscountInput] = useState<string>('');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (open && productosProp.length === 0) {
      loadProductos();
    } else if (productosProp.length > 0) {
      setProductos(productosProp);
    }
  }, [open, productosProp]);

  const loadProductos = async () => {
    try {
      const productosData = await apiService.getProductos();
      if (productosData) {
        setProductos(productosData);
      }
    } catch (error) {
      console.error("Error al cargar productos:", error);
    }
  };

  const addToCart = (producto: ProductoDto) => {
    const product: Product = {
      id: producto.idProducto,
      name: producto.nombreProducto || '',
      price: Number(producto.precioProducto),
      stock: producto.stockProducto,
      category: producto.nombreCategoria || '',
      image: producto.imagenProducto || 'https://via.placeholder.com/200'
    };
    
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
    setCart(currentCart => {
      const updatedCart = currentCart.map(item => {
        if (item.id === productId) {
          const newQuantity = item.quantity + delta;
          if (newQuantity <= 0) {
            // Retornar null para indicar que debe eliminarse
            return null;
          }
          if (newQuantity > item.stock) {
            toast.error('Stock insuficiente');
            return item;
          }
          return { ...item, quantity: newQuantity };
        }
        return item;
      });
      
      // Filtrar los nulls (productos eliminados)
      return updatedCart.filter((item): item is CartItem => item !== null && item.quantity > 0);
    });
  };

  const removeFromCart = (productId: number) => {
    setCart(cart.filter(item => item.id !== productId));
  };

  const subtotal = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  const discountAmount = (subtotal * discount) / 100;
  const subtotalAfterDiscount = subtotal - discountAmount;
  const iva = subtotalAfterDiscount * 0.15; // IVA del 15%
  const total = subtotalAfterDiscount + iva;

  const handleConfirm = async () => {
    if (!selectedClienteId || !selectedMetodoPagoId || !selectedEstadoPedidoId) {
      toast.error('Por favor completa todos los campos requeridos');
      return;
    }

    if (cart.length === 0) {
      toast.error('El pedido debe tener al menos un producto');
      return;
    }

    // Esperar a que termine la carga de autenticación
    if (authLoading) {
      toast.info('Verificando autenticación...');
      return;
    }

    // Obtener el ID del usuario (del contexto o del localStorage como fallback)
    let userId: number | null = null;
    
    if (currentUser && currentUser.id) {
      userId = currentUser.id;
    } else {
      // Intentar obtener del localStorage como respaldo
      const storedUser = localStorage.getItem('auth_user');
      if (storedUser) {
        try {
          const usuario: any = JSON.parse(storedUser);
          if (usuario && usuario.idUsuario) {
            userId = usuario.idUsuario;
            console.log('Usuario obtenido del localStorage:', userId);
          }
        } catch (error) {
          console.error('Error al parsear usuario del storage:', error);
        }
      }
    }

    // Validar que tenemos un ID de usuario
    if (!userId) {
      toast.error('Debes estar autenticado para crear un pedido. Por favor, recarga la página o inicia sesión nuevamente.');
      console.error('No se pudo obtener el ID del usuario:', { currentUser, authLoading, token: apiService.getToken() });
      return;
    }

    try {
      setIsLoading(true);

      // Crear detalles del pedido
      const detalles = cart.map(item => ({
        idProducto: item.id,
        cantidadProducto: item.quantity,
        precioUnitarioProducto: item.price,
        subtotalProducto: item.price * item.quantity
      }));

      // Calcular subtotal y total
      const subtotal = detalles.reduce((sum, detalle) => sum + (detalle.subtotalProducto || 0), 0);
      const descuentoFinal = discount || 0; // Usar el descuento del estado
      const totalPedido = subtotal * (1 - descuentoFinal / 100);

      // Crear el pedido
      await apiService.createPedido({
        idCliente: parseInt(selectedClienteId),
        idUsuario: userId,
        idEstadoPedido: parseInt(selectedEstadoPedidoId),
        idMetodoPago: parseInt(selectedMetodoPagoId),
        subtotalPedido: subtotal,
        descuento: descuentoFinal,
        totalPedido: totalPedido,
        detalles: detalles
      });

      toast.success('Pedido creado exitosamente', {
        description: `Total: $${total.toLocaleString()}`,
      });
      
      if (onOrderCreated) {
        onOrderCreated();
      }
      
      onOpenChange(false);
      resetForm();
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al crear pedido";
      toast.error("Error", {
        description: errorMessage,
      });
    } finally {
      setIsLoading(false);
    }
  };

  const resetForm = () => {
    setStep(1);
    setSelectedClienteId('');
    setSelectedMetodoPagoId('');
    setSelectedEstadoPedidoId('');
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
              <Label>Cliente *</Label>
              <Select value={selectedClienteId} onValueChange={setSelectedClienteId}>
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar cliente" />
                </SelectTrigger>
                <SelectContent>
                  {clientes.length > 0 ? (
                    clientes.map((cliente) => (
                      <SelectItem key={cliente.idCliente} value={cliente.idCliente.toString()}>
                        {cliente.nombreCliente} - {cliente.emailCliente}
                      </SelectItem>
                    ))
                  ) : (
                    <SelectItem value="" disabled>No hay clientes disponibles</SelectItem>
                  )}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Estado del Pedido *</Label>
              <Select value={selectedEstadoPedidoId} onValueChange={setSelectedEstadoPedidoId}>
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar estado" />
                </SelectTrigger>
                <SelectContent>
                  {estadosPedido.length > 0 ? (
                    estadosPedido.map((estado) => (
                      <SelectItem key={estado.idEstadoPedido} value={estado.idEstadoPedido.toString()}>
                        {estado.nombreEstadoPedido}
                      </SelectItem>
                    ))
                  ) : (
                    <SelectItem value="" disabled>No hay estados disponibles</SelectItem>
                  )}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Método de Pago *</Label>
              <Select value={selectedMetodoPagoId} onValueChange={setSelectedMetodoPagoId}>
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar método" />
                </SelectTrigger>
                <SelectContent>
                  {metodosPago.length > 0 ? (
                    metodosPago.map((metodo) => (
                      <SelectItem key={metodo.idMetodoPago} value={metodo.idMetodoPago.toString()}>
                        {metodo.nombreMetodoPago}
                      </SelectItem>
                    ))
                  ) : (
                    <SelectItem value="" disabled>No hay métodos de pago disponibles</SelectItem>
                  )}
                </SelectContent>
              </Select>
            </div>
            <div className="flex justify-end">
              <Button
                onClick={() => setStep(2)}
                disabled={!selectedClienteId || !selectedMetodoPagoId || !selectedEstadoPedidoId}
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
              {productos.filter(p => p.estadoProducto === 'Activo' && p.stockProducto > 0).map((producto) => (
                <Card key={producto.idProducto} className="overflow-hidden">
                  <img
                    src={producto.imagenProducto || 'https://via.placeholder.com/200'}
                    alt={producto.nombreProducto || 'Producto'}
                    className="w-full h-32 object-cover"
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = 'https://via.placeholder.com/200';
                    }}
                  />
                  <div className="p-3 space-y-2">
                    <h4 className="text-sm font-medium">{producto.nombreProducto}</h4>
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium text-[#C9A664]">${Number(producto.precioProducto).toLocaleString()}</span>
                      <Badge variant="outline" className="text-xs">
                        Stock: {producto.stockProducto}
                      </Badge>
                    </div>
                    <Button
                      size="sm"
                      onClick={() => addToCart(producto)}
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
                        <Button 
                          size="sm" 
                          variant="ghost" 
                          onClick={() => removeFromCart(item.id)}
                          className="text-red-600 hover:text-red-700 hover:bg-red-50"
                        >
                          <Trash2 className="h-4 w-4" />
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
                  <span>{clientes.find(c => c.idCliente.toString() === selectedClienteId)?.nombreCliente || 'N/A'}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Estado:</span>
                  <span>{estadosPedido.find(e => e.idEstadoPedido.toString() === selectedEstadoPedidoId)?.nombreEstadoPedido || 'N/A'}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Método de pago:</span>
                  <span>{metodosPago.find(m => m.idMetodoPago.toString() === selectedMetodoPagoId)?.nombreMetodoPago || 'N/A'}</span>
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
                type="text"
                inputMode="numeric"
                value={discountInput}
                onChange={(e) => {
                  const value = e.target.value;
                  
                  // Permitir borrar todo el contenido (permite escribir desde cero)
                  if (value === '') {
                    setDiscountInput('');
                    setDiscount(0);
                    return;
                  }

                  // Solo permitir números enteros (0-100)
                  if (!/^\d*$/.test(value)) {
                    return;
                  }

                  const numValue = parseInt(value);
                  if (!isNaN(numValue) && numValue >= 0 && numValue <= 100) {
                    setDiscountInput(value);
                    setDiscount(numValue);
                  }
                }}
                onBlur={() => {
                  // Validar cuando el usuario sale del campo
                  if (discountInput === '' || discountInput.trim() === '') {
                    setDiscountInput('0');
                    setDiscount(0);
                  } else {
                    const numValue = parseInt(discountInput);
                    if (isNaN(numValue) || numValue < 0) {
                      setDiscountInput('0');
                      setDiscount(0);
                    } else if (numValue > 100) {
                      setDiscountInput('100');
                      setDiscount(100);
                    } else {
                      setDiscountInput(numValue.toString());
                      setDiscount(numValue);
                    }
                  }
                }}
                placeholder="0"
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
                <Button onClick={handleConfirm} disabled={isLoading} className="bg-[#C9A664] hover:bg-[#B8965A]">
                  {isLoading ? 'Creando...' : 'Confirmar Pedido'}
                </Button>
              </div>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}