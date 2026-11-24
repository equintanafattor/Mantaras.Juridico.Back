# Brief front: importación de feriados nacionales

Rama back: `feat/importacion-feriados` (desde `main` en `bde4c8b`).
Convenciones de siempre: enums como string, fechas `yyyy-MM-dd`, fechas-hora ISO UTC, errores `ApiErrorResponse { errors: [{ code, message }] }`. Todos los endpoints requieren sesión (igual que el ABM de días inhábiles).

## Flujo

1. El usuario elige un año → `GET .../{anio}/vista-previa`.
2. La pantalla muestra el resumen, los feriados nacionales (`items`) y, en un bloque separado, `requierenRevision`. Permite marcar fechas de ese bloque.
3. Confirmar → `POST .../{anio}` con la `huella` de la vista previa y las fechas de revisión marcadas.
4. Si responde 409 `VistaPreviaDesactualizada`, volver a pedir la vista previa y mostrarla de nuevo.

La vista previa no escribe nada. La confirmación vuelve a descargar la fuente y a leer la base: solo aplica si el resultado coincide exactamente con lo que vio el usuario.

## 1. Vista previa

`GET /api/dias-inhabiles/importacion-nacional/{anio}/vista-previa`

Respuesta real de 2025 (recortada):

```json
{
  "anio": 2025,
  "fuente": {
    "codigo": "argentina.gob.ar",
    "nombre": "Feriados nacionales - Ministerio del Interior (argentina.gob.ar)",
    "url": "https://www.argentina.gob.ar/sites/default/files/holidays-2025-es.json"
  },
  "fechaPublicacionFuente": "2026-06-10T11:49:06Z",
  "consultadoEn": "2026-11-24T22:01:55.9655825Z",
  "huella": "4774250ace6db84e39dee9dec279441d3d56c81997e3729bf5fa9d492786f7c2",
  "calendarioCargado": false,
  "ultimaSincronizacion": null,
  "resumen": {
    "nuevos": 16, "existentes": 0, "actualizables": 0, "conflictos": 0,
    "requierenRevision": 15, "omitidos": 1, "ausentesEnFuente": 0
  },
  "items": [
    {
      "fecha": "2025-01-01",
      "descripcion": "Año Nuevo",
      "categoria": "FeriadoNacional",
      "estado": "Nuevo",
      "registrosFuente": [{ "nombre": "Año Nuevo", "tipo": "inamovible" }],
      "diaInhabilIdExistente": null,
      "fechaExistente": null,
      "origenExistente": null,
      "descripcionExistente": null,
      "activoExistente": null,
      "modificadoManualmenteExistente": null,
      "motivo": null
    }
  ],
  "requierenRevision": [
    {
      "fecha": "2025-03-31",
      "descripcion": "Fiesta de la Ruptura del Ayuno del Sagrado Mes de Ramadán (Id al-Fitr) (c)",
      "categoria": "RequiereRevision",
      "estado": "Nuevo",
      "registrosFuente": [{ "nombre": "Fiesta de la Ruptura del Ayuno del Sagrado Mes de Ramadán (Id al-Fitr) (c)", "tipo": "no_laborable" }],
      "diaInhabilIdExistente": null,
      "fechaExistente": null,
      "origenExistente": null,
      "descripcionExistente": null,
      "activoExistente": null,
      "modificadoManualmenteExistente": null,
      "motivo": "Día no laborable de aplicación particular (por credo o comunidad): no es feriado nacional."
    }
  ],
  "omitidos": [
    { "fecha": "2026-01-01", "descripcion": "Año nuevo", "tipo": "inamovible",
      "motivo": "La fecha no pertenece a 2025; se importa con el calendario de 2026." }
  ],
  "ausentesEnFuente": []
}
```

### Qué es cada bloque

| Campo | Contenido | ¿Se aplica al confirmar? |
|---|---|---|
| `items` | Fechas con al menos un feriado `inamovible` o `trasladable` (Ley 27.399). | Sí, según `estado`. |
| `requierenRevision` | Fechas que solo tienen `no_laborable`, `turistico`, `especial` (alcance regional) u otro tipo desconocido. | Solo las que se envían en `fechasRevisionIncluidas`. |
| `omitidos` | Registros de otro año (cada archivo trae el 1/1 del año siguiente). | Nunca. Se cargan con el calendario de su año. |
| `ausentesEnFuente` | Feriados importados antes que la fuente ya no publica. Mismo formato que `DiaInhabilResponse`. | Nunca se borran. Se informan para que el usuario decida. |

`resumen.nuevos/existentes/actualizables/conflictos` cuentan solo `items`.

Si una fecha viene varias veces en la fuente (2026: el 2/4 es feriado de Malvinas y también día no laborable), se muestra una sola fila: `registrosFuente` trae todos los registros de esa fecha y `descripcion` usa el feriado nacional.

### `estado` de cada ítem

| Estado | Cuándo | Al confirmar |
|---|---|---|
| `Nuevo` | No hay ningún registro para la fecha. | Se crea con origen `FeriadoNacional`. |
| `Existente` | Ya hay un día inhábil activo (manual o importado). | No se toca. Si la descripción difiere, `motivo` lo explica. |
| `Actualizable` | Importado antes, sin cambios manuales, y la fuente cambió la descripción. | Se actualiza la descripción. |
| `Conflicto` | Hay un registro desactivado en esa fecha, o un importado de esa fecha que el usuario movió a otro día (`fechaExistente`). | No se toca: no se reactiva ni se vuelve a crear. |

Sugerencia de UI: `Conflicto` con aviso y `motivo` visible; `Existente` en gris.

### Errores

| HTTP | `code` | Cuándo |
|---|---|---|
| 400 | `DiasInhabiles.DatosInvalidos` | Año fuera de 2000–2100. |
| 404 | `DiasInhabiles.AnioNoDisponible` | La fuente no publica ese año. Hoy solo existen **2025 y 2026**; 2027 da 404. |
| 502 | `DiasInhabiles.FuenteInvalida` | La fuente respondió con un formato inesperado o con un registro ilegible. |
| 503 | `DiasInhabiles.FuenteNoDisponible` | Fuente caída, error HTTP o timeout (15 s). |

En los tres errores de fuente, el calendario guardado no se modifica.

## 2. Confirmar

`POST /api/dias-inhabiles/importacion-nacional/{anio}`

```json
{ "huella": "4774250a…f7c2", "fechasRevisionIncluidas": ["2025-11-21"] }
```

`fechasRevisionIncluidas` es opcional (por defecto `[]`, máximo 100). Solo admite fechas de `requierenRevision`; cualquier otra devuelve 400.

200:

```json
{
  "anio": 2026,
  "fuente": { "codigo": "argentina.gob.ar", "nombre": "…", "url": "…/holidays-2026-es.json" },
  "creados": 17,
  "actualizados": 0,
  "existentes": 0,
  "conflictos": 0,
  "revisionIncluidos": 0,
  "sincronizadoEn": "2026-11-24T22:01:13.356532Z",
  "yaAplicada": false
}
```

Reintentos y doble clic: repetir la confirmación con la misma `huella` y las mismas fechas devuelve 200 con `yaAplicada: true` y las cantidades de la confirmación original, sin escribir nada. Si llegan dos confirmaciones al mismo tiempo, se procesan de a una y la segunda devuelve `yaAplicada: true`.

Si se vuelve a importar un año sin cambios, la nueva vista previa sale con `nuevos: 0`. Su huella coincide con la última confirmación, así que confirmarla devuelve `yaAplicada: true` y no hay cambios.

Errores: los mismos de la vista previa, más:

| HTTP | `code` | Cuándo |
|---|---|---|
| 400 | `DiasInhabiles.DatosInvalidos` | Huella mal formada o fecha de revisión no listada. |
| 409 | `DiasInhabiles.VistaPreviaDesactualizada` | Cambió la fuente o algún registro afectado (alta, edición o baja manual) desde la vista previa. |

## 3. Estado de cobertura del año

`GET /api/dias-inhabiles/importacion-nacional/estado?anio=2026`

```json
{
  "anio": 2026,
  "calendarioCargado": true,
  "fuente": { "codigo": "argentina.gob.ar", "nombre": "…", "url": "…/holidays-2026-es.json" },
  "fechaPublicacionFuente": "2026-10-02T12:42:31Z",
  "primeraSincronizacion": "2026-11-24T22:01:12.882222Z",
  "ultimaSincronizacion": "2026-11-24T22:01:13.356532Z",
  "usuarioSincronizacion": "Emmanuel",
  "diasInhabilesActivos": 17,
  "feriadosImportadosActivos": 17,
  "diasManualesActivos": 0
}
```

Para un año nunca importado: `calendarioCargado: false` y `fuente`, fechas y usuario en `null`. Los contadores igual informan los días manuales del año. Este endpoint no consulta la fuente externa.

## 4. Cambios en `DiaInhabilResponse` (listado y detalle)

Campos nuevos:

```json
{
  "origen": "FeriadoNacional",
  "fuente": "argentina.gob.ar",
  "fechaFuente": "2026-01-01",
  "fechaSincronizacion": "2026-11-24T22:01:12.882222Z",
  "modificadoManualmente": false
}
```

- `origen`: `Manual` | `FeriadoNacional`. Los registros previos a la migración quedan como `Manual`.
- `fechaFuente`: la fecha publicada por la fuente. Puede diferir de `fecha` si el usuario la movió.
- `modificadoManualmente`: pasa a `true` cuando el usuario edita, da de baja o reactiva un importado. Desde ese momento la importación ya no lo modifica. El `origen` sigue siendo `FeriadoNacional`.

El alta, la edición, la baja y la reactivación manuales no cambian. Las ferias y excepciones judiciales se siguen cargando a mano como hasta ahora.

## 5. Advertencias en cálculos

`AplicacionReglaVencimientoResponse` (aplicar regla) y `EnvioCobroResponse` (seguimiento de cobro a 120 días hábiles) suman:

```json
"advertencias": [
  {
    "codigo": "DiasInhabiles.AnioSinCalendario",
    "mensaje": "El cálculo usa días hábiles de 2027, pero ese año no tiene calendario de feriados importado. Revisá la fecha calculada.",
    "anio": 2027
  }
]
```

- Solo para cómputo en `DiasHabiles`. En días corridos siempre va `[]`.
- Cubre todos los años entre la fecha base y la fecha calculada.
- Se guarda al calcular: si después se importa el año, la advertencia y la fecha de lo ya generado no cambian. Los vencimientos ya generados no se recalculan.
- Las generaciones anteriores a esta versión devuelven `[]`.

Ejemplo verificado: una regla de 10 días hábiles desde el 21/12/2026, con 2026 importado y 2027 sin cargar, da el **05/01/2027** con advertencia 2027. El 25/12 se saltea; el 1/1/2027 cuenta como hábil porque no está cargado.

## Reglas de cobertura (resumen)

- Un año está cubierto solo si se confirmó al menos una importación desde la fuente oficial.
- Tener fechas manuales en un año no lo vuelve cubierto.
- Los cálculos usan solo los días inhábiles guardados y activos (importados y manuales). Nunca consultan la fuente externa.

## Migración

`20261124215133_AgregarImportacionFeriadosNacionales`. Solo agrega columnas y una tabla, sin pérdida de datos:

- `DiasInhabiles`: se agregan `Origen` (default 1 = Manual), `Fuente`, `FechaFuente`, `FechaSincronizacion` y `ModificadoManualmente` (default false), más un índice en `FechaFuente`.
- `AgendaGeneracionesReglas`: se agrega `AniosSinCalendario integer[]` (default `{}`).
- Tabla nueva `CalendariosFeriadosImportados`, con una fila por año importado.

Se probó subir y bajar la migración con datos manuales cargados: se conservan en ambos sentidos. La API la aplica sola al arrancar.

## Decisiones tomadas

- Si una edición manual coincide con una importación que actualiza la descripción del mismo registro, queda la última escritura. No se agrega control de concurrencia para ese caso.

## Pendiente o a decidir

- **2027 no está publicado** en la fuente. Los seguimientos de cobro calculados desde mediados de 2026 cruzan a 2027 y van a mostrar la advertencia hasta que se publique y se importe.
- La página del dataset en datos.gob.ar (`jgm-feriados-nacionales`) y su API devolvían 404 al implementar. Se usa directamente el recurso de argentina.gob.ar; la URL es configurable (`FeriadosNacionales:UrlPlantilla`).
- **Etiqueta de los días de revisión incluidos** (por ejemplo, el 21/11 turístico): se guardan con origen `FeriadoNacional`, porque el back no guarda la categoría de la fuente. Definir si necesitan otro origen o etiqueta.
- **Permisos**: hoy cualquier usuario autenticado puede importar, igual que en el ABM manual. Definir si debe ser solo Administrador.
- **Pruebas sin hacer**:
  - Advertencia en `EnvioCobroResponse` probada de punta a punta: está cubierta por el mismo mapeo que la aplicación de reglas, pero no se probó con un expediente real.
  - Caída real de la fuente con la API levantada: solo está cubierta con pruebas automáticas (HTTP 500, timeout, sin red, HTML en lugar de JSON). Se puede probar apuntando `FeriadosNacionales:UrlPlantilla` a una URL inválida.
