# 02_DECISIONES.md

FORMATO: 1.0

## Reglas

Guardar unicamente decisiones tecnicas o de arquitectura que no deben volver
a discutirse.

Una decision puede ser reemplazada por otra. Nunca se borra historial: la
decision vieja se marca como REEMPLAZADA.

Solo registrar decisiones reales tomadas. No opiniones ni propuestas no aprobadas.

Cuando se registra una decision, indicar el numero siguiente libre
(D-001, D-002, etc).

Las decisiones sembradas en el bootstrap provienen de la documentación del
proyecto (`/Contexto`). Se marcan VIGENTE; el TEAM LEAD puede confirmarlas,
corregirlas o rechazarlas en la revision inicial.

Al tomar una decision nueva, agregar una ficha con el ID siguiente y
actualizar el indice.

## Indice

| ID | Decision | Fecha | Estado |
|----|----------|-------|--------|
| D-001 | Monolito modular (sin microservicios, CQRS ni distribuido) | 2026-05-07 | VIGENTE |
| D-002 | Stack: ASP.NET Core 6 MVC + SQL Server 2016 + IIS | 2026-05-07 | REEMPLAZADA |
| D-003 | Arquitectura en 3 capas internas: Application / Domain / Infrastructure | 2026-05-07 | VIGENTE |
| D-004 | EstadoActual como tabla física dedicada | 2026-05-07 | VIGENTE |
| D-005 | Evento es histórico append-only (solo INSERT) | 2026-05-07 | VIGENTE |
| D-006 | Usuario multi-sector mediante relación N:M (UsuarioSector) | 2026-05-07 | VIGENTE |
| D-007 | Monitores sin login: acceso por URL con token (`/monitor/{token}`) | 2026-05-07 | VIGENTE |
| D-008 | Máximo 3 campos configurables por sector | 2026-05-07 | VIGENTE |
| D-009 | Tiempo real mediante AJAX polling (5s), sin SignalR/WebSockets | 2026-05-07 | VIGENTE |
| D-010 | No existe botón Guardar: el botón de estado ES la acción de guardar | 2026-05-07 | VIGENTE |
| D-011 | Al cambiar estado: INSERT en Evento + UPDATE en EstadoActual | 2026-05-07 | VIGENTE |
| D-012 | Prefijo de clases UI del proyecto: `ls-` (Design System propio) | 2026-09-14 | VIGENTE |
| D-013 | UI construida según MANUAL_UI_MODULAR: tokens, componentes, vistas delgadas | 2026-09-14 | VIGENTE |
| D-014 | Memoria de proyecto según MANUAL_MEMORIA_AGENTES (carpeta AI/) | 2026-09-14 | VIGENTE |
| D-015 | Nuevo stack Linux: .NET 10 + PostgreSQL + Kestrel/Nginx + Docker | 2026-09-14 | VIGENTE |
| D-016 | BD de desarrollo y tests local, Postgres contenedor Docker | 2026-09-14 | VIGENTE |
| D-017 | Deploy futuro: contenedor Docker (app + Nginx) en VPS Linux | 2026-09-14 | VIGENTE |
| D-018 | Contenedores de dev: PostgreSQL 17 + Redis 8 (imágenes alpine) vía compose.yaml | 2026-09-15 | VIGENTE |
| D-019 | Dockerfile multi-stage sdk:10.0 -> aspnet:10.0, usuario no-root, healthcheck a /Home/Privacy | 2026-09-28 | VIGENTE |
| D-020 | Connection string solo por variable de entorno; puertos en 127.0.0.1 | 2026-09-28 | VIGENTE |
| D-021 | Claves de DataProtection en volumen `appkeys` con `appkeys-init` | 2026-09-28 | VIGENTE |
| D-022 | Backups con `pg_dump` verificados restaurando; `Backups/` fuera de git | 2026-09-28 | VIGENTE |
| D-023 | Nginx es la única puerta de entrada; `resolver` de Docker en vez de `upstream` | 2026-09-30 | VIGENTE |

## Detalle

### D-001

Fecha: 2026-05-07

Decision: LiteState es un monolito modular. Se evitan microservicios, CQRS complejo y arquitecturas distribuidas.

Motivo: simplicidad, velocidad de desarrollo, menor costo, compatibilidad con hosting economico (DonWeb).

Consecuencias: toda futura escalabilidad (APIs, SignalR, React) debe migrar sin romper la base actual.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-002

Fecha: 2026-05-07

Decision: Stack definido: ASP.NET Core 6 MVC, SQL Server 2016, IIS, hosting compartido DonWeb, EF Core, Bootstrap 5, JavaScript vanilla.

Motivo: compatibilidad con hosting Windows economico y runtime .NET 6.0.x.

Consecuencias: deploy framework-dependent win-x64; no usar. tecnologías que exijan más recursos (Blazor WASM, SignalR).

Reemplaza a: NINGUNA

Estado: REEMPLAZADA (2026-09-14, por D-015)

---

### D-003

Fecha: 2026-05-07

Decision: Arquitectura en capas: LiteState.Application (DTOs/Services), LiteState.Domain (entidades), LiteState.Infrastructure (EF Core/Identity). El proyecto Web coordina Controllers/Views.

Motivo: separar responsabilidades y facilitar mantenimiento y escalabilidad.

Consecuencias: flujo Request → Controller → Application Service → Infrastructure → SQL Server. Nunca romper ese flujo.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-004

Fecha: 2026-05-07

Decision: EstadoActual es una tabla física optimizada (un registro por sector, se actualiza al cambiar de estado).

Motivo: dashboards ultra rápidos, evitar consultas pesadas al histórico, optimizar hosting compartido.

Consecuencias: al cambiar estado se hace UPDATE en EstadoActual y INSERT en Evento.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-005

Fecha: 2026-05-07

Decision: Evento es un histórico append-only: solo INSERT, nunca UPDATE, nunca DELETE lógico operativo.

Motivo: trazabilidad, auditoría y métricas futuras.

Consecuencias: el histórico crece solo; el estado vivo (EstadoActual) está separado.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-006

Fecha: 2026-05-07

Decision: Un usuario puede operar varios sectores mediante tabla intermedia UsuarioSector (N:M).

Motivo: flexibilidad operativa (un operario cubre varias líneas).

Consecuencias: la pantalla operario debe permitir cambiar rápido de sector y recordar el último usado.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-007

Fecha: 2026-05-07

Decision: Los monitores/dashboards no usan login: se accede por URL pública con token (`/monitor/{token}`).

Motivo: TVs y pantallas sin sesiones de usuario; simplicidad.

Consecuencias: el token debe ser aleatorio y no adivinable; dashboards desacoplados de administración.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-008

Fecha: 2026-05-07

Decision: Cada sector define hasta 3 campos configurables (texto corto, número o lista).

Motivo: información operativa mínima clave; evitar formularios largos.

Consecuencias: el modelo Sector lleva Campo1..3 Nombre/Tipo; Evento y EstadoActual llevan los valores.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-009

Fecha: 2026-05-07

Decision: Tiempo real mediante AJAX polling simple cada 5 segundos. Sin SignalR, WebSockets, Redis.

Motivo: simplicidad, hosting compartido, bajo consumo.

Consecuencias: dashboards se actualizan con recarga parcial; latencia máxima ~5s.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-010

Fecha: 2026-05-07

Decision: No existe botón Guardar en la pantalla operario. El botón de estado ES la acción de guardar.

Motivo: actualizar estado en 2-3 segundos; velocidad sobre información.

Consecuencias: al presionar un estado se valida, se guarda, se actualiza EstadoActual y se inserta Evento en un solo paso.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-011

Fecha: 2026-05-07

Decision: Al cambiar de estado se ejecuta: INSERT en Evento + UPDATE en EstadoActual, dentro de la misma operación.

Motivo: mantener consistencia entre histórico y estado vivo.

Consecuencias: la operación debe ser atómica (transacción) al conectar la DB.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-012

Fecha: 2026-09-14

Decision: El Design System propio usa el prefijo de clases `ls-` (ls-btn, ls-card, ls-input...), según MANUAL_UI_MODULAR.

Motivo: coherencia visual, DRY, bajo alucinaciones visuales del agente.

Consecuencias: nada de colores/medidas hardcodeados; todo vía tokens CSS (`ls-tokens.css`).

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-013

Fecha: 2026-09-14

Decision: La UI se construye según el MANUAL_UI_MODULAR: tokens (Capa 1), componentes (Capa 2), páginas por módulo (Capa 3) y vistas delgadas que componen (Capa 4).

Motivo: consistencia, mantenimiento barato, velocidad de desarrollo y salidas predecibles del agente.

Consecuencias: se registran en 02_DECISIONES los cambios estructurales de UI; los componentes nuevos van al catálogo.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-014

Fecha: 2026-09-14

Decision: El proyecto mantiene memoria progresiva según el MANUAL_MEMORIA_AGENTES (carpeta AI/ en la raíz del repo).

Motivo: continuidad entre sesiones de agentes, costo mínimo de arranque, fuente única de verdad operativa.

Consecuencias: arranque = leer 00_MEMORIA.md + 01_ESTADO.md; actualizar memoria al cerrar cada tarea; solo el TEAM LEAD confirma CONSOLIDADO.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-015

Fecha: 2026-09-14

Decision: Nuevo stack linux-first: ASP.NET Core MVC sobre .NET 10 (SDK 10.0.112 en dev), PostgreSQL como base de datos principal, Kestrel + Nginx en producción, sin IIS/SQL Server/DonWeb. ORM: EF Core.

Motivo: migrar el entorno de desarrollo a Linux, eliminar vendor lock-in y costos de licencias, priorizar aprendizaje del stack moderno .NET en Linux.

Consecuencias: el proyecto se desarrolla, compila y corre full en Linux (ya apunta a net10.0). Todas las tareas nuevas (persistencia, migraciones, monitores) deben construirse para PostgreSQL. El hosting producción deja de ser Windows/IIS/DonWeb: pasa a VPS Linux con contenedores.

Reemplaza a: D-002

Estado: VIGENTE

---

### D-016

Fecha: 2026-09-14

Decision: La base de datos de desarrollo y tests se ejecuta como contenedor Docker (PostgreSQL); los tests de integración usan Testcontainers en lugar de mocks/in-memory.

Motivo: entorno reproducible, cero instalación manual de BD, tests contra la BD real igual a producción.

Consecuencias: atajo de arranque = `docker compose up -d postgres` + `dotnet run`; `dotnet test` levanta BD efímeras de Postgres automáticamente. Se agrega `compose.yaml` al repo.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-017

Fecha: 2026-09-14

Decision: El deploy futuro se hace como contenedor Docker de la app .NET detrás de Nginx (reverse proxy) en un VPS Linux. CI/CD con GitHub Actions (build + test en cada push).

Motivo: deploy consistente, rollback simple, bajo costo, patrón estándar de la industria .NET en 2026.

Consecuencias: se agrega Dockerfile multi-stage al repo; el pipeline de CI no requiere Windows.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-018

Fecha: 2026-09-15

Decision: Los contenedores de desarrollo usan PostgreSQL 17 y Redis 8 (ambos alpine), definidos en `compose.yaml` en la raíz del repo. Credenciales de dev: usuario `litestate`, password `litestate_dev`, DB `litestate`, puerto 5432 host.

Motivo: versiones estables actuales; alpine para menor tamaño; siguen D-015/D-016.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-019

Fecha: 2026-09-28

Decision: La app se dockeriza con `Dockerfile` multi-stage (sdk:10.0 -> aspnet:10.0) en `LiteState/Dockerfile`, con contexto de build `./LiteState`. La imagen final corre como usuario no-root (`$APP_UID`), expone 8080 y tiene HEALTHCHECK contra `/Home/Privacy`. Se agrega `LiteState/.dockerignore` para excluir `bin/`, `obj/` y `appsettings.Development.json`.

Motivo: la imagen final no debe incluir el SDK ni archivos de configuracion con credenciales de dev; `obj/` del host contiene rutas absolutas que rompen `dotnet restore` dentro del contenedor. El healthcheck sobre una vista real valida routing + controller + views + static files, mas que un endpoint estatico.

Consecuencias: el build se divide en dos capas (`csproj` primero) para que el cache de NuGet sobreviva a cambios de codigo.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-020

Fecha: 2026-09-28

Decision: La connection string NUNCA se define en archivos `appsettings*.json` para el entorno containerizado. Se inyecta por variable de entorno `ConnectionStrings__DefaultConnection` desde `.env` (raiz del repo, ignorado por git) con `Host=postgres`. Los secretos de Postgres/Redis salen de `.env`. Todos los puertos publicados se enlazan a `127.0.0.1`, nunca a `0.0.0.0`.

Motivo: el doble guion bajo es la convencion de .NET para descender un nivel en la configuracion; las variables de entorno pisan a `appsettings.json`, lo que permite cambiar la base sin reconstruir la imagen. Publicar en `0.0.0.0` exponia postgres (5432) y redis (6379) a toda la red local.

Consecuencias: `appsettings.Development.json` queda solo para `dotnet run` en el host. En el VPS el puerto de la app se elimina por completo y solo Nginx expone 80/443.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-021

Fecha: 2026-09-28

Decision: Las claves de DataProtection se persisten en el volumen `appkeys`, montado en `/var/lib/litestate-keys`. La ruta se configura en `Program.cs` leyendo `DataProtection:Keys:Path` (inyectada por `.env`); NO se hardcodea. Se usa `SetApplicationName("LiteState")` para atar las claves a un nombre fijo y no al directorio de contenido. Un servicio auxiliar `appkeys-init` (alpine, `user: root`, `restart: "no"`) ejecuta `chown` sobre el volumen antes de que arranque la app, y la app lo espera con `depends_on: condition: service_completed_successfully`.

Motivo: (1) sin volumen, recrear el contenedor pierde las claves y cae la sesion de todos los usuarios; (2) `DataProtection:Keys:Path` NO se lee automaticamente de la configuracion, hace falta registrarla explicitamente con `AddDataProtection().PersistKeysToFileSystem()`; (3) Docker crea los volumenes como `root` (0:0) y la app corre como `APP_UID`, por lo que sin `chown` previo DataProtection lanza `UnauthorizedAccessException` al arrancar. `SetApplicationName` evita que un cambio de content root invalide las claves.

Consecuencias: `APP_UID` quedo centralizado en `.env` porque debe coincidir con el de la imagen `aspnet:10.0`; si la imagen base cambia de UID hay que actualizarlo. Cambiar la ruta de las claves en produccion invalida las existentes.

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-022

Fecha: 2026-09-28

Decision: Los backups de la base se hacen con `pg_dump` desde el contenedor hacia `Backups/` en la raiz del repo, y `Backups/` + `*.sql` quedan ignorados por git. Ningun backup se considera valido hasta que se restaura y se verifica el contenido.

Motivo: un volumen NO es un backup (si alguien borra una tabla, el volumen tambien la tiene borrada). Los dumps pueden contener datos sensibles de usuarios y no deben quedar en el historial de git.

Consecuencias: verificacion realizada restaurando `Backups/litestate-2026-09-28.sql` (27K, 15 tablas) en una base descartable `litestate_prueba`. En el VPS esto se automatiza con cron (pendiente, Fase 16).

Reemplaza a: NINGUNA

Estado: VIGENTE

---

### D-023

Fecha: 2026-09-30

Decision: Nginx pasa a ser la UNICA puerta de entrada del stack. El servicio `app` pierde su
`ports:` y ya no es alcanzable desde el host: solo se entra por `127.0.0.1:80` (en el VPS,
`80:80`). La config vive en `nginx/litestate.conf`, montada en
`/etc/nginx/conf.d/default.conf:ro`. Se usa `resolver 127.0.0.11` + una variable en
`proxy_pass` en lugar de un bloque `upstream`, para que el proxy resuelva el nombre del
servicio en cada request. La app registra `ForwardedHeaders` (XForwardedFor | XForwardedProto
| XForwardedHost) con `app.UseForwardedHeaders()` como primer middleware, sin limpiar
`KnownNetworks`/`KnownProxies`. Nginx tiene su propio endpoint `/nginx-health` que no pasa
por el proxy, usado como healthcheck del contenedor.

Motivo: (1) un bloque `upstream` resuelve el hostname UNA vez al arrancar, asi que cada
`docker compose up --build` que recrea `app` le deja una IP interna nueva a nginx y produce
502 permanentes hasta reiniciarlo a mano; con `resolver` se resuelve por request y el proxy
siempre encuentra al contenedor vivo. (2) Sin `ForwardedHeaders`, Kestrel cree que todas las
peticiones vienen de nginx: se pierden las IPs reales en los logs y, cuando haya HTTPS
(Fase 14), se rompen las cookies `Secure` y las redirecciones. (3) El healthcheck de nginx no
puede pasar por el proxy: si lo hiciera, una caida de PostgreSQL marcaria a nginx como
`unhealthy` y `docker compose ps` senalaria un culpable equivocado. (4) El `HEALTHCHECK` del
Dockerfile sigue funcionando porque corre dentro del contenedor contra `localhost:8080`, sin
depender del puerto publicado.

Consecuencias: en el laboratorio nginx publica `127.0.0.1:80` (coherente con D-020) y en el
VPS pasa a `"80:80"`; ese es el unico cambio de puerto necesario. `postgres` y `redis`
conservan su `ports:` en `127.0.0.1` porque `dotnet run` en el host los necesita (D-016).
Los healthchecks de los cuatro servicios son independientes y cada uno mide una capa.
Verificado: 502 con la app detenida, 200 con PostgreSQL detenido, y nginx sigue sirviendo tras
recrear la app (cambia su IP). La afirmacion anterior del manual de que "con PostgreSQL
parado nginx responde 502" era incorrecta y quedo corregida.

Reemplaza a: NINGUNA (extiende D-017 y D-020)

Estado: VIGENTE

---

## Registro de decisiones nuevas

Al tomar una decision nueva: agregar ficha con el ID siguiente libre (D-015, ...)
y actualizar el indice. Registrar bajo confirmacion del TEAM LEAD.