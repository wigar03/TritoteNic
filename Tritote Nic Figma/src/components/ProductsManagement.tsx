import { useState } from "react";
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

interface Product {
  id: number;
  name: string;
  category: string;
  price: number;
  stock: number;
  image: string;
}

const mockProducts: Product[] = [
  { id: 1, name: 'Tote Bag Clásico Beige', category: 'Canvas', price: 450, stock: 25, image: 'https://images.unsplash.com/photo-1574365569389-a10d488ca3fb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop' },
  { id: 2, name: 'Tote Bag Eco Natural', category: 'Ecológico', price: 520, stock: 18, image: 'https://images.unsplash.com/photo-1758708536099-9f46dc81fffc?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop' },
  { id: 3, name: 'Tote Bag Shopping Negro', category: 'Canvas', price: 480, stock: 30, image: 'https://images.unsplash.com/photo-1759463408569-c81a969a6c3d?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop' },
  { id: 4, name: 'Tote Bag Playero Azul', category: 'Playa', price: 580, stock: 15, image: 'https://images.unsplash.com/photo-1465742744535-bac796b80f3a?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop' },
  { id: 5, name: 'Tote Bag Minimalista Blanco', category: 'Minimalista', price: 550, stock: 22, image: 'https://images.unsplash.com/photo-1647426112650-6c96ce7ab5f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop' },
  { id: 6, name: 'Tote Bag Market Kraft', category: 'Market', price: 420, stock: 28, image: 'https://images.unsplash.com/photo-1663154438413-244fae34c9a7?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop' },
  { id: 7, name: 'Tote Bag Premium Gris', category: 'Premium', price: 650, stock: 12, image: 'https://images.unsplash.com/photo-1574365569389-a10d488ca3fb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop' },
  { id: 8, name: 'Tote Bag Organic Verde', category: 'Ecológico', price: 580, stock: 20, image: 'https://images.unsplash.com/photo-1758708536099-9f46dc81fffc?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400&h=400&fit=crop' },
];

export function ProductsManagement() {
  const [products, setProducts] = useState(mockProducts);
  const [searchTerm, setSearchTerm] = useState('');
  const [newProductOpen, setNewProductOpen] = useState(false);
  const [editProductOpen, setEditProductOpen] = useState(false);
  const [deleteProductOpen, setDeleteProductOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const { hasPermission } = usePermissions();

  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.category.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleDelete = (id: number) => {
    if (!hasPermission('productos.delete')) {
      toast.error('No tienes permisos para eliminar productos');
      return;
    }
    toast.success('Producto eliminado');
    setProducts(products.filter(p => p.id !== id));
  };

  const handleProductCreated = (newProduct: Product) => {
    // FASE 3: PERSISTENCIA - Agregar producto al estado
    setProducts([...products, newProduct]);
  };

  const handleProductEdited = (editedProduct: Product) => {
    // FASE 3: PERSISTENCIA - Actualizar producto en el estado
    setProducts(products.map(p => p.id === editedProduct.id ? editedProduct : p));
  };

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
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {filteredProducts.map((product) => (
          <Card key={product.id} className="overflow-hidden hover:shadow-lg transition-shadow">
            <div className="aspect-square overflow-hidden bg-muted">
              <img
                src={product.image}
                alt={product.name}
                className="w-full h-full object-cover hover:scale-105 transition-transform"
              />
            </div>
            <CardContent className="p-4">
              <div className="space-y-3">
                <div>
                  <h3 className="text-lg">{product.name}</h3>
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

      <NewProductDialog
        open={newProductOpen}
        onOpenChange={setNewProductOpen}
        onProductCreated={handleProductCreated}
      />

      <EditProductDialog
        open={editProductOpen}
        onOpenChange={setEditProductOpen}
        onProductEdited={handleProductEdited}
        product={selectedProduct}
      />

      <ConfirmDialog
        open={deleteProductOpen}
        onOpenChange={() => setDeleteProductOpen(false)}
        onConfirm={() => {
          if (selectedProduct) {
            handleDelete(selectedProduct.id);
          }
        }}
        title="Eliminar Producto"
        description="¿Estás seguro de que quieres eliminar este producto? Esta acción no se puede deshacer."
      />
    </div>
  );
}