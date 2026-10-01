FORMATO: 1.0

## Reglas

Guardar problemas conocidos que no estan resueltos.

Un problema resuelto se mueve a RESUELTOS con causa y solucion: es patron
a no repetir.

## Problemas

- P-001: LiteStateDbContext vacío: no hay modelo de datos ni conexión a SQL
  Server, los datos de Operación están hardcodeados en OperacionService. Sintoma:
  nada persiste entre sesiones. Causa sospechada: proyecto en bootstrap, sin
  EF Core configurado. Afecta a: Operación, Monitores, Histórico.
- P-002: Entidades de Domain (EstadoActual, Evento) están vacías. Sintoma:
  no hay campos definidos para persistir. Causa sospechada: bootstrap inicial.
  Afecta a: todo el modelo de datos.
- P-003: No existe autenticación ni roles. Sintoma: /Operacion/Index accesible
  sin login; OperacionService asume "Juan Pérez". Causa sospechada: Identity no
  implementada. Afecta a: Operación, Administración.
- P-004: El ViewModel de Operación tiene los labels de campos operativos
  quemados ("Orden", "Lote", "Cantidad"); no provienen de la configuración del
  sector. Sintoma: todos los sectores muestran los mismos campos. Causa
  sospechada: no hay tabla Sectores aún. Afecta a: Operación.
- P-005: El selector de sector cambia el panel via GET sin POST (ObtenerPanelSector
  es GET). Sintoma: el "último sector usado" no se persiste; el flujo oficial
  define CambiarSector POST aparte. Causa sospechada: convivencia de dos
  mecanismos de cambio de sector. Afecta a: Operación.

## RESUELTOS

- P-006: nginx con bloque `upstream { server app:8080; }` devolvía 502
  permanentes después de cada `docker compose up --build`. Causa: nginx resuelve
  los nombres UNA sola vez al arrancar; recrear el contenedor `app` le asigna una
  IP interna nueva y nginx sigue hablando con la vieja. Solución: `resolver
  127.0.0.11 valid=10s ipv6=off` + `set $var http://app:8080` y
  `proxy_pass $var`, para que la resolución ocurra en cada request. Se pierde el
  pooling de conexiones del `upstream`; para LiteState (peticiones chicas cada 5s,
  D-009) el canje es favorable. Patrón a no repetir en cualquier config de
  nginx dentro de Docker. Ver D-023 y la Fase 13 del manual.

- P-007: la Fase 13 afirmaba que con PostgreSQL detenido nginx respondía 502.
  Es falso: da 200 (o 500 si la página consulta la base). Causa: confusion entre
  capas. El 502 significa "no pude hablar con la app", nunca "se cayó la base".
  Solución: verificado que cada capa falla con su propio código y que por eso los
  4 servicios tienen healthchecks separados. Patrón a no repetir: "caer la base de
  datos" y "caerse el servidor" son fallos distintos con diagnósticos distintos.
