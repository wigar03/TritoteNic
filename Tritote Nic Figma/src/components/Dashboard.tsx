import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Badge } from "./ui/badge";
import { Alert, AlertDescription } from "./ui/alert";
import { TrendingUp, TrendingDown, ShoppingBag, DollarSign, Package, AlertTriangle, Clock } from "lucide-react";
import { BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";

const salesData = [
  { name: 'Lun', ventas: 4500 },
  { name: 'Mar', ventas: 6200 },
  { name: 'Mié', ventas: 5800 },
  { name: 'Jue', ventas: 7100 },
  { name: 'Vie', ventas: 8900 },
  { name: 'Sáb', ventas: 9500 },
  { name: 'Dom', ventas: 6300 },
];

const topProducts = [
  { name: 'Tote Bag Clásico Beige', ventas: 45 },
  { name: 'Tote Bag Playero Azul', ventas: 38 },
  { name: 'Tote Bag Premium Gris', ventas: 35 },
  { name: 'Tote Bag Shopping Negro', ventas: 32 },
  { name: 'Tote Bag Minimalista Blanco', ventas: 28 },
];

const categoryData = [
  { name: 'Canvas', value: 40, color: '#C9A664' },
  { name: 'Ecológico', value: 25, color: '#D4B996' },
  { name: 'Premium', value: 20, color: '#E5D5C3' },
  { name: 'Playa', value: 10, color: '#F0E9DD' },
  { name: 'Market', value: 5, color: '#F5F3EE' },
];

const monthlyTrend = [
  { mes: 'Ene', ventas: 85000 },
  { mes: 'Feb', ventas: 92000 },
  { mes: 'Mar', ventas: 78000 },
  { mes: 'Abr', ventas: 105000 },
  { mes: 'May', ventas: 118000 },
  { mes: 'Jun', ventas: 125000 },
];

export function Dashboard() {
  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div>
        <h1>Dashboard</h1>
        <p className="text-muted-foreground">Resumen general de Tritote Nicaragua</p>
      </div>

      {/* Alertas */}
      <div className="grid gap-4 md:grid-cols-2">
        <Alert className="border-amber-200 bg-amber-50">
          <AlertTriangle className="h-4 w-4 text-amber-600" />
          <AlertDescription className="text-amber-800">
            <span className="font-medium">3 productos</span> con stock bajo
          </AlertDescription>
        </Alert>
        <Alert className="border-red-200 bg-red-50">
          <Clock className="h-4 w-4 text-red-600" />
          <AlertDescription className="text-red-800">
            <span className="font-medium">2 pedidos</span> retrasados
          </AlertDescription>
        </Alert>
      </div>

      {/* Tarjetas de resumen */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Ventas del Día</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">$8,945</div>
            <div className="flex items-center text-xs text-green-600 mt-1">
              <TrendingUp className="h-3 w-3 mr-1" />
              +12.5% vs ayer
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Ventas de la Semana</CardTitle>
            <ShoppingBag className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">$48,300</div>
            <div className="flex items-center text-xs text-green-600 mt-1">
              <TrendingUp className="h-3 w-3 mr-1" />
              +8.2% vs semana anterior
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Ventas del Mes</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">$125,400</div>
            <div className="flex items-center text-xs text-green-600 mt-1">
              <TrendingUp className="h-3 w-3 mr-1" />
              +15.3% vs mes anterior
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Pedidos Activos</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">32</div>
            <div className="flex items-center text-xs text-muted-foreground mt-1">
              18 pendientes, 14 en proceso
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Gráficos */}
      <div className="grid gap-4 md:grid-cols-2">
        {/* Ventas de la semana */}
        <Card>
          <CardHeader>
            <CardTitle>Ventas de la Semana</CardTitle>
            <CardDescription>Ingresos diarios en pesos</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={salesData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip 
                  formatter={(value) => [`$${value}`, 'Ventas']}
                  contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
                />
                <Bar dataKey="ventas" fill="#C9A664" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Productos más vendidos */}
        <Card>
          <CardHeader>
            <CardTitle>Productos Más Vendidos</CardTitle>
            <CardDescription>Top 5 del mes</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={topProducts} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis type="number" />
                <YAxis dataKey="name" type="category" width={120} />
                <Tooltip 
                  formatter={(value) => [`${value} unidades`, 'Vendidas']}
                  contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
                />
                <Bar dataKey="ventas" fill="#C9A664" radius={[0, 8, 8, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Ventas por categoría */}
        <Card>
          <CardHeader>
            <CardTitle>Distribución por Categoría</CardTitle>
            <CardDescription>Porcentaje de ventas</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={categoryData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                  outerRadius={100}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {categoryData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Tendencia mensual */}
        <Card>
          <CardHeader>
            <CardTitle>Tendencia Mensual</CardTitle>
            <CardDescription>Evolución de ventas en el año</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={monthlyTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="mes" />
                <YAxis />
                <Tooltip 
                  formatter={(value) => [`$${value}`, 'Ventas']}
                  contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
                />
                <Line 
                  type="monotone" 
                  dataKey="ventas" 
                  stroke="#C9A664" 
                  strokeWidth={2}
                  dot={{ fill: '#C9A664', r: 4 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}