# Fya Créditos — Backend (.NET 8 + EF Core + PostgreSQL)

API REST para registrar y consultar créditos, construida para el examen
técnico `GH-CTP-I+D-ET-01` de Fya Social Capital. Al registrar un crédito
se encola automáticamente el envío de un correo (asíncrono, en segundo
plano) a `fyasocialcapital@gmail.com`.

## Stack

- **.NET 8** (ASP.NET Core Web API, Controllers)
- **Entity Framework Core** + **Npgsql** sobre **PostgreSQL**
- **MailKit** para el envío de correo por SMTP
- **JWT Bearer** para proteger el registro de créditos
- **Rate limiting** nativo de ASP.NET Core
- **Swashbuckle** (Swagger / OpenAPI)

## Estructura

```
src/FyaCreditos.Api/
  Controllers/     AuthController, CreditosController
  Models/          Credito, NotificacionCorreo
  Dtos/            DTOs de entrada/salida y de query string
  Data/            AppDbContext (mapeo "database-first")
  Services/        JWT, envío de correo (SmtpEmailSender) y su cola
                    en segundo plano (EmailOutboxBackgroundService)
  Options/         Clases de configuración (Smtp, Jwt, AuthCredentials)
db/
  init.sql         Esquema de la base de datos (tablas + índices)
  seed.sql         Los 10 créditos de ejemplo del anexo del examen
tests/FyaCreditos.Api.Tests/   Pruebas unitarias de validación (xUnit)
tools/generate_password_hash.py  Genera el hash BCrypt del usuario admin
.github/workflows/backend-ci.yml Build + tests + verificación de db/*.sql en CI
```

## Cómo correrlo

### Opción A: Docker Compose (recomendada)

```bash
cp .env.example .env
# Edita el .env: genera un JWT_SECRET propio (32+ caracteres aleatorios)
docker compose up --build
```

- API disponible en `http://localhost:5000`
- Swagger UI en `http://localhost:5000/swagger`
- Postgres expuesto en `localhost:5432` (usuario `fya`, ver `.env`)

`docker-compose.yml` monta `db/init.sql` y `db/seed.sql` en
`/docker-entrypoint-initdb.d/`, así que la primera vez que se crea el
volumen de Postgres, el esquema y los 10 créditos de ejemplo quedan
cargados automáticamente.

### Opción B: local con el SDK de .NET instalado

```bash
# 1. Levanta solo Postgres (o usa una instancia propia) y aplica los scripts
psql -h localhost -U fya -d fya_creditos -f db/init.sql
psql -h localhost -U fya -d fya_creditos -f db/seed.sql

# 2. Configura appsettings.Development.json (cópialo desde el .example) o
#    exporta las variables de entorno equivalentes (ver abajo)
cp src/FyaCreditos.Api/appsettings.Development.json.example \
   src/FyaCreditos.Api/appsettings.Development.json
# edítalo con tus datos de SMTP, tu JWT secret y el hash de tu contraseña

# 3. Corre la API
dotnet run --project src/FyaCreditos.Api
```

## Variables de entorno / configuración

Todas se pueden setear como variables de entorno usando `__` para anidar
(ej: `Smtp__Host`), o en `appsettings.Development.json` local (no se sube
al repo).

| Variable                          | Descripción                                                   |
|-----------------------------------|-----------------------------------------------------------------|
| `ConnectionStrings__Default`      | Cadena de conexión a PostgreSQL                                |
| `Smtp__Host` / `Port` / `User` / `Password` / `FromAddress` | Datos del SMTP que enviará el correo |
| `Smtp__NotificationRecipient`     | Destinatario de la notificación (por defecto `fyasocialcapital@gmail.com`) |
| `Jwt__Secret`                     | Clave simétrica para firmar los JWT (mínimo 32 caracteres)     |
| `Cors__AllowedOrigins__0..n`      | Orígenes permitidos para el frontend                            |

**Nunca se comitean credenciales reales** — `.env`, `appsettings.Development.json`
y `appsettings.Local.json` están en `.gitignore`.

## Autenticación y Registro de Usuarios

El sistema utiliza autenticación basada en JWT (JSON Web Tokens). La validación se realiza contra una tabla de usuarios almacenada en la base de datos PostgreSQL. 

Para utilizar la API o acceder mediante el cliente web por primera vez, es necesario registrar un usuario inicial. Esto puede realizarse a través de:
1. El endpoint `POST /api/auth/register`
2. La opción "Registrarme" provista en la interfaz gráfica del frontend.

No se requieren credenciales administrativas pre-configuradas ni scripts adicionales para generar contraseñas.

## Endpoints principales

Documentación interactiva completa en Swagger (`/swagger`). Resumen:

- `POST /api/auth/login` — devuelve un JWT (`{ username, password }` → `{ token, expiresAtUtc }`).
- `POST /api/creditos` *(requiere JWT)* — registra un crédito y encola su notificación por correo.
- `GET /api/creditos?nombre=&cedula=&comercial=&sortBy=fecha|valor&order=asc|desc&page=&pageSize=` *(requiere JWT)* — lista paginada con filtros y orden.
- `GET /api/creditos/{id}` *(requiere JWT)* — un crédito puntual.
- `GET /api/health` — verificación simple de que la API está viva.

## Envío de correo (cómo funciona el "en segundo plano")

1. `POST /api/creditos` guarda el crédito **y** una fila en
   `notificaciones_correo` (estado `PENDIENTE`) en la misma transacción.
2. `EmailOutboxBackgroundService` corre cada 5 segundos, toma las
   notificaciones pendientes, envía el correo por SMTP (MailKit) y marca
   el resultado (`ENVIADO` o reintenta hasta 5 veces antes de marcar `ERROR`).
3. Así, la petición HTTP de registro nunca espera al SMTP, y si la API se
   reinicia justo después de un registro, la notificación pendiente no se
   pierde (queda en la base, no en memoria).

## Seguridad implementada

- JWT Bearer para proteger el registro y la consulta de créditos.
- Rate limiting: 5 intentos/min en `/api/auth/login`, 30 registros/min en `POST /api/creditos`.
- Validación de entrada con DataAnnotations en el DTO (frontend valida también, ver el repo del frontend).
- Todas las consultas van parametrizadas vía EF Core (sin concatenar SQL), lo que previene inyección por diseño.
- CORS restringido a los orígenes configurados.

## Pruebas

```bash
dotnet test tests/FyaCreditos.Api.Tests
```

Cubren las reglas de validación de `CreditoCreateDto` (valores negativos,
plazos inválidos, cédula con caracteres no permitidos) y la normalización
de paginación en `CreditoQueryParametersDto`.

## Entregable / checklist contra el examen

- [x] Formulario/registro con los 6 campos mínimos pedidos.
- [x] Envío de correo asíncrono con nombre, valor, comercial y fecha.
- [x] Consulta con filtros (nombre, cédula, comercial) y orden (fecha, valor).
- [x] OpenAPI (Swagger).
- [x] Base de datos con script de creación (`db/init.sql`) + semilla (`db/seed.sql`).
- [x] JWT, rate limiting, validación en backend, sanitización (EF Core parametrizado).
- [ ] Deploy — no incluido por defecto; `docker-compose.yml` deja todo listo para desplegar en cualquier proveedor que soporte contenedores.
