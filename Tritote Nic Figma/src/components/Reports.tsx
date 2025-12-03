import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Download, FileSpreadsheet, FileText, TrendingUp, TrendingDown } from "lucide-react";
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { toast } from "sonner";

const salesComparison = [
  { period: 'Semana 1', actual: 42000, anterior: 38000 },
  { period: 'Semana 2', actual: 48000, anterior: 41000 },
  { period: 'Semana 3', actual: 45000, anterior: 43000 },
  { period: 'Semana 4', actual: 52000, anterior: 45000 },
];

const topProducts = [
  { name: 'Bolso Clásico Negro', rotacion: 95, ventas: 145 },
  { name: 'Cartera Mini Rosa', rotacion: 88, ventas: 132 },
  { name: 'Tote Bag Beige', rotacion: 82, ventas: 118 },
  { name: 'Mochila Urbana', rotacion: 76, ventas: 98 },
  { name: 'Clutch Dorado', rotacion: 71, ventas: 87 },
];

const lowPerformers = [
  { name: 'Bolso XXL', rotacion: 12, ventas: 8 },
  { name: 'Clutch Plateado', rotacion: 18, ventas: 12 },
  { name: 'Cartera Vintage', rotacion: 22, ventas: 15 },
];

const colorTrends = [
  { color: 'Negro', porcentaje: 35, ventas: 245 },
  { color: 'Beige/Nude', porcentaje: 28, ventas: 196 },
  { color: 'Rosa', porcentaje: 18, ventas: 126 },
  { color: 'Dorado', porcentaje: 12, ventas: 84 },
  { color: 'Otros', porcentaje: 7, ventas: 49 },
];

const seasonalTrends = [
  { mes: 'Ene', primavera: 85, verano: 45, otono: 120, invierno: 95 },
  { mes: 'Feb', primavera: 92, verano: 52, otono: 108, invierno: 88 },
  { mes: 'Mar', primavera: 125, verano: 78, otono: 85, invierno: 72 },
  { mes: 'Abr', primavera: 145, verano: 95, otono: 65, invierno: 58 },
  { mes: 'May', primavera: 132, verano: 118, otono: 52, invierno: 45 },
  { mes: 'Jun', primavera: 108, verano: 165, otono: 38, invierno: 35 },
];

export function Reports() {
  const handleExport = (format: 'excel' | 'pdf') => {
    toast.success(`Exportando reporte a ${format.toUpperCase()}...`);
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1>Reportes y Análisis</h1>
          <p className="text-muted-foreground">Análisis detallado de tendencias de ventas</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => handleExport('excel')}>
            <FileSpreadsheet className="h-4 w-4 mr-2" />
            Exportar Excel
          </Button>
          <Button className="bg-[#C9A664] hover:bg-[#B8965A]" onClick={() => handleExport('pdf')}>
            <FileText className="h-4 w-4 mr-2" />
            Exportar PDF
          </Button>
        </div>
      </div>

      {/* Filtros de período */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center gap-4">
            <div className="flex-1">
              <Select defaultValue="month">
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="day">Por día</SelectItem>
                  <SelectItem value="week">Por semana</SelectItem>
                  <SelectItem value="month">Por mes</SelectItem>
                  <SelectItem value="year">Por año</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <Button>Aplicar Filtros</Button>
          </div>
        </CardContent>
      </Card>

      {/* Comparativa de ventas */}
      <Card>
        <CardHeader>
          <CardTitle>Comparativa de Ventas</CardTitle>
          <CardDescription>Período actual vs período anterior</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={350}>
            <BarChart data={salesComparison}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="period" />
              <YAxis />
              <Tooltip 
                formatter={(value) => [`$${value}`, '']}
                contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
              />
              <Legend />
              <Bar dataKey="actual" name="Período Actual" fill="#C9A664" radius={[8, 8, 0, 0]} />
              <Bar dataKey="anterior" name="Período Anterior" fill="#E5D5C3" radius={[8, 8, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Productos con mayor y menor rotación */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-green-600" />
              Mayor Rotación
            </CardTitle>
            <CardDescription>Productos más vendidos</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {topProducts.map((product, index) => (
                <div key={index} className="flex items-center justify-between">
                  <div className="flex-1">
                    <p className="text-sm font-medium">{product.name}</p>
                    <div className="w-full bg-muted rounded-full h-2 mt-1">
                      <div
                        className="bg-green-500 h-2 rounded-full"
                        style={{ width: `${product.rotacion}%` }}
                      />
                    </div>
                  </div>
                  <div className="ml-4 text-right">
                    <p className="text-sm font-medium">{product.ventas}</p>
                    <p className="text-xs text-muted-foreground">unidades</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingDown className="h-5 w-5 text-red-600" />
              Menor Rotación
            </CardTitle>
            <CardDescription>Productos con baja demanda</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {lowPerformers.map((product, index) => (
                <div key={index} className="flex items-center justify-between">
                  <div className="flex-1">
                    <p className="text-sm font-medium">{product.name}</p>
                    <div className="w-full bg-muted rounded-full h-2 mt-1">
                      <div
                        className="bg-red-500 h-2 rounded-full"
                        style={{ width: `${product.rotacion}%` }}
                      />
                    </div>
                  </div>
                  <div className="ml-4 text-right">
                    <p className="text-sm font-medium">{product.ventas}</p>
                    <p className="text-xs text-muted-foreground">unidades</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tendencias de color */}
      <Card>
        <CardHeader>
          <CardTitle>Tendencias de Color</CardTitle>
          <CardDescription>Preferencias de color de los clientes</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={colorTrends} layout="vertical">
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis type="number" />
              <YAxis dataKey="color" type="category" width={100} />
              <Tooltip 
                formatter={(value, name) => [
                  name === 'ventas' ? `${value} unidades` : `${value}%`,
                  name === 'ventas' ? 'Ventas' : 'Porcentaje'
                ]}
                contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
              />
              <Bar dataKey="porcentaje" fill="#C9A664" radius={[0, 8, 8, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Tendencias por temporada */}
      <Card>
        <CardHeader>
          <CardTitle>Tendencias por Temporada</CardTitle>
          <CardDescription>Análisis de ventas según colecciones de temporada</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={350}>
            <LineChart data={seasonalTrends}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="mes" />
              <YAxis />
              <Tooltip 
                contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
              />
              <Legend />
              <Line type="monotone" dataKey="primavera" stroke="#82ca9d" strokeWidth={2} dot={{ r: 4 }} />
              <Line type="monotone" dataKey="verano" stroke="#ffc658" strokeWidth={2} dot={{ r: 4 }} />
              <Line type="monotone" dataKey="otono" stroke="#ff8042" strokeWidth={2} dot={{ r: 4 }} />
              <Line type="monotone" dataKey="invierno" stroke="#8884d8" strokeWidth={2} dot={{ r: 4 }} />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>
    </div>
  );
}
