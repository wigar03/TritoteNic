import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { toast } from "sonner";

interface Product {
  id: number;
  name: string;
  category: string;
  price: number;
  stock: number;
  image: string;
}

import type { CategoriaDto } from "../types/api";

interface Product {
  id: number;
  name: string;
  category: string;
  price: number;
  stock: number;
  image: string;
  idCategoria: number;
  estadoProducto?: string;
}

interface EditProductDialogProps {
  product: Product | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onProductEdited: (product: Product) => void;
  categorias?: CategoriaDto[];
}

export function EditProductDialog({ product, open, onOpenChange, onProductEdited, categorias = [] }: EditProductDialogProps) {
  const [formData, setFormData] = useState<Product>({
    id: 0,
    name: '',
    category: 'Canvas',
    price: 0,
    stock: 0,
    image: '',
    idCategoria: 0,
    estadoProducto: 'Activo'
  });

  const [priceInput, setPriceInput] = useState<string>('');
  const [stockInput, setStockInput] = useState<string>('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (product) {
      setFormData({
        ...product,
        idCategoria: product.idCategoria || 0,
        estadoProducto: product.estadoProducto || 'Activo'
      });
      setPriceInput(product.price > 0 ? product.price.toString() : '');
      setStockInput(product.stock > 0 ? product.stock.toString() : '');
      setErrors({});
    }
  }, [product]);

  const handlePriceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setPriceInput(value);
    
    // Permitir borrar completamente
    if (value === '') {
      return;
    }
    
    // Validar que sea un número válido con máximo 2 decimales
    if (/^\d*\.?\d{0,2}$/.test(value)) {
      const numValue = parseFloat(value);
      if (!isNaN(numValue) && numValue >= 0) {
        setFormData({ ...formData, price: numValue });
        setErrors({ ...errors, price: '' });
      }
    }
  };

  const handlePriceBlur = () => {
    if (priceInput === '' || priceInput === '0') {
      setErrors({ ...errors, price: 'El precio es requerido y debe ser mayor a 0' });
    } else {
      const numValue = parseFloat(priceInput);
      if (isNaN(numValue) || numValue <= 0) {
        setErrors({ ...errors, price: 'El precio debe ser mayor a 0' });
      } else {
        setErrors({ ...errors, price: '' });
      }
    }
  };

  const handleStockChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setStockInput(value);
    
    // Permitir borrar completamente
    if (value === '') {
      return;
    }
    
    // Validar que sea un número entero válido
    if (/^\d*$/.test(value)) {
      const numValue = parseInt(value);
      if (!isNaN(numValue) && numValue >= 0) {
        setFormData({ ...formData, stock: numValue });
        setErrors({ ...errors, stock: '' });
      }
    }
  };

  const handleStockBlur = () => {
    if (stockInput === '') {
      setErrors({ ...errors, stock: 'El stock es requerido' });
    } else {
      const numValue = parseInt(stockInput);
      if (isNaN(numValue) || numValue < 0) {
        setErrors({ ...errors, stock: 'El stock debe ser 0 o mayor' });
      } else {
        setErrors({ ...errors, stock: '' });
      }
    }
  };

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'El nombre es requerido';
    }

    if (priceInput === '' || priceInput === '0' || parseFloat(priceInput) <= 0) {
      newErrors.price = 'El precio es requerido y debe ser mayor a 0';
    }

    if (stockInput === '' || parseInt(stockInput) < 0) {
      newErrors.stock = 'El stock es requerido y debe ser 0 o mayor';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      toast.error('Por favor, corrija los errores en el formulario');
      return;
    }

    onProductEdited(formData);
    toast.success('Producto actualizado exitosamente');
    onOpenChange(false);
  };

  if (!product) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Editar Producto</DialogTitle>
          <DialogDescription>
            Modifica la información del producto
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="edit-product-name">Nombre del Producto *</Label>
            <Input
              id="edit-product-name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className={errors.name ? 'border-red-500' : ''}
              placeholder="Ej: Tote Bag Clásico Beige"
            />
            {errors.name && (
              <p className="text-sm text-red-600">{errors.name}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="edit-product-category">Categoría *</Label>
              <Select
                value={formData.idCategoria.toString()}
                onValueChange={(value) => {
                  const categoria = categorias.find(c => c.idCategoria.toString() === value);
                  setFormData({ 
                    ...formData, 
                    idCategoria: parseInt(value),
                    category: categoria?.nombreCategoria || ''
                  });
                }}
              >
                <SelectTrigger id="edit-product-category">
                  <SelectValue placeholder="Selecciona una categoría" />
                </SelectTrigger>
                <SelectContent>
                  {categorias.length > 0 ? (
                    categorias.map((cat) => (
                      <SelectItem key={cat.idCategoria} value={cat.idCategoria.toString()}>
                        {cat.nombreCategoria}
                      </SelectItem>
                    ))
                  ) : (
                    <SelectItem value="" disabled>No hay categorías disponibles</SelectItem>
                  )}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="edit-product-status">Estado *</Label>
              <Select
                value={formData.estadoProducto || 'Activo'}
                onValueChange={(value) => setFormData({ ...formData, estadoProducto: value })}
              >
                <SelectTrigger id="edit-product-status">
                  <SelectValue placeholder="Selecciona un estado" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Activo">Activo</SelectItem>
                  <SelectItem value="Inactivo">Inactivo</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="edit-product-price">Precio (C$) *</Label>
              <Input
                id="edit-product-price"
                type="text"
                value={priceInput}
                onChange={handlePriceChange}
                onBlur={handlePriceBlur}
                className={errors.price ? 'border-red-500' : ''}
                placeholder="450.00"
              />
              {errors.price && (
                <p className="text-sm text-red-600">{errors.price}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="edit-product-stock">Stock *</Label>
              <Input
                id="edit-product-stock"
                type="text"
                value={stockInput}
                onChange={handleStockChange}
                onBlur={handleStockBlur}
                className={errors.stock ? 'border-red-500' : ''}
                placeholder="25"
              />
              {errors.stock && (
                <p className="text-sm text-red-600">{errors.stock}</p>
              )}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="edit-product-image">URL de Imagen</Label>
            <Input
              id="edit-product-image"
              value={formData.image}
              onChange={(e) => setFormData({ ...formData, image: e.target.value })}
              placeholder="https://ejemplo.com/imagen.jpg"
            />
            <p className="text-xs text-muted-foreground">
              URL de la imagen del producto (opcional)
            </p>
          </div>

          {formData.image && (
            <div className="space-y-2">
              <Label>Vista Previa</Label>
              <div className="aspect-square w-32 overflow-hidden rounded-lg border">
                <img
                  src={formData.image}
                  alt="Preview"
                  className="w-full h-full object-cover"
                  onError={(e) => {
                    e.currentTarget.src = 'https://images.unsplash.com/photo-1574365569389-a10d488ca3fb?w=400&h=400&fit=crop';
                  }}
                />
              </div>
            </div>
          )}

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