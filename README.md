# Fleet Monitor

Sistema de monitoreo IoT para flotas vehiculares. Permite visualizar ubicación y telemetría en un mapa, consultar históricos de sensores, recibir alertas predictivas de combustible bajo y consultar datos cacheados cuando la API no está disponible.

Monorepo con backend (ASP.NET Core 8, PostgreSQL, SignalR) y frontend (React 19, TypeScript, Vite).

> Las decisiones de stack y trade-offs técnicos se documentan en [DESIGN.md](./DESIGN.md).

---

## Tabla de contenidos

- [Arquitectura](#arquitectura)
- [Requisitos](#requisitos)
- [Instalación local](#instalación-local)
- [Variables de entorno](#variables-de-entorno)
- [API REST](#api-rest)
- [Frontend](#frontend)
- [Autenticación y roles](#autenticación-y-roles)
- [Alertas y SignalR](#alertas-y-signalr)
- [Datos de prueba](#datos-de-prueba)
- [Tests](#tests)
- [Solución de problemas](#solución-de-problemas)

---

## Arquitectura

```
React (navegador)  ──HTTP REST + consultas cada 15 s──►  ASP.NET Core 8 API
                   ──WebSocket (alertas)──────────────►  /hubs/alerts
                   ──localStorage (caché offline)
                                        │
                                        ▼ EF Core
                              PostgreSQL (Docker :5433)
```

| Capa | Responsabilidad |
|------|-----------------|
| Frontend | Login, dashboard, mapa, gráficos, panel de alertas, caché offline |
| API REST | Auth JWT, lectura de flota, ingesta de sensores, alertas |
| SignalR | Notificación inmediata de alertas nuevas a usuarios Admin |
| PostgreSQL | Usuarios, vehículos, lecturas históricas, alertas |

---

## Requisitos

| Herramienta | Versión |
|-------------|---------|
| Docker Desktop | Reciente |
| .NET SDK | 8.0 |
| Node.js | 20+ |
| npm | 10+ |

Puertos libres: `5433` (Postgres), `5209` (API), `5173` (frontend).

---

## Instalación local

### 1. Variables de entorno

Desde la raíz del monorepo:

```bash
cp .env.example .env
```

El archivo `.env` en la raíz alimenta la API (credenciales de base de datos y JWT). Los valores por defecto coinciden con `docker-compose.yml`.

En el frontend:

```bash
cd frontend
cp .env.example .env
npm install
```

### 2. Base de datos

```bash
docker compose up -d
```

PostgreSQL queda en `localhost:5433` (usuario `fleet`, contraseña `fleet123`, base `fleetmonitor`). El contenedor debe aparecer como `healthy` en `docker compose ps`.

### 3. API

```bash
cd backend/FleetMonitor.Api
dotnet run
```

Al arrancar, la API carga el `.env`, aplica migraciones EF Core y ejecuta el seed si la base está vacía.

| Recurso | URL |
|---------|-----|
| Swagger | http://localhost:5209/swagger |
| Hub SignalR | ws://localhost:5209/hubs/alerts |

### 4. Frontend

En otra terminal:

```bash
cd frontend
npm run dev
```

La aplicación queda en http://localhost:5173.

### 5. Orden de arranque

```
1. docker compose up -d
2. dotnet run          (backend)
3. npm run dev         (frontend)
```

Si el frontend arranca antes que la API, pueden aparecer errores de red hasta que el backend esté listo; basta recargar la página.

### 6. Verificación del flujo

1. Se inicia sesión en http://localhost:5173 como Admin (`admin@fleetmonitor.com` / `Admin123!`).
2. El dashboard muestra mapa, tabla de flota y panel de alertas con badge **Tiempo real**.
3. En Swagger, con token Admin: `GET /api/devices` → se copia un `id` → `POST /api/sensors/ingest` con `"fuel": 5`.
4. En el dashboard (sin recargar) aparece un toast y la alerta nueva en el panel.

### Comandos útiles

| Acción | Comando |
|--------|---------|
| Parar Postgres | `docker compose down` |
| Resetear base de datos | `docker compose down -v && docker compose up -d` |
| Tests backend | `cd backend && dotnet test` |
| Build frontend | `cd frontend && npm run build` |
| Nueva migración EF | `cd backend/FleetMonitor.Api && dotnet ef migrations add Nombre` |

---

## Variables de entorno

### Backend (`.env` en la raíz)

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `DB_HOST_DEV` | Host PostgreSQL | `localhost` |
| `DB_PORT_DEV` | Puerto | `5433` |
| `DB_NAME_DEV` | Base de datos | `fleetmonitor` |
| `DB_USER_DEV` / `DB_PASSWORD_DEV` | Credenciales | `fleet` / `fleet123` |
| `JWT_SECRET_KEY` | Clave HMAC (mín. 32 caracteres) | ver `.env.example` |
| `JWT_ISSUER` / `JWT_AUDIENCE` | Emisor y audiencia | `fleet-monitor` |
| `JWT_EXPIRATION_MINUTES` | Expiración del token | `60` |

En Development se usan las variables `*_DEV`. En producción, `DB_HOST`, `DB_PORT`, etc.

### Frontend (`frontend/.env`)

| Variable | Default |
|----------|---------|
| `VITE_API_BASE_URL` | `http://localhost:5209/api` |
| `VITE_HUB_URL` | `http://localhost:5209/hubs/alerts` |
| `VITE_MAP_STYLE_URL` | OpenFreeMap Liberty |

Si cambia el puerto de la API, deben actualizarse las URLs del frontend.

---

## API REST

Todos los endpoints (excepto login y Swagger) requieren:

```
Authorization: Bearer <token>
```

| Método | Ruta | Rol | Descripción |
|--------|------|-----|-------------|
| `POST` | `/api/auth/login` | Público | Devuelve JWT, email, rol y expiración |
| `GET` | `/api/devices` | Admin / Viewer | Lista de vehículos con última telemetría |
| `GET` | `/api/devices/{id}/readings` | Admin / Viewer | Histórico (`?from=` y `?to=` opcionales) |
| `GET` | `/api/alerts` | Admin | Alertas de combustible |
| `POST` | `/api/sensors/ingest` | Admin | Ingesta de lectura de sensor |

**Login:**

```json
{ "email": "admin@fleetmonitor.com", "password": "Admin123!" }
```

**Ingesta** (simula llegada de datos IoT):

```json
{
  "deviceId": "<guid>",
  "lat": 4.65,
  "lng": -74.08,
  "fuel": 5.0,
  "temperature": 22.0,
  "speed": 45.0
}
```

La ingesta actualiza el vehículo, guarda la lectura, evalúa alerta de combustible y, si corresponde, notifica por SignalR (`NewAlert`). El `deviceId` real se obtiene en `GET /api/devices` (Admin ve el Guid completo).

Para SignalR, el token se envía como `?access_token=<jwt>` en la URL del WebSocket. Rutas públicas: `/api/auth/login`, `/swagger`.

Documentación interactiva: http://localhost:5209/swagger

---

## Frontend

| Ruta | Acceso | Descripción |
|------|--------|-------------|
| `/login` | Público | Formulario de login |
| `/` | Autenticado | Dashboard (mapa, tabla, gráficos, alertas) |

| Dato | Mecanismo | Frecuencia |
|------|-----------|------------|
| Vehículos (mapa + tabla) | HTTP | Cada 15 s |
| Lecturas (gráficos) | HTTP | Al seleccionar vehículo |
| Alertas históricas | HTTP | Al cargar el dashboard |
| Alertas nuevas | SignalR | Tiempo real |

Si la API no responde, el dashboard muestra la última copia guardada en `localStorage` (vehículos y lecturas) con un banner de modo offline.

Scripts: `npm run dev`, `npm run build`, `npm run preview`, `npm run lint`.

---

## Autenticación y roles

| Capacidad | Admin | Viewer |
|-----------|:-----:|:------:|
| Mapa, tabla y gráficos | ✅ | ✅ |
| ID de dispositivo | Guid completo | ID enmascarado |
| Panel de alertas y SignalR | ✅ | ❌ |
| Ingesta y listado de alertas | ✅ | ❌ |

La sesión persiste en `localStorage` (`fleetmonitor:auth`) y se valida la expiración al recargar.

---

## Alertas y SignalR

**Fórmula predictiva:**

```
minutos_restantes = (fuelLevel / fuelConsumptionRate) × 60
```

| Condición | Resultado |
|-----------|-----------|
| Consumo ≤ 0 o minutos ≥ 60 | No se crea alerta |
| Alerta `LowFuel` del mismo vehículo en los últimos 15 min | No se duplica |
| Minutos < 60 y sin alerta reciente | Alerta + notificación SignalR |

El hub `/hubs/alerts` acepta solo usuarios Admin. Al conectar, el cliente entra al grupo `"admins"` y recibe el evento `NewAlert`.

Ejemplo (seed): Camión Sur con 6 L y 9 L/h → `(6/9)×60 = 40 min` → genera alerta en la primera ingesta con esos valores.

---

## Datos de prueba

El seed crea usuarios, cinco vehículos en Bogotá y 25 lecturas históricas por vehículo (últimas 24 h).

| Rol | Email | Contraseña |
|-----|-------|------------|
| Admin | admin@fleetmonitor.com | Admin123! |
| Viewer | viewer@fleetmonitor.com | Viewer123! |

| Vehículo | Combustible | Rate (L/h) |
|----------|-------------|------------|
| Camión Norte | 45 L | 8.5 |
| Camión Sur | 6 L | 9.0 |
| Van Centro | 28 L | 6.0 |
| Camión Occidente | 15 L | 7.5 |
| Van Oriente | 32 L | 5.5 |

---

## Tests

```bash
cd backend
dotnet test
```

| Suite | Cobertura |
|-------|-----------|
| `JwtServiceTests` | Generación y validación de JWT |
| `FuelAlertServiceTests` | Umbrales, deduplicación y creación de alertas |

Los tests usan EF InMemory; no requieren PostgreSQL en ejecución.

---

## Solución de problemas

**PostgreSQL no conecta:** verificar Docker (`docker compose ps`), que `DB_PORT_DEV=5433` coincida con `docker-compose.yml`, y revisar logs (`docker compose logs postgres`).

**Token en Swagger:** login en `POST /api/auth/login`, copiar solo el token (sin `Bearer`) y pegarlo en **Authorize**.

**Frontend sin datos:** confirmar que la API corre en el puerto de `frontend/.env`, revisar consola (CORS o 401) y renovar sesión si el token expiró.

**Alertas sin tiempo real:** requiere rol Admin, badge **Tiempo real** activo, ingesta que genere alerta nueva (no se reenvían históricas) y reinicio de la API tras cambios en SignalR. La deduplicación de 15 min bloquea repetir alerta en el mismo vehículo.

**Mapa sin calles:** comprobar internet (tiles OpenFreeMap) o definir `VITE_MAP_STYLE_URL`.

**SignalR “stopped during negotiation” en dev:** comportamiento habitual con React Strict Mode; si aparece `WebSocket connected`, la conexión está activa.

**Reset completo de datos:**

```bash
docker compose down -v
docker compose up -d
cd backend/FleetMonitor.Api && dotnet run
```

---

## Licencia

Proyecto de prueba técnica — uso educativo y demostrativo.
