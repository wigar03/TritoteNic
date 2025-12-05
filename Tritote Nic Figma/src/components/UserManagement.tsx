import { useState, useEffect } from "react";
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
import { apiService } from "../services/apiService";
import type { UsuarioDto, RolDto } from "../types/api";

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

const roleLabels = {
  admin: 'Administrador',
  seller: 'Vendedor'
};

const roleColors = {
  admin: 'bg-purple-100 text-purple-800 border-purple-200',
  seller: 'bg-blue-100 text-blue-800 border-blue-200'
};

// Función para mapear nombre de rol a UserRole
const mapRolToUserRole = (nombreRol: string | null | undefined): UserRole => {
  if (!nombreRol) return 'seller';
  const rol = nombreRol.toLowerCase();
  if (rol.includes('admin') || rol.includes('administrador')) return 'admin';
  return 'seller';
};

export function UserManagement() {
  const [users, setUsers] = useState<User[]>([]);
  const [roles, setRoles] = useState<RolDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedRole, setSelectedRole] = useState<UserRole>('admin');
  const [newUserOpen, setNewUserOpen] = useState(false);
  const [editUserOpen, setEditUserOpen] = useState(false);
  const [deleteUserOpen, setDeleteUserOpen] = useState(false);
  const [viewUserOpen, setViewUserOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const { hasPermission } = usePermissions();
  const { currentUser } = useAuth();

  useEffect(() => {
    loadUsers();
    loadRoles();
  }, []);

  const loadUsers = async () => {
    try {
      setIsLoading(true);
      const usuarios = await apiService.getUsuarios();
      if (usuarios) {
        const mappedUsers: User[] = usuarios.map(u => ({
          id: u.idUsuario,
          name: u.nombreUsuario || '',
          email: u.emailUsuario || '',
          role: mapRolToUserRole(u.nombreRol),
          active: u.estadoUsuario === 'Activo',
          lastLogin: u.ultimoAcceso ? new Date(u.ultimoAcceso).toISOString().split('T')[0] : '-',
          idRol: u.idRol
        }));
        setUsers(mappedUsers);
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al cargar usuarios";
      toast.error("Error", {
        description: errorMessage,
      });
    } finally {
      setIsLoading(false);
    }
  };

  const loadRoles = async () => {
    try {
      const rolesData = await apiService.getRoles();
      if (rolesData) {
        setRoles(rolesData);
      }
    } catch (error) {
      console.error("Error al cargar roles:", error);
    }
  };

  const filteredUsers = users.filter(user =>
    user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    user.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const toggleUserStatus = async (userId: number) => {
    if (!hasPermission('usuarios.edit')) {
      toast.error('No tienes permisos para modificar usuarios');
      return;
    }
    
    const user = users.find(u => u.id === userId);
    if (!user) return;

    try {
      await apiService.updateUsuario(userId, {
        estadoUsuario: user.active ? 'Inactivo' : 'Activo'
      });
      toast.success(`Usuario ${user.active ? 'desactivado' : 'activado'} exitosamente`);
      await loadUsers(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al actualizar usuario";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  const handleUserCreated = async (newUser: User) => {
    try {
      // El diálogo ya creó el usuario, solo recargar
      await loadUsers();
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al crear usuario";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  const handleUserEdited = async (editedUser: User) => {
    try {
      await apiService.updateUsuario(editedUser.id, {
        nombreUsuario: editedUser.name,
        emailUsuario: editedUser.email,
        idRol: editedUser.idRol
      });
      toast.success('Usuario actualizado exitosamente');
      await loadUsers(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al actualizar usuario";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  const handleDeleteUser = async () => {
    if (!selectedUser) return;
    
    // No permitir eliminar el usuario actual
    if (currentUser && selectedUser.id === currentUser.id) {
      toast.error('No puedes eliminar tu propio usuario');
      setDeleteUserOpen(false);
      setSelectedUser(null);
      return;
    }

    try {
      await apiService.deleteUsuario(selectedUser.id);
      toast.success('Usuario eliminado exitosamente');
      await loadUsers(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al eliminar usuario";
      toast.error("Error", {
        description: errorMessage,
      });
    } finally {
      setDeleteUserOpen(false);
      setSelectedUser(null);
    }
  };

  if (isLoading) {
    return (
      <div className="p-6 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#C9A664] mx-auto mb-4"></div>
          <p className="text-muted-foreground">Cargando usuarios...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1>Gestión de Usuarios</h1>
          <p className="text-muted-foreground">Administra los usuarios del sistema</p>
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

      {/* Estadísticas */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm">Total Usuarios</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{users.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm">Administradores</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {users.filter(u => u.role === 'admin').length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm">Vendedores</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {users.filter(u => u.role === 'seller').length}
            </div>
          </CardContent>
        </Card>
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
          <CardTitle>Lista de Usuarios</CardTitle>
          <CardDescription>
            Mostrando {filteredUsers.length} de {users.length} usuarios
          </CardDescription>
        </CardHeader>
        <CardContent>
          {filteredUsers.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              {users.length === 0 ? "No hay usuarios registrados" : "No se encontraron usuarios"}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nombre</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Rol</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead>Último Acceso</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredUsers.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell className="font-medium">{user.name}</TableCell>
                    <TableCell className="text-muted-foreground">{user.email}</TableCell>
                    <TableCell>
                      <Badge variant="outline" className={roleColors[user.role]}>
                        {roleLabels[user.role]}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Switch
                          checked={user.active}
                          onCheckedChange={() => toggleUserStatus(user.id)}
                          disabled={!hasPermission('usuarios.edit') || (currentUser && user.id === currentUser.id)}
                        />
                        <span className={user.active ? 'text-green-600' : 'text-gray-400'}>
                          {user.active ? 'Activo' : 'Inactivo'}
                        </span>
                      </div>
                    </TableCell>
                    <TableCell>{user.lastLogin}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button variant="ghost" size="sm" onClick={() => {
                          setSelectedUser(user);
                          setViewUserOpen(true);
                        }}>
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => {
                          setSelectedUser(user);
                          setEditUserOpen(true);
                        }} disabled={!hasPermission('usuarios.edit')}>
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="sm" 
                          onClick={() => {
                            setSelectedUser(user);
                            setDeleteUserOpen(true);
                          }}
                          disabled={!hasPermission('usuarios.delete') || (currentUser && user.id === currentUser.id)}
                          className="text-red-600 hover:text-red-700 hover:bg-red-50 disabled:opacity-30"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <NewUserDialog
        open={newUserOpen}
        onOpenChange={setNewUserOpen}
        onUserCreated={handleUserCreated}
        roles={roles}
      />

      <EditUserDialog
        open={editUserOpen}
        onOpenChange={setEditUserOpen}
        user={selectedUser}
        onUserEdited={handleUserEdited}
        roles={roles}
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
        description={`¿Estás seguro de que quieres eliminar al usuario "${selectedUser?.name}"? Esta acción no se puede deshacer.`}
        onConfirm={handleDeleteUser}
        confirmText="Eliminar"
        variant="destructive"
      />
    </div>
  );
}
