# MANUAL DE DOCKER: DE CERO A UN VPS

Manual del recorrido completo para aprender a levantar **un sistema .NET con base de datos
dentro de Docker en Linux**, y después publicarlo en un VPS real.

> Objetivo final: un solo comando levanta la aplicación, la base de datos y el servidor web,
> y ese mismo stack funciona igual en tu PC de desarrollo y en un servidor de Internet.
>
> Todas las fases 8 a 13 están **ya aplicadas** en el proyecto LiteState: los archivos
> citados son reales y están en el repo. Las fases 1 a 7 las hiciste como ejercicios sueltos
> y las fases 14 a 18 están planificadas.

---

## 0. La idea final (leela antes de empezar)

Al terminar vas a tener esto:

```
                        INTERNET
                           │
                           ▼
                   ┌───────────────┐
                   │      VPS      │
                   │ Ubuntu Linux  │
                   └───────┬───────┘
                           │
                       Docker
                           │
              ┌────────────┴────────────┐
              │                         │
              ▼                         ▼
        ┌───────────┐             ┌──────────────┐
        │   Nginx   │             │  PostgreSQL  │
        │  Reverse  │             │     DB       │
        │   Proxy   │             └──────┬───────┘
        └─────┬─────┘                    │
              │                          ▼
              ▼                   ┌──────────────┐
        ┌───────────┐            │    Volume    │
        │  .NET App │            │  postgres-   │
        │  (Kestrel)│            │    data      │
        └─────┬─────┘            └──────────────┘
              │
              ▼
        red interna de Docker
```

Y todo descrito en **un solo archivo**:

```bash
docker compose up -d
```

**La regla de oro del recorrido:** los contenedores se comunican entre sí por la red
interna de Docker, usando el **nombre del servicio** como hostname. No se usan IPs, no se
publican puertos por la mitad. Lo único que se publica hacia Internet es Nginx.

---

## Mapa de fases

| Fase | Tema | Qué aprendés | Estado |
|-----|------|--------------|--------|
| 1 | Instalar Docker | Motor de contenedores en Linux | ✅ |
| 2 | Primeros contenedores | `run`, `ps`, `logs`, `exec` | ✅ |
| 3 | Imágenes y Dockerfile | Empaquetar un programa propio | ✅ |
| 4 | Puertos | `-p` y la diferencia entre mundo interno y externo | ✅ |
| 5 | Volúmenes | Que los datos sobrevivan al contenedor | ✅ |
| 6 | Laboratorio nginx + postgres | Dos programas conviviendo | ✅ |
| 7 | Mantenimiento | `inspect`, `stats`, `prune`, `restart` | ✅ |
| 8 | Docker Networks | Cómo se hablan dos contenedores ⭐⭐⭐⭐⭐ | ✅ |
| 9 | Dockerizar tu app .NET ⭐⭐⭐⭐⭐ | Dockerfile multi-stage | ✅ |
| 10 | Variables de entorno ⭐⭐⭐⭐⭐ | Secretos y configuración externa | ✅ |
| 11 | Docker Compose ⭐⭐⭐⭐⭐ | Describir el stack entero en un YAML | ✅ |
| 12 | Persistencia y backups ⭐⭐⭐⭐ | Volúmenes, `pg_dump`, restore verificado | ✅ |
| **13** | **Nginx reverse proxy ⭐⭐⭐⭐⭐** | **La única puerta de entrada** | **✅** |
| 14 | Dominio y HTTPS ⭐⭐⭐⭐⭐ | Certificados con Let's Encrypt | ⬜ SIGUIENTE |
| 15 | Seguridad del VPS ⭐⭐⭐⭐⭐ | SSH, firewall, qué puertos abrir | ⬜ |
| 16 | Deploy en VPS ⭐⭐⭐⭐⭐ | Llevar el stack a un servidor real | ⬜ |
| 17 | Actualización de versiones ⭐⭐⭐ | Publicar v2 sin romper la base | ⬜ |
| 18 | Git + CI/CD ⭐⭐⭐⭐ | Deploy automático | ⬜ |

---

# FASE 1 — Instalar Docker

Docker es un **motor de contenedores**: te permite correr un programa (con todo lo que
necesita) de forma aislada, sin instalar nada en el sistema.

Conceptos mínimos:

| Término | Qué es |
|---------|--------|
| **Imagen** | El "paquete" con el programa y su sistema mínimo. Es solo lectura. |
| **Contenedor** | Una imagen **corriendo**. Es la instancia viva. |
| **Dockerfile** | El archivo donde escribís cómo se construye una imagen. |
| **Volumen** | Disco persistente que sobrevive al contenedor. |
| **Red** | Canal de comunicación entre contenedores. |
| **Compose** | Un archivo YAML que describe varios contenedores y cómo se conectan. |

## Instalación en Ubuntu/Debian

```bash
sudo apt update
sudo apt install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
  -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo $VERSION_CODENAME) stable" \
  | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io \
  docker-buildx-plugin docker-compose-plugin
```

## Verificar

```bash
docker run hello-world
docker --version
docker compose version
```

Si `hello-world` imprime un saludo, Docker funciona.

## Permisos (para no escribir `sudo` siempre)

```bash
sudo usermod -aG docker $USER
newgrp docker          # o cerrá y volvé a iniciar sesión
```

> ⚠️ El grupo `docker` es equivalente a root. Solo agregá a personas de confianza.

---

# FASE 2 — Primeros contenedores

## El ciclo de vida en 6 comandos

```bash
# 1. Descargar la imagen y crear un contenedor corriendo
docker run -d --name web nginx

# 2. Ver qué hay corriendo
docker ps

# 3. Ver la salida del programa
docker logs -f web

# 4. Entrar adentro del contenedor
docker exec -it web bash

# 5. Detener
docker stop web

# 6. Borrar el contenedor (NO borra la imagen)
docker rm web
```

Desglose de `docker run -d --name web nginx`:

| Parte | Significado |
|-------|-------------|
| `-d` | **d**etach: corre en segundo plano |
| `--name web` | Le ponés nombre, para no depender del ID autogenerado |
| `nginx` | La imagen a usar (Docker la baja sola si no está) |

## Comandos que vas a usar siempre

```bash
docker images                  # imágenes descargadas
docker ps -a                   # contenedores, incluidos los parados
docker inspect <id_o_nombre>   # toda la config de un contenedor (JSON)
docker stats                   # consumo de CPU y memoria en vivo
docker system df               # cuánto disco ocupa Docker
```

## Guardá la imagen o el contenedor

```bash
docker stop web
docker commit web web:mi-version
docker images
```

> ⚠️ `commit` sirve para pruebas, **no** para construir imágenes. Para eso está el Dockerfile.

---

# FASE 3 — Imágenes y Dockerfile

Una imagen se construye con un **Dockerfile**: un archivo de texto con instrucciones.

## Ejemplo: servir una página con Nginx

```dockerfile
FROM nginx:alpine

COPY ./html /usr/share/nginx/html

EXPOSE 80
```

Y se construye con:

```bash
docker build -t mi-web:v1 .
```

## Las instrucciones que importan

| Instrucción | Qué hace |
|-------------|----------|
| `FROM imagen` | Sobre qué imagen base construyo |
| `COPY de origen` | Copia archivos de mi proyecto a la imagen |
| `RUN comando` | Ejecuta algo **durante la construcción** |
| `ENVclave=valor` | Variable de entorno dentro del contenedor |
| `WORKDIR /app` | Carpeta de trabajo |
| `EXPOSE puerto` | Documenta qué puerto usa (no lo publica) |
| `USER usuario` | Con qué usuario corre (no root = más seguro) |
| `CMD` / `ENTRYPOINT` | Qué se ejecuta al arrancar |

> **Ojo:** `EXPOSE` NO abre nada. Solo documenta. Lo que publica de verdad es `-p`.

---

# FASE 4 — Puertos

Esta es una de las confusiones más importantes. Docker tiene **dos mundos**:

```
    TU MACHINE (mundo externo)            CONTENEDOR (mundo interno)
    ──────────────────────────            ──────────────────────────
    localhost:8080  ◄── -p 8080:80 ──►   puerto 80 (dentro)
```

## Formas de publicar

```bash
# 1. Conectar a un puerto libre cualquiera del host
docker run -d -p 80:80 nginx

# 2. Fijar el puerto del host
docker run -d -p 8080:80 nginx

# 3. Solo accesible desde tu propia máquina (¡recomendado!)
docker run -d -p 127.0.0.1:5432:5432 postgres
```

## ⚠️ La diferencia que define tu proyecto

| Escritura | Significado | Riesgo |
|-----------|-------------|--------|
| `-p 5432:5432` | Escucha en **todas** las interfaces | Cualquiera de tu red entra a la base ❌ |
| `-p 127.0.0.1:5432:5432` | Escucha **solo en loopback** | Nadie de la red entra ✅ |

**En LiteState, todos los puertos están atados a `127.0.0.1`.** Eso ya está aplicado en
`compose.yaml` y es una decisión registrada (D-020).

---

# FASE 5 — Volúmenes

Un contenedor es **desechable**. Si guardás datos adentro, se pierden al borrarlo.

```
   docker rm mi-postgres   →   ¿dónde quedó la base?
```

Un **volumen** resuelve esto: es un disco que vive en Docker, aparte del contenedor.

```bash
docker volume create pgdata
docker run -d --name mi-postgres \
  -e POSTGRES_PASSWORD=lab123 \
  -p 127.0.0.1:5432:5432 \
  -v pgdata:/var/lib/postgresql/data \
  postgres:17-alpine
```

## Volumen nombrado vs bind mount

| Tipo | Escritura | ¿Dónde vive? | ¿Para qué? |
|------|-----------|--------------|------------|
| **Volumen nombrado** | `-v pgdata:/ruta` | En Docker (`/var/lib/docker/volumes`) | **Datos** (base, archivos de la app) |
| **Bind mount** | `-v $(pwd)/config:/ruta` | En tu carpeta | **Configuración** (código, `.conf`) |

## Comandos

```bash
docker volume ls
docker volume inspect pgdata
docker volume prune          # borra volúmenes SIN USAR (¡cuidado!)
```

> 🧠 **Regla que vale Oro:** nunca confíes en el sistema de archivos interno del contenedor.
> Si un volumen no está, el dato se fue.

---

# FASE 6 — Laboratorio: Nginx + PostgreSQL

Antes de compose, se levantan contenedores a mano. Sirve para entender qué hace compose
por debajo.

```bash
# 1. Base de datos
docker volume create postgres-lab
docker run -d --name postgres-lab \
  -e POSTGRES_DB=lab \
  -e POSTGRES_USER=lab \
  -e POSTGRES_PASSWORD=lab123 \
  -p 127.0.0.1:5432:5432 \
  -v postgres-lab:/var/lib/postgresql/data \
  --restart unless-stopped \
  postgres:17-alpine

# 2. Un nginx suelto
docker run -d --name nginx \
  -p 8080:80 \
  --restart unless-stopped \
  nginx:alpine

# 3. Verificar
docker ps
docker logs postgres-lab
```

Entrar a la base:

```bash
docker exec -it postgres-lab psql -U lab -d lab
```

```sql
\dt        -- listar tablas
\l         -- listar bases
\q         -- salir
```

### `restart: unless-stopped`

| Política | Comportamiento |
|----------|----------------|
| `no` (default) | Si el contenedor muere, no vuelve |
| `always` | Siempre vuelve, incluso si lo paraste a mano |
| **`unless-stopped`** | **Vuelve salvo que vos lo hayas parado** ✅ |

---

# FASE 7 — Mantenimiento

```bash
# Ver todo el detalle de un contenedor
docker inspect postgres-lab
docker inspect -f '{{.NetworkSettings.IPAddress}}' postgres-lab

# Ver qué procesos hay adentro
docker exec postgres-lab ps aux

# Logs con timestamp
docker logs -f --timestamps postgres-lab

# Limpieza
docker container prune     # contenedores parados
docker image prune         # imágenes sin usar
docker system prune        # todo lo anterior
docker system prune -a --volumes   # ⚠️ borra volúmenes: NO en producción
```

---

# FASE 8 — Docker Networks ⭐⭐⭐⭐⭐

**Esta fase es el corazón del recorrido.** Sin esto no entendés por qué funciona compose.

## El problema

Un contenedor tiene su propia IP interna, que **cambia cada vez que se recrea**. Si la app
guardara la IP de la base, se rompería al reiniciar.

## La solución: una red + DNS

Cuando dos contenedores están en la misma red, Docker les regala un **servidor de nombres**.
Cada contenedor es resoluble por su nombre o su alias.

```bash
# 1. Crear la red
docker network create red-lab

# 3. Levantar cada uno en la red
docker run -d --name postgres-lab --network red-lab postgres:17-alpine
docker run -d --name cliente     --network red-lab alpine sleep 3600

# 4. Probar la resolución de nombres
docker exec cliente nslookup postgres-lab
```

## `localhost` vs IP vs nombre

| Si pones... | Desde dentro del contenedor | Resultado |
|-------------|-----------------------------|-----------|
| `localhost` | a **sí mismo** | ❌ no encuentra a la base |
| `172.18.0.3` | la IP interna | ⚠️ funciona, pero cambia al recrear |
| `postgres-lab` | el **nombre en la red** | ✅ estable y legible |

**Por eso en LiteState la connection string dice `Host=postgres`:**

```
Host=postgres;Port=5432;Database=litestate;Username=...;Password=...
```

`postgres` no es una IP ni un nombre inventado: es el **alias** que compose registra
automáticamente en la red del proyecto.

## Comandos de red

```bash
docker network ls
docker network inspect red-lab
docker network connect red-lab otro-contenedor
docker network disconnect red-lab otro-contenedor
```

> 🧠 **Concepto clave:** la red de Docker es como una LAN privada. Lo que está en la red se
> ven entre sí; lo que no, está como si estuviera en otra máquina. Por eso **no hace falta**
> publicar el puerto 5432 para que la app hable con la base.

---

# FASE 9 — Dockerizar tu aplicación .NET ⭐⭐⭐⭐⭐

Acá se aplica todo a un proyecto real: LiteState.

## Multi-stage build

Un solo `FROM` produce una imagen enorme (con el SDK completo, ~800 MB). La solución son
**dos etapas**: una compila, la otra solo ejecuta.

```dockerfile
# ===== Etapa 1: BUILD (contiene el SDK, pesa ~800MB, se descarta) =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar SOLO el .csproj y restaurar primero.
# Mientras el csproj no cambie, esta capa sale del caché de Docker
# y los builds siguientes son mucho más rápidos.
COPY LiteState.csproj ./
RUN dotnet restore LiteState.csproj

# Recién ahora el código fuente.
COPY . .
RUN dotnet publish LiteState.csproj -c Release -o /app/publish --no-restore


# ===== Etapa 2: RUNTIME (solo el runtime + la app publicada) =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -fsS http://localhost:8080/Home/Privacy || exit 1

ENTRYPOINT ["dotnet", "LiteState.dll"]
```

Este es el `LiteState/Dockerfile` real del proyecto. Puntos clave:

| Concepto | Por qué importa |
|----------|-----------------|
| `sdk:10.0` vs `aspnet:10.0` | La primera compila (y se tira), la segunda solo ejecuta. Imagen final mucho más chica. |
| `COPY csproj` **antes** del código | El caché de NuGet sobrevive a cambios de código. Build 10× más rápido. |
| `--no-restore` | No repetir el restore que ya se hizo. |
| `AS build` / `--from=build` | Se copia de una etapa a otra, no del host. |
| `ASPNETCORE_HTTP_PORTS=8080` | .NET 8+ no usa `ASPNETCORE_URLS`; escucha en **todos** los puertos no privilegiados. |
| `USER $APP_UID` | Corre sin root. `APP_UID` (1654) lo define la imagen base. |
| `HEALTHCHECK` | Docker sabe si la app está viva. Se usa en `depends_on`. |
| Healthcheck sobre `/Home/Privacy` | Verifica routing + controller + views + static files, no solo que el puerto esté abierto. |

## El `.dockerignore`

Sin esto, se te cuela basura a la imagen. El real del proyecto:

```text
bin/
obj/

.vs/
.vscode/
*.user
*.suo

Dockerfile
.dockerignore

*.md
AI/
Docs/
Contexto/

appsettings.Development.json
```

> 🔴 **Detalle crítico:** `obj/project.assets.json` de tu máquina tiene rutas absolutas
> (`/home/tu-usuario/...`) y **rompe** el `dotnet restore` dentro del contenedor. Por eso
> `obj/` va excluido sí o sí.

## Construir y correr

```bash
docker build -t litestate:1.0 .
docker run -d --name test -p 127.0.0.1:5194:8080 litestate:1.0
docker logs -f test
```

---

# FASE 10 — Variables de entorno y configuración ⭐⭐⭐⭐⭐

Nada de contraseñas en el código ni en la imagen.

## El doble guion bajo

En .NET, la configuración es un árbol JSON:

```json
{ "ConnectionStrings": { "DefaultConnection": "..." } } }
```

En variables de entorno no se puede escribir `:` (es el separador de Linux), así que se usa
`__` para bajar un nivel:

```
ConnectionStrings__DefaultConnection=...
```

Las variables de entorno **pisan** a `appsettings.json`. Esa es la clave: la imagen es la
misma para desarrollo y producción, lo que cambia es la configuración.

## El archivo `.env`

Compose lo lee automáticamente desde la raíz del proyecto:

```bash
POSTGRES_DB=litestate
POSTGRES_USER=litestate
POSTGRES_PASSWORD=<secreto-real>

DataProtection__Keys__Path=/var/lib/litestate-keys
APP_UID=1654
```

Y se inyecta así:

```yaml
app:
  environment:
    ASPNETCORE_ENVIRONMENT: Production
    ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
```

`${POSTGRES_DB}` es interpolación: compose reemplaza con el valor del `.env`.

## `.gitignore`: los secretos no van a git

```text
*.env
Backups/
*.sql
```

> 🔴 **Nunca subas el `.env`.** En el VPS se genera uno nuevo con las claves reales.
> Lo que sí conviene versionar es un `.env.example` con los nombres y valores de ejemplo.

## DataProtection: el detalle que muerde

ASP.NET Core cifra las cookies de sesión. Si las claves cambian, **se cae la sesión de
todos los usuarios**. Por eso hay que persistirlas en un volumen:

```csharp
var dataProtectionKeysPath = builder.Configuration["DataProtection:Keys:Path"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName("LiteState");
}
```

Y como Docker crea los volúmenes como `root` y la app corre como `APP_UID`, hace falta un
servicio auxiliar que iguale los permisos antes:

```yaml
appkeys-init:
  image: alpine:latest
  user: root
  command: chown -R ${APP_UID}:${APP_UID} /keys && chmod 700 /keys
  volumes:
    - appkeys:/keys
  restart: "no"
```

```yaml
app:
  depends_on:
    appkeys-init:
      condition: service_completed_successfully
```

> Este es un detalle **real** que apareció al construir LiteState (decisión D-021). No
> aparece en ningún tutorial, pero es de los que hacen perder una tarde.

---

# FASE 11 — Docker Compose ⭐⭐⭐⭐⭐

Compose reemplaza todos los `docker run` sueltos por **un archivo que describe el stack
entero**.

```bash
docker compose up -d --build     # levantar
docker compose ps                # ver estado
docker compose logs -f app       # ver logs de un servicio
docker compose down              # parar (NO borra datos)
docker compose down -v           # ⚠️ parar Y BORRAR VOLÚMENES
```

## El `compose.yaml` real de LiteState

```yaml
# Fijar el nombre del proyecto: determina los nombres de red y volumen.
# Sin esto, compose lo deduce del nombre de la carpeta y un rename
# del repo crearía un volumen VACÍO (base de datos en blanco).
name: litestate

services:
  appkeys-init:
    image: alpine:latest
    container_name: litestate-appkeys-init
    user: root
    command: chown -R ${APP_UID}:${APP_UID} /keys && chmod 700 /keys
    volumes:
      - appkeys:/keys
    restart: "no"

  app:
    build:
      context: ./LiteState
      dockerfile: Dockerfile
    image: litestate:1.0
    container_name: litestate-app
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      DataProtection__Keys__Path: ${DataProtection__Keys__Path}
    ports:
      - "127.0.0.1:5194:8080"
    volumes:
      - appkeys:/var/lib/litestate-keys
    depends_on:
      postgres:
        condition: service_healthy
      appkeys-init:
        condition: service_completed_successfully
    # NO se declara `networks:` a propósito: compose crea la red
    # `litestate_default` automáticamente y es la que da los alias.

  postgres:
    image: postgres:17-alpine
    container_name: litestate-postgres
    restart: unless-stopped
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "127.0.0.1:5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 5s
      timeout: 5s
      retries: 10

  redis:
    image: redis:8-alpine
    container_name: litestate-redis
    restart: unless-stopped
    ports:
      - "127.0.0.1:6379:6379"
    volumes:
      - redisdata:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 10

volumes:
  pgdata:
  redisdata:
  appkeys:
```

## Conceptos clave de compose

| Concepto | Para qué sirve |
|----------|----------------|
| `name: litestate` | Fija el prefijo de red y volúmenes. Sin esto, un rename de carpeta = base vacía. |
| `services:` | Un bloque por contenedor. |
| `build: context:` | Dónde está el Dockerfile. `context: ./LiteState` = el contexto es esa carpeta. |
| `ports: "127.0.0.1:5194:8080"` | `puerto_host:puerto_contenedor`, atado a loopback. |
| `volumes:` | Persistencia. |
| `healthcheck:` | Cómo Docker sabe si el servicio está sano. |
| `depends_on: condition:` | `service_healthy` = espera real, no solo "arrancó". |
| `service_completed_successfully` | Para tareas de un solo uso (como `appkeys-init`). |
| `restart: unless-stopped` | Sobrevive reinicios del servidor. |
| Red implícita | `litestate_default` se crea sola y da los alias por nombre de servicio. |

## ⚠️ El error clásico: la base vacía

Si renombrás la carpeta del repo, compose deduce otro nombre de proyecto, crea **otro
volumen** y la base aparece vacía. Por eso `name:` explícito en la línea 11.

---

# FASE 12 — Persistencia y backups ⭐⭐⭐⭐

## Dónde viven los datos

```
   contenedor postgres
          │
          └── /var/lib/postgresql/data   ← filesystem interno, DESECHABLE
                    │
                    ▼
            volumen  pgdata             ← el que sobrevive
```

## Reglas

1. **Los datos van en volúmenes**, nunca en el contenedor.
2. Un volumen **no es un backup**: si alguien borra una tabla, el volumen también la tiene
   borrada.
3. Solo un backup verificado es un backup.

## Backup con `pg_dump`

```bash
mkdir -p Backups
docker exec litestate-postgres \
  pg_dump -U litestate -d litestate -Fc \
  > Backups/litestate-$(date +%F).dump
```

Para un `.sql` legible (más fácil de inspeccionar, más grande):

```bash
docker exec litestate-postgres \
  pg_dump -U litestate -d litestate \
  > Backups/litestate-$(date +%F).sql
```

## ⚠️ Backup en la base equivocada

Este es un error muy común y muy grave:

```bash
# ❌ MAL: se conecta a la BD llamada "postgres" y no a la tuya
docker exec litestate-postgres \
  pg_dump -U litestate -d postgres > backup.sql

# ✅ BIEN
docker exec litestate-postgres \
  pg_dump -U litestate -d litestate > backup.sql
```

El backup "sale bien" (sin error) pero **está vacío**. Siempre verificá el tamaño del
archivo y su contenido antes de darte por satisfecho.

## Verificar un backup (restaurarlo)

Un backup no se considera válido hasta que se restauró y se revisó:

```bash
# 1. Crear una base descartable
docker exec litestate-postgres \
  psql -U litestate -d postgres -c "CREATE DATABASE litestate_prueba;"

# 2. Restaurar encima
docker exec -i litestate-postgres \
  psql -U litestate -d litestate_prueba < Backups/litestate-2026-09-28.sql

# 3. Verificar contenido
docker exec -it litestate-postgres psql -U litestate -d litestate_prueba -c "\dt"
```

En LiteState esto se hizo y se verificaron **15 tablas** (decisión D-022).

## Automatizar con cron (en el VPS)

```bash
crontab -e
```

```cron
# Backup diario a las 3:07 (minuto 7 para no coincidir con todos)
7 3 * * * cd /opt/litestate && docker exec litestate-postgres pg_dump -U litestate -d litestate -Fc > Backups/litestate-$(date +\%F).dump 2>> /var/log/litestate-backup.log
```

> Always: probá el restore. Un backup no verificado es una suposición.

---

# FASE 13 — Nginx reverse proxy ⭐⭐⭐⭐⭐ ✅ APLICADA (2026-09-30)

**Objetivo:** que Nginx sea la única puerta de entrada, y que la app .NET deje de estar
publicada en el host.

**Estado:** aplicada y verificada. Decisión registrada como D-023.

**Cómo leer esta fase:** la **Parte 1** explica el porqué y el qué, y se lee de corrido como
una clase. La **Parte 2** es el anexo técnico: configs exactas, cambios exactos y tablas de
verificación. No hace falta leer la Parte 2 para entender la fase, pero sí para implementarla
o para consultarla después.

---

# PARTE 1 — LA EXPLICACIÓN

> Nota de método: no arrancamos definiendo qué es nginx. Empezar por la definición es la
> forma más rápida de no entender nada. **Se entiende un problema, no un nombre.**

## 1. El problema que teníamos

Esto es lo que había en la máquina antes de esta fase:

```
Internet  →  nada. Puerto 80 cerrado.
             (nadie de afuera puede entrar)

   Tu PC   →  127.0.0.1:5194   app .NET       ← puerta 1
             127.0.0.1:5432   PostgreSQL     ← puerta 2
             127.0.0.1:6379   Redis          ← puerta 3
```

Lo primero que hay que entender: **"en tu PC" suena seguro, y lo es.** Esos puertos estaban
atados a `127.0.0.1`, que es el loopback, un callejón sin salida. Nadie de la red te podía
tocar.

Pero el problema no es de hoy. Es que **para llegar a internet en el futuro vas a necesitar
abrir un puerto**, y no es opcional: en 2026 una web sin HTTPS no sirve para casi nada, así
que el puerto 80 se va a abrir sí o sí.

La pregunta va a ser: **cuando abras el 80, ¿qué queda expuesto debajo?**

Y la respuesta es todo: Kestrel en 5194, PostgreSQL en 5432, Redis en 6379. En seguridad esto
se llama **amplia superficie de ataque**: no una puerta, tres.

El punto de fondo: **no querías que la app quedara expuesta. Querías que la app quedara
escondida detrás de algo.**

## 2. La idea: el portero de un edificio

Imaginate un edificio de oficinas. No querés que cualquiera que pasa por la calle entre a
tocar los servidores, ni vea la planilla de salarios, ni pueda cortar la luz.

Entonces contratás un portero. Y la regla de oro del edificio es:

> **Nadie, jamás, por ningún motivo, entra sin pasar por el portero.**

| | Sin portero | Con portero |
|---|---|---|
| ¿Quién puede tocar los servidores? | Cualquiera que sepa la dirección | Solo los empleados |
| ¿El visitante sabe dónde están los servidores? | Sí | No |
| ¿Dónde se instala el candado nuevo? | En cada puerta (hay varias) | En el portón, una sola vez |

La observación clave, la que hace que todo lo demás caiga solo:

**El portero no es que "agregue trabajo". El portero es el que hace posible que adentro estés
tranquilo.** El día que instales HTTPS —que es un candado caro y complicado— lo instalás **una
vez, en el portón**, y los servidores de adentro ni se enteran.

Eso es un reverse proxy. Nada más.

> **Bonus etimológico:** la palabra inglesa *host* —que usamos todo el tiempo
> (`localhost`, `Host: localhost`, `hostname`)— viene de la figura de alguien que **hospeda**:
> el que recibe invitados en su casa. Un host es un portero. Docker, nginx y tu computadora
> usan literalmente la misma palabra porque hacen el mismo papel.

## 3. Qué es nginx, con palabras simples

Nginx es **un programa que recibe peticiones HTTP y se las pasa a otro programa**. Eso es
todo. Cuatro cosas que hace, y las cuatro se ven en esta fase:

1. **Recibe** la petición por un puerto (el 80).
2. **Decide** a quién se la pasa: lee un archivo de configuración y busca qué regla coincide.
3. **Agrega contexto**: le dice al otro programa de dónde vino la petición realmente.
4. **Recibe la respuesta, la puede transformar** (comprimirla, por ejemplo) y la devuelve.

La palabra clave es la segunda: **nginx no adivina, nginx obedece un archivo de
configuración.** Toda su inteligencia está en un archivo de texto. Es una buena noticia: es
100% predecible y 100% reversible.

### Vocabulario de la fase

| Palabra | Qué significa, en una línea |
|---|---|
| **proxy** | Alguien en el medio que habla por vos |
| **reverse proxy** | Alguien en el medio que habla por **el servidor** (el cliente ni sabe que está) |
| **puerto** | La puerta numerada de un programa: `80`, `5432`… |
| **DNS** | El libro de direcciones que traduce nombres a IPs |
| **header** | Metadato que viaja *acompañando* a la petición: quién la manda, por qué canal |
| **server block** | El `server { }` de la config: un grupo de reglas para un dominio/puerto |
| **`location`** | Regla dentro del server block: "las peticiones que empiezan por X, hacé Y" |
| **healthcheck** | Chequeo automático y periódico de "¿estoy vivo?" |
| **upstream** | Bloque de nginx que nombra un grupo de servidores detrás del proxy |
| **resolver** | Dirección del DNS a consultar para resolver nombres en runtime |

**¿Por qué "reverse"?** Porque los papeles están invertidos. En un proxy normal, el que usa
el intermediario sos vos (trabaja *para el cliente*). En un reverse proxy, el intermediario
trabaja *para el servidor*, y vos, el visitante, ni sabés que existe. De ahí el nombre.

## 4. Recorrido de UNA petición

Esta es la parte más importante de la fase. Si entendés esto, entendés todo.

Tocás Enter en el navegador con `http://localhost/Operacion`. Esto pasa:

```
   navegador                     nginx                 Kestrel (app)
      │                            │                        │
      │  ① se abre conexión TCP   │                        │
      ├───────────────────────────►│                        │
      │                            │                        │
      │  ② "GET /Operacion"       │                        │
      ├───────────────────────────►│                        │
      │                            │                        │
      │                            │  ③ busca en su config  │
      │                            │     qué regla matchea  │
      │                            │     "todo lo que       │
      │                            │      empieza por /"   │
      │                            │                        │
      │                            │  ④ resuelve el nombre  │
      │                            │     "app" ──────────►  172.18.0.4
      │                            │                        │
      │                            │  ⑤ NUEVA conexión TCP  │
      │                            ├───────────────────────►│
      │                            │  ⑥ reenvía la petición │
      │                            │     + 4 headers extra │
      │                            ├───────────────────────►│
      │                            │                        │  ⑦ Kestrel corre
      │                            │                        │     MVC, consulta
      │                            │  ⑧ vuelve el HTML      │     PostgreSQL
      │                            │◄───────────────────────┤
      │                            │                        │
      │  ⑨ vuelve el HTML          │                        │
      │     (comprimido con gzip)  │                        │
      │◄───────────────────────────┤                        │
```

### Lo que hay que mirar: hay DOS conexiones

La línea ① y la línea ⑤ son **dos conexiones TCP distintas, contra máquinas distintas**.

- El navegador habla con **nginx**, y cree que nginx es la aplicación.
- La aplicación habla con **nginx**, y cree que el visitante es nginx.

**Nadie habla directo con la aplicación.** Eso es literalmente el significado de "reverse
proxy": la aplicación es inalcanzable.

Y de ahí sale, sin esfuerzo, por qué sirve: si borrás la línea ①, no pasa nada, porque nginx
**no depende** de que el navegador esté. Por eso nginx puede seguir vivo cuando la app se
cae, y por eso responde 502 en vez de desaparecer. Lo probamos: matamos la app y nginx
siguió ahí, solo, informando el problema.

### Los 4 headers: el portero le susurra a la app

Kestrel ve la conexión y sabe con certeza una sola cosa: "se conectó nginx". No tiene forma
de saber que del otro lado había un humano. Sin ayuda, la app cree que **vos sos nginx** y,
peor, que **todos los visitantes son vos**.

Por eso nginx, al reenviar, agrega 4 headers con lo que él sí sabe:

| Header | Qué le dice a la app | Por qué importa |
|---|---|---|
| `Host` | qué sitio pidió el visitante | URLs y redirecciones correctas |
| `X-Real-IP` | la IP real del visitante | logs, seguridad, auditoría |
| `X-Forwarded-For` | la IP real **+** la cadena de proxies | en Fase 14 la cadena completa sirve para detectar ataques |
| `X-Forwarded-Proto` | si el visitante entró por `http` o `https` | crítico en la Fase 14, ver abajo |

**Por qué `X-Forwarded-Proto` es el que más duele si falta:** si el visitante entra por HTTPS,
nginx le habla a la app por HTTP (es un enlace interno, sin cifrar). Una app que no sabe
esto cree que el visitante está en HTTP, y entonces, cuando toca generar el cookie de sesión,
le pone `Secure` o no **según una información falsa**. Con HTTPS en producción, el cookie no
se guarda y queda un login que funciona una vez y se pierde al recargar. Ese bug es clásico
y tarda meses en aparecer.

Por eso en la app hay que activar `UseForwardedHeaders()`. Sin eso, los 4 headers llegan… y
se ignoran. Como una carta con el remitente correcto pero sin sello: el contenido está, la
instrucción de "créelo" no.

## 5. "¿Para qué meter algo en el medio si es más lento?"

Pregunta muy legítima: agregamos un salto de red para todo. La respuesta son las 4 cosas que
**solo se pueden hacer una vez, en un mismo lugar**.

**1. Esconder la app.** Es el objetivo principal. Kestrel, PostgreSQL y Redis dejan de tener
puerta. Se ve en `docker compose ps`:

| | Antes | Después |
|---|---|---|
| `litestate-app` | `Up (healthy) 127.0.0.1:5194->8080` | `Up (healthy) 8080/tcp` |
| `litestate-nginx` | — no existía — | `Up (healthy) 127.0.0.1:80->80/tcp` |

`8080/tcp` sin ninguna IP es la confirmación de que ya no hay camino. Cuando llegue la Fase
16, se copia `compose.yaml` al VPS, se cambia **una línea** (`127.0.0.1:80:80` → `80:80`) y
la app sigue sin puerto. Ese es el beneficio.

**2. Cosas que se instalan una vez y le sirven a todos.** HTTPS, gzip, límites de subida,
timeouts, cuotas de peticiones, logs. Hoy se puso gzip con 6 líneas; en la app .NET eso
eran varias líneas de middleware o un paquete externo. Y lo que se agregue mañana sale
igual de gratis.

**3. Compatibilidad.** Todo el mundo de internet habla HTTP en el puerto 80: un navegador, un
TV, un teléfono, un bot de monitoreo. Que la app hable un idioma estándar evita que cada
cliente requiera configuración.

**4. Escalar más adelante.** Nginx reparte la carga entre varias copias de la app cambiando
una línea. Probablemente no se vaya a necesitar, pero la posición sale gratis.

**Y el costo real es casi cero:** un salto dentro de la red de Docker, en el mismo cable. Se
midió todo lo que importa: gzip activo, 200 en todas las rutas, 15 tablas de base intactas.

## 6. La trampa que casi nos muerde

La parte de la fase con más valor a futuro, y que casi todos los tutoriales pasan por alto.

### El error clásico que te enseñan en internet

Casi todo lo que se lee empieza así:

```nginx
upstream litestate_app {
    server app:8080;          # ← el error
}
proxy_pass http://litestate_app;
```

Parece más prolijo: un grupo de servidores con nombre. **No funciona bien en Docker, y se
rompe siempre en el momento menos conveniente.**

### Por qué se rompe

Nginx resuelve los nombres **una sola vez, al arrancar**. Después se queda con la respuesta
para siempre.

Y acá está el problema: cuando hacés `docker compose up -d --build`, el contenedor `app` se
**destruye y se recrea**, y un contenedor nuevo recibe **una IP interna nueva**.

Entonces nginx tiene anotada la IP vieja, la app ya no está ahí, y nginx sigue insistiendo
con una dirección donde ya no vive nadie:

```
[error] connect() failed (111: Connection refused)
```

502. Y no se arregla esperando ni reiniciando la app: hay que reiniciar **nginx a mano**. En
un VPS eso significa que cada deploy deja el sitio caído hasta que alguien se da cuenta. Es
un incidente recurrente, no un problema teórico.

### La analogía que lo explica en 5 segundos

> **`upstream server app:8080` es como mandar una carta a la dirección exacta de Juan.**
> Calle Falsa 123, segundo piso, timbre 4B.
>
> **El `resolver` es como mandar la carta a "Juan".**
> El correo busca dónde vive Juan *hoy*.

La analogía del correo es literal, no un adorno: el correo **resuelve el nombre en el momento
de entregar**, no antes. Y es exactamente lo que hace la solución.

### La solución aplicada

```nginx
resolver 127.0.0.11 valid=10s ipv6=off;

location / {
    set $app_upstream http://app:8080;   # ← una variable
    proxy_pass $app_upstream;           # ← que contiene la URL
}
```

`127.0.0.11` es la dirección del **libro de direcciones interno de Docker**: toda la red de
contenedores es un DNS. Por eso `app` funciona como nombre: no es una IP ni un invento, es el
nombre del servicio en `compose.yaml`.

La clave está en que `proxy_pass` recibe una **variable** en vez de texto fijo. Nginx no puede
resolverla antes de arrancar, así que está obligado a resolverla **en cada petición**, y
siempre encuentra al contenedor que está vivo. `valid=10s` es el caché del DNS y `ipv6=off`
evita que busque en AAAA, que Docker no usa.

**Lo que se pierde:** el *pooling* de conexiones del bloque `upstream` (nginx reutiliza un
socket abierto en vez de abrir uno nuevo).

**Lo que se gana:** el proxy sobrevive a todos los deploys.

Para LiteState el canje es obvio: son peticiones chicas cada 5 segundos (D-009). Ganamos
estabilidad a cambio de fracciones de milisegundo. Y hay una confirmación bonita: en la
prueba con la app detenida, el log mostró `app could not be resolved`, o sea **nginx estaba
buscando el nombre en ese instante**, no lo tenía cacheado desde el arranque.

## 7. Los 3 cambios, uno por uno

Solo tres archivos. Nada más. Los detalles exactos están en la Parte 2.

### 7.1 `nginx/litestate.conf` (nuevo)

La configuración, montada en `/etc/nginx/conf.d/default.conf:ro`. El `:ro` significa que
nginx puede leer el archivo pero no escribirlo: es una capa barata, si algún día un bug de
la app intenta tocar la configuración, no puede.

Las cuatro piezas y qué hace cada una:

| Pieza | Para qué |
|---|---|
| `resolver 127.0.0.11` | buscar `app` en cada request (la trampa del punto 6) |
| `gzip on` | comprimir HTML/CSS/JS |
| `location /` | "todo lo que empiece por / va a la app" |
| `location = /nginx-health` | un endpoint que **no** pasa por el proxy |

`/nginx-health` merece explicación, porque parece un detalle y es una idea de diseño:
responde `nginx ok` **sin tocar la app**. Sirve para separar dos preguntas que se parecen pero
no son iguales: ¿se cayó **nginx**, o se cayó **la app**?

Si el healthcheck de nginx pasara por el proxy y lo que estuviera caído fuera PostgreSQL,
nginx sería marcado como `unhealthy` aunque estuviera perfecto. `docker compose ps` te
mostraría un culpable equivocado y te mandaría a debuggear la pieza equivocada. **Cada
servicio tiene que medir su propia capa.**

### 7.2 `compose.yaml`

Tres cosas: el servicio nuevo, y **eliminamos el `ports:` de la app**.

Esa eliminación es la línea más importante de toda la fase, y es una línea que **se borra**:

```yaml
    # Este bloque se fue:
    ports:
      - "127.0.0.1:5194:8080"
```

`ports:` era exactamente lo que exponía la app. Sin ese bloque no hay camino. No es que esté
"protegido" o "filtrado": es que **físicamente no existe la puerta**.

El `HEALTHCHECK` del `Dockerfile` sigue funcionando sin cambios, y vale la pena entender por
qué: corre **adentro** del contenedor y pregunta a `localhost:8080`. Nunca dependió del puerto
publicado.

### 7.3 `Program.cs`

```csharp
app.UseForwardedHeaders();
```

Tiene que ser **el primer middleware de la línea**. El orden importa y no es un detalle
estético: si va después, los middleware anteriores ya leyeron la petición con la información
falsa, y ya no hay arreglo.

Además, una decisión fina que vale la pena entender porque el reflejo es hacer lo contrario:
**no se limpia la lista de proxies de confianza** de .NET.

.NET viene con una lista de rangos en los que confía por omisión, y `172.16.0.0/12` está en
esa lista. La red de Docker vive en `172.17`–`172.31`, o sea, adentro. Así que **ya confía en
nuestro nginx sin tocar nada**.

Limpiar esa lista habría sido "aceptar headers de proxies de cualquier origen": suena más
abierto, es más inseguro, y acá no aporta nada, porque la app no tiene puerto publicado y no
hay forma de saltarse a nginx. **A veces la mejor decisión de seguridad es la que no se toma.**

## 8. Cómo comprobamos que funciona

Y acá hay una lección de método, tan importante como la técnica.

**El error fácil:** levantar todo, ver que la página carga, y declarar victoria. Eso no
demuestra nada. Demuestra que *algo* anda, que es un test mucho más débil.

Lo que sí demuestra: **provocar el fallo y ver que se comporte bien.** Un sistema no se prueba
cuando funciona, se prueba cuando falla.

| Prueba | Qué rompe | Resultado | **Qué prueba** |
|---|---|---|---|
| Parar la app | el backend | **502** | que nginx realmente proxea |
| Parar PostgreSQL | la base | **200** | que las capas fallan por separado |
| Recrear la app | su IP interna | sigue 200 | que el `resolver` funciona |
| Pedir la IP real | qué cree la app | `172.18.0.1` | que `ForwardedHeaders` se aplica |

### El 502 es la prueba de oro

Hay dos cosas distintas que pueden producir un 200:

- **(a)** nginx sirve la página.
- **(b)** nginx le pasa la página a la app que la generó.

Si la app se cae y seguís viendo 200, estabas en (a): nginx no estaba proxeando nada. Si la
app se cae y ves **502**, estabas en (b). El 502 es la firma de que el proxy está trabajando.

### Una corrección importante (y por qué importa para pensar)

El borrador de esta fase decía: *"con PostgreSQL parado, nginx responde 502"*. **Es falso**;
se probó y dio 200.

El error no es tonto, es revelador: mezclamos dos capas. La realidad es:

| Qué se cae | Qué responde | Quién lo dice |
|---|---|---|
| nginx | nada (no hay quien responda) | nadie |
| la app | **502** | nginx |
| PostgreSQL | **200** o **500** | la propia app |

Cada capa falla con su propio código. Por eso los cuatro servicios tienen healthchecks
separados, y por eso los logs se leen así: si el 502 es tuyo, el problema es la app; si la
página da error de base, el problema es la base.

El error conceptual detrás es uno solo, y vale grabárselo: **"caer la base de datos" no es lo
mismo que "caerse el servidor".** Son dos capas con dos consecuencias distintas, y confundirlas
es la causa número uno de diagnósticos largos.

### Y la prueba de que la app "entiende"

Que nginx *mande* los headers no prueba que la app los *use*. Hay que mirarlo. Se agregó un
endpoint temporal que devuelve lo que la app cree, y se compararon las dos entradas:

| | entrando por nginx | directo a Kestrel |
|---|---|---|
| IP de la conexión | `172.18.0.5` (nginx) | `::1` |
| **visitante que ve la app** | **`172.18.0.1`** | *ninguno* |
| Host que ve la app | `localhost` | `localhost:8080` |

La app sigue viendo `172.18.0.5` como IP física —no puede evitarlo, es la realidad del
socket— pero **reporta `172.18.0.1` como visitante**. Esa diferencia entre las dos IPs es la
prueba de que el header fue honrado. Y después se sacó el endpoint.

El hábito que queda: **no creas que una configuración de proxy funciona porque no viste lo
esperado. Probala y mirá lo que el otro lado realmente cree.**

## 9. Errores que te vas a encontrar

| Síntoma | Causa | Solución |
|---|---|---|
| 502 después de un deploy | `upstream` con la IP vieja | `resolver` + variable en `proxy_pass` |
| 404 en rutas con `/` en el medio | `proxy_pass` con barra final | sacar la barra: `proxy_pass $var;` |
| 413 al subir un archivo | límite default de 1 MB de nginx | `client_max_body_size 20M;` |
| Cookies que no se guardan con HTTPS | falta `X-Forwarded-Proto` | headers + `UseForwardedHeaders()` |
| Todas las IPs en el log iguales | falta `X-Forwarded-For` | headers + `UseForwardedHeaders()` |
| 502 apenas levantás el stack | nginx arrancó antes que la app | `depends_on: condition: service_healthy` |

Los dos primeros son errores **sintácticamente correctos**: `nginx -t` pasa, no hay ningún
aviso, y fallan en producción. Es un recordatorio de que "no tira error" y "funciona" son
cosas distintas.

Por eso la config se valida **antes** de tocar el stack (ver Parte 2, sección 8.4).

## 10. Lo que hay que saber decir de memoria

Si mañana alguien pregunta "¿para qué sirve el nginx?", con estos tres alcanza:

1. **Es la única puerta de entrada.** La app y la base están ocultas; nadie entra sin pasar
   por nginx.
2. **Es donde se instalan las cosas comunes** (HTTPS, compresión, límites) una sola vez, en
   un solo lugar, y el interior ni se entera.
3. **Usa `resolver`, no `upstream`,** para que no se rompa cada vez que se despliega.

Y una cuarta, que es la que más se agradece dentro de seis meses:

4. **Si algo falla, cada capa lo dice con su propio código.** Por eso el 502 es de nginx y el
   error de base es de la app. Si no los distinguís, estás buscando el problema en el lugar
   equivocado.

## 11. Una nota sobre lo que sigue

La Fase 14 (dominio y HTTPS) va a caer en el gol de esta. Todo lo que se hizo hoy existe
**para que eso sea un cambio de una línea**:

```
antes:   HTTP  →  http://IP-del-VPS
después: HTTPS →  https://litestate.com.ar
```

El certificado lo instala nginx. La app ya sabe que hay un proxy, ya confía en sus headers, y
ya tiene `X-Forwarded-Proto` para saber que el visitante está en HTTPS. Si hoy no se hubiera
configurado `ForwardedHeaders`, la Fase 14 habría dado dos días de cookies rotas sin saber
por qué.

Ese es el patrón general de todo el recorrido: **cada fase deja la siguiente preparada.** Por
eso se hacen las cosas ordenadas, y no es por ascética.

---

# PARTE 2 — ANEXO TÉCNICO

Referencia exacta. Se consulta al implementar o al depurar; no hace falta leerla para
entender la fase.

## 1. Archivos que toca la fase

| Archivo | Acción | Rol |
|---|---|---|
| `nginx/litestate.conf` | **nuevo** | la configuración del proxy |
| `compose.yaml` | modificado | servicio `nginx` nuevo; se saca `ports:` de `app` |
| `LiteState/Program.cs` | modificado | `ForwardedHeaders` + `UseForwardedHeaders()` |
| `Docs/…Manual Docker…md` | modificado | esta fase |
| `AI/01_ESTADO.md`, `AI/02_DECISIONES.md` | modificados | D-023 y estado del laboratorio |

## 2. Antes / Después

```
Internet → ?  (nada abierto todavía)      Internet :80/:443
app en 127.0.0.1:5194   ← solo tu PC             │
postgres en 127.0.0.1:5432                        ▼
redis en 127.0.0.1:6379                 ┌──────────────────┐
                                       │  nginx (nuevo)   │  :80
                                       └────────┬─────────┘
                                                │ red interna de Docker
                                                ▼
                                       app:8080   (SIN ports:)
                                                │
                                                ▼
                                         postgres:5432
```

> En el laboratorio nginx publica `127.0.0.1:80` para que solo vos puedas entrar (coherente
> con D-020). **En el VPS esta línea pasa a `"80:80"`** (sin IP delante) porque ahí sí tiene
> que ser público. Es el único cambio de puerto de toda la fase.

## 3. La configuración aplicada

`nginx/litestate.conf`, montado en `/etc/nginx/conf.d/default.conf:ro`. La imagen oficial de
nginx tiene un `include /etc/nginx/conf.d/*.conf` en su `nginx.conf`, así que reemplazar
`default.conf` es toda la integración que hace falta — `nginx.conf` no se toca.

```nginx
resolver 127.0.0.11 valid=10s ipv6=off;

gzip              on;
gzip_vary         on;
gzip_proxied      any;
gzip_min_length   1024;
gzip_types        text/plain text/css text/javascript
                  application/javascript application/json
                  application/xml image/svg+xml;

server {
    listen      80 default_server;
    server_name _;                    # en la Fase 14 pasa a ser el dominio real

    location = /nginx-health {
        access_log off;
        add_header Content-Type text/plain;
        return 200 "nginx ok\n";
    }

    client_max_body_size 20M;

    location / {
        set $app_upstream http://app:8080;
        proxy_pass $app_upstream;
        proxy_http_version 1.1;

        proxy_set_header Host              $host;
        proxy_set_header X-Real-IP         $remote_addr;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        proxy_connect_timeout 5s;
        proxy_send_timeout    60s;
        proxy_read_timeout    60s;
    }
}
```

### Notas de cada línea que no es obvia

| Línea | Por qué |
|---|---|
| `resolver 127.0.0.11` | DNS de Docker. Sin esto, `upstream` rompe en cada deploy (Parte 1, punto 6) |
| `gzip_min_length 1024` | los bodies más chicos no los comprime: no hay ganancia. Por eso un 302 vacío nunca llega comprimido |
| `listen 80 default_server` + `server_name _` | "atendeme cualquier nombre". En Fase 14 pasa a ser el dominio real |
| `proxy_http_version 1.1` | obligatorio para que los headers `Connection` y `Upgrade` sean válidos; sin esto nginx degrada a HTTP/1.0 |
| `X-Forwarded-For` con `$proxy_add_...` | **agrega** la IP a la lista existente en vez de pisarla: así queda trazada toda la cadena de proxies |
| `X-Forwarded-Proto $scheme` | **http/https según lo que vio el cliente**, no según el enlace nginx→app |
| timeouts 60s | el polling es de 5s (D-009), muy holgado; se dejan explícitos para que un cambio futuro no los herede por sorpresa |

### Sobre `set` + variable en `proxy_pass`

Con una variable, `proxy_pass` **no puede llevar ruta**. Escribir `proxy_pass http://app:8080/;`
(con barra final) hace que nginx **reemplace** la URI original en vez de reenviarla, y las
rutas con segmentos empiezan a dar 404. Por eso va `proxy_pass $app_upstream;` a secas.

## 4. Los cambios en compose

```yaml
  nginx:
    image: nginx:1.29-alpine          # versión fijada, nunca `latest`
    container_name: litestate-nginx
    restart: unless-stopped
    ports:
      - "127.0.0.1:80:80"             # en el VPS: "80:80"
    volumes:
      - ./nginx/litestate.conf:/etc/nginx/conf.d/default.conf:ro
    depends_on:
      app:
        condition: service_healthy    # no levanta antes que la app
    healthcheck:
      test: ["CMD", "wget", "--quiet", "--tries=1", "--spider", "http://127.0.0.1/nginx-health"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 5s
```

Y en el servicio `app`, **esto desaparece**:

```yaml
    ports:
      - "127.0.0.1:5194:8080"
```

`postgres` y `redis` **mantienen** su `ports:` en `127.0.0.1` porque `dotnet run` en el host
los necesita (D-016). En el VPS también se pueden quitar.

## 5. `ForwardedHeaders` en la app

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                             | ForwardedHeaders.XForwardedProto
                             | ForwardedHeaders.XForwardedHost;
});

// ...y en el pipeline, SIEMPRE primero:
app.UseForwardedHeaders();
```

`UseForwardedHeaders()` tiene que ser **el primer middleware**: si va después, los anteriores
ya leyeron la petición con los datos falsos.

**No se limpian `KnownNetworks` ni `KnownProxies` a propósito** (ver Parte 1, punto 7.3):
.NET solo acepta headers de proxies de rango privado, y la red de Docker
(172.17–172.31) cae dentro de `172.16.0.0/12`, así que ya confía en el nginx del compose sin
tocar nada. Limpiar esas listas sería aceptar proxies de cualquier origen.

## 6. Verificación

```bash
docker compose up -d --build
docker compose ps
curl -I http://localhost/
docker compose logs -f nginx
```

| Prueba | Resultado esperado | Resultado obtenido |
|---|---|---|
| `curl -I http://localhost/` | 302 → `/Operacion` (redirect del controller, no del proxy) | 302 ✅ |
| `curl http://localhost/nginx-health` | `nginx ok` | `nginx ok` ✅ |
| `curl http://localhost/Home/Privacy` | 200 (healthcheck de la app sigue verde) | 200 ✅ |
| `curl .../css/ls-tokens.css` | 200 | 200 ✅ |
| `curl -H "Accept-Encoding: gzip" .../Home/Privacy` | `Content-Encoding: gzip` | gzip ✅ |
| **`docker compose stop app`** | **502**, y `/nginx-health` sigue 200 | 502 ✅ |
| **`docker compose stop postgres`** | **nginx sigue 200** (no 502) | 200 ✅ |
| Rebuild de la app (cambia su IP interna) | nginx sigue sirviendo, sin reiniciarlo | OK ✅ |

> Nota: `GET /` devuelve **302**, no 200, porque `HomeController` redirige a `/Operacion`. No
> es un efecto del proxy.

## 7. Receta: comprobar que `ForwardedHeaders` se aplica

Agregar temporalmente en `Program.cs`:

```csharp
app.MapGet("/_proxytest", (HttpContext ctx) =>
{
    var xff = ctx.Request.Headers["X-Forwarded-For"].ToString();
    return Results.Text(
        $"Scheme que VE la app : {ctx.Request.Scheme}\n" +
        $"Host que VE la app   : {ctx.Request.Host}\n" +
        $"IP de conexion directa: {ctx.Connection.RemoteIpAddress}\n" +
        $"X-Forwarded-For     : {(string.IsNullOrEmpty(xff) ? "(AUSENTE)" : xff)}\n");
});
```

Comparar:

```bash
curl -s http://localhost/_proxytest                        # por nginx
docker exec litestate-app curl -s http://localhost:8080/_proxytest   # directo
```

Si por nginx aparece `X-Forwarded-For` con una IP y la conexión directa dice `(AUSENTE)`, el
middleware está funcionando. **Sacarlo después.**

---

# FASE 14 — Dominio y HTTPS ⭐⭐⭐⭐⭐

## DNS

El dominio (ej. `midominio.com`) es un nombre que apunta a la **IP del VPS**. Se configura
en el panel del registrador con dos registros:

| Tipo | Nombre | Valor |
|------|--------|-------|
| `A` | `@` | `203.0.113.10` (la IP del VPS) |
| `A` | `www` | `203.0.113.10` |

> La propagación puede tardar hasta 24 h, aunque suele ser minutos.

## Certificados con Let's Encrypt

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d midominio.com -d www.midominio.com
```

Certbot:
1. Se conecta al puerto 80 del VPS.
2. **Verifica que el dominio apunte a esa máquina** (prueba de control del dominio).
3. Instala el certificado.
4. **Configura Nginx solo** para redirigir 80 → 443.
5. Instala un **timer de renovación automática**.

```bash
sudo systemctl status certbot.timer
sudo certbot renew --dry-run    # probar la renovación
```

## Configurar la app para confiar en el proxy

Nginx termina TLS, así que la app ve HTTP. Hay que avisarle:

```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
```

Sin esto, la app cree que siempre entra por HTTP: rompe cookies seguras y redirecciones.

---

# FASE 15 — Seguridad del VPS ⭐⭐⭐⭐⭐

Antes de abrir nada a Internet.

## Qué se abre y qué no

```
Internet
   │
   ├── 22    → SSH            (preferiblemente con clave, no con password)
   ├── 80    → Nginx          ✅ público
   └── 443   → Nginx          ✅ público

   NO:
   ├── 5432 → PostgreSQL      ❌ jamás
   ├── 6379 → Redis           ❌ jamás
   └── 5194 → .NET directo    ❌ jamás
```

## Firewall

```bash
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo ufw status verbose
```

## Claves SSH en vez de contraseñas

```bash
# En tu PC
ssh-keygen -t ed25519 -C "tu@email"
ssh-copy-id usuario@TU_IP

# En el VPS
sudo nano /etc/ssh/sshd_config
```

```text
PasswordAuthentication no
PermitRootLogin no
```

```bash
sudo systemctl restart ssh
```

> 🧠 **Cuidado:** si cerrás el acceso por contraseña **antes** de confirmar que tu clave
> funciona, te quedás afuera del servidor. Probá la clave en una terminal nueva primero.

## Docker y permisos

- Los contenedores con `USER $APP_UID` corren sin root ✅
- La app **no** necesita puertos publicados: la diferencia entre "puerto abierto" en el servidor y "puerto publicado" por Docker es justamente la que importa acá.
- `ufw` y Docker no se llevan bien: Docker **ignora el firewall** cuando publica puertos.
  Por eso atar a `127.0.0.1` (`-p 127.0.0.1:5432:5432`) es más confiable que confiar en `ufw`.

## Actualizaciones

```bash
sudo apt update && sudo apt upgrade -y
sudo apt autoremove
```

---

# FASE 16 — Deploy en el VPS ⭐⭐⭐⭐⭐

## Preparar el servidor

```bash
# 1. Crear un usuario no root
sudo adduser deploy
sudo usermod -aG docker deploy

# 2. Instalar Docker (Fase 1, completa)
# 3. Traer el código
git clone https://github.com/tu-usuario/tu-repo.git /opt/miapp
cd /opt/miapp

# 4. Crear el .env de PRODUCCIÓN
nano .env
```

```bash
POSTGRES_DB=miapp_prod
POSTGRES_USER=miapp
POSTGRES_PASSWORD=<contraseña larga y única>
DataProtection__Keys__Path=/var/lib/miapp-keys
APP_UID=1654
```

> En producción **no** se usan las mismas credenciales que en desarrollo.

## Levantar

```bash
docker compose up -d --build
docker compose ps
curl -I http://localhost/
```

## Publicar el stack completo

```bash
# Repo completo
git clone https://github.com/tu-usuario/litestate.git /opt/litestate

# Permisos
sudo chown -R deploy:deploy /opt/litestate

# Levantar como el usuario deploy (miembro del grupo docker)
cd /opt/litestate
docker compose up -d --build
```

## Comandos de operación diaria

```bash
cd /opt/litestate

docker compose ps                  # estado
docker compose logs -f app         # logs en vivo
docker compose logs --tail=200 app # últimas 200 líneas
docker compose restart app         # reiniciar solo la app (no la base)
docker compose stop                # parar todo
docker compose up -d               # volver a levantar
```

## ⚠️ Sobre las migraciones de EF Core

Migraciones con `dotnet ef` **no funcionan** dentro del contenedor final: la imagen de
runtime no trae el SDK ni la herramienta. Dos opciones:

1. Correr las migraciones desde tu PC apuntando a la base del VPS (túnel SSH).
2. Agregar al compose un servicio de un solo uso con la imagen `sdk` que aplique las
   migraciones antes de arrancar la app.

> Decisión a tomar en esta fase. **No** la resuelvas sin pensar: si aplicás una migración
> mala contra la base de producción, se pierden datos.

---

# FASE 17 — Actualizar versiones ⭐⭐⭐

## El flujo

```
   v1  ──▶  docker build  ──▶  up -d  ──▶  usuarios
   │
   │ modificás código
   ▼
   v2  ──▶  docker build  ──▶  up -d  ──▶  usuarios
```

## Reconstruir y reiniciar solo la app

```bash
cd /opt/litestate
git pull
docker compose up -d --build app
```

> ⚠️ **Nunca uses `docker compose down -v` en producción.** La `-v` borra los volúmenes:
> base de datos, Redis y claves de sesión. Adiós datos. La forma segura es `up -d --build`.

## Verificar antes de dar por buena una versión

```bash
docker compose ps                       # todo "healthy"
docker compose logs --tail=100 app      # sin errores
curl -I http://localhost/               # 200
docker exec litestate-postgres \
  psql -U litestate -d litestate -c "\dt"   # las tablas siguen ahí
```

## Rollback

```bash
git checkout <commit-de-la-version-anterior>
docker compose up -d --build app
```

> El rollback de **código** es fácil. El de **datos** (migraciones) no: por eso las
> migraciones de EF Core en producción requieren una revisión extra.

---

# FASE 18 — Git + CI/CD ⭐⭐⭐⭐

El nivel final: deploy automático.

```
GitHub  ──push──▶  GitHub Actions  ──build──▶  imagen  ──push──▶  VPS (docker compose pull)
```

## Workflow

```yaml
name: Deploy

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Build image
        run: docker build -t ${{ secrets.DOCKER_USER }}/litestate:${{ github.sha }} ./LiteState

      - name: Login to registry
        run: docker login -u ${{ secrets.DOCKER_USER }} -p ${{ secrets.DOCKER_TOKEN }}

      - name: Push
        run: docker push ${{ secrets.DOCKER_USER }}/litestate:${{ github.sha }}
```

Y en el VPS, un **watcher** que detecta la imagen nueva:

```yaml
services:
  watcher:
    image: containrrr/watchtower
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - /opt/litestate/compose.yaml:/compose.yaml
    command: --interval 60 --cleanup
    restart: unless-stopped
```

> ⚠️ **Watchtower es un arma de doble filo**: reinicia contenedores automáticamente.
> Con `compose.yaml` mal escrito, puede **borrar el volumen de la base**. Usalo solo
> después de dominar las fases 11 a 17 a mano, y sacalo antes de llevarlo a producción.

---

# Cheatsheet: los 20 comandos que más vas a usar

```bash
# Levantar / parar
docker compose up -d --build        # construir y levantar
docker compose down                 # parar (conserva datos)
docker compose restart app          # reiniciar un servicio
docker compose ps                   # estado

# Mirar
docker compose logs -f app          # logs en vivo
docker ps -a                        # contenedores
docker images                       # imágenes
docker stats                        # recursos en vivo

# Entrar
docker exec -it litestate-postgres psql -U litestate -d litestate
docker exec -it litestate-postgres bash

# Base
docker exec litestate-postgres pg_dump -U litestate -d litestate > backup.sql

# Limpieza
docker system df
docker system prune                 # ⚠️ no en producción
```

# Cheatsheet: errores que vas a cometer

| Síntoma | Causa probable | Solución |
|---------|----------------|----------|
| La app no conecta a la base | Usás `Host=localhost` en vez de `Host=postgres` | Usá el nombre del servicio (Fase 8) |
| La base aparece vacía | Renombraste la carpeta y cambió el nombre del proyecto | Fijá `name:` en compose (Fase 11) |
| `UnauthorizedAccessException` al arrancar | El volumen es de root y la app corre como `APP_UID` | El servicio `appkeys-init` (Fase 10) |
| Se cae la sesión de todos | Se recreó el contenedor y cambiaron las claves | Volumen `appkeys` (Fase 10) |
| `dotnet restore` falla en el build | `obj/` del host con rutas absolutas | `.dockerignore` con `obj/` (Fase 9) |
| El build tarda una eternidad | Se copió todo el código antes del csproj | `COPY csproj` primero (Fase 9) |
| Backup de 0 bytes | `pg_dump -d postgres` en vez de tu base | Verificar contenido, no solo el exit code (Fase 12) |
| El puerto 5432 está abierto en Internet | `-p 5432:5432` en vez de `-p 127.0.0.1:5432:5432` | Atar a loopback (Fase 4) |
| `docker compose down -v` y perdiste la base | La `-v` borra volúmenes | Nunca usarla en producción (Fase 17) |
| El puerto 5194 no abre | La app escucha en el puerto equivocado dentro del contenedor | `ASPNETCORE_HTTP_PORTS=8080` (Fase 9) |

# Glosario

| Término | Qué es |
|---------|--------|
| Imagen | Paquete de solo lectura con un programa y su sistema |
| Contenedor | Instancia corriendo de una imagen |
| Dockerfile | Receta para construir una imagen |
| Build multi-stage | Imagen que compila en una etapa y ejecuta en otra, más chica |
| Volumen | Disco persistente que sobrevive al contenedor |
| Bind mount | Montar una carpeta de tu disco dentro del contenedor |
| Red de Docker | Red privada donde los contenedores se ven por nombre |
| Alias / DNS interno | Nombre con el que un contenedor ve a otro (`Host=postgres`) |
| Compose | Archivo YAML que describe y conecta varios contenedores |
| `healthcheck` | Cómo Docker sabe si un servicio está sano |
| `depends_on` | Orden de arranque entre servicios |
| `restart: unless-stopped` | Política de reinicio tras reinicio del servidor |
| Reverse proxy | Servidor que recibe peticiones y las reenvía a otro contenedor |
| `proxy_pass` | Directiva de Nginx que define a dónde reenvía |
| TLS/SSL | Cifrado del tráfico HTTP |
| Let's Encrypt | Entidad que emite certificados gratis |
| DNS | Traduce el dominio a la IP del servidor |
| UFW | Firewall de Linux |
| SSH | Acceso remoto seguro al servidor |
| CI/CD | Integración y despliegue continuos automatizados |

---

## Related

- `Docs/Manual PostgreSQL basico.md` — SQL y conexión desde C#
- `AI/02_DECISIONES.md` — decisiones tomadas en cada fase (D-015 a D-022)
