# 00_MEMORIA.md

FORMATO: 1.0

## Proyecto

LiteState

Sistema web para visualizar en tiempo real el estado operativo de una empresa, enfocado en monitores/TVs de planta. NO es un ERP, NO gestiona órdenes ni tareas complejas: solo captura y muestra el estado actual de cada sector con información mínima clave.

## Stack

- ASP.NET Core MVC (Razor Views) sobre .NET 10 (LTS)
- Base de datos principal: PostgreSQL (efectiva desde V1 persistente; ver D-015)
- Servidor web: Kestrel + Nginx (reverse proxy) en Linux; sin IIS ni hosting Windows
- Entity Framework Core
- ASP.NET Core Identity (roles: Admin, Supervisor, Operador)
- Bootstrap 5 + Design System propio (prefijo `ls-`)
- JavaScript vanilla (AJAX polling, sin SignalR/React)
- Infraestructura: Docker / Docker Compose (PostgreSQL, Redis), Testcontainers para tests
- Herramientas: .NET CLI + `dotnet watch`, GitHub + GitHub Actions (CI/CD), VS Code + C# Dev Kit

## Arquitectura

Solución `LiteState.slnx` con un solo proyecto Web MVC que contiene 3 capas internas:

```
LiteState/ (Web: Controllers, Views, DTOs, Services, wwwroot, Models)
├── Controllers/
├── Views/
├── Models/
├── LiteState.Application/  (DTOs, Services: lógica de negocio)
├── LiteState.Domain/       (Entidades)
└── LiteState.Infrastructure/ (DbContext, EF Core, repositorios)
```

Flujo: Request → Controller → Application Service → (Infrastructure → PostgreSQL).

Los Services coordinan. Las entidades (Domain) contienen el núcleo del negocio. Los DTOs transportan datos entre capas. Las Views solo muestran y ensamblan: nunca llaman servicios ni repositorios.

Entorno de desarrollo y producción: Linux. La solución corre sobre .NET 10 y se despliega como contenedor Docker detrás de Nginx (Kestrel). Sin dependencias de IIS/Windows.

Nunca romper este flujo.

## Objetivo actual

Pasar de arquitectura conceptual a una V1 real, usable y desplegable. La V1 incluye: Operación (login, selección de sector, estados, 3 campos configurables, usuario multi-sector), Monitores (dashboard general + por sector, polling 5s, estados atenuados), Administración (CRUD Usuarios/Sectores/Estados/Dashboards + asignación UsuarioSector), Histórico (lista simple de eventos con filtros).

Fuera de V1: métricas, gráficos, SignalR, QR, app móvil, reportes, APIs públicas, multiempresa avanzada.

## Plan activo

Estado del ciclo: IMPLEMENTACION.

Siguiente paso: terminar el Módulo Operación (persistencia con EF Core + SQL Server) y luego construir el Dashboard TV.

Ciclo en curso:
1) Operación (estados, campos, comparar CSS por módulo)
2) Monitor / Dashboard TV
3) Administración básica
4) Histórico

## Fuentes de verdad

`/Contexto` (fuera del repo) contiene la documentación conceptual y funcional del producto. No se duplica en AI/.

- `Contexto general LiteState.txt` - visión, concepto, modelo de datos simplificado
- `Contexto general.txt` - visón + UX + arquitectura + entidades agregadas
- `Visual dashboard operario.txt` - pantalla operario
- `Visual de dashboard de monitores.txt` - dashboard TV
- `Arquitectura funcional de modulos.txt` - módulos funcionales
- `Arquitectura tecnica.txt` - stack y estructura técnica
- `Entidades y relaciones.txt` - modelo de datos completo
- `CASOS DE USO REALES (RESUMEN OPERATIVO).txt` - casos de uso
- `PASOS DE DESARROLLO (V1).txt` - alcance V1 y plan
- `Pasos estrategicos antes del codigo.txt` - pasos estratégicos

## Archivos de esta memoria

Leer en este orden segun la tarea.

- 01_ESTADO.md - estado real por modulo. Solo el TEAM LEAD confirma CONSOLIDADO.
- 02_DECISIONES.md - decisiones tecnicas preservadas (IDs D-XXX).
- 03_PROBLEMAS.md - problemas conocidos (IDs P-XXX) y resueltos.

## Rutas de lectura por tarea

Toda tarea: leer este archivo y 01_ESTADO.md.

- Tarea de implementacion: leer ademas 02_DECISIONES.md y las secciones de 03_PROBLEMAS.md que afecten al modulo.
- Tarea de arquitectura o tecnologia: leer 02_DECISIONES.md completo.
- Tarea de diagnostico o bug: leer 03_PROBLEMAS.md completo y 01_ESTADO.md del modulo.
- Tarea de producto o UX: consultar `/Contexto`.
- Tarea de UI / frontend: leer `LiteState/wwwroot/css/ls-tokens.css`, `ls-components.css`, `ls-layout.css` y el catalogo `LiteState/Views/Shared/Components/`.
- Tarea de modelo de datos: consultar `Contexto/Entidades y relaciones.txt` y 02_DECISIONES.md.

## Reglas de la memoria

Solo el TEAM LEAD confirma que una etapa esta terminada, aprobada y funcionando.

Nunca auto-consolidar una etapa.

Una implementacion existente pero no confirmada no es CONSOLIDADO.

Lo que falla se registra en 03_PROBLEMAS.md.

Lo que se decide se registra en 02_DECISIONES.md.

Lo que se confirma se registra en 01_ESTADO.md.

No guardar conversaciones, logs, razonamientos internos ni opiniones temporales.

No duplicar la documentacion de producto.

La memoria permite a cualquier modelo continuar el trabajo sin conocer conversaciones anteriores.