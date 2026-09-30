# LiteState - UI Catalog

Catalogo vivo de componentes del design system `ls-`. ANTES de crear un
componente, buscar aqui. Un componente nuevo se registra aqui el mismo dia.

Prefijo de clases: `ls-`. Tokens: `wwwroot/css/ls-tokens.css`.
Componentes: `Views/Shared/Components/_X.cshtml`.

---

### _PageHeader
Resuelve: bloque de titulo de pantalla (titulo + subtitulo + accion primaria).
Slots: Title (obligatorio) | Subtitle | PrimaryAction | PrimaryActionLabel
Uso:
```razor
@Html.Partial("~/Views/Shared/Components/_PageHeader.cshtml",
    new ViewDataDictionary {
        { "Title", "Clientes" },
        { "Subtitle", "Administra tus clientes" },
        { "PrimaryAction", Url.Action("Create") },
        { "PrimaryActionLabel", "Nuevo Cliente" }
    })
```
Usado en: (pendiente de las primeras vistas CRUD).

---

### _Badge
Resuelve: etiqueta de estado operativo con color semantico.
Slots: Text (obligatorio) | Tone (success|warning|danger|neutral, default neutral)
Uso:
```razor
@Html.Partial("~/Views/Shared/Components/_Badge.cshtml",
    new ViewDataDictionary { { "Text", "PRODUCIENDO" }, { "Tone", "success" } })
```
Usado en: (pendiente).

---

### _EmptyState
Resuelve: pantalla vacia con explicacion y accion. Nunca deja una lista sin guia.
Slots: Title (obligatorio) | Description | ActionUrl | ActionLabel
Uso:
```razor
@Html.Partial("~/Views/Shared/Components/_EmptyState.cshtml",
    new ViewDataDictionary {
        { "Title", "No tienes clientes" },
        { "Description", "Crea tu primer cliente para empezar" },
        { "ActionUrl", Url.Action("Create") },
        { "ActionLabel", "Crear Cliente" }
    })
```
Usado en: (pendiente).

---

### _SectorCard
Resuelve: tarjeta de sector del dashboard TV (nombre, estado dominante, campos
operativos, tiempo desde ultima actualizacion). Finalizados atenuados.
Slots: Nombre | EstadoNombre | Tone (success|warning|danger|neutral) | Campos (HTML ya formateado) | ActualizadoHace
Uso (dentro de `.ls-tv-grid`):
```razor
@Html.Partial("~/Views/Shared/Components/_SectorCard.cshtml",
    new ViewDataDictionary {
        { "Nombre", "PRODUCCION LINEA 1" },
        { "EstadoNombre", "PRODUCIENDO" },
        { "Tone", "success" },
        { "Campos", "Orden: 15422<br/>Lote: A-55" },
        { "ActualizadoHace", "Actualizado hace 2 min" }
    })
```
Usado en: (pendiente del dashboard TV).

---

## Notas

- Ninguna vista escribe media queries propias: lo resuelve el shell/layout (`ls-layout.css`).
- Ningun color o medida hardcodeado: todo via `ls-tokens.css`.
- Componente nuevo solo cuando el patron aparece en >= 2 lugares (ver MANUAL_UI_MODULAR seccion 9).