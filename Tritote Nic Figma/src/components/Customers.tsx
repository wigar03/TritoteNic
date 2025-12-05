import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Input } from "./ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "./ui/table";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Search, UserPlus, Pencil, Trash2 } from "lucide-react";
import { NewCustomerDialog } from "./NewCustomerDialog";
import { EditCustomerDialog } from "./EditCustomerDialog";
import { ConfirmDialog } from "./ConfirmDialog";
import { usePermissions } from "../contexts/AuthContext";
import { toast } from "sonner";
import { apiService } from "../services/apiService";
import type { ClienteDto } from "../types/api";

interface Customer {
  id: number;
  name: string;
  email: string;
  phone: string;
  totalOrders: number;
  totalSpent: number;
  lastOrder: string | null;
}

export function Customers() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [newCustomerOpen, setNewCustomerOpen] = useState(false);
  const [editCustomerOpen, setEditCustomerOpen] = useState(false);
  const [deleteCustomerOpen, setDeleteCustomerOpen] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [pedidosAsociados, setPedidosAsociados] = useState<number>(0);
  const { hasPermission } = usePermissions();

  useEffect(() => {
    loadCustomers();
  }, []);

  const loadCustomers = async () => {
    try {
      setIsLoading(true);
      const clientes = await apiService.getClientes();
      if (clientes) {
        const mappedCustomers: Customer[] = clientes.map(c => ({
          id: c.idCliente,
          name: c.nombreCliente || '',
          email: c.emailCliente || '',
          phone: c.telefonoCliente || '',
          totalOrders: c.totalPedidos || 0,
          totalSpent: Number(c.totalGastado),
          lastOrder: c.fechaUltimoPedido ? new Date(c.fechaUltimoPedido).toISOString().split('T')[0] : null
        }));
        setCustomers(mappedCustomers);
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al cargar clientes";
      toast.error("Error", {
        description: errorMessage,
      });
    } finally {
      setIsLoading(false);
    }
  };

  const filteredCustomers = customers.filter(customer =>
    customer.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    customer.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getCustomerTier = (totalSpent: number) => {
    if (totalSpent >= 70) return { label: 'VIP', className: 'bg-[#C9A664] text-white border-[#C9A664]' };
    if (totalSpent >= 10) return { label: 'Frecuente', className: 'bg-blue-100 text-blue-800 border-blue-200' };
    return { label: 'Regular', className: 'bg-gray-100 text-gray-800 border-gray-200' };
  };

  const handleCustomerCreated = async (newCustomer: Customer) => {
    try {
      await apiService.createCliente({
        nombreCliente: newCustomer.name,
        emailCliente: newCustomer.email,
        telefonoCliente: newCustomer.phone,
        direccionCliente: ''
      });
      toast.success('Cliente creado exitosamente');
      await loadCustomers(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al crear cliente";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  const handleCustomerUpdated = async (updatedCustomer: Customer) => {
    try {
      await apiService.updateCliente(updatedCustomer.id, {
        nombreCliente: updatedCustomer.name,
        emailCliente: updatedCustomer.email,
        telefonoCliente: updatedCustomer.phone
      });
      toast.success('Cliente actualizado exitosamente');
      await loadCustomers(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al actualizar cliente";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  const handleDeleteCustomer = async () => {
    if (!selectedCustomer) return;
    
    try {
      await apiService.deleteCliente(selectedCustomer.id);
      toast.success('Cliente eliminado exitosamente');
      await loadCustomers(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al eliminar cliente";
      toast.error("Error", {
        description: errorMessage,
      });
    } finally {
      setDeleteCustomerOpen(false);
      setSelectedCustomer(null);
    }
  };

  const handleEditCustomer = (customer: Customer) => {
    setSelectedCustomer(customer);
    setEditCustomerOpen(true);
  };

  const handleDeleteClick = async (customer: Customer) => {
    setSelectedCustomer(customer);
    
    // Obtener pedidos asociados al cliente
    try {
      const pedidos = await apiService.getPedidos();
      const pedidosDelCliente = pedidos.filter((p: any) => p.idCliente === customer.id);
      setPedidosAsociados(pedidosDelCliente.length);
    } catch (error) {
      console.error("Error al obtener pedidos asociados:", error);
      setPedidosAsociados(0);
    }
    
    setDeleteCustomerOpen(true);
  };

  if (isLoading) {
    return (
      <div className="p-6 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#C9A664] mx-auto mb-4"></div>
          <p className="text-muted-foreground">Cargando clientes...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1>Clientes</h1>
          <p className="text-muted-foreground">Gestiona tu base de clientes</p>
        </div>
        <Button 
          onClick={() => setNewCustomerOpen(true)}
          className="bg-[#C9A664] hover:bg-[#B8965A]"
          disabled={!hasPermission('clientes.create')}
        >
          <UserPlus className="h-4 w-4 mr-2" />
          Nuevo Cliente
        </Button>
      </div>

      {/* Estadísticas rápidas */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm">Total Clientes</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{customers.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm">Clientes VIP</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {customers.filter(c => c.totalSpent >= 70).length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm">Valor Promedio</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              ${customers.length > 0 ? Math.round(customers.reduce((sum, c) => sum + c.totalSpent, 0) / customers.length).toLocaleString() : '0'}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Búsqueda */}
      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Buscar clientes..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Tabla de clientes */}
      <Card>
        <CardHeader>
          <CardTitle>Lista de Clientes</CardTitle>
          <CardDescription>
            Mostrando {filteredCustomers.length} de {customers.length} clientes
          </CardDescription>
        </CardHeader>
        <CardContent>
          {filteredCustomers.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              {customers.length === 0 ? "No hay clientes registrados" : "No se encontraron clientes"}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nombre</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Teléfono</TableHead>
                  <TableHead>Pedidos</TableHead>
                  <TableHead>Total Gastado</TableHead>
                  <TableHead>Categoría</TableHead>
                  <TableHead>Último Pedido</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredCustomers.map((customer) => {
                  const tier = getCustomerTier(customer.totalSpent);
                  return (
                    <TableRow key={customer.id}>
                      <TableCell className="font-medium">{customer.name}</TableCell>
                      <TableCell className="text-muted-foreground">{customer.email}</TableCell>
                      <TableCell className="text-muted-foreground">{customer.phone}</TableCell>
                      <TableCell>{customer.totalOrders}</TableCell>
                      <TableCell className="font-medium">${customer.totalSpent.toLocaleString()}</TableCell>
                      <TableCell>
                        {customer.totalSpent > 0 ? (
                          <Badge variant="outline" className={tier.className}>
                            {tier.label}
                          </Badge>
                        ) : (
                          <span className="text-sm text-muted-foreground">Sin categoría</span>
                        )}
                      </TableCell>
                      <TableCell>{customer.lastOrder || '-'}</TableCell>
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end gap-1">
                          <Button variant="ghost" size="sm" onClick={() => handleEditCustomer(customer)}>
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button 
                            variant="ghost" 
                            size="sm" 
                            onClick={() => handleDeleteClick(customer)}
                            disabled={!hasPermission('clientes.delete')}
                            className="text-red-600 hover:text-red-700 hover:bg-red-50 disabled:opacity-30"
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <NewCustomerDialog
        open={newCustomerOpen}
        onOpenChange={setNewCustomerOpen}
        onCustomerCreated={handleCustomerCreated}
      />

      <EditCustomerDialog
        open={editCustomerOpen}
        onOpenChange={setEditCustomerOpen}
        customer={selectedCustomer}
        onCustomerUpdated={handleCustomerUpdated}
      />

      <ConfirmDialog
        open={deleteCustomerOpen}
        onOpenChange={(open) => {
          setDeleteCustomerOpen(open);
          if (!open) {
            setPedidosAsociados(0);
          }
        }}
        title="Eliminar Cliente"
        description={
          <div className="space-y-2">
            <p>¿Estás seguro de que quieres eliminar al cliente "{selectedCustomer?.name}"?</p>
            {pedidosAsociados > 0 && (
              <div className="mt-3 p-3 bg-yellow-50 border border-yellow-200 rounded-md">
                <p className="text-sm font-medium text-yellow-800">
                  ⚠️ Este cliente tiene {pedidosAsociados} pedido{pedidosAsociados > 1 ? 's' : ''} asociado{pedidosAsociados > 1 ? 's' : ''}.
                </p>
                <p className="text-sm text-yellow-700 mt-1">
                  Si continúas, se eliminarán todos los pedidos asociados a este cliente, lo que afectará las estadísticas y gráficas del sistema.
                </p>
              </div>
            )}
            <p className="text-sm text-red-600 font-medium mt-2">Esta acción no se puede deshacer.</p>
          </div>
        }
        onConfirm={handleDeleteCustomer}
        confirmText="Eliminar"
        variant="destructive"
      />
    </div>
  );
}
