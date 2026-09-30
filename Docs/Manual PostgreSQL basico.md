# MANUAL BÁSICO DE POSTGRESQL

Aprende PostgreSQL desde cero: instalar, configurar, usar y conectar a C#.

> Pensado para alguien que conoce lo básico de SQL (SELECT, INSERT, UPDATE, DELETE).
> Se usa **Docker**, porque es la forma más limpia de levantar PostgreSQL en CUALQUIER máquina, sin instalaciones complejas.

---

## 1. ¿Qué es PostgreSQL?

Es un **motor de base de datos**. Un programa que guarda datos en tablas y te deja consultarlos con SQL.

Piénsalo como una **planilla de Excel gigante** en un servidor:
- Las **tablas** son las hojas.
- Las **columnas** son los encabezados (con un tipo: texto, número, fecha...).
- Las **filas** son cada registro (cada dato guardado).

¿Por qué PostgreSQL y no otro? Porque es gratis, robusto, estándar y funciona perfecto con .NET.

---

## 2. Instalación (con Docker)

Docker "empaqueta" programas para que corran en cualquier sistema. PostgreSQL ya viene listo en una imagen oficial.

### Paso 1: Crear un archivo `compose.yaml`

Crea un archivo llamado `compose.yaml` con este contenido:

```yaml
services:
  postgres:
    image: postgres:17-alpine
    container_name: mi-postgres
    restart: unless-stopped
    environment:
      POSTGRES_DB: mi_bd
      POSTGRES_USER: mi_usuario
      POSTGRES_PASSWORD: mi_password
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

### ¿Qué significa cada línea?

| Línea | Significado |
|-------|-------------|
| `image: postgres:17-alpine` | Qué imagen usar (versión 17, versión liviana) |
| `container_name` | Nombre con el que identificamos el contenedor |
| `restart: unless-stopped` | Si la PC se reinicia, el contenedor arranca solo |
| `POSTGRES_DB / _USER / _PASSWORD` | La base, el usuario y la contraseña que se CREAN solos al arrancar |
| `ports: "5432:5432"` | Puerto del contenedor expuesto en tu PC (así C# puede conectarse) |
| `volumes` | Disco donde se guardan los datos (¡sobreviven a reinicios!) |

> `POSTGRES_USER` es como el "root" de la base: el usuario con todos los permisos.

### Paso 2: Levantarlo

```bash
docker compose up -d        # "-d" = en segundo plano (background)
docker compose ps           # Ver el estado (debe decir "Up" y "healthy")
```

Listo, PostgreSQL ya está corriendo.

---

## 3. Cómo entrar a la base (psql)

`psql` es el programa de terminal para hablar con PostgreSQL.

Si NO tenés psql instalado en tu PC, podés usar el que vive dentro del contenedor:

```bash
docker exec -it mi-postgres psql -U mi_usuario -d mi_bd
```

Vas a ver una terminal así: `mi_bd=>`. Ya estás "dentro" de PostgreSQL.

Para salir:

```sql
\q
```

> ¿Por qué `docker exec`? Es como decirle a Docker: "ejecutá este comando dentro del contenedor llamado mi-postgres".

---

## 4. Comandos más usados

Estos se escriben DENTRO de psql (después de ver el prompt `mi_bd=>`).

### Comandos de psql (empiezan con `\`)

```sql
\l                    -- listar todas las bases de datos
\c otra_bd            -- conectarse a otra base
\dt                   -- listar las tablas de la base actual
\d nombre_tabla       -- ver la estructura (columnas) de una tabla
\du                   -- listar usuarios
\q                    -- salir
```

### Comandos SQL (los de siempre, universales)

```sql
CREATE DATABASE ...;
CREATE TABLE ...;
SELECT ...;
INSERT INTO ...;
UPDATE ...;
DELETE FROM ...;
```

> Regla de oro de PostgreSQL: **cada sentencia SQL termina con punto y coma (`;`)**.

---

## 5. Mi primera base de datos

### Crear una base nueva

```sql
CREATE DATABASE tienda;
```

Para usarla, conectate a ella (fuera del `\q`):

```bash
docker exec -it mi-postgres psql -U mi_usuario -d tienda
```

### Crear la primera tabla

```sql
CREATE TABLE productos (
    id       INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre   VARCHAR(100) NOT NULL,
    precio   NUMERIC(10,2) NOT NULL,
    stock    INTEGER DEFAULT 0
);
```

Bajada línea por línea:

- `id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY` → número único que se genera solo (autoincremental). No lo tenés que cargar vos.
- `nombre VARCHAR(100) NOT NULL` → texto de hasta 100 letras, obligatorio.
- `precio NUMERIC(10,2)` → número con 2 decimales.
- `stock INTEGER DEFAULT 0` → número entero; si no lo cargás, vale 0.

### Insertar datos

```sql
INSERT INTO productos (nombre, precio, stock)
VALUES ('Teclado', 25.50, 10);

INSERT INTO productos (nombre, precio, stock)
VALUES ('Mouse', 15.00, 30), ('Monitor', 200.00, 5);   -- varios a la vez
```

### Consultar

```sql
SELECT * FROM productos;                              -- todos los datos
SELECT nombre, precio FROM productos;                 -- solo columnas elegidas
SELECT * FROM productos WHERE stock < 10;             -- con filtro
SELECT * FROM productos ORDER BY precio DESC;         -- ordenado de mayor a menor
```

### Actualizar y borrar

```sql
UPDATE productos SET precio = 22.00 WHERE nombre = 'Teclado';   -- cambiar datos
DELETE FROM productos WHERE nombre = 'Monitor';                 -- borrar fila
```

> ⚠️ **Práctica de seguridad**: `UPDATE` y `DELETE` casi siempre llevan `WHERE`. Sin `WHERE`, ¡modificás o borrás TODA la tabla!

---

## 6. Conectarla a un proyecto en C#

PostgreSQL se conecta a .NET con **Entity Framework Core** (EF Core), el "puente" entre tu código y la base.

### Paso 1: Agregar los paquetes NuGet

Dentro de la carpeta de tu proyecto web ejecutá:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

- El primero es el traductor de EF Core → PostgreSQL.
- El segundo permite crear migraciones.
- El tercero es para login/usuarios (Identity), opcional si usás eso.

### Paso 2: Escribir el "connection string"

Es la dirección que le dice a C# dónde está la base y con qué usuario entrar.

En `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tienda;Username=mi_usuario;Password=mi_password"
  }
}
```

Desglose: `Host=localhost` (está en tu PC) · `Port=5432` (el del compose) · `Database=tienda` (la base que creaste) · `Username` y `Password` (los del compose).

### Paso 3: Crear tus "tablas en C#" (entidades)

Una clase de C# por tabla. Ejemplo:

```csharp
public class Producto
{
    public int Id { get; set; }              // columna id
    public string Nombre { get; set; }       // columna nombre
    public decimal Precio { get; set; }      // columna precio
    public int Stock { get; set; }           // columna stock
}
```

> Por convención, EF Core convierte `Producto` en la tabla `Productos` y cada propiedad en una columna.

### Paso 4: El "puente" → DbContext

Crea una clase que herede de `DbContext`:

```csharp
using Microsoft.EntityFrameworkCore;

public class MiDbContext : DbContext
{
    public MiDbContext(DbContextOptions<MiDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
}
```

> Un `DbSet<Producto>` es la "tabla Productos lista para usar". Lo agregás por cada entidad.

### Paso 5: Registrar la conexión en Program.cs

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MiDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();
app.Run();
```

### Paso 6: Migraciones (crear la tabla desde C#)

Una migración es "la receta de la base": EF Core la genera comparando tus entidades y la aplica a PostgreSQL.

```bash
dotnet tool restore                    # instala la herramienta dotnet-ef
dotnet ef migrations add Inicial      # crea la migración
dotnet ef database update             # la aplica a la base de datos
```

Después de esto, tu base `tienda` ya tiene la tabla `Productos` creada por EF Core.

Verificá con:

```bash
docker exec -it mi-postgres psql -U mi_usuario -d tienda
# dentro de psql:
\dt            -- deberías ver la tabla Productos
```

### Paso 7: Usar la tabla desde C#

```csharp
public class ProductoService
{
    private readonly MiDbContext _db;

    public ProductoService(MiDbContext db) => _db = db;

    public async Task<List<Producto>> ObtenerTodos()
        => await _db.Productos.ToListAsync();

    public async Task Crear(Producto p)
    {
        _db.Productos.Add(p);
        await _db.SaveChangesAsync();   // INSERT
    }
}
```

- `_db.Productos.Add(p)` + `SaveChangesAsync()` = hacer un `INSERT`.
- `_db.Productos.ToListAsync()` = hacer un `SELECT *`.
- Cambiar una propiedad y `SaveChangesAsync()` = hacer un `UPDATE`.
- `_db.Productos.Remove(p)` + `SaveChangesAsync()` = hacer un `DELETE`.

---

## 7. Resumen en 10 pasos

1. Creá `compose.yaml` y corré `docker compose up -d`.
2. Entrá con `docker exec -it mi-postgres psql -U mi_usuario -d mi_bd`.
3. Creá bases con `CREATE DATABASE ...;`.
4. Creá tablas con `CREATE TABLE ...;`.
5. Manipulá datos con `INSERT` / `SELECT` / `UPDATE` / `DELETE`.
6. En C#, agregá los 3 paquetes NuGet.
7. Escribí el connection string.
8. Modelá entidades + `DbContext`.
9. Registrá el `DbContext` en `Program.cs`.
10. `dotnet ef migrations add Inicial` + `dotnet ef database update`.

---

## Glosario rápido

| Término | ¿Qué es? |
|---------|----------|
| Contenedor | Programa aislado de la PC, con su propio sistema mínimo |
| Base de datos | Conjunto de tablas de un mismo dominio |
| Tabla | "Hoja" con columnas y filas |
| Columna | Campo con un tipo (texto, número...) |
| Fila / Registro | Un dato completo de la tabla |
| Connection string | "Dirección" con usuario y contraseña para conectar la base |
| DbContext | Clase-puente entre C# y la base |
| Entidad | Clase de C# que representa una tabla |
| Migración | Script que crea/actualiza tablas según tus entidades |