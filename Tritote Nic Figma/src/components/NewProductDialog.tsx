import { useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { toast } from "sonner";

interface NewProductDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onProductCreated: (product: any) => void;
}

const categories = [
  "Canvas",
  "Ecológico",
  "Playa",
  "Minimalista",
  "Market",
  "Premium"
];

export function NewProductDialog({ open, onOpenChange, onProductCreated }: NewProductDialogProps) {
  const [nombre, setNombre] = useState('');
  const [categoria, setCategoria] = useState('');
  const [precio, setPrecio] = useState('');
  const [stock, setStock] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    // Validación 1: Nombre es obligatorio
    if (!nombre.trim()) {
      newErrors.nombre = 'El nombre del producto es obligatorio';
    }

    // Validación 2: Categoría es obligatoria
    if (!categoria) {
      newErrors.categoria = 'La categoría es obligatoria';
    }

    // Validación 3: Precio debe ser mayor a 0
    const precioNum = parseFloat(precio);
    if (!precio || isNaN(precioNum) || precioNum <= 0) {
      newErrors.precio = 'El precio debe ser mayor a 0';
    }

    // Validación 4: Stock debe ser mayor o igual a 0
    const stockNum = parseInt(stock);
    if (!stock || isNaN(stockNum) || stockNum < 0) {
      newErrors.stock = 'El stock debe ser 0 o mayor';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = () => {
    // FASE 1: VALIDACIONES
    if (!validateForm()) {
      toast.error('Por favor corrija los errores del formulario');
      return;
    }

    // FASE 2: CREACIÓN DE ENTIDAD
    const nuevoProducto = {
      id: Date.now(), // Simular ID autogenerado
      name: nombre.trim(),
      category: categoria,
      price: parseFloat(precio),
      stock: parseInt(stock),
      image: `https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=400&h=400&fit=crop` // Imagen por defecto
    };

    // FASE 3: PERSISTENCIA (simulada)
    onProductCreated(nuevoProducto);

    // Notificación de éxito
    toast.success('Producto creado exitosamente', {
      description: `${nuevoProducto.name} - $${nuevoProducto.price.toLocaleString()}`
    });

    // Resetear formulario
    resetForm();
    onOpenChange(false);
  };

  const resetForm = () => {
    setNombre('');
    setCategoria('');
    setPrecio('');
    setStock('');
    setErrors({});
  };

  return (
    <Dialog open={open} onOpenChange={(open) => {
      if (!open) resetForm();
      onOpenChange(open);
    }}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Nuevo Producto</DialogTitle>
          <DialogDescription>
            Agrega un nuevo producto al catálogo
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {/* Nombre del Producto */}
          <div className="space-y-2">
            <Label htmlFor="nombre">
              Nombre del Producto <span className="text-red-500">*</span>
            </Label>
            <Input
              id="nombre"
              placeholder="Ej: Bolso Clásico Negro"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className={errors.nombre ? 'border-red-500' : ''}
            />
            {errors.nombre && (
              <p className="text-sm text-red-500">{errors.nombre}</p>
            )}
          </div>

          {/* Categoría */}
          <div className="space-y-2">
            <Label htmlFor="categoria">
              Categoría <span className="text-red-500">*</span>
            </Label>
            <Select value={categoria} onValueChange={setCategoria}>
              <SelectTrigger className={errors.categoria ? 'border-red-500' : ''}>
                <SelectValue placeholder="Seleccionar categoría" />
              </SelectTrigger>
              <SelectContent>
                {categories.map((cat) => (
                  <SelectItem key={cat} value={cat}>
                    {cat}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.categoria && (
              <p className="text-sm text-red-500">{errors.categoria}</p>
            )}
          </div>

          {/* Precio */}
          <div className="space-y-2">
            <Label htmlFor="precio">
              Precio (C$) <span className="text-red-500">*</span>
            </Label>
            <Input
              id="precio"
              type="number"
              step="0.01"
              min="0"
              placeholder="0.00"
              value={precio}
              onChange={(e) => setPrecio(e.target.value)}
              className={errors.precio ? 'border-red-500' : ''}
            />
            {errors.precio && (
              <p className="text-sm text-red-500">{errors.precio}</p>
            )}
          </div>

          {/* Stock Inicial */}
          <div className="space-y-2">
            <Label htmlFor="stock">
              Stock Inicial <span className="text-red-500">*</span>
            </Label>
            <Input
              id="stock"
              type="number"
              min="0"
              placeholder="0"
              value={stock}
              onChange={(e) => setStock(e.target.value)}
              className={errors.stock ? 'border-red-500' : ''}
            />
            {errors.stock && (
              <p className="text-sm text-red-500">{errors.stock}</p>
            )}
          </div>

          {/* Botones */}
          <div className="flex justify-end gap-2 pt-4">
            <Button
              variant="outline"
              onClick={() => {
                resetForm();
                onOpenChange(false);
              }}
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSubmit}
              className="bg-[#C9A664] hover:bg-[#B8965A]"
            >
              Crear Producto
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}