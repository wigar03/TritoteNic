import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Badge } from "./ui/badge";
import { Alert, AlertDescription } from "./ui/alert";
import { TrendingUp, TrendingDown, ShoppingBag, DollarSign, Package, AlertTriangle, Clock } from "lucide-react";
import { BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { apiService } from "../services/apiService";
import { toast } from "sonner";
import type { DashboardDto } from "../types/api";

export function Dashboard() {
  const [dashboardData, setDashboardData] = useState<DashboardDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setIsLoading(true);
      console.log("Cargando datos del dashboard...");
      const data = await apiService.getDashboard();
      console.log("Datos del dashboard recibidos:", data);
      if (data) {
        setDashboardData(data);
      } else {
        console.warn("Dashboard devolvió null o undefined");
        setDashboardData(null);
      }
    } catch (error) {
      console.error("Error al cargar dashboard:", error);
      if (error instanceof Error) {
        console.error("Mensaje de error:", error.message);
        console.error("Stack:", error.stack);
      }
      const errorMessage = error instanceof Error ? error.message : "Error al cargar el dashboard";
      toast.error("Error", {
        description: errorMessage,
      });
      setDashboardData(null);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <div className="p-6 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#C9A664] mx-auto mb-4"></div>
          <p className="text-muted-foreground">Cargando dashboard...</p>
        </div>
      </div>
    );
  }

  if (!dashboardData) {
    return (
      <div className="p-6">
        <Alert>
          <AlertTriangle className="h-4 w-4" />
          <AlertDescription>No se pudieron cargar los datos del dashboard</AlertDescription>
        </Alert>
      </div>
    );
  }

  // Preparar datos para gráficos
  const ventasDiarias = dashboardData.ventasDiarias?.map(v => ({
    name: new Date(v.fecha).toLocaleDateString('es-ES', { weekday: 'short' }),
    ventas: Number(v.totalVentas)
  })) || [];

  const topProducts = dashboardData.productosMasVendidos?.map(p => ({
    name: p.nombreProducto || 'Sin nombre',
    ventas: p.cantidadVendida
  })) || [];

  // Alertas
  const productosStockBajo = dashboardData.alertas?.filter(a => a.tipo === 'StockBajo') || [];
  const pedidosRetrasados = dashboardData.alertas?.filter(a => a.tipo === 'PedidoRetrasado') || [];

  // Formatear números
  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('es-NI', { style: 'currency', currency: 'NIO' }).format(value);
  };

  const formatNumber = (value: number) => {
    return new Intl.NumberFormat('es-NI').format(value);
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div>
        <h1>Dashboard</h1>
        <p className="text-muted-foreground">Resumen general de Tritote Nicaragua</p>
      </div>

      {/* Alertas */}
      {(productosStockBajo.length > 0 || pedidosRetrasados.length > 0) && (
        <div className="grid gap-4 md:grid-cols-2">
          {productosStockBajo.length > 0 && (
            <Alert className="border-amber-200 bg-amber-50">
              <AlertTriangle className="h-4 w-4 text-amber-600" />
              <AlertDescription className="text-amber-800">
                <span className="font-medium">{productosStockBajo.length} producto(s)</span> con stock bajo
              </AlertDescription>
            </Alert>
          )}
          {pedidosRetrasados.length > 0 && (
            <Alert className="border-red-200 bg-red-50">
              <Clock className="h-4 w-4 text-red-600" />
              <AlertDescription className="text-red-800">
                <span className="font-medium">{pedidosRetrasados.length} pedido(s)</span> retrasados
              </AlertDescription>
            </Alert>
          )}
        </div>
      )}

      {/* Tarjetas de resumen */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Ventas del Día</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatCurrency(dashboardData.ventasKpi?.ventasDia || 0)}</div>
            {dashboardData.ventasKpi && dashboardData.ventasKpi.porcentajeCambioDia !== 0 && (
              <div className={`flex items-center text-xs mt-1 ${dashboardData.ventasKpi.porcentajeCambioDia >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {dashboardData.ventasKpi.porcentajeCambioDia >= 0 ? (
                  <TrendingUp className="h-3 w-3 mr-1" />
                ) : (
                  <TrendingDown className="h-3 w-3 mr-1" />
                )}
                {Math.abs(dashboardData.ventasKpi.porcentajeCambioDia).toFixed(1)}% vs ayer
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Ventas de la Semana</CardTitle>
            <ShoppingBag className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatCurrency(dashboardData.ventasKpi?.ventasSemana || 0)}</div>
            {dashboardData.ventasKpi && dashboardData.ventasKpi.porcentajeCambioSemana !== 0 && (
              <div className={`flex items-center text-xs mt-1 ${dashboardData.ventasKpi.porcentajeCambioSemana >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {dashboardData.ventasKpi.porcentajeCambioSemana >= 0 ? (
                  <TrendingUp className="h-3 w-3 mr-1" />
                ) : (
                  <TrendingDown className="h-3 w-3 mr-1" />
                )}
                {Math.abs(dashboardData.ventasKpi.porcentajeCambioSemana).toFixed(1)}% vs semana anterior
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Ventas del Mes</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatCurrency(dashboardData.ventasKpi?.ventasMes || 0)}</div>
            {dashboardData.ventasKpi && dashboardData.ventasKpi.porcentajeCambioMes !== 0 && (
              <div className={`flex items-center text-xs mt-1 ${dashboardData.ventasKpi.porcentajeCambioMes >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {dashboardData.ventasKpi.porcentajeCambioMes >= 0 ? (
                  <TrendingUp className="h-3 w-3 mr-1" />
                ) : (
                  <TrendingDown className="h-3 w-3 mr-1" />
                )}
                {Math.abs(dashboardData.ventasKpi.porcentajeCambioMes).toFixed(1)}% vs mes anterior
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Pedidos Activos</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboardData.pedidosKpi?.totalPedidos || 0}</div>
            <div className="flex items-center text-xs text-muted-foreground mt-1">
              {dashboardData.pedidosKpi?.pedidosPendientes || 0} pendientes, {dashboardData.pedidosKpi?.pedidosEnProceso || 0} en proceso
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Gráficos */}
      <div className="grid gap-4 md:grid-cols-2">
        {/* Ventas de la semana */}
        {ventasDiarias.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle>Ventas de la Semana</CardTitle>
              <CardDescription>Ingresos diarios</CardDescription>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={ventasDiarias}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip 
                    formatter={(value) => [formatCurrency(Number(value)), 'Ventas']}
                    contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
                  />
                  <Bar dataKey="ventas" fill="#C9A664" radius={[8, 8, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        )}

        {/* Productos más vendidos */}
        {topProducts.length > 0 && (
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
        )}
      </div>
    </div>
  );
}
