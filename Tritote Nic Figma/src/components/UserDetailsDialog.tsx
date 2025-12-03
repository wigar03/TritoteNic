import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Badge } from "./ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Mail, Calendar, Shield, Activity } from "lucide-react";

type UserRole = 'admin' | 'seller';

interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  active: boolean;
  lastLogin: string;
}

interface UserDetailsDialogProps {
  user: User | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const roleConfig = {
  admin: { 
    label: 'Administrador',
    className: 'bg-purple-100 text-purple-800 border-purple-200',
    permissions: ['Acceso total', 'Gestionar usuarios', 'Gestionar productos', 'Reportes', 'Pedidos'] 
  },
  seller: { 
    label: 'Vendedor',
    className: 'bg-blue-100 text-blue-800 border-blue-200',
    permissions: ['Crear pedidos', 'Ver productos', 'Crear/editar clientes', 'Ver reportes', 'Ver dashboard'] 
  }
};

export function UserDetailsDialog({ user, open, onOpenChange }: UserDetailsDialogProps) {
  if (!user) return null;

  const roleInfo = roleConfig[user.role];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[800px] max-h-[90vh] overflow-y-auto">{/* Aumentado tamaño */}
        <DialogHeader>
          <div className="flex items-center justify-between">
            <div>
              <DialogTitle className="text-2xl">{user.name}</DialogTitle>
              <DialogDescription>Información detallada del usuario</DialogDescription>
            </div>
            <div className="flex gap-2">
              <Badge variant="outline" className={roleInfo.className}>
                {roleInfo.label}
              </Badge>
              <Badge variant="outline" className={user.active ? 'bg-green-100 text-green-800 border-green-200' : 'bg-red-100 text-red-800 border-red-200'}>
                {user.active ? 'Activo' : 'Inactivo'}
              </Badge>
            </div>
          </div>
        </DialogHeader>

        <div className="space-y-6">
          {/* Información del Usuario */}
          <Card>
            <CardHeader>
              <CardTitle>Información de Contacto</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-1">
                  <div className="flex items-center gap-2 text-muted-foreground">
                    <Mail className="h-4 w-4" />
                    <span className="text-sm">Email</span>
                  </div>
                  <p className="font-medium">{user.email}</p>
                </div>
                <div className="space-y-1">
                  <div className="flex items-center gap-2 text-muted-foreground">
                    <Calendar className="h-4 w-4" />
                    <span className="text-sm">Último Acceso</span>
                  </div>
                  <p className="font-medium">{user.lastLogin}</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Permisos del Rol */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Shield className="h-5 w-5" />
                Permisos del Rol
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {roleInfo.permissions.map((permission, index) => (
                  <div key={index} className="flex items-center gap-2 text-sm">
                    <div className="h-2 w-2 rounded-full bg-[#C9A664]" />
                    <span>{permission}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Estadísticas de Actividad */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Activity className="h-5 w-5" />
                Actividad Reciente
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-3 gap-4">
                <div className="text-center p-4 bg-muted rounded-lg">
                  <p className="text-2xl font-medium text-[#C9A664]">12</p>
                  <p className="text-sm text-muted-foreground">Pedidos Creados</p>
                </div>
                <div className="text-center p-4 bg-muted rounded-lg">
                  <p className="text-2xl font-medium text-[#C9A664]">8</p>
                  <p className="text-sm text-muted-foreground">Clientes Gestionados</p>
                </div>
                <div className="text-center p-4 bg-muted rounded-lg">
                  <p className="text-2xl font-medium text-[#C9A664]">24</p>
                  <p className="text-sm text-muted-foreground">Accesos este Mes</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </DialogContent>
    </Dialog>
  );
}