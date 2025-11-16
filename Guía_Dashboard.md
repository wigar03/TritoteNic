## Paso 6 – Implementación del Dashboard (API)

Este documento describe, paso a paso, cómo implementar el endpoint del dashboard sin incluir código fuente literal.

---

### 1. Crear `DashboardDto.cs` en `SharedModels/Dto`

1. En el proyecto `SharedModels`, crea un archivo en la carpeta `Dto` llamado `DashboardDto.cs`.
2. Dentro de ese archivo define las clases de DTO necesarias (solo modelos, sin lógica):
   - Una clase raíz llamada `DashboardDto` que contenga:
     - Una propiedad para los KPI de ventas (por ejemplo, un objeto con ventas del día, de la semana y del mes, más los porcentajes de cambio).
     - Una propiedad para los KPI de pedidos (por ejemplo, total activos, pendientes y en proceso).
     - Una colección de alertas (para stock bajo y pedidos retrasados).
     - Una colección con las ventas de los últimos 7 días.
     - Una colección con los productos más vendidos del mes.
   - Una clase para los KPI de ventas del dashboard, con propiedades numéricas para los montos y porcentajes.
   - Una clase para los KPI de pedidos del dashboard, con propiedades para el total de pedidos activos, pendientes y en proceso.
   - Una clase para las alertas, con propiedades como tipo de alerta, mensaje y cantidad.
   - Una clase para representar las ventas diarias (día de la semana, fecha y total).
   - Una clase para representar los productos más vendidos (id del producto, nombre, cantidad vendida y total vendido).
3. Asegúrate de que el archivo use el mismo espacio de nombres (`namespace`) que el resto de los DTO de `SharedModels`.

---

### 2. Crear `DashboardController.cs` en `TritoteNic/Controllers`

1. En el proyecto de la API (`TritoteNic`), crea un archivo en la carpeta `Controllers` llamado `DashboardController.cs`.
2. Declara la clase del controlador del dashboard, haciendo que herede de la clase base de controladores de API.
3. Agrega los atributos básicos del controlador:
   - Ruta base del tipo `api/[controller]`.
   - Atributo para marcarlo como controlador de API.
   - Atributo de autorización para que el endpoint requiera un token JWT válido.
4. Inyecta en el constructor:
   - El contexto de base de datos de la aplicación.
   - Un logger para registrar la actividad y errores.
5. Define una acción pública que responda a solicitudes GET en la ruta base del controlador y que devuelva un objeto del tipo `DashboardDto` dentro de una respuesta de API.

---

### 3. Lógica del método `GET /api/dashboard`

En el cuerpo del método de acción del dashboard, realiza los siguientes pasos de forma asíncrona, utilizando el contexto de datos:

#### 3.1. Calcular fechas base

1. Obtén la fecha actual (solo la parte de la fecha, sin hora).
2. Calcula el inicio de la semana actual a partir de la fecha actual, considerando el día de la semana.
3. Calcula el primer día del mes actual.
4. Calcula el primer día y el último día del mes anterior a partir de la fecha del primer día del mes actual.
5. Crea una instancia del DTO principal del dashboard que vas a ir rellenando con los resultados.

#### 3.2. Ventas del día, semana y mes con comparación

1. Usa la tabla de pedidos para calcular:
   - La suma del total de los pedidos del día actual.
   - La suma del total de los pedidos del día anterior (para el porcentaje de cambio diario).
   - La suma del total de los pedidos desde el inicio de la semana actual (para ventas de la semana).
   - La suma del total de los pedidos de la semana anterior (para el porcentaje de cambio semanal).
   - La suma del total de los pedidos desde el inicio del mes actual (para ventas mensuales).
   - La suma del total de los pedidos del mes anterior (para el porcentaje de cambio mensual).
2. Asigna estos totales a la sección de KPI de ventas del DTO.
3. Calcula los porcentajes de cambio comparando el valor actual con el valor anterior, teniendo cuidado de evitar divisiones por cero cuando el valor anterior sea cero (en esos casos puedes dejar el porcentaje en cero o nulo).

#### 3.3. Pedidos activos, pendientes y en proceso

1. Decide qué nombres de estado se consideran “activos” (por ejemplo, pendientes y en proceso).
2. Consulta la tabla de pedidos incluyendo el estado del pedido.
3. Filtra los pedidos que estén en alguno de los estados considerados activos.
4. Calcula:
   - El número total de pedidos activos.
   - El número de pedidos en estado pendiente.
   - El número de pedidos en estado en proceso.
5. Asigna estos valores a la sección de KPI de pedidos del DTO del dashboard.

#### 3.4. Alertas: stock bajo y pedidos retrasados

1. Para **stock bajo**:
   - Consulta la tabla de productos para contar cuántos tienen un stock por debajo de un umbral (por ejemplo, menor a 10) y están activos.
   - Si la cantidad es mayor que cero, agrega una alerta a la lista de alertas del dashboard, indicando el tipo de alerta (por ejemplo, “stock”), un mensaje descriptivo y la cantidad de productos afectados.
2. Para **pedidos retrasados**:
   - Consulta la tabla de pedidos (incluyendo el estado del pedido) para contar cuántos están en el estado que consideres como “retrasado”.
   - Si la cantidad es mayor que cero, agrega otra alerta a la lista de alertas con el tipo adecuado (por ejemplo, “retraso”) y un mensaje descriptivo.

#### 3.5. Ventas de los últimos 7 días

1. Recorre los últimos 7 días empezando desde hace 6 días hasta hoy para devolver los datos en orden cronológico.
2. Para cada día del rango:
   - Calcula la suma de los totales de los pedidos cuya fecha coincide con ese día.
   - Construye un objeto de ventas diarias con el nombre del día (por ejemplo, abreviatura en español), la fecha y el total de ventas.
3. Agrega cada objeto de ventas diarias a la colección correspondiente del DTO del dashboard.

#### 3.6. Top 5 productos más vendidos del mes

1. Trabaja sobre la tabla de detalles de pedidos, incluyendo la relación con el pedido y el producto.
2. Filtra los detalles cuyos pedidos pertenezcan al mes actual (usando la fecha del pedido y el inicio de mes calculado anteriormente).
3. Agrupa los detalles por producto (id y nombre).
4. Para cada grupo de producto:
   - Calcula la suma de las cantidades vendidas.
   - Calcula la suma del subtotal vendido en ese periodo.
5. Ordena los productos resultantes de mayor a menor según la cantidad vendida.
6. Toma los primeros cinco registros y conviértelos en objetos de tipo producto vendido para el DTO.
7. Asigna esta lista a la colección de productos más vendidos dentro del DTO del dashboard.

#### 3.7. Manejo de errores y respuesta

1. Envuélvelo todo en un bloque de manejo de excepciones:
   - Registra mensajes informativos al inicio de la acción (por ejemplo, “Obteniendo datos del dashboard”).
   - Registra errores en caso de que ocurra alguna excepción.
2. Si todo sale bien, devuelve el DTO del dashboard con una respuesta de éxito.
3. Si ocurre un error inesperado, devuelve un código de error de servidor junto con un mensaje genérico.

---

### 4. Proteger el controlador con JWT (`[Authorize]`)

1. Asegúrate de que la autenticación JWT ya esté configurada en `Program.cs` (ya la agregaste en pasos anteriores).
2. Agrega el atributo de autorización en la declaración de la clase del `DashboardController` para que exija un token JWT válido.
3. Opcionalmente, si quieres restringir el dashboard solo a ciertos roles (por ejemplo, administrador o analista), añade la configuración de roles en el atributo de autorización.

---

### 5. Pruebas del endpoint `/api/dashboard`

1. Inicia la API y abre Swagger o tu cliente de pruebas (Postman, por ejemplo).
2. Primero obtén un token JWT haciendo una solicitud al endpoint de login (el que devuelve el token para un usuario válido).
3. Configura el token en tu herramienta de pruebas:
   - En Swagger, utiliza el botón de autorización y coloca el token con el esquema adecuado.
   - En Postman o similar, agrega un encabezado de autorización con el tipo Bearer y el token.
4. Haz una solicitud GET al endpoint del dashboard (`/api/dashboard`):
   - Verifica que la respuesta sea exitosa (código 200).
   - Revisa que el JSON devuelto contenga las secciones de KPI, alertas, ventas de la semana y top de productos.
5. Crea algunos pedidos de prueba en distintos días y con distintos productos y estados para observar cómo cambian los datos del dashboard.
6. Si observas valores inesperados (por ejemplo, porcentajes incorrectos o totales en cero), revisa las consultas y rangos de fecha utilizados en el controlador.

---

Con estos pasos, tendrás un endpoint de dashboard que entrega la información de alto nivel necesaria para tu interfaz WPF, respetando la autenticación JWT y listo para ser consumido por tu frontend.


