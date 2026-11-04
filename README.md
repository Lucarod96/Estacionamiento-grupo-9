# Sistema de Gestión de Estacionamiento – Grupo 9

Aplicación ASP.NET Core MVC (.NET 10) para registrar el ingreso y egreso de vehículos, asignar plazas y cobrar la estadía. Incluye login con roles, una API REST protegida con JWT y un CRUD con Vue.js.

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
- **Operador**: registra ingresos, consulta y cobra estadías, y gestiona clientes.

Pagos y estadías no se pueden eliminar: se conservan como historial (la API responde `405`). Una estadía solo se cierra cobrándola.

## API REST (JWT)

Iniciar sesión (acepta usuario o email):
```
curl -X POST http://localhost:5022/api/auth/login -H "Content-Type: application/json" -d "{\"email\":\"admin\",\"password\":\"<CLAVE_ADMIN>\"}"
```
Devuelve un `token`. Se envía en cada pedido:
```
curl http://localhost:5022/api/cliente -H "Authorization: Bearer <TOKEN>"
```

Recursos: `api/cliente`, `api/plaza`, `api/tipodeplaza`, `api/estadia`, `api/pago` (cobro en `POST api/pago/cobrar`) y `api/staff`.

Códigos de respuesta principales:
- `200`/`201`: éxito.
- `400`: datos inválidos.
- `401`: token ausente, vencido o alterado.
- `403`: el rol no tiene permiso.
- `404`: no existe el recurso.
- `405`: operación no permitida (eliminar pagos o estadías).
- `409`: conflicto de estado (cobro repetido, plaza ocupada, patente ya adentro).
- `429`: demasiados intentos fallidos de login (bloqueo de 5 minutos).

Hay una colección de Postman en `Estacionamiento_API.postman_collection.json`.

## Archivos subidos
Los avatares (`wwwroot/uploads/avatars`) y los comprobantes de pago en PDF/JPG/PNG (`wwwroot/uploads/comprobantes`) se guardan en disco y no se versionan.