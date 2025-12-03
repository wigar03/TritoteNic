import { useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { toast } from "sonner";
import { Shield } from "lucide-react";
import { ValidationHint } from "./ValidationHint";

type UserRole = 'admin' | 'seller';

interface NewUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onUserCreated: (user: any) => void;
}

const roleConfig = {
  admin: { 
    label: 'Administrador', 
    permissions: ['Acceso total', 'Gestionar usuarios', 'Gestionar productos', 'Reportes', 'Pedidos'] 
  },
  seller: { 
    label: 'Vendedor', 
    permissions: ['Crear pedidos', 'Ver productos', 'Crear/editar clientes', 'Ver reportes', 'Ver dashboard'] 
  }
};

export function NewUserDialog({ open, onOpenChange, onUserCreated }: NewUserDialogProps) {
  const [nombre, setNombre] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [role, setRole] = useState<UserRole | ''>('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [showPasswordHint, setShowPasswordHint] = useState(false);

  // Reglas de validación para la contraseña
  const passwordRules = [
    { label: 'Mínimo 6 caracteres', valid: password.length >= 6 },
    { label: 'Contiene letras y números', valid: /[a-zA-Z]/.test(password) && /[0-9]/.test(password) },
  ];

  // Reglas de validación para el email
  const emailRules = [
    { label: 'Formato válido (usuario@dominio.com)', valid: /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) },
    { label: 'Dominio @tritote.com.ni', valid: email.endsWith('@tritote.com.ni') },
  ];

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
    } else if (!email.endsWith('@tritote.com.ni')) {
      newErrors.email = 'El email debe ser del dominio @tritote.com.ni';
    }

    // Validación 3: Contraseña obligatoria y segura
    if (!password) {
      newErrors.password = 'La contraseña es obligatoria';
    } else if (password.length < 6) {
      newErrors.password = 'La contraseña debe tener al menos 6 caracteres';
    }

    // Validación 4: Confirmación de contraseña
    if (!confirmPassword) {
      newErrors.confirmPassword = 'Debe confirmar la contraseña';
    } else if (password !== confirmPassword) {
      newErrors.confirmPassword = 'Las contraseñas no coinciden';
    }

    // Validación 5: Rol es obligatorio
    if (!role) {
      newErrors.role = 'Debe seleccionar un rol';
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
    const nuevoUsuario = {
      id: Date.now(), // Simular ID autogenerado
      name: nombre.trim(),
      email: email.trim().toLowerCase(),
      role: role as UserRole,
      active: true,  // Nuevo usuario inicia activo
      lastLogin: '-' // Sin acceso aún
    };

    // FASE 3: PERSISTENCIA (simulada)
    onUserCreated(nuevoUsuario);

    // Notificación de éxito
    toast.success('Usuario creado exitosamente', {
      description: `${nuevoUsuario.name} - ${roleConfig[nuevoUsuario.role].label}`
    });

    // Resetear formulario
    resetForm();
    onOpenChange(false);
  };

  const resetForm = () => {
    setNombre('');
    setEmail('');
    setPassword('');
    setConfirmPassword('');
    setRole('');
    setErrors({});
    setShowPasswordHint(false);
  };

  return (
    <Dialog open={open} onOpenChange={(open) => {
      if (!open) resetForm();
      onOpenChange(open);
    }}>
      <DialogContent className="max-w-md max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Nuevo Usuario</DialogTitle>
          <DialogDescription>
            Crea un nuevo usuario del sistema
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
              placeholder="Ej: Carlos Vendedor"
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
              Email Corporativo <span className="text-red-500">*</span>
            </Label>
            <Input
              id="email"
              type="email"
              placeholder="usuario@tritote.com.ni"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              onFocus={() => {}}
              className={errors.email ? 'border-red-500' : ''}
            />
            {email && <ValidationHint rules={emailRules} show={true} />}
            {errors.email && (
              <p className="text-sm text-red-500">{errors.email}</p>
            )}
          </div>

          {/* Contraseña */}
          <div className="space-y-2">
            <Label htmlFor="password">
              Contraseña <span className="text-red-500">*</span>
            </Label>
            <Input
              id="password"
              type="password"
              placeholder="Mínimo 6 caracteres"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              onFocus={() => setShowPasswordHint(true)}
              onBlur={() => setShowPasswordHint(false)}
              className={errors.password ? 'border-red-500' : ''}
            />
            <ValidationHint rules={passwordRules} show={showPasswordHint || password.length > 0} />
            {errors.password && (
              <p className="text-sm text-red-500">{errors.password}</p>
            )}
          </div>

          {/* Confirmar Contraseña */}
          <div className="space-y-2">
            <Label htmlFor="confirmPassword">
              Confirmar Contraseña <span className="text-red-500">*</span>
            </Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder="Repita la contraseña"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              className={errors.confirmPassword ? 'border-red-500' : ''}
            />
            {errors.confirmPassword && (
              <p className="text-sm text-red-500">{errors.confirmPassword}</p>
            )}
          </div>

          {/* Rol */}
          <div className="space-y-2">
            <Label htmlFor="role">
              Rol <span className="text-red-500">*</span>
            </Label>
            <Select value={role} onValueChange={(value) => setRole(value as UserRole)}>
              <SelectTrigger className={errors.role ? 'border-red-500' : ''}>
                <SelectValue placeholder="Seleccionar rol" />
              </SelectTrigger>
              <SelectContent>
                {Object.entries(roleConfig).map(([key, config]) => (
                  <SelectItem key={key} value={key}>
                    <div className="flex items-center gap-2">
                      <Shield className="h-4 w-4" />
                      {config.label}
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.role && (
              <p className="text-sm text-red-500">{errors.role}</p>
            )}
          </div>

          {/* Permisos del rol seleccionado */}
          {role && (
            <div className="bg-muted rounded-lg p-3">
              <p className="text-sm font-medium mb-2">Permisos de {roleConfig[role as UserRole].label}:</p>
              <ul className="text-xs text-muted-foreground space-y-1 ml-4">
                {roleConfig[role as UserRole].permissions.map((permission, index) => (
                  <li key={index} className="flex items-center gap-2">
                    <div className="h-1.5 w-1.5 rounded-full bg-[#C9A664]" />
                    {permission}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Nota de seguridad */}
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3">
            <p className="text-xs text-yellow-800">
              <strong>Nota de Seguridad:</strong> El usuario recibirá un email con sus credenciales de acceso. Se recomienda cambiar la contraseña en el primer inicio de sesión.
            </p>
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
              Crear Usuario
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}