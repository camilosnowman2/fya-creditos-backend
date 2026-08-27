# AGENTS.md — fya-creditos-backend

## Descripción general

API REST para el registro y consulta de créditos financieros de **Fya Créditos**, desarrollada en **.NET 8** con **Entity Framework Core** y **PostgreSQL**. Incluye autenticación JWT, envío asíncrono de correos electrónicos (patrón outbox con MailKit), validación de entradas, rate limiting y documentación OpenAPI (Swagger).

---

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Runtime | .NET 8 |
| ORM | Entity Framework Core (Npgsql) |
| Base de datos | PostgreSQL 15 |
| Autenticación | JWT Bearer |
| Correos | MailKit sobre SMTP (Gmail / SendGrid / Mailpit) |
| Documentación | Swagger / OpenAPI (`/swagger`) |
| Contenedor | Docker + docker-compose |
| CI | GitHub Actions (`.github/workflows/`) |

---

## Arquitectura y estructura de carpetas

```
fya-creditos-backend/
├── Controllers/          # Endpoints REST (AuthController, CreditosController)
├── Dtos/                 # Objetos de entrada/salida (CreditoCreateDto, etc.)
├── Models/               # Entidades de EF Core (Credito, Usuario, NotificacionCorreo)
├── Services/             # Lógica de negocio (CreditoService, SmtpEmailSender, etc.)
├── Options/              # Clases de configuración fuertemente tipadas (JwtOptions, SmtpOptions)
├── Data/                 # DbContext y migraciones
├── db/
│   ├── init.sql          # Script DDL — crea tablas e índices
│   └── seed.sql          # Datos de ejemplo (10 créditos del anexo)

├── .env.example          # Variables de entorno requeridas
└── docker-compose.yml    # Levanta backend + PostgreSQL + Mailpit (dev)
```

---

## Cómo correr el proyecto

### Con Docker (recomendado)

```bash
cp .env.example .env
# Editar .env: completar JWT_SECRET, SMTP_* y los datos de la BD
docker compose up --build
```

- API disponible en: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Mailpit (interceptor de correos en dev): `http://localhost:8025`

### Sin Docker (local)

```bash
# Requiere PostgreSQL corriendo localmente
cp .env.example .env
# Configurar la cadena de conexión en .env
dotnet restore
dotnet ef database update   # aplica migraciones
dotnet run
```

---

## Variables de entorno

| Variable | Descripción |
|----------|-------------|
| `CONNECTION_STRING` | Cadena de conexión a PostgreSQL |
| `JWT_SECRET` | Secreto para firmar tokens JWT (mín. 32 chars) |
| `SMTP_HOST` | Host SMTP (e.g. `smtp.gmail.com`) |
| `SMTP_PORT` | Puerto SMTP (e.g. `587`) |
| `SMTP_USER` | Usuario SMTP |
| `SMTP_PASS` | Contraseña / app password SMTP |
| `SMTP_FROM` | Dirección del remitente |
| `SMTP_TO` | Dirección destino (p. ej. `fyasocialcapital@gmail.com`) |

> **Nota — autenticación**: El sistema usa registro/login contra una tabla `usuarios` en base de datos. El primer usuario se crea mediante `POST /api/auth/register` (o el botón "Registrarme" del frontend). **No existe un usuario admin precargado**; hay que crear uno antes de usar la app.

---

## Endpoints principales

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `POST` | `/api/auth/register` | No | Registra un nuevo usuario |
| `POST` | `/api/auth/login` | No | Autentica y devuelve un JWT |
| `POST` | `/api/creditos` | JWT | Registra un crédito nuevo y encola el correo |
| `GET` | `/api/creditos` | JWT | Lista créditos con filtros y ordenamiento |

Parámetros de consulta para `GET /api/creditos`:
- `nombre`, `cedula`, `comercial` — filtros de texto (case-insensitive)
- `sortBy=fecha|valor` — campo de ordenamiento
- `order=asc|desc` — dirección

---

## Cómo correr los tests

```bash
dotnet test
```

Los tests unitarios están en el proyecto `*.Tests/` dentro de la misma solución.

---

## Convenciones para agentes / asistentes de código

- **No modificar** `db/init.sql` ni `db/seed.sql` sin actualizar también las migraciones de EF Core (`Data/Migrations/`).
- **No commitear** secretos reales; usar siempre `.env` (está en `.gitignore`).
- **No eliminar** `Options/JwtOptions.cs` ni `Options/SmtpOptions.cs`; son las únicas clases de configuración activas.
- Respetar el patrón outbox: el servicio `EmailOutboxBackgroundService` es el único que envía correos; no llamar a `IEmailSender` directamente desde los controladores.
- Los filtros de búsqueda usan `ILike` (case-insensitive nativo de PostgreSQL via EF Core); mantener este patrón para nuevos filtros.
- El rate limiting está configurado en `Program.cs` con las políticas `login` (5/min) y `creditos` (30/min); ajustar los límites ahí si es necesario.

---

## CI / GitHub Actions

| Workflow | Cuándo corre | Qué hace |
|----------|-------------|---------|
| `build.yml` | Push / PR a `main` | Build + tests |

---

## Notas para el evaluador

- No hay credenciales ni API keys reales en el repositorio (verificado).
- El `docker-compose.yml` incluye **Mailpit** para interceptar los correos en entorno de desarrollo sin necesidad de cuenta SMTP real.
- Los 10 registros del anexo del examen están precargados en `db/seed.sql`.
