import { useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { toast } from "sonner";

interface NewCustomerDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCustomerCreated: (customer: any) => void;
}

export function NewCustomerDialog({ open, onOpenChange, onCustomerCreated }: NewCustomerDialogProps) {
  const [nombre, setNombre] = useState('');
  const [email, setEmail] = useState('');
  const [telefono, setTelefono] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    // Validación 1: Nombre es obligatorio
    if (!nombre.trim()) {
      newErrors.nombre = 'El nombre es obligatorio';
    }

    // Validación 2: Email es obligatorio y debe tener formato válido
    if (!email.trim()) {
      newErrors.email = 'El email es obligatorio';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = 'El formato del email no es válido';
    }

    // Validación 3: Teléfono es obligatorio
    if (!telefono.trim()) {
      newErrors.telefono = 'El teléfono es obligatorio';
    } else if (telefono.trim().length < 8) {
      newErrors.telefono = 'El teléfono debe tener al menos 8 caracteres';
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
    // Siguiendo la lógica de negocio: nuevo cliente inicia con valores por defecto
    const nuevoCliente = {
      id: Date.now(), // Simular ID autogenerado
      name: nombre.trim(),
      email: email.trim().toLowerCase(),
      phone: telefono.trim(),
      totalOrders: 0,           // Inicializar en 0 (sin pedidos)
      totalSpent: 0,            // Inicializar en 0 (no ha gastado)
      lastOrder: null,          // Sin pedidos aún
      categoriaCliente: null    // Sin categoría (se asigna automáticamente cuando gaste > $0)
    };

    // FASE 3: PERSISTENCIA (simulada)
    onCustomerCreated(nuevoCliente);

    // Notificación de éxito
    toast.success('Cliente creado exitosamente', {
      description: `${nuevoCliente.name} - ${nuevoCliente.email}`
    });

    // Resetear formulario
    resetForm();
    onOpenChange(false);
  };

  const resetForm = () => {
    setNombre('');
    setEmail('');
    setTelefono('');
    setErrors({});
  };

  return (
    <Dialog open={open} onOpenChange={(open) => {
      if (!open) resetForm();
      onOpenChange(open);
    }}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Nuevo Cliente</DialogTitle>
          <DialogDescription>
            Registra un nuevo cliente en el sistema
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {/* Nombre Completo */}
          <div className="space-y-2">
            <Label htmlFor="nombre">
              Nombre Completo <span className="text-red-500">*</span>
            </Label>
            <Input
              id="nombre"
              placeholder="Ej: María González"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className={errors.nombre ? 'border-red-500' : ''}
            />
            {errors.nombre && (
              <p className="text-sm text-red-500">{errors.nombre}</p>
            )}
          </div>

          {/* Email */}
          <div className="space-y-2">
            <Label htmlFor="email">
              Email <span className="text-red-500">*</span>
            </Label>
            <Input
              id="email"
              type="email"
              placeholder="cliente@email.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={errors.email ? 'border-red-500' : ''}
            />
            {errors.email && (
              <p className="text-sm text-red-500">{errors.email}</p>
            )}
          </div>

          {/* Teléfono */}
          <div className="space-y-2">
            <Label htmlFor="telefono">
              Teléfono <span className="text-red-500">*</span>
            </Label>
            <Input
              id="telefono"
              type="tel"
              placeholder="+505 8888-8888"
              value={telefono}
              onChange={(e) => setTelefono(e.target.value)}
              className={errors.telefono ? 'border-red-500' : ''}
            />
            {errors.telefono && (
              <p className="text-sm text-red-500">{errors.telefono}</p>
            )}
          </div>

          {/* Nota informativa */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
            <p className="text-sm text-blue-800">
              <strong>Nota:</strong> El cliente iniciará sin categoría. La categoría se asignará automáticamente según el total gastado:
            </p>
            <ul className="text-xs text-blue-700 mt-2 space-y-1 ml-4">
              <li>• VIP: ≥ $100,000</li>
              <li>• Frecuente: ≥ $50,000</li>
              <li>• Regular: {'>'} $0</li>
            </ul>
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
              Crear Cliente
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}