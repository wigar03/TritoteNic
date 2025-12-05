import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { FileText, TrendingUp, TrendingDown } from "lucide-react";
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { toast } from "sonner";
import { apiService } from "../services/apiService";
import type { AnalisisCompletoDto } from "../types/api";

export function Reports() {
  const [reportData, setReportData] = useState<AnalisisCompletoDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [period, setPeriod] = useState('month');

  useEffect(() => {
    loadReportData();
  }, [period]);

  const loadReportData = async () => {
    try {
      setIsLoading(true);
      console.log("Cargando datos de reportes para período:", period);
      
      const hoy = new Date();
      let mes: number | undefined;
      let año: number | undefined;

      switch (period) {
        case 'day':
          // Por día: usar el mes y año actual
          mes = hoy.getMonth() + 1;
          año = hoy.getFullYear();
          break;
        case 'week':
          // Por semana: usar el mes y año actual
          mes = hoy.getMonth() + 1;
          año = hoy.getFullYear();
          break;
        case 'month':
          // Por mes: usar el mes y año actual
          mes = hoy.getMonth() + 1;
          año = hoy.getFullYear();
          break;
        case 'year':
          // Por año: solo usar el año actual
          año = hoy.getFullYear();
          mes = undefined;
          break;
        default:
          mes = hoy.getMonth() + 1;
          año = hoy.getFullYear();
      }

      const data = await apiService.getAnalisisCompleto(mes, año);
      console.log("Datos de reportes recibidos:", data);
      if (data) {
        setReportData(data);
      } else {
        console.warn("Reportes devolvieron null o undefined");
        setReportData(null);
      }
    } catch (error) {
      console.error("Error al cargar reportes:", error);
      if (error instanceof Error) {
        console.error("Mensaje de error:", error.message);
        console.error("Stack:", error.stack);
      }
      const errorMessage = error instanceof Error ? error.message : "Error al cargar reportes";
      toast.error("Error", {
        description: errorMessage,
      });
      setReportData(null);
    } finally {
      setIsLoading(false);
    }
  };

  const handleExportPDF = async () => {
    try {
      // Importar jsPDF dinámicamente
      const { jsPDF } = await import('jspdf');
      
      if (!reportData) {
        toast.error('No hay datos para exportar');
        return;
      }
      
      const doc = new jsPDF();
      
      // Configuración de colores y estilos
      const primaryColor = [201, 166, 100]; // #C9A664
      const textColor = [51, 51, 51];
      const grayColor = [128, 128, 128];
      
      let yPos = 20;
      
      // Encabezado
      doc.setFillColor(...primaryColor);
      doc.rect(0, 0, 210, 30, 'F');
      doc.setTextColor(255, 255, 255);
      doc.setFontSize(20);
      doc.setFont('helvetica', 'bold');
      doc.text('TRITOTE NIC', 105, 15, { align: 'center' });
      doc.setFontSize(12);
      doc.text('Reportes y Análisis', 105, 25, { align: 'center' });
      
      yPos = 40;
      doc.setTextColor(...textColor);
      
      // Información del período
      const periodLabels: Record<string, string> = {
        'day': 'Por día',
        'week': 'Por semana',
        'month': 'Por mes',
        'year': 'Por año'
      };
      
      doc.setFontSize(14);
      doc.setFont('helvetica', 'bold');
      doc.text(`Reporte - Período: ${periodLabels[period] || 'Por mes'}`, 20, yPos);
      
      yPos += 15;
      
      // Comparativa de ventas
      if (reportData.comparativaVentas) {
        doc.setFontSize(12);
        doc.setFont('helvetica', 'bold');
        doc.text('Comparativa de Ventas', 20, yPos);
        yPos += 8;
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(10);
        doc.setTextColor(...grayColor);
        doc.text(`Cambio: ${reportData.comparativaVentas.porcentajeCambio >= 0 ? '+' : ''}${reportData.comparativaVentas.porcentajeCambio.toFixed(1)}%`, 20, yPos);
        yPos += 10;
      }
      
      // Tendencias temporales
      if (tendenciasTemporada.length > 0) {
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(12);
        doc.setTextColor(...textColor);
        doc.text('Tendencias Temporales', 20, yPos);
        yPos += 8;
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(10);
        doc.setTextColor(...grayColor);
        
        tendenciasTemporada.slice(0, 6).forEach((t: any) => {
          if (yPos > 250) {
            doc.addPage();
            yPos = 20;
          }
          doc.text(`${t.mes}: C$${t.ventas.toFixed(2)}`, 20, yPos);
          yPos += 6;
        });
        yPos += 5;
      }
      
      // Productos por rotación
      if (productosRotacion.length > 0) {
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(12);
        doc.setTextColor(...textColor);
        doc.text('Productos por Rotación', 20, yPos);
        yPos += 8;
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(10);
        doc.setTextColor(...grayColor);
        
        productosRotacion.slice(0, 10).forEach((p: any) => {
          if (yPos > 250) {
            doc.addPage();
            yPos = 20;
          }
          doc.text(`${p.name}: ${p.rotacion.toFixed(1)}%`, 20, yPos);
          yPos += 6;
        });
      }
      
      // Pie de página
      const pageHeight = doc.internal.pageSize.height;
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(8);
      doc.setTextColor(...grayColor);
      doc.text(`Generado el ${new Date().toLocaleDateString('es-NI')} ${new Date().toLocaleTimeString('es-NI')}`, 105, pageHeight - 10, { align: 'center' });
      
      // Guardar PDF
      doc.save(`Reporte_${period}_${new Date().toISOString().split('T')[0]}.pdf`);
      
      toast.success('PDF generado exitosamente');
    } catch (error) {
      console.error('Error al generar PDF:', error);
      toast.error('Error al generar el PDF');
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('es-NI', { style: 'currency', currency: 'NIO' }).format(value);
  };

  if (isLoading) {
    return (
      <div className="p-6 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#C9A664] mx-auto mb-4"></div>
          <p className="text-muted-foreground">Cargando reportes...</p>
        </div>
      </div>
    );
  }

  if (!reportData) {
    return (
      <div className="p-6">
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground">No se pudieron cargar los datos del reporte</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Preparar datos para gráficos
  const ventasSemanales = reportData.comparativaVentas?.ventasSemanales?.map(v => ({
    periodo: v.periodo,
    actual: Number(v.ventasActuales),
    anterior: Number(v.ventasAnteriores)
  })) || [];

  const tendenciasTemporada = reportData.tendenciasTemporada?.map(t => ({
    mes: t.nombreMes,
    ventas: Number(t.totalVentas)
  })) || [];

  const productosRotacion = reportData.productosRotacion?.map(p => ({
    name: p.nombreProducto || 'Sin nombre',
    rotacion: Number(p.rotacion),
    ventas: p.cantidadVendida,
    tipo: p.tipoRotacion
  })) || [];

  const tendenciasColor = reportData.tendenciasColor?.map(t => ({
    color: t.color,
    porcentaje: Number(t.porcentajeVentas),
    ventas: t.cantidadVendida
  })) || [];

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1>Reportes y Análisis</h1>
          <p className="text-muted-foreground">Análisis detallado de tendencias de ventas</p>
        </div>
        <div className="flex gap-2">
          <Button className="bg-[#C9A664] hover:bg-[#B8965A]" onClick={() => handleExportPDF()}>
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
              <Select value={period} onValueChange={(value) => {
                setPeriod(value);
                // El useEffect se ejecutará automáticamente cuando cambie period
              }}>
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
          </div>
        </CardContent>
      </Card>

      {/* Comparativa de ventas */}
      {ventasSemanales.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Comparativa de Ventas</CardTitle>
            <CardDescription>Período actual vs período anterior</CardDescription>
            {reportData.comparativaVentas && (
              <div className="flex items-center gap-4 mt-2">
                <div className="flex items-center gap-2">
                  {reportData.comparativaVentas.porcentajeCambio >= 0 ? (
                    <TrendingUp className="h-4 w-4 text-green-600" />
                  ) : (
                    <TrendingDown className="h-4 w-4 text-red-600" />
                  )}
                  <span className={`text-sm font-medium ${reportData.comparativaVentas.porcentajeCambio >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {Math.abs(reportData.comparativaVentas.porcentajeCambio).toFixed(1)}% de cambio
                  </span>
                </div>
              </div>
            )}
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={350}>
              <BarChart data={ventasSemanales}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="periodo" />
                <YAxis />
                <Tooltip 
                  formatter={(value) => [formatCurrency(Number(value)), 'Ventas']}
                  contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
                />
                <Legend />
                <Bar dataKey="actual" fill="#C9A664" name="Período Actual" radius={[8, 8, 0, 0]} />
                <Bar dataKey="anterior" fill="#D4B996" name="Período Anterior" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {/* Tendencias temporales */}
      {tendenciasTemporada.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Tendencias Temporales</CardTitle>
            <CardDescription>Evolución de ventas por mes</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={350}>
              <LineChart data={tendenciasTemporada}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="mes" />
                <YAxis />
                <Tooltip 
                  formatter={(value) => [formatCurrency(Number(value)), 'Ventas']}
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
      )}

      {/* Productos por rotación */}
      {productosRotacion.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Rotación de Productos</CardTitle>
            <CardDescription>Productos con mayor y menor rotación</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={350}>
              <BarChart data={productosRotacion.slice(0, 10)} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis type="number" />
                <YAxis dataKey="name" type="category" width={150} />
                <Tooltip 
                  formatter={(value) => [`${Number(value).toFixed(1)}%`, 'Rotación']}
                  contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
                />
                <Bar dataKey="rotacion" fill="#C9A664" radius={[0, 8, 8, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {/* Tendencias de color */}
      {tendenciasColor.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Tendencias de Color</CardTitle>
            <CardDescription>Distribución de ventas por color</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={350}>
              <BarChart data={tendenciasColor}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="color" />
                <YAxis />
                <Tooltip 
                  formatter={(value) => [`${Number(value).toFixed(1)}%`, 'Porcentaje']}
                  contentStyle={{ borderRadius: '8px', border: '1px solid #e5e5e5' }}
                />
                <Bar dataKey="porcentaje" fill="#C9A664" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
