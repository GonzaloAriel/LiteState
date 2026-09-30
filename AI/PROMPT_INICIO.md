# PROMPT DE INICIO - LITESTATE

Eres un agente de desarrollo del proyecto LiteState, un sistema web para
visualizar en tiempo real el estado operativo de una empresa (Linux + .NET 10 + PostgreSQL + Kestrel/Nginx + Docker).

Root del proyecto: /home/gonzo50/Proyectos/Proyecto LiteState/LiteState
Proyecto Web:    /home/gonzo50/Proyectos/Proyecto LiteState/LiteState/LiteState

=====================================================
ARRANQUE (lectura minima obligatoria, siempre)
=====================================================
Lee en orden estos 2 archivos. Con ellos tenes contexto actualizado al minimo costo:
1. AI\00_MEMORIA.md  -> indice, stack, arquitectura, plan activo, rutas de lectura
2. AI\01_ESTADO.md   -> estado real por modulo (que esta CONSOLIDADO y que falta)

No leas mas archivos de arranque. Los SESION_*.md son resumenes ya consolidados
en los registros: solo leerlos si la tarea lo pide.

=====================================================
RUTAS DE LECTURA POR TIPO DE TAREA
=====================================================
Despues del arranque, lee segun corresponda:

- MODULO NUEVO / CRUD nuevo:
    AI\02_DECISIONES.md (reglas de arquitectura vigentes)
    AI\03_PROBLEMAS.md  (patrones a no repetir)

- FRONTEND / UI:
    LiteState/wwwroot/css/ls-tokens.css
    LiteState/wwwroot/css/ls-components.css
    LiteState/wwwroot/css/ls-layout.css
    LiteState/Views/Shared/Components/  (catalogo de componentes)

- MODELO DE DATOS / ENTIDADES / MIGRACIONES:
    /home/gonzo50/Proyectos/Proyecto LiteState/Contexto/Entidades y relaciones.txt
    AI\02_DECISIONES.md (decisiones que afectan el schema)

- NEGOCIO / REGLAS / PRODUCTO:
    /home/gonzo50/Proyectos/Proyecto LiteState/Contexto/Contexto general.txt
    /home/gonzo50/Proyectos/Proyecto LiteState/Contexto/Visual dashboard operario.txt
    /home/gonzo50/Proyectos/Proyecto LiteState/Contexto/Visual de dashboard de monitores.txt

- ROADMAP / PRIORIDADES:
    /home/gonzo50/Proyectos/Proyecto LiteState/Contexto/PASOS DE DESARROLLO (V1).txt

- BUG / DIAGNOSTICO:
    AI\03_PROBLEMAS.md (completo)
    AI\01_ESTADO.md    (modulo afectado)
    AI\02_DECISIONES.md (decisiones del modulo)

- DEPLOY / PRODUCCION:
    /contexto no disponible dentro del repo; consultar al TEAM LEAD.

=====================================================
REGLAS OBLIGATORIAS (aplicar siempre)
=====================================================
1. Arquitectura en capas: Request → Controller → Application Service → Infrastructure → Base de datos.
   Las Views NUNCA llaman servicios ni repositorios.
2. Modelo de datos: EstadoActual es tabla fisica (1 por sector); Evento es
   append-only (solo INSERT). Al cambiar estado: INSERT Evento + UPDATE EstadoActual.
3. UI: usar SIEMPRE el design system `ls-` (tokens + componentes). Cero colores
   o medidas hardcodeadas. Vistas delgadas que componen componentes existentes.
4. El boton de estado ES la accion de guardar: no existe boton Guardar.
5. Tiemporeal por AJAX polling cada 5s, no SignalR.
6. Si hay duda de arquitectura: NO decidir; detenerte, explicar el problema
   y proponer alternativas.
7. No auto-consolidar etapas: solo el TEAM LEAD confirma CONSOLIDADO.
   No modificar AI\01_ESTADO.md sin confirmacion.
8. Al terminar una tarea: verificar compilacion/tests y actualizar la memoria
   (02_DECISIONES.md si decidiste algo, 03_PROBLEMAS.md si algo falla).
9. Commit solo bajo pedido explicito del TEAM LEAD.
