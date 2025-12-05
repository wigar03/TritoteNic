import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { toast } from "sonner";
import type { RolDto } from "../types/api";

type UserRole = 'admin' | 'seller';

interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  active: boolean;
  lastLogin: string;
  idRol: number;
}

interface EditUserDialogProps {
  user: User | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onUserEdited: (user: User) => void;
  roles?: RolDto[];
}

export function EditUserDialog({ user, open, onOpenChange, onUserEdited, roles = [] }: EditUserDialogProps) {
  const [formData, setFormData] = useState<User>({
    id: 0,
    name: '',
    email: '',
    role: 'seller',
    active: true,
    lastLogin: '',
    idRol: 0
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (user) {
      setFormData(user);
      setErrors({});
    }
  }, [user]);

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'El nombre es requerido';
    }

    if (!formData.email.trim()) {
      newErrors.email = 'El email es requerido';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Email inválido';
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

    onUserEdited(formData);
    toast.success('Usuario actualizado exitosamente');
    onOpenChange(false);
  };

  if (!user) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Editar Usuario</DialogTitle>
          <DialogDescription>
            Modifica la información del usuario
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="edit-name">Nombre Completo *</Label>
            <Input
              id="edit-name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className={errors.name ? 'border-red-500' : ''}
            />
            {errors.name && (
              <p className="text-sm text-red-600">{errors.name}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="edit-email">Email *</Label>
            <Input
              id="edit-email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              className={errors.email ? 'border-red-500' : ''}
            />
            {errors.email && (
              <p className="text-sm text-red-600">{errors.email}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="edit-role">Rol *</Label>
            <Select
              value={formData.idRol.toString()}
              onValueChange={(value) => {
                const rolSeleccionado = roles.find(r => r.idRol.toString() === value);
                if (rolSeleccionado) {
                  const roleKey = rolSeleccionado.nombreRol?.toLowerCase().includes('admin') ? 'admin' : 'seller';
                  setFormData({ ...formData, idRol: rolSeleccionado.idRol, role: roleKey });
                }
              }}
            >
              <SelectTrigger id="edit-role">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {roles.length > 0 ? (
                  roles.map((rol) => (
                    <SelectItem key={rol.idRol} value={rol.idRol.toString()}>
                      {rol.nombreRol}
                    </SelectItem>
                  ))
                ) : (
                  <SelectItem value="" disabled>No hay roles disponibles</SelectItem>
                )}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="edit-status">Estado *</Label>
            <Select
              value={formData.active ? 'active' : 'inactive'}
              onValueChange={(value: 'active' | 'inactive') => setFormData({ ...formData, active: value === 'active' })}
            >
              <SelectTrigger id="edit-status">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="active">Activo</SelectItem>
                <SelectItem value="inactive">Inactivo</SelectItem>
              </SelectContent>
            </Select>
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