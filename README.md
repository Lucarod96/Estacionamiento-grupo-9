# Sistema de Gestión de Estacionamiento – Grupo 9

## Descripción general
Aplicación web ASP.NET Core MVC (.NET 10) para administrar un estacionamiento: registrar el ingreso de vehículos, asignarles una plaza, controlar que no haya dos vehículos en la misma plaza ni el mismo vehículo en dos plazas a la vez, y cobrar la estadía según la tarifa de cada tipo de plaza.

**Problema que resuelve:** reemplaza el registro en papel o en planillas. Antes era difícil saber qué plazas están libres, cuánto corresponde cobrar por cada vehículo y qué empleado registró cada movimiento. Ahora el ingreso, el cobro y el historial quedan en una base de datos, con usuarios que tienen distintos permisos (Administrador y Operador) y una API REST con JWT para integrar otros sistemas.

Diagrama del modelo: [docs/der.png](docs/der.png)

## Matriz de Evidencias de Requerimientos Mínimos

| Requerimiento mínimo | Estado | Enlace al código / controladores | Evidencia |
|---|---|---|---|
| 1. 6 tablas relacionadas, con relaciones 1:N | ✅ Cumplido | [Estadia.cs](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Models/Estadia.cs#L6) · [Plaza.cs](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Models/Plaza.cs#L6) · [Pago.cs](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Models/Pago.cs#L7) · [AppDbContext.cs](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Data/AppDbContext.cs#L10) · [SQL: tabla estadia con sus claves foráneas](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/SQL/estacionamiento.sql#L238) | [Diagrama de clases](docs/der.png). Staff, Cliente, TipoDePlaza, Plaza, Estadia y Pago; Estadia depende de Plaza, Cliente y Staff. |
| 2. Login, roles y avatar | ✅ Cumplido | [Login](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/StaffController.cs#L54) · [Perfil y avatar](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/StaffController.cs#L97) · [AvatarService.cs](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Services/AvatarService.cs#L34) · [Roles](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Models/Rol.cs#L4) · [Identity](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Program.cs#L30) · [Gestión de Staff solo Admin](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/StaffController.cs#L183) | [Perfil con avatar](docs/evidencias/02-perfil-avatar.png) · [Acceso denegado al Operador](docs/evidencias/02-rol-denegado.png) |
| 3. Manejo de archivos (extra al avatar) | ✅ Cumplido | [Cobrar con comprobante](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/PagoController.cs#L122) · [Guardado y validación del archivo](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/PagoController.cs#L355) · [Vista del formulario](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Views/Pago/Cobrar.cshtml#L42) | [Subida de comprobante](docs/evidencias/03-comprobante.png) |
| 4. ABM en Vue.js + AJAX | ✅ Cumplido | [abm-estacionamiento.js](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/wwwroot/js/abm-estacionamiento.js#L20) · [Vista Plaza](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Views/Plaza/Index.cshtml) · [Vista Tipo de plaza](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Views/TipoDePlaza/Index.cshtml) · [Listado JSON](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/PlazaController.cs#L38) · [Alta/modificación](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/PlazaController.cs#L76) · [Baja](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/PlazaController.cs#L173) | [ABM sin recargar la página](docs/evidencias/04-abm-vue.gif) |
| 5. Paginado por servidor | ✅ Cumplido | [Estadias](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Models/RepositorioEstadia.cs#L50) · [Clientes](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Models/RepositorioCliente.cs#L114) · [Pagos](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Models/RepositorioPago.cs#L185) · [Controlador de Estadia](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/EstadiaController.cs#L31) · [Paginado de la lista en Vue](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/wwwroot/js/abm-estacionamiento.js#L99) | [Inspector de red con pagina=2](docs/evidencias/05-paginado-network.png) |
| 6. Búsqueda asíncrona de entidades relacionadas | ✅ Cumplido | [Buscar cliente](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/EstadiaController.cs#L176) · [Buscar plaza libre](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/EstadiaController.cs#L195) · [estadia-create.js](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/wwwroot/js/estadia-create.js#L57) · [Buscar tipo de plaza (formulario de Plaza)](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/wwwroot/js/abm-estacionamiento.js#L196) | [Búsqueda en el ingreso](docs/evidencias/06-busqueda-ajax.gif) |
| 7. Web API REST + JWT | ✅ Cumplido | [Login y token](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/AuthController.cs#L30) · [Validación del JWT](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Program.cs#L57) · [Cobro por API](https://github.com/Lucarod96/Estacionamiento-grupo-9/blob/main/Controllers/PagoController.cs#L250) · [Colección de Postman](Estacionamiento_API.postman_collection.json) | [Postman con Bearer Token](docs/evidencias/07-postman-bearer.png) |

## Requisitos
- .NET SDK 10
- MySQL 8.x (por ejemplo con MySQL Workbench)

## Instalación desde cero

### 1. Base de datos
1. Abrí MySQL Workbench y conectate a tu servidor local.
2. Ejecutá el archivo `SQL/estacionamiento.sql` completo (File → Open SQL Script → botón del rayo). Crea la base `estacionamiento`, las tablas y los datos de prueba.

### 2. Secretos (no están en el repositorio)
Desde la carpeta del proyecto (donde está el `.csproj`):

```
dotnet user-secrets set "ConnectionStrings:MySql" "Server=localhost;Port=3306;Database=estacionamiento;User=root;Password=<TU_CLAVE_DE_MYSQL>;"
dotnet user-secrets set "Jwt:Key" "<una clave larga de al menos 32 caracteres>"
```

Si da error de que no existe un `UserSecretsId`, ejecutá antes `dotnet user-secrets init`.

Opcionales:
- `Jwt:ExpireMinutes`: duración del token en minutos (por defecto 60).
- `SeedAdmin:Password`: solo hace falta si la tabla `staff` está vacía; con el SQL importado no es necesario.

### 3. Ejecutar
```
dotnet run
```
La aplicación queda en http://localhost:5022. Hay que ejecutarla en modo Development (el perfil por defecto ya lo hace), porque los secretos solo se leen en ese modo. Si falta alguno, la app se detiene al arrancar con un mensaje que indica cuál.

## Usuarios de prueba

| Usuario | Contraseña | Rol |
|---|---|---|
| admin | `Admin123` | Admin |
| operador | `Operador123` | Operador |

Cualquier usuario puede cambiar su contraseña y su avatar desde **Mi perfil**.

## Roles y permisos
- **Admin**: todo lo del Operador, más gestión de Staff (alta, edición, baja y roles), Plazas y Tipos de plaza.
- **Operador**: registra ingresos, consulta y cobra estadías, y da de alta y edita clientes (eliminarlos es solo del Admin).

Pagos y estadías no se pueden eliminar: se conservan como historial (la API responde `405`). Una estadía solo se cierra cobrándola.

## API REST (JWT)

La API usa tokens JWT. El flujo es siempre el mismo: **1) iniciar sesión y obtener un token, 2) enviarlo en cada pedido**. Sin token, o con un token vencido o alterado, la API responde `401`.

Los ejemplos usan `curl.exe` en PowerShell (viene con Windows 10 y 11). La app tiene que estar corriendo.

> **Importante:** para evitar problemas con las comillas, los cuerpos JSON se guardan en archivos de texto (por ejemplo en el Escritorio, **fuera de la carpeta del proyecto**) y se envían con `-d "@archivo.json"`.

### 1. Iniciar sesión y guardar el token
Creá `login.json`:
```json
{"email":"admin","password":"<CLAVE_ADMIN>"}
```
Luego, desde la carpeta donde está el archivo:
```powershell
$token = (curl.exe -s -X POST http://localhost:5022/api/auth/login -H "Content-Type: application/json" -d "@login.json" | ConvertFrom-Json).token
```
Comprobá que se guardó:
```powershell
$token
```
Tiene que mostrar un texto largo que empieza con `eyJ`. Si queda vacío, el usuario o la contraseña son incorrectos (probá sin `| ConvertFrom-Json` para ver la respuesta).
El token dura 60 minutos; si vence hay que repetir este paso. Cuatro intentos fallidos seguidos están permitidos; al quinto el usuario se bloquea 5 minutos (respuesta `429`).

### 2. Consultar datos (GET)
```powershell
curl.exe -i http://localhost:5022/api/cliente -H "Authorization: Bearer $token"
```
Esperado: `200 OK` y la lista en JSON. Para paginar: `api/cliente?pagina=2`. Para buscar: `api/cliente?busqueda=TEST`.

### 3. Probar que sin token no entra
```powershell
curl.exe -i http://localhost:5022/api/cliente
```
Esperado: `401 Unauthorized`.

### 4. Crear un cliente (POST)
Creá `cliente.json`:
```json
{"patente":"ABC123","nombre":"Ana","apellido":"Perez","telefono":"2664111111"}
```
```powershell
curl.exe -i -X POST http://localhost:5022/api/cliente -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d "@cliente.json"
```
Esperado: `201 Created`. Si lo repetís, la API rechaza la patente duplicada con `400`.

### 5. Registrar un ingreso y cobrarlo
Creá `ingreso.json` con una patente registrada y una **plaza libre**:
```json
{"patente":"ABC123","id_plaza":1}
```
```powershell
curl.exe -i -X POST http://localhost:5022/api/estadia -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d "@ingreso.json"
```
Esperado: `201 Created`. En la respuesta anotá el número del campo `id_Estadia` (en el ejemplo, `7`). Si repetís el mismo ingreso, responde `409` porque la plaza y la patente ya están ocupadas.

Cobro (reemplazá `7` por tu número):
```powershell
curl.exe -i -X POST http://localhost:5022/api/pago/cobrar -H "Authorization: Bearer $token" -F "idEstadia=7"
```
Esperado: `201 Created`, con el monto calculado (horas × tarifa, mínimo una hora). Si lo repetís: `409`, "ya fue cobrada". Con un número de estadía que no existe: `404`.

### 6. Probar los permisos por rol
Iniciá sesión como operador (creá `login-operador.json` con `{"email":"operador","password":"<CLAVE_OPERADOR>"}`):
```powershell
$tokenOp = (curl.exe -s -X POST http://localhost:5022/api/auth/login -H "Content-Type: application/json" -d "@login-operador.json" | ConvertFrom-Json).token
```
Listar el staff es solo del Admin:
```powershell
curl.exe -i http://localhost:5022/api/staff -H "Authorization: Bearer $tokenOp"
curl.exe -i http://localhost:5022/api/staff -H "Authorization: Bearer $token"
```
Esperado: la primera da `403 Forbidden` (operador) y la segunda `200 OK` (admin).

### 7. Operaciones no permitidas
Los pagos y las estadías no se pueden eliminar:
```powershell
curl.exe -i -X DELETE http://localhost:5022/api/pago/1 -H "Authorization: Bearer $token"
```
Esperado: `405 Method Not Allowed`, y el pago sigue existiendo.

### Resumen de códigos
| Código | Significado |
|---|---|
| 200 / 201 | Éxito / creado |
| 400 | Datos inválidos |
| 401 | Token ausente, vencido o alterado, o credenciales inválidas |
| 403 | El rol no tiene permiso |
| 404 | No existe el recurso |
| 405 | Operación no permitida (eliminar pagos o estadías) |
| 409 | Conflicto de estado (cobro repetido, plaza ocupada, patente ya adentro) |
| 429 | Demasiados intentos fallidos de login |

Recursos: `api/cliente`, `api/plaza`, `api/tipodeplaza`, `api/estadia`, `api/pago` y `api/staff`. También hay una colección de Postman en `Estacionamiento_API.postman_collection.json`.