# Cursos Catolicos - Plataforma de Formacion en la Fe

Plataforma web de formacion catolica en linea construida con arquitectura cliente-servidor:

- `CursosIglesia`: Frontend en Blazor Server (.NET 9)
- `CursosIglesiaAPI`: API REST en ASP.NET Core (.NET 9)
- SQL Server con procedimientos almacenados (USP) para reglas de negocio y persistencia

## Estado Actual

- Sin mocks en el flujo principal de autenticacion, perfil y cursos.
- JWT real entre frontend y API.
- Patron MVVM en frontend con Inyeccion de Dependencias.
- Backend por capas (Controllers -> Services -> SQL Server USP).

## Tecnologias y Paquetes

### Base

- .NET SDK 9.0
- C# 13
- Blazor Server
- ASP.NET Core Web API
- SQL Server
- Dapper

### Paquetes principales (Frontend)

- `Blazored.LocalStorage` (manejo de token/perfil en navegador)
- `Microsoft.AspNetCore.Components.Authorization` (estado de autenticacion)

### Paquetes principales (API)

- `Microsoft.AspNetCore.Authentication.JwtBearer` (validacion JWT)
- `System.IdentityModel.Tokens.Jwt` (emision/lectura de tokens)
- `Dapper` (acceso a datos)
- `Microsoft.Data.SqlClient` (conexion SQL Server)
- `BCrypt.Net-Next` (hash de contrasenas)

## Arquitectura (MVVM + DI + USP)

## Flujo General

1. La vista Razor invoca acciones del ViewModel.
2. El ViewModel usa interfaces de servicio (DI).
3. El servicio cliente llama la API REST (HttpClient).
4. La API valida JWT y deriva a Services.
5. Services ejecutan USP via Dapper.
6. SQL Server responde y se devuelve DTO/modelo al frontend.

### Frontend (`CursosIglesia`)

- `Components/Pages`: UI por modulo (`Login`, `Registro`, `Perfil`, `Cursos`, etc.).
- `ViewModels`: logica de pantalla (estado, validaciones, orquestacion).
- `Services/Interfaces`: contratos.
- `Services/Implementations/ApiClients`: consumo HTTP hacia `CursosIglesiaAPI`.
- `Program.cs`: registro DI + HttpClient + auth state provider.

### API (`CursosIglesiaAPI`)

- `Controllers`: endpoints HTTP.
- `Services/Interfaces`: contratos de negocio.
- `Services/Implementations`: logica y llamadas a USP.
- `Models` + `Models/DTOs`: entidades y contratos de entrada/salida.
- `Program.cs`: DI, JWT, CORS, pipeline HTTP.

### Base de datos

- Procedimientos almacenados como fuente principal de operaciones.
- Ejemplo central: `usp_AutenticacionYUsuarios`.
- Script de referencia para ajustes: `update_sp_profile.sql`.

## Estructura de Solucion

```
iglesia/
|- iglesia.sln
|- CursosIglesia/
|  |- Components/
|  |- Models/
|  |- Services/
|  |- ViewModels/
|  |- wwwroot/
|  |- Program.cs
|- CursosIglesiaAPI/
|  |- Controllers/
|  |- Models/
|  |- Services/
|  |- Program.cs
|- update_sp_profile.sql
```

## Como Ejecutar (Dev)

### Prerrequisitos

- .NET 9 SDK
- SQL Server con la BD `CursosIglesia`
- Cadenas de conexion y JWT configurados en `CursosIglesiaAPI/appsettings.json`

### Pasos

```bash
dotnet restore
dotnet build iglesia.sln
dotnet run --project CursosIglesiaAPI
dotnet run --project CursosIglesia
```

### URLs comunes

- API: `http://localhost:5285`
- Frontend: revisar `CursosIglesia/Properties/launchSettings.json`

## Paso a Paso: Como se consume una API en este proyecto

Ejemplo: consumir perfil de usuario autenticado.

1. Vista Razor dispara accion (`Perfil.razor`).
2. ViewModel (`UserProfileViewModel`) llama `IUserService.GetProfileAsync()`.
3. Implementacion (`ApiUserService`) crea request HTTP a `api/user/profile`.
4. Servicio agrega header `Authorization: Bearer <token>` leyendo `authToken` de localStorage.
5. API recibe en `UsersController` con `[Authorize]`.
6. `UserService` ejecuta USP (`usp_AutenticacionYUsuarios`, accion `ObtenerPerfil`) via Dapper.
7. Se mapea respuesta a `UserProfile` y regresa al frontend.
8. ViewModel actualiza estado y la vista renderiza.

## Guia Pro: crear un nuevo modulo API desde cero

Ejemplo: modulo `Certificados`.

### 1) Disenar contrato funcional

- Definir casos de uso: crear, listar, obtener por id, eliminar.
- Definir autorizacion: publico, autenticado o rol especifico.

### 2) Crear modelos/DTOs

- API:
	- `CursosIglesiaAPI/Models/Certificate.cs`
	- `CursosIglesiaAPI/Models/DTOs/CertificateDTOs.cs`
- Frontend:
	- `CursosIglesia/Models/Certificate.cs` (si aplica)
	- `CursosIglesia/Models/DTOs/CertificateDTOs.cs`

### 3) Crear interfaz de servicio en API

- Archivo: `CursosIglesiaAPI/Services/Interfaces/ICertificateService.cs`
- Definir metodos async necesarios.

### 4) Implementar servicio en API

- Archivo: `CursosIglesiaAPI/Services/Implementations/CertificateService.cs`
- Usar Dapper + SqlConnection.
- Invocar USP con parametros claros (`@Accion`, ids, filtros).

### 5) Crear controlador en API

- Archivo: `CursosIglesiaAPI/Controllers/CertificatesController.cs`
- Exponer endpoints REST.
- Validar entrada y responder codigos HTTP correctos.

### 6) Registrar DI en API

- En `CursosIglesiaAPI/Program.cs`:
	- `builder.Services.AddScoped<ICertificateService, CertificateService>();`

### 7) Crear/actualizar USP en SQL Server

- Crear script versionado, por ejemplo:
	- `sql/usp_Certificados.sql`
- Patron recomendado:
	- `@Accion` para multiplexar operaciones.
	- Parametros de entrada tipados.
	- Parametros de salida (`@Exito`, `@MensajeError`) para operaciones de escritura.
	- `SET NOCOUNT ON`.

### 8) Consumir desde frontend

- Crear contrato frontend:
	- `CursosIglesia/Services/Interfaces/ICertificateService.cs`
- Crear cliente API:
	- `CursosIglesia/Services/Implementations/ApiClients/ApiCertificateService.cs`
- Registrar DI en `CursosIglesia/Program.cs` con `AddHttpClient`.

### 9) Integrar en MVVM

- Crear o extender ViewModel.
- Conectar Razor Page y bind de estado.
- Manejar loading/error/success para UX robusta.

### 10) Pruebas minimas recomendadas

- Probar endpoint con `.http` o Postman.
- Probar flujo real desde pantalla.
- Verificar autorizacion y manejo de errores.
- Verificar que no se rompa el contrato JSON esperado.

## Convenciones recomendadas

- Nombres de interfaces: `I<Modulo>Service`.
- Implementaciones API client: `Api<Modulo>Service`.
- DTO requests: `<Operacion>Request`.
- DTO responses: `<Modulo>Response` o `<Operacion>Response`.
- Endpoints REST en plural (`api/certificates`).
- Evitar SQL inline para operaciones de negocio, preferir USP.

## Git y Commit

Antes de commit:

```bash
git status --short
git add .
git commit -m "feat: actualiza arquitectura API + MVVM + JWT + documentacion"
```

Sugerencia: separar en commits pequenos por area:

1. `feat(api): ...`
2. `feat(frontend): ...`
3. `docs(readme): ...`
4. `chore(gitignore): ...`

## Notas de seguridad

- No subir secretos reales en `appsettings.*.json`.
- Mantener `Jwt:Key` fuera del repo en produccion (User Secrets, variables de entorno o vault).
- Las contrasenas se validan/almacenan con BCrypt, nunca texto plano.

## Licencia

Proyecto de uso privado.
