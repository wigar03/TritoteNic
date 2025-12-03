import { useState } from "react";
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

interface Customer {
  id: number;
  name: string;
  email: string;
  phone: string;
  totalOrders: number;
  totalSpent: number;
  lastOrder: string | null;
}

const mockCustomers: Customer[] = [
  {
    id: 1,
    name: 'Maria Castillo',
    email: 'maria.castillo@gmail.com',
    phone: '+505 8765-4321',
    totalOrders: 15,
    totalSpent: 187500,
    lastOrder: '2024-01-10'
  },
  {
    id: 2,
    name: 'Carlos Mendoza',
    email: 'carlos.mendoza@gmail.com',
    phone: '+505 7654-3210',
    totalOrders: 8,
    totalSpent: 95000,
    lastOrder: '2024-01-12'
  },
  {
    id: 3,
    name: 'Ana Sanchez',
    email: 'ana.sanchez@gmail.com',
    phone: '+505 8123-4567',
    totalOrders: 22,
    totalSpent: 275000,
    lastOrder: '2024-01-15'
  },
  {
    id: 4,
    name: 'Roberto Lopez',
    email: 'roberto.lopez@gmail.com',
    phone: '+505 8234-5678',
    totalOrders: 5,
    totalSpent: 52000,
    lastOrder: '2024-01-08'
  },
  {
    id: 5,
    name: 'Sofia Ramirez',
    email: 'sofia.ramirez@gmail.com',
    phone: '+505 8345-6789',
    totalOrders: 12,
    totalSpent: 135000,
    lastOrder: '2024-01-14'
  }
];

export function Customers() {
  const [customers, setCustomers] = useState(mockCustomers);
  const [searchTerm, setSearchTerm] = useState('');
  const [newCustomerOpen, setNewCustomerOpen] = useState(false);
  const [editCustomerOpen, setEditCustomerOpen] = useState(false);
  const [deleteCustomerOpen, setDeleteCustomerOpen] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const { hasPermission } = usePermissions();

  const filteredCustomers = customers.filter(customer =>
    customer.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    customer.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getCustomerTier = (totalSpent: number) => {
    if (totalSpent >= 150000) return { label: 'VIP', className: 'bg-[#C9A664] text-white border-[#C9A664]' };
    if (totalSpent >= 80000) return { label: 'Frecuente', className: 'bg-blue-100 text-blue-800 border-blue-200' };
    return { label: 'Regular', className: 'bg-gray-100 text-gray-800 border-gray-200' };
  };

  const handleCustomerCreated = (newCustomer: Customer) => {
    // FASE 3: PERSISTENCIA - Agregar cliente al estado
    setCustomers([...customers, newCustomer]);
  };

  const handleCustomerUpdated = (updatedCustomer: Customer) => {
    setCustomers(customers.map(c =>
      c.id === updatedCustomer.id ? updatedCustomer : c
    ));
  };

  const handleDeleteCustomer = () => {
    if (selectedCustomer) {
      setCustomers(customers.filter(c => c.id !== selectedCustomer.id));
      toast.success('Cliente eliminado exitosamente');
    }
    setDeleteCustomerOpen(false);
    setSelectedCustomer(null);
  };

  const handleEditCustomer = (customer: Customer) => {
    setSelectedCustomer(customer);
    setEditCustomerOpen(true);
  };

  const handleDeleteClick = (customer: Customer) => {
    setSelectedCustomer(customer);
    setDeleteCustomerOpen(true);
  };

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
              {customers.filter(c => c.totalSpent >= 150000).length}
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
        onOpenChange={setDeleteCustomerOpen}
        title="Eliminar Cliente"
        description={`¿Estás seguro de que quieres eliminar al cliente "${selectedCustomer?.name}"? Esta acción no se puede deshacer.`}
        onConfirm={handleDeleteCustomer}
        confirmText="Eliminar"
        variant="destructive"
      />
    </div>
  );
}