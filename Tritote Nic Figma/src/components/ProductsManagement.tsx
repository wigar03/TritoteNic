import { useState, useEffect } from "react";
import { Button } from "./ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Input } from "./ui/input";
import { Badge } from "./ui/badge";
import { Plus, Search, Edit, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { NewProductDialog } from "./NewProductDialog";
import { EditProductDialog } from "./EditProductDialog";
import { ConfirmDialog } from "./ConfirmDialog";
import { usePermissions } from "../contexts/AuthContext";
import { apiService } from "../services/apiService";
import type { ProductoDto, CategoriaDto } from "../types/api";

interface Product {
  id: number;
  name: string;
  category: string;
  price: number;
  stock: number;
  image: string;
  idCategoria: number;
  estadoProducto?: string;
}

export function ProductsManagement() {
  const [products, setProducts] = useState<Product[]>([]);
  const [categorias, setCategorias] = useState<CategoriaDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [newProductOpen, setNewProductOpen] = useState(false);
  const [editProductOpen, setEditProductOpen] = useState(false);
  const [deleteProductOpen, setDeleteProductOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const { hasPermission } = usePermissions();

  useEffect(() => {
    loadProducts();
    loadCategorias();
  }, []);

  const loadProducts = async () => {
    try {
      setIsLoading(true);
      const productos = await apiService.getProductos();
      if (productos) {
        const mappedProducts: Product[] = productos.map(p => ({
          id: p.idProducto,
          name: p.nombreProducto || '',
          category: p.nombreCategoria || 'Sin categoría',
          price: Number(p.precioProducto),
          stock: p.stockProducto,
          image: p.imagenProducto || 'https://via.placeholder.com/400',
          idCategoria: p.idCategoria,
          estadoProducto: p.estadoProducto || 'Activo'
        }));
        setProducts(mappedProducts);
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al cargar productos";
      toast.error("Error", {
        description: errorMessage,
      });
    } finally {
      setIsLoading(false);
    }
  };

  const loadCategorias = async () => {
    try {
      const cats = await apiService.getCategorias();
      if (cats) {
        setCategorias(cats);
      }
    } catch (error) {
      console.error("Error al cargar categorías:", error);
    }
  };

  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.category.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleDelete = async (id: number) => {
    if (!hasPermission('productos.delete')) {
      toast.error('No tienes permisos para eliminar productos');
      return;
    }
    
    try {
      await apiService.deleteProducto(id);
      toast.success('Producto eliminado exitosamente');
      await loadProducts(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al eliminar producto";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  const handleProductCreated = async (newProduct: Product) => {
    try {
      await apiService.createProducto({
        nombreProducto: newProduct.name,
        descripcionProducto: '',
        precioProducto: newProduct.price,
        stockProducto: newProduct.stock,
        idCategoria: newProduct.idCategoria,
        estadoProducto: newProduct.estadoProducto || 'Activo'
      });
      toast.success('Producto creado exitosamente');
      await loadProducts(); // Recargar desde la API
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error al crear producto";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  const handleProductEdited = async (editedProduct: Product) => {
    try {
      // Validar que el id existe
      if (!editedProduct.id || editedProduct.id === 0) {
        toast.error('Error: No se pudo identificar el producto');
        return;
      }

      const updateDto = {
        idProducto: editedProduct.id,
        nombreProducto: editedProduct.name,
        precioProducto: editedProduct.price,
        stockProducto: editedProduct.stock,
        idCategoria: editedProduct.idCategoria,
        estadoProducto: editedProduct.estadoProducto || 'Activo'
      };

      console.log('Actualizando producto:', updateDto);
      
      await apiService.updateProducto(editedProduct.id, updateDto);
      toast.success('Producto actualizado exitosamente');
      await loadProducts(); // Recargar desde la API
      setEditProductOpen(false);
    } catch (error) {
      console.error('Error al actualizar producto:', error);
      const errorMessage = error instanceof Error ? error.message : "Error al actualizar producto";
      toast.error("Error", {
        description: errorMessage,
      });
    }
  };

  if (isLoading) {
    return (
      <div className="p-6 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#C9A664] mx-auto mb-4"></div>
          <p className="text-muted-foreground">Cargando productos...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1>Catálogo de Productos</h1>
          <p className="text-muted-foreground">Gestiona el inventario de Tote Bags</p>
        </div>
        <Button 
          onClick={() => setNewProductOpen(true)}
          className="bg-[#C9A664] hover:bg-[#B8965A]"
          disabled={!hasPermission('productos.create')}
        >
          <Plus className="h-4 w-4 mr-2" />
          Nuevo Producto
        </Button>
      </div>

      {/* Búsqueda */}
      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Buscar productos..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Grid de productos */}
      {filteredProducts.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground">
              {products.length === 0 ? "No hay productos registrados" : "No se encontraron productos"}
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {filteredProducts.map((product) => (
            <Card key={product.id} className="overflow-hidden hover:shadow-lg transition-shadow">
              <div className="aspect-square overflow-hidden bg-muted">
                <img
                  src={product.image}
                  alt={product.name}
                  className="w-full h-full object-cover hover:scale-105 transition-transform"
                  onError={(e) => {
                    (e.target as HTMLImageElement).src = 'https://via.placeholder.com/400';
                  }}
                />
              </div>
              <CardContent className="p-4">
                <div className="space-y-3">
                  <div>
                    <h3 className="text-lg font-medium">{product.name}</h3>
                    <p className="text-sm text-muted-foreground">{product.category}</p>
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <span className="text-xl font-medium text-[#C9A664]">
                      ${product.price.toLocaleString()}
                    </span>
                    <Badge 
                      variant="outline" 
                      className={product.stock < 10 ? 'border-red-200 bg-red-50 text-red-700' : ''}
                    >
                      Stock: {product.stock}
                    </Badge>
                  </div>

                  <div className="flex gap-2">
                    <Button 
                      variant="outline" 
                      size="sm" 
                      className="flex-1"
                      disabled={!hasPermission('productos.edit')}
                      onClick={() => {
                        setSelectedProduct(product);
                        setEditProductOpen(true);
                      }}
                    >
                      <Edit className="h-3 w-3 mr-1" />
                      Editar
                    </Button>
                    <Button 
                      variant="outline" 
                      size="sm" 
                      className="flex-1 text-destructive hover:bg-destructive hover:text-destructive-foreground disabled:opacity-50"
                      onClick={() => {
                        setSelectedProduct(product);
                        setDeleteProductOpen(true);
                      }}
                      disabled={!hasPermission('productos.delete')}
                    >
                      <Trash2 className="h-3 w-3 mr-1" />
                      Eliminar
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <NewProductDialog
        open={newProductOpen}
        onOpenChange={setNewProductOpen}
        onProductCreated={handleProductCreated}
        categorias={categorias}
      />

      <EditProductDialog
        open={editProductOpen}
        onOpenChange={setEditProductOpen}
        onProductEdited={handleProductEdited}
        product={selectedProduct}
        categorias={categorias}
      />

      <ConfirmDialog
        open={deleteProductOpen}
        onOpenChange={() => setDeleteProductOpen(false)}
        onConfirm={() => {
          if (selectedProduct) {
            handleDelete(selectedProduct.id);
            setDeleteProductOpen(false);
            setSelectedProduct(null);
          }
        }}
        title="Eliminar Producto"
        description={`¿Estás seguro de que quieres eliminar "${selectedProduct?.name}"? Esta acción no se puede deshacer.`}
        confirmText="Eliminar"
        variant="destructive"
      />
    </div>
  );
}
