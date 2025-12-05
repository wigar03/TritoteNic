import { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { apiService } from '../services/apiService';
import type { UsuarioDto } from '../types/api';

type UserRole = 'admin' | 'seller';

interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
}

interface AuthContextType {
  currentUser: User | null;
  setCurrentUser: (user: User | null) => void;
  hasPermission: (permission: Permission) => boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  isLoading: boolean;
}

type Permission = 
  // Clientes
  | 'clientes.view'
  | 'clientes.create'
  | 'clientes.edit'
  | 'clientes.delete'
  // Productos
  | 'productos.view'
  | 'productos.create'
  | 'productos.edit'
  | 'productos.delete'
  // Pedidos
  | 'pedidos.view'
  | 'pedidos.create'
  | 'pedidos.edit'
  | 'pedidos.delete'
  // Usuarios
  | 'usuarios.view'
  | 'usuarios.create'
  | 'usuarios.edit'
  | 'usuarios.delete'
  // Reportes
  | 'reportes.view'
  | 'reportes.export'
  // Dashboard
  | 'dashboard.view';

const rolePermissions: Record<UserRole, Permission[]> = {
  admin: [
    // Acceso completo a todo
    'clientes.view', 'clientes.create', 'clientes.edit', 'clientes.delete',
    'productos.view', 'productos.create', 'productos.edit', 'productos.delete',
    'pedidos.view', 'pedidos.create', 'pedidos.edit', 'pedidos.delete',
    'usuarios.view', 'usuarios.create', 'usuarios.edit', 'usuarios.delete',
    'reportes.view', 'reportes.export',
    'dashboard.view'
  ],
  seller: [
    // Ver clientes, productos, pedidos
    'clientes.view', 'clientes.create', 'clientes.edit', // Puede crear y modificar clientes
    'productos.view', // Solo ver productos
    'pedidos.view', 'pedidos.create', // Ver y crear pedidos
    'reportes.view', // Ver reportes
    'dashboard.view' // Ver dashboard
    // NO puede: eliminar clientes, productos, usuarios, ni gestionar catálogos
  ]
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Función para mapear el rol de la API al rol interno
function mapRoleFromApi(nombreRol: string | null | undefined): UserRole {
  if (!nombreRol) return 'seller';
  const rolLower = nombreRol.toLowerCase();
  if (rolLower.includes('admin') || rolLower.includes('administrador')) {
    return 'admin';
  }
  return 'seller';
}

// Función para convertir UsuarioDto a User
function usuarioDtoToUser(usuario: UsuarioDto): User {
  return {
    id: usuario.idUsuario,
    name: usuario.nombreUsuario || '',
    email: usuario.emailUsuario || '',
    role: mapRoleFromApi(usuario.nombreRol),
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Recuperar usuario del localStorage al cargar
  useEffect(() => {
    const loadUserFromStorage = async () => {
      try {
        const token = apiService.getToken();
        if (token) {
          // Intentar recuperar información del usuario del localStorage
          const storedUser = localStorage.getItem('auth_user');
          if (storedUser) {
            const usuario: UsuarioDto = JSON.parse(storedUser);
            setCurrentUser(usuarioDtoToUser(usuario));
          } else {
            // Si hay token pero no usuario, limpiar token (sesión inválida)
            apiService.logout();
          }
        }
      } catch (error) {
        console.error('Error al cargar usuario desde storage:', error);
        // Si hay error, limpiar todo
        apiService.logout();
        localStorage.removeItem('auth_user');
      } finally {
        setIsLoading(false);
      }
    };

    loadUserFromStorage();
  }, []);

  const login = async (email: string, password: string): Promise<void> => {
    try {
      const response = await apiService.login({ email, password });
      
      // Verificar que el usuario esté activo antes de guardar
      if (response.usuario.estadoUsuario !== 'Activo') {
        await logout();
        throw new Error('Usuario inactivo. No se puede iniciar sesión.');
      }
      
      // Guardar usuario en localStorage
      localStorage.setItem('auth_user', JSON.stringify(response.usuario));
      
      // Convertir y establecer usuario actual
      const user = usuarioDtoToUser(response.usuario);
      setCurrentUser(user);
    } catch (error) {
      throw error;
    }
  };

  const logout = async (): Promise<void> => {
    await apiService.logout();
    localStorage.removeItem('auth_user');
    setCurrentUser(null);
  };

  const hasPermission = (permission: Permission): boolean => {
    if (!currentUser) return false;
    return rolePermissions[currentUser.role].includes(permission);
  };

  return (
    <AuthContext.Provider value={{ 
      currentUser, 
      setCurrentUser, 
      hasPermission,
      login,
      logout,
      isLoading
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

export function usePermissions() {
  const { hasPermission } = useAuth();
  return { hasPermission };
}