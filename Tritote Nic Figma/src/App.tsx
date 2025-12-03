import { useState } from "react";
import { SidebarProvider, Sidebar, SidebarContent, SidebarHeader, SidebarMenu, SidebarMenuItem, SidebarMenuButton, SidebarFooter, SidebarTrigger } from "./components/ui/sidebar";
import { LayoutDashboard, ShoppingCart, Package, Users, BarChart3, Settings, Moon, LogOut } from "lucide-react";
import { Dashboard } from "./components/Dashboard";
import { OrdersManagement } from "./components/OrdersManagement";
import { ProductsManagement } from "./components/ProductsManagement";
import { Customers } from "./components/Customers";
import { Reports } from "./components/Reports";
import { UserManagement } from "./components/UserManagement";
import { Login } from "./components/Login";
import { Toaster } from "./components/ui/sonner";
import { Button } from "./components/ui/button";
import { toast } from "sonner";
import { AuthProvider, useAuth } from "./contexts/AuthContext";

type View = 'dashboard' | 'orders' | 'products' | 'customers' | 'reports' | 'users';

function AppContent() {
  const [currentView, setCurrentView] = useState<View>('dashboard');
  const { currentUser, logout, isLoading } = useAuth();

  const menuItems = [
    { id: 'dashboard' as View, label: 'Dashboard', icon: LayoutDashboard },
    { id: 'orders' as View, label: 'Pedidos', icon: ShoppingCart },
    { id: 'products' as View, label: 'Productos', icon: Package },
    { id: 'customers' as View, label: 'Clientes', icon: Users },
    { id: 'reports' as View, label: 'Reportes', icon: BarChart3 },
    { id: 'users' as View, label: 'Usuarios', icon: Settings },
  ];

  const handleLogout = async () => {
    try {
      await logout();
      setCurrentView('dashboard');
      toast.success('Sesión cerrada exitosamente');
    } catch (error) {
      toast.error('Error al cerrar sesión');
    }
  };

  const renderView = () => {
    switch (currentView) {
      case 'dashboard':
        return <Dashboard />;
      case 'orders':
        return <OrdersManagement />;
      case 'products':
        return <ProductsManagement />;
      case 'customers':
        return <Customers />;
      case 'reports':
        return <Reports />;
      case 'users':
        return <UserManagement />;
      default:
        return <Dashboard />;
    }
  };

  // Mostrar loading mientras se verifica la autenticación
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#C9A664] mx-auto mb-4"></div>
          <p className="text-muted-foreground">Cargando...</p>
        </div>
      </div>
    );
  }

  // Si no hay usuario autenticado, mostrar login
  if (!currentUser) {
    return (
      <>
        <Login />
        <Toaster />
      </>
    );
  }

  // Mapeo de roles para mostrar
  const roleLabels = {
    admin: 'Administrador',
    seller: 'Vendedor'
  };

  // Si hay usuario autenticado, mostrar el sistema
  return (
    <SidebarProvider>
      <div className="min-h-screen flex w-full bg-background">
        <Sidebar>
          <SidebarHeader className="border-b border-sidebar-border p-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-gradient-to-br from-[#C9A664] to-[#D4B996] flex items-center justify-center">
                <Moon className="h-5 w-5 text-white" />
              </div>
              <div>
                <h2 className="font-semibold">Tritote Nicaragua</h2>
                <p className="text-xs text-muted-foreground">Sistema de Gestión</p>
              </div>
            </div>
          </SidebarHeader>

          <SidebarContent>
            <SidebarMenu>
              {menuItems.map((item) => (
                <SidebarMenuItem key={item.id}>
                  <SidebarMenuButton
                    onClick={() => setCurrentView(item.id)}
                    isActive={currentView === item.id}
                    className="w-full"
                  >
                    <item.icon className="h-4 w-4" />
                    <span>{item.label}</span>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarContent>

          <SidebarFooter className="border-t border-sidebar-border p-4">
            <div className="space-y-3">
              <div className="flex items-center gap-3">
                <div className="h-8 w-8 rounded-full bg-[#C9A664] flex items-center justify-center text-white text-sm">
                  {currentUser.name.split(' ').map(n => n[0]).join('').slice(0, 2)}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{currentUser.name}</p>
                  <p className="text-xs text-muted-foreground">{roleLabels[currentUser.role]}</p>
                </div>
              </div>
              <Button
                variant="outline"
                size="sm"
                className="w-full"
                onClick={handleLogout}
              >
                <LogOut className="h-4 w-4 mr-2" />
                Cerrar Sesión
              </Button>
            </div>
          </SidebarFooter>
        </Sidebar>

        <main className="flex-1 overflow-auto">
          <div className="border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 sticky top-0 z-10">
            <div className="flex h-14 items-center gap-4 px-4">
              <SidebarTrigger />
              <div className="flex-1" />
              <div className="text-sm text-muted-foreground">
                {new Date().toLocaleDateString('es-AR', { 
                  weekday: 'long', 
                  year: 'numeric', 
                  month: 'long', 
                  day: 'numeric' 
                })}
              </div>
            </div>
          </div>
          
          <div className="flex-1">
            {renderView()}
          </div>
        </main>

        <Toaster />
      </div>
    </SidebarProvider>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}