import { API_CONFIG } from '../config/api';
import type {
  LoginDto,
  LoginResponseDto,
  ClienteDto,
  ClienteCreateDto,
  ClienteUpdateDto,
  ProductoDto,
  ProductoCreateDto,
  ProductoUpdateDto,
  PedidoDto,
  PedidoCreateDto,
  PedidoUpdateDto,
  DashboardDto,
  CategoriaDto,
  EstadoPedidoDto,
  MetodoPagoDto,
  UsuarioDto,
  UsuarioCreateDto,
  UsuarioUpdateDto,
  RolDto,
  AnalisisCompletoDto,
} from '../types/api';

class ApiService {
  private baseURL: string;
  private token: string | null = null;

  constructor() {
    this.baseURL = API_CONFIG.baseURL;
    // Recuperar token del localStorage si existe
    this.token = localStorage.getItem('auth_token');
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseURL}${endpoint}`;
    
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    // Agregar token de autenticación si existe
    if (this.token) {
      headers['Authorization'] = `Bearer ${this.token}`;
    }

    const config: RequestInit = {
      ...options,
      headers,
    };

    try {
      const response = await fetch(url, config);
      
      // Si la respuesta no es exitosa, lanzar error
      if (!response.ok) {
        const errorText = await response.text();
        let errorMessage = 'Error en la solicitud';
        
        try {
          const errorJson = JSON.parse(errorText);
          errorMessage = errorJson.message || errorJson.title || errorMessage;
        } catch {
          errorMessage = errorText || `Error ${response.status}: ${response.statusText}`;
        }
        
        throw new Error(errorMessage);
      }

      // Si la respuesta está vacía (204 No Content), retornar null
      if (response.status === 204) {
        return null as T;
      }

      const data = await response.json();
      return data as T;
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }
      throw new Error('Error desconocido en la solicitud');
    }
  }

  // Métodos de autenticación
  setToken(token: string | null) {
    this.token = token;
    if (token) {
      localStorage.setItem('auth_token', token);
    } else {
      localStorage.removeItem('auth_token');
    }
  }

  getToken(): string | null {
    return this.token;
  }

  async login(credentials: LoginDto): Promise<LoginResponseDto> {
    const response = await this.request<LoginResponseDto>('/Auth/login', {
      method: 'POST',
      body: JSON.stringify({
        email: credentials.email,
        password: credentials.password,
      }),
    });
    
    // Guardar token
    if (response.token) {
      this.setToken(response.token);
    }
    
    return response;
  }

  async logout(): Promise<void> {
    this.setToken(null);
  }

  // Clientes
  async getClientes(): Promise<ClienteDto[]> {
    return this.request<ClienteDto[]>('/Cliente');
  }

  async getCliente(id: number): Promise<ClienteDto> {
    return this.request<ClienteDto>(`/Cliente/${id}`);
  }

  async createCliente(cliente: ClienteCreateDto): Promise<ClienteDto> {
    return this.request<ClienteDto>('/Cliente', {
      method: 'POST',
      body: JSON.stringify(cliente),
    });
  }

  async updateCliente(id: number, cliente: ClienteUpdateDto): Promise<void> {
    return this.request<void>(`/Cliente/${id}`, {
      method: 'PUT',
      body: JSON.stringify(cliente),
    });
  }

  async deleteCliente(id: number): Promise<void> {
    return this.request<void>(`/Cliente/${id}`, {
      method: 'DELETE',
    });
  }

  // Productos
  async getProductos(): Promise<ProductoDto[]> {
    return this.request<ProductoDto[]>('/Producto');
  }

  async getProducto(id: number): Promise<ProductoDto> {
    return this.request<ProductoDto>(`/Producto/${id}`);
  }

  async createProducto(producto: ProductoCreateDto): Promise<ProductoDto> {
    return this.request<ProductoDto>('/Producto', {
      method: 'POST',
      body: JSON.stringify(producto),
    });
  }

  async updateProducto(id: number, producto: ProductoUpdateDto): Promise<void> {
    return this.request<void>(`/Producto/${id}`, {
      method: 'PUT',
      body: JSON.stringify(producto),
    });
  }

  async deleteProducto(id: number): Promise<void> {
    return this.request<void>(`/Producto/${id}`, {
      method: 'DELETE',
    });
  }

  // Pedidos
  async getPedidos(): Promise<PedidoDto[]> {
    return this.request<PedidoDto[]>('/Pedido');
  }

  async getPedido(id: number): Promise<PedidoDto> {
    return this.request<PedidoDto>(`/Pedido/${id}`);
  }

  async createPedido(pedido: PedidoCreateDto): Promise<PedidoDto> {
    return this.request<PedidoDto>('/Pedido', {
      method: 'POST',
      body: JSON.stringify(pedido),
    });
  }

  async updatePedido(id: number, pedido: PedidoUpdateDto): Promise<void> {
    return this.request<void>(`/Pedido/${id}`, {
      method: 'PUT',
      body: JSON.stringify(pedido),
    });
  }

  async deletePedido(id: number): Promise<void> {
    return this.request<void>(`/Pedido/${id}`, {
      method: 'DELETE',
    });
  }

  // Dashboard
  async getDashboard(): Promise<DashboardDto> {
    return this.request<DashboardDto>('/Dashboard');
  }

  // Reportes
  async getAnalisisCompleto(): Promise<AnalisisCompletoDto> {
    return this.request<AnalisisCompletoDto>('/Reportes/analisis-completo');
  }

  // Categorías
  async getCategorias(): Promise<CategoriaDto[]> {
    return this.request<CategoriaDto[]>('/Categoria');
  }

  // Estados de Pedido
  async getEstadosPedido(): Promise<EstadoPedidoDto[]> {
    return this.request<EstadoPedidoDto[]>('/EstadoPedido');
  }

  // Métodos de Pago
  async getMetodosPago(): Promise<MetodoPagoDto[]> {
    return this.request<MetodoPagoDto[]>('/MetodoPago');
  }

  // Usuarios
  async getUsuarios(): Promise<UsuarioDto[]> {
    return this.request<UsuarioDto[]>('/Usuario');
  }

  async createUsuario(usuario: UsuarioCreateDto): Promise<UsuarioDto> {
    return this.request<UsuarioDto>('/Usuario', {
      method: 'POST',
      body: JSON.stringify(usuario),
    });
  }

  async updateUsuario(id: number, usuario: UsuarioUpdateDto): Promise<void> {
    return this.request<void>(`/Usuario/${id}`, {
      method: 'PUT',
      body: JSON.stringify(usuario),
    });
  }

  async deleteUsuario(id: number): Promise<void> {
    return this.request<void>(`/Usuario/${id}`, {
      method: 'DELETE',
    });
  }

  // Roles
  async getRoles(): Promise<RolDto[]> {
    return this.request<RolDto[]>('/Rol');
  }
}

// Exportar instancia singleton
export const apiService = new ApiService();

