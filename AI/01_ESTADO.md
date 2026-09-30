FORMATO: 1.0

## Regla de estados

CONSOLIDADO - probado y confirmado explicitamente por el TEAM LEAD como
terminado, aprobado y funcionando.

EN PROGRESO - trabajo activo, aun no terminado ni confirmado.

PENDIENTE - no evaluado.

## Estado actual

Cada linea describe el estado honesto del modulo segun la evidencia del codigo.

### Operacion

- Pantalla operario (Index + panel parcial) con selector de sector y botones de
  estado: EN PROGRESO (2026-09-14). Funciona con datos simulados en memoria
  (OperacionService), sin DB.
- ActualizarEstado (POST AJAX): EN PROGRESO. Ya inserta lógica simulada; falta
  persistencia real (INSERT Evento + UPDATE EstadoActual).
- CambiarSector (POST AJAX): EN PROGRESO. Simulado; falta persistencia/sesión.
- Login / autenticacion: PENDIENTE. No existe aún.
- Recordar último sector usado: PENDIENTE.
- Campos configurables por sector: EN PROGRESO. Nombre/label existen en el
  ViewModel y se renderizan; la configuración por sector no existe (campos
  quemados en el service).

### Monitores / Dashboard TV

- Dashboard general: PENDIENTE. No existe.
- Dashboard por sector: PENDIENTE. No existe.
- Acceso por token (`/monitor/{token}`): PENDIENTE. No existe.
- Polling cada 5 segundos: PENDIENTE.
- Estados finalizados atenuados: PENDIENTE.

### Administracion

- CRUD Usuarios: PENDIENTE.
- CRUD Sectores: PENDIENTE.
- CRUD Estados: PENDIENTE.
- CRUD Dashboards: PENDIENTE.
- Asignacion UsuarioSector: PENDIENTE.

### Historico

- Lista simple de eventos: PENDIENTE.
- Filtros basicos: PENDIENTE.

### Sistema / infraestructura

- Stack/plataforma base (D-015 a D-017): Linux + .NET 10 + PostgreSQL +
  Kestrel/Nginx + Docker. DECIDIDO (2026-09-14). Ver 02_DECISIONES.
- Docker Engine 29.8.0 + Compose v5.5.1: INSTALADO y VERIFICADO (2026-09-15,
  `docker run hello-world` OK). No es CONSOLIDADO hasta confirmación del
  TEAM LEAD.
- PostgreSQL (contenedor) + Redis (contenedor) vía compose: EN PROGRESO
  (2026-09-15). `compose.yaml` creado en la raíz del repo (D-018: Postgres 17 +
  Redis 8, alpine); contenedores arriba y saludables. Pendiente confirmación
  del TEAM LEAD.
- Cliente `psql` en host: PENDIENTE (requiere `sudo apt install
  postgresql-client`). No es bloqueante: se usa `docker exec -it
  litestate-postgres psql -U litestate -d litestate`.
- Conexión EF Core ↔ PostgreSQL en LiteState: EN PROGRESO (2026-09-15).
  Paquetes Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3 + Microsoft.EntityFrameworkCore.Design
  + Microsoft.AspNetCore.Identity.EntityFrameworkCore agregados. Connection string
  `DefaultConnection` en appsettings.Development.json. DbContext registrado en Program.cs.
- DbContext (EF Core): EN PROGRESO (2026-09-15). `LiteStateDbContext` hereda de
  IdentityDbContext y tiene las 7 entidades de negocio configuradas (Fluent API).
- Identidad (login/roles): PENDIENTE. Solo existen las tablas Identity (AspNetUsers, etc.)
  generadas por la migración; no hay flujo de login ni roles configurados.
- Modelo de datos (Empresa, Usuario, Sector, Estado, Evento, EstadoActual,
  Dashboard, UsuarioSector): EN PROGRESO (2026-09-15). 8 entidades completas en
  LiteState.Domain según `Contexto/Entidades y relaciones.txt`.
- Migraciones: EN PROGRESO (2026-09-15). Migración `Inicial` creada y aplicada;
  15 tablas en PostgreSQL (7 de negocio + 7 Identity + __EFMigrationsHistory).
- Design System (`ls-*`): EN PROGRESO (2026-09-14). Tokens y componentes
  iniciales creados; sin usar aún en vistas reales.
- Memoria de proyecto (AI/): EN PROGRESO (2026-09-14). Decisión de stack
  registrada (D-015/D-016/D-017); pendiente primeras sesiones de mantenimiento.

### Laboratorio Docker (aprendizaje: de cero a VPS)

Recorrido Ordered en 18 fases para aprender a levantar el stack completo en
Docker y publicarlo en un VPS. Manual completo en
`Docs/Manual Docker - de cero a VPS.md`.

- Fases 1-7 (instalación, contenedores, imágenes, puertos, volúmenes,
  laboratorio nginx+postgres, mantenimiento): COMPLETADAS (2026-09-15).
- Fase 8 (Docker Networks): COMPLETADA. La app resuelve `Host=postgres` por
  alias DNS en la red `litestate_default` que compose crea automáticamente.
- Fase 9 (Dockerizar .NET): COMPLETADA (D-019). `LiteState/Dockerfile`
  multi-stage sdk:10.0 -> aspnet:10.0, `USER $APP_UID`, healthcheck contra
  `/Home/Privacy`, más `LiteState/.dockerignore`.
- Fase 10 (Variables de entorno y secretos): COMPLETADA (D-020, D-021).
  `.env` en la raíz (ignorado por git), `ConnectionStrings__DefaultConnection`
  inyectada, puertos atados a `127.0.0.1`, claves de DataProtection
  persistidas en volumen `appkeys` con servicio auxiliar `appkeys-init`.
- Fase 11 (Docker Compose): COMPLETADA. `compose.yaml` con 4 servicios
  (app, appkeys-init, postgres, redis), healthchecks, `depends_on` con
  condiciones, volúmenes nombrados y `name: litestate` fijo.
- Fase 12 (Persistencia y backups): COMPLETADA (D-022). Volumen `pgdata`;
  dump en `Backups/litestate-2026-09-28.sql` restaurado y verificado en la
  base descartable `litestate_prueba` (15 tablas).
- **Fase 13 (Nginx reverse proxy): APLICADA (2026-09-30).** Servicio `nginx`
  (`nginx:1.29-alpine`) con config en `nginx/litestate.conf` montada en `:ro`;
  publica `127.0.0.1:80`. El `ports:` de la app fue eliminado: ya no es
  alcanzable desde el host. `ForwardedHeaders` configurado en `Program.cs`
  (D-023). Verificado: 502 con la app detenida, 200 con PostgreSQL detenido,
  gzip activo, estáticos OK, y nginx sigue sirviendo tras recrear la app
  (gracias al `resolver` de Docker). En el VPS el puerto pasa a `"80:80"`.
- Fases 14-18 (HTTPS/Let's Encrypt, seguridad del VPS, deploy, actualización
  de versiones, CI/CD): PENDIENTES. **Fase 14 (dominio + HTTPS) es la
  siguiente.**

Ningún ítem de esta sección está CONSOLIDADO: son etapas de aprendizaje y la
confirmación del TEAM LEAD es pendiente.

### Estado del laboratorio (2026-09-30)

Contenedores verificados en ejecución tras la Fase 13:

| Contenedor | Imagen | Estado | Puertos |
|---|---|---|---|
| litestate-nginx | nginx:1.29-alpine | Up (healthy) | 127.0.0.1:80->80/tcp |
| litestate-app | litestate:1.0 | Up (healthy) | 8080/tcp (sin publicar) |
| litestate-postgres | postgres:17-alpine | Up (healthy) | 127.0.0.1:5432->5432 |
| litestate-redis | redis:8-alpine | Up (healthy) | 127.0.0.1:6379->6379 |
| litestate-appkeys-init | alpine:latest | Exited (0) | — (tarea de un solo uso) |

Red: `litestate_default`. Volúmenes: `litestate_pgdata`,
`litestate_redisdata`, `litestate_appkeys`.

Único servicio con puerto publicado hacia el exterior del stack: nginx (:80).
Los datos siguen intactos: la Fase 13 no tocó los volúmenes.

Pendiente de git: los archivos del laboratorio (compose.yaml, nginx/litestate.conf,
.env ignorado, Dockerfile, .dockerignore, Docs/, Backups/, AI/ y ~20 archivos
modificados del proyecto) siguen sin commitear desde `acb5fd5`.