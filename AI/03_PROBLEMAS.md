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

(ninguno aún)