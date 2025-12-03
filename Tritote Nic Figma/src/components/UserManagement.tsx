import { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "./ui/table";
import { Badge } from "./ui/badge";
import { Switch } from "./ui/switch";
import { UserPlus, Search, Edit, Trash2, Shield, Eye } from "lucide-react";
import { toast } from "sonner";
import { NewUserDialog } from "./NewUserDialog";
import { EditUserDialog } from "./EditUserDialog";
import { UserDetailsDialog } from "./UserDetailsDialog";
import { ConfirmDialog } from "./ConfirmDialog";
import { usePermissions, useAuth } from "../contexts/AuthContext";

type UserRole = 'admin' | 'seller';

interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  active: boolean;
  lastLogin: string;
}

const mockUsers: User[] = [
  {
    id: 1,
    name: 'William Garcia',
    email: 'william.garcia@tritote.com.ni',
    role: 'admin',
    active: true,
    lastLogin: '2024-01-15'
  },
  {
    id: 2,
    name: 'Andres Gonzalez',
    email: 'andres.gonzalez@tritote.com.ni',
    role: 'seller',
    active: true,
    lastLogin: '2024-01-14'
  },
  {
    id: 3,
    name: 'Maria Rodriguez',
    email: 'maria.rodriguez@tritote.com.ni',
    role: 'seller',
    active: true,
    lastLogin: '2024-01-13'
  }
];

const roleLabels = {
  admin: 'Administrador',
  seller: 'Vendedor'
};

const roleColors = {
  admin: 'bg-purple-100 text-purple-800 border-purple-200',
  seller: 'bg-blue-100 text-blue-800 border-blue-200'
};

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

export function UserManagement() {
  const [users, setUsers] = useState(mockUsers);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedRole, setSelectedRole] = useState<UserRole>('admin');
  const [newUserOpen, setNewUserOpen] = useState(false);
  const [editUserOpen, setEditUserOpen] = useState(false);
  const [deleteUserOpen, setDeleteUserOpen] = useState(false);
  const [viewUserOpen, setViewUserOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const { hasPermission } = usePermissions();
  const { user } = useAuth();

  const filteredUsers = users.filter(user =>
    user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    user.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const toggleUserStatus = (userId: number) => {
    if (!hasPermission('usuarios.edit')) {
      toast.error('No tienes permisos para modificar usuarios');
      return;
    }
    setUsers(users.map(user =>
      user.id === userId ? { ...user, active: !user.active } : user
    ));
    toast.success('Estado de usuario actualizado');
  };

  const handleDelete = (userId: number) => {
    if (!hasPermission('usuarios.delete')) {
      toast.error('No tienes permisos para eliminar usuarios');
      return;
    }
    setUsers(users.filter(u => u.id !== userId));
    toast.success('Usuario eliminado');
  };

  const handleUserCreated = (newUser: User) => {
    // FASE 3: PERSISTENCIA - Agregar usuario al estado
    setUsers([...users, newUser]);
  };

  const handleUserEdited = (editedUser: User) => {
    // FASE 3: PERSISTENCIA - Actualizar usuario en el estado
    setUsers(users.map(u =>
      u.id === editedUser.id ? editedUser : u
    ));
  };

  const handleDeleteUser = (userId: number) => {
    // Verificar si es el usuario actual
    if (user && user.email === users.find(u => u.id === userId)?.email) {
      toast.error('No puedes eliminar tu propio usuario');
      return;
    }
    setDeleteUserOpen(true);
    setSelectedUser(users.find(user => user.id === userId) || null);
  };

  const handleConfirmDelete = () => {
    if (selectedUser) {
      handleDelete(selectedUser.id);
    }
    setDeleteUserOpen(false);
    setSelectedUser(null);
  };

  const handleEditUser = (userId: number) => {
    setEditUserOpen(true);
    setSelectedUser(users.find(user => user.id === userId) || null);
  };

  const handleViewUser = (userId: number) => {
    setViewUserOpen(true);
    setSelectedUser(users.find(user => user.id === userId) || null);
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1>Gestión de Usuarios</h1>
          <p className="text-muted-foreground">Administra usuarios y permisos del sistema</p>
        </div>
        <Button 
          onClick={() => setNewUserOpen(true)}
          className="bg-[#C9A664] hover:bg-[#B8965A]"
          disabled={!hasPermission('usuarios.create')}
        >
          <UserPlus className="h-4 w-4 mr-2" />
          Nuevo Usuario
        </Button>
      </div>

      {/* Roles y permisos */}
      <div className="grid gap-4 md:grid-cols-3">
        {Object.entries(roleConfig).map(([role, config]) => (
          <Card key={role} className="hover:shadow-md transition-shadow cursor-pointer" onClick={() => setSelectedRole(role as UserRole)}>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-base">
                <Shield className="h-4 w-4" />
                {config.label}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="space-y-1 text-sm text-muted-foreground">
                {config.permissions.map((permission, index) => (
                  <li key={index} className="flex items-center gap-2">
                    <div className="h-1.5 w-1.5 rounded-full bg-muted-foreground" />
                    {permission}
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Búsqueda */}
      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Buscar usuarios..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Tabla de usuarios */}
      <Card>
        <CardHeader>
          <CardTitle>Usuarios del Sistema</CardTitle>
          <CardDescription>
            Mostrando {filteredUsers.length} de {users.length} usuarios
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nombre</TableHead>
                <TableHead>Email</TableHead>
                <TableHead>Rol</TableHead>
                <TableHead>Último Acceso</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredUsers.map((user) => (
                <TableRow key={user.id}>
                  <TableCell className="font-medium">{user.name}</TableCell>
                  <TableCell className="text-muted-foreground">{user.email}</TableCell>
                  <TableCell>
                    <Badge variant="outline" className={roleConfig[user.role].className}>
                      {roleConfig[user.role].label}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-muted-foreground">{user.lastLogin}</TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <Switch
                        checked={user.active}
                        onCheckedChange={() => toggleUserStatus(user.id)}
                      />
                      <span className="text-sm">
                        {user.active ? 'Activo' : 'Inactivo'}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button variant="ghost" size="sm" onClick={() => handleViewUser(user.id)}>
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="sm" onClick={() => handleEditUser(user.id)}>
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:bg-destructive hover:text-destructive-foreground"
                        onClick={() => handleDeleteUser(user.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <NewUserDialog
        open={newUserOpen}
        onOpenChange={setNewUserOpen}
        onUserCreated={handleUserCreated}
      />

      <EditUserDialog
        open={editUserOpen}
        onOpenChange={setEditUserOpen}
        user={selectedUser}
        onUserEdited={handleUserEdited}
      />

      <UserDetailsDialog
        open={viewUserOpen}
        onOpenChange={setViewUserOpen}
        user={selectedUser}
      />

      <ConfirmDialog
        open={deleteUserOpen}
        onOpenChange={setDeleteUserOpen}
        title="Eliminar Usuario"
        description="¿Estás seguro de que quieres eliminar este usuario? Esta acción no se puede deshacer."
        onConfirm={handleConfirmDelete}
      />
    </div>
  );
}