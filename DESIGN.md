# Fleet Monitor — Diseño técnico

Documento de elección de stack y trade-offs técnicos. El detalle operativo está en [README.md](./README.md).

---

## Arquitectura (resumen)

```
React (navegador)  ──peticiones HTTP cada 15 s──►  API ASP.NET Core 8  ──►  PostgreSQL
                   ──WebSocket (alertas)────────►  /hubs/alerts
                   ──localStorage (datos guardados si no hay red)
```

El proyecto se organiza como monorepo con backend y frontend. Solo PostgreSQL se ejecuta en Docker; en desarrollo, la API y la aplicación corren en el host.

---

## Backend

### ASP.NET Core 8 + PostgreSQL + EF Core

El backend se apoya en .NET por su encaje natural con APIs REST, migraciones y un modelo relacional claro (usuarios, vehículos, lecturas y alertas). PostgreSQL en Docker evita instalar una base de datos localmente.

**Trade-off:** la API no está containerizada. Es suficiente para una demostración, pero un despliegue productivo requeriría empaquetarla por separado.

### JWT manual (sin ASP.NET Identity)

El sistema define dos roles (Admin y Viewer) y un único flujo de login. La autenticación se resuelve con JWT firmado en HMAC-SHA256 y un middleware propio, sin incorporar ASP.NET Identity ni tablas adicionales.

**Trade-off:** no hay refresh tokens ni revocación de sesiones emitidas. La autorización se aplica de forma explícita en los controllers y en el hub de SignalR.

### SignalR para alertas; consultas periódicas para el mapa

Las alertas nuevas deben notificarse de inmediato, por lo que se utiliza SignalR sobre WebSocket. La telemetría del mapa no exige la misma latencia: el frontend solicita la lista de vehículos cada 15 segundos mediante peticiones HTTP (*polling* — consultar de forma repetida si hay datos nuevos, en lugar de esperar un aviso del servidor).

**Trade-off:** con muchos clientes conectados, el volumen de peticiones HTTP supera al de un canal en tiempo real dedicado a la telemetría.

### Ingesta y alerta de combustible en un solo flujo

Al recibir una lectura del sensor, el servicio actualiza el vehículo, persiste el histórico, evalúa la alerta de combustible y, cuando corresponde, notifica por SignalR en la misma operación.

**Trade-off:** la predicción se basa en una regla lineal (litros ÷ consumo por hora × 60 minutos) con tasa fija por vehículo. No incluye reglas configurables ni modelos más elaborados.

---

## Frontend

### React 19 + TypeScript + Vite

La interfaz se construye como SPA con tipado estático hacia la API y un entorno de desarrollo ágil.

**Trade-off:** la aplicación depende por completo del backend; no hay renderizado en servidor.

### Ant Design, Recharts y MapLibre (OpenFreeMap)

Ant Design cubre formularios, tablas y layout. Recharts proporciona los gráficos de combustible y velocidad. MapLibre, con tiles de OpenFreeMap, permite trabajar en local sin API keys de mapas.

**Trade-off:** la interfaz sigue el estilo de la librería de componentes y el mapa requiere conexión a internet para cargar los tiles.

### localStorage para sesión y soporte offline parcial

Cuando la API no responde, el dashboard muestra la última copia de vehículos y lecturas almacenada en el navegador, sin dependencias adicionales.

**Trade-off:** el modo offline limita la consulta de datos ya guardados. El login, la ingesta y las alertas en tiempo real siguen requiriendo conectividad.

### Token JWT en la URL del WebSocket

Los WebSockets del navegador no transmiten de forma fiable el header `Authorization`, por lo que el token se envía como parámetro de consulta (`?access_token=...`).

**Trade-off:** el token queda expuesto en la URL de la conexión, un patrón habitual en SignalR que exige HTTPS en producción.

---

## Otras decisiones

| Decisión | Ventaja | Trade-off |
|----------|---------|-----------|
| Refresco del mapa cada 15 s | Comportamiento simple y fácil de depurar | La posición no se actualiza en tiempo real |
| ID enmascarado para Viewer | No expone el Guid interno | Admin y Viewer operan con identificadores distintos |
| Migraciones y seed al arrancar | Configuración en un solo paso (`dotnet run`) | Puede generar conflictos si arrancan varias instancias simultáneamente |
| Tests en JWT y alertas | Cubre la lógica más sensible | Sin pruebas end-to-end ni de frontend |
| CORS restringido a localhost | Adecuado para desarrollo | Requiere ajuste al desplegar en otros entornos |

---

## Fuera del alcance

Deliberadamente no se incluyeron:

- Refresh tokens, OAuth ni rate limiting en el login.
- Telemetría en vivo por WebSocket (solo consultas periódicas cada 15 s).
- Interfaz para marcar alertas como leídas (el campo existe en base de datos).
- Containerización de API/frontend ni pipeline de CI/CD.

Estos puntos quedan como extensiones naturales si el producto evoluciona más allá del alcance de la prueba técnica.
