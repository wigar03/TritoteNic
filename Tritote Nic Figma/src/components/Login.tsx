import { useState } from "react";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "./ui/card";
import { Moon, Lock, Mail, Eye, EyeOff, Wifi, WifiOff } from "lucide-react";
import { toast } from "sonner";
import { useAuth } from "../contexts/AuthContext";
import { testApiConnection } from "../utils/testConnection";
import { API_CONFIG } from "../config/api";

export function Login() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isTestingConnection, setIsTestingConnection] = useState(false);
  const [connectionStatus, setConnectionStatus] = useState<{ success: boolean; message: string } | null>(null);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!email || !password) {
      toast.error("Por favor completa todos los campos");
      return;
    }

    setIsLoading(true);

    try {
      await login(email, password);
      toast.success("¡Inicio de sesión exitoso!");
    } catch (error) {
      const errorMessage = error instanceof Error 
        ? error.message 
        : "Error al iniciar sesión. Por favor verifica tus credenciales.";
      toast.error("Error de autenticación", {
        description: errorMessage,
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleTestConnection = async () => {
    setIsTestingConnection(true);
    setConnectionStatus(null);
    
    try {
      const result = await testApiConnection();
      setConnectionStatus(result);
      if (result.success) {
        toast.success("Conexión exitosa", {
          description: result.message,
        });
      } else {
        toast.error("Error de conexión", {
          description: result.message,
        });
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Error desconocido";
      setConnectionStatus({
        success: false,
        message: errorMessage,
      });
      toast.error("Error al probar conexión", {
        description: errorMessage,
      });
    } finally {
      setIsTestingConnection(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-neutral-50 to-neutral-100 p-4">
      <div className="w-full max-w-md">
        {/* Logo y título */}
        <div className="text-center mb-8">
          <div className="inline-flex h-16 w-16 rounded-2xl bg-gradient-to-br from-[#C9A664] to-[#D4B996] items-center justify-center mb-4 shadow-lg">
            <Moon className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-3xl mb-2">Tritote Nicaragua</h1>
          <p className="text-muted-foreground">
            Sistema de Gestión de Pedidos
          </p>
        </div>

        {/* Card de login */}
        <Card className="shadow-xl border-neutral-200">
          <CardHeader>
            <CardTitle>Iniciar Sesión</CardTitle>
            <CardDescription>
              Ingresa tus credenciales para acceder al sistema
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleLogin} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="email"
                    type="email"
                    placeholder="tu@email.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="pl-9"
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="password">Contraseña</Label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="password"
                    type={showPassword ? "text" : "password"}
                    placeholder="••••••••"
                    value={password}
                    onChange={(e) =>
                      setPassword(e.target.value)
                    }
                    className="pl-9 pr-9"
                    required
                  />
                  <button
                    type="button"
                    onClick={() =>
                      setShowPassword(!showPassword)
                    }
                    className="absolute right-3 top-1/2 transform -translate-y-1/2 text-muted-foreground hover:text-foreground"
                  >
                    {showPassword ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </button>
                </div>
              </div>

              <Button
                type="submit"
                className="w-full bg-[#C9A664] hover:bg-[#B8965A]"
                disabled={isLoading}
              >
                {isLoading
                  ? "Iniciando sesión..."
                  : "Iniciar Sesión"}
              </Button>
            </form>

            {/* Botón de prueba de conexión */}
            <div className="mt-4 pt-4 border-t">
              <Button
                type="button"
                variant="outline"
                className="w-full"
                onClick={handleTestConnection}
                disabled={isTestingConnection}
              >
                {isTestingConnection ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-current mr-2"></div>
                    Probando conexión...
                  </>
                ) : (
                  <>
                    {connectionStatus?.success ? (
                      <Wifi className="h-4 w-4 mr-2 text-green-500" />
                    ) : (
                      <WifiOff className="h-4 w-4 mr-2 text-red-500" />
                    )}
                    Probar Conexión API
                  </>
                )}
              </Button>
              {connectionStatus && (
                <p className={`text-xs mt-2 ${connectionStatus.success ? 'text-green-600' : 'text-red-600'}`}>
                  {connectionStatus.message}
                </p>
              )}
              <p className="text-xs text-muted-foreground mt-2">
                API: {API_CONFIG.baseURL}
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Credenciales de prueba */}
        <Card className="mt-4 bg-muted/50 border-dashed">
          <CardContent className="pt-6">
            <p className="text-sm font-medium mb-2">
              Usuarios de prueba:
            </p>
            <div className="space-y-2 text-xs text-muted-foreground">
              <div className="flex justify-between">
                <span>william.garcia@tritote.com.ni</span>
                <span>admin123</span>
              </div>
              <div className="flex justify-between">
                <span>andres.gonzalez@tritote.com.ni</span>
                <span>seller123</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}