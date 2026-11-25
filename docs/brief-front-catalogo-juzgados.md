# Brief front: catálogo de Juzgados

Rama back: `feat/catalogo-juzgados` (desde `main` en `e12fe4a`), integrada en `main`.
Convenciones de siempre: enums como string, fechas `yyyy-MM-dd`, fechas-hora ISO UTC, errores de negocio `ApiErrorResponse { errors: [{ code, message }] }`. Todos los endpoints requieren sesión.

## Resumen de cambios

- Nuevo catálogo `/api/juzgados`, igual a Tipos de beneficio y Tipos de expediente administrativo.
- El expediente judicial deja de guardar el juzgado como texto libre. En los requests, `juzgado` (string) pasa a ser `juzgadoId` (number, opcional). En las responses, `juzgado` se reemplaza por `juzgadoId` + `juzgadoNombre` + `juzgadoActivo`.
- Los textos que ya existían se migran al catálogo y los expedientes quedan vinculados (ver **Migración**).

## 1. Catálogo `/api/juzgados`

| Acción | Método y ruta | Éxito | Errores |
|---|---|---|---|
| Listar | `GET /api/juzgados` | 200 `PagedResponse<JuzgadoResponse>` | 400 |
| Obtener | `GET /api/juzgados/{id}` | 200 `JuzgadoResponse` | 404 |
| Alta | `POST /api/juzgados` | 201 `JuzgadoResponse` | 400, 409 |
| Edición | `PUT /api/juzgados/{id}` | 200 `JuzgadoResponse` | 400, 404, 409 |
| Baja | `DELETE /api/juzgados/{id}` | 204 | 404 |
| Restaurar | `PATCH /api/juzgados/{id}/reactivar` | 204 | 404 |

La baja y la restauración son idempotentes: dar de baja un juzgado que ya está inactivo, o reactivar uno activo, responde 204.

### Parámetros del listado

| Param | Default | Descripción |
|---|---|---|
| `busqueda` | — | Contiene, sin distinguir mayúsculas; los espacios repetidos se colapsan. Máx. 150. |
| `soloActivos` | `true` | Con `false` incluye los inactivos. |
| `page` | `1` | ≥ 1 |
| `pageSize` | `50` | 1 a 100 |

Orden: por nombre y después por id.

### Cuerpos

```json
// POST y PUT
{ "nombre": "Juzgado Federal N° 2 de Paraná" }
```

```json
// JuzgadoResponse
{
  "juzgadoId": 5,
  "nombre": "JUZGADO FEDERAL N° 2 DE PARANÁ",
  "activo": true,
  "fechaCreacion": "2026-11-25T19:40:12.345Z",
  "fechaModificacion": null
}
```

- **Normalización:** como en los otros catálogos, el back recorta, colapsa los espacios (incluido el espacio duro) y guarda el nombre **en mayúsculas**. Mostrá siempre el `nombre` que devuelve la respuesta.
- **Largo máximo:** 500 caracteres después de normalizar, el mismo límite que tenía el texto libre.
- **Duplicados:** dos nombres que solo difieren en mayúsculas o espacios se consideran el mismo. La comparación incluye a los inactivos: si el juzgado existe dado de baja, responde 409 y hay que reactivarlo.
- **`activo`:** se cambia solo con la baja y la restauración, no con el PUT.

### Errores del catálogo

| HTTP | `code` | Mensaje |
|---|---|---|
| 409 | `Juzgados.NombreDuplicado` | Ya existe un juzgado con ese nombre. Si está inactivo, reactivá el registro existente. |
| 404 | `Juzgados.NoEncontrado` | El juzgado solicitado no existe. |
| 400 | `Juzgados.DatosInvalidos` | Mensajes del validador unidos. Solo aparece en el listado (paginación o búsqueda inválida). |

**Ojo:** un nombre vacío o de más de 500 caracteres en POST/PUT lo rechaza la validación automática, igual que en los demás catálogos. La respuesta es 400 con formato `ValidationProblemDetails`, no `ApiErrorResponse`:

```json
{ "status": 400, "title": "One or more validation errors occurred.",
  "errors": { "Nombre": ["El nombre del juzgado es obligatorio."] } }
```

## 2. Requests de expediente

Afecta a:

- `POST /api/expedientes` (`CrearExpedienteRequest`)
- `PUT /api/expedientes/{id}` (`ActualizarExpedienteRequest`)
- `POST /api/casos/con-expediente-principal`, dentro de `expediente` (`CrearExpedientePrincipalRequest`)

```diff
- "juzgado": "Juzgado Federal N° 2 de Paraná",
+ "juzgadoId": 5,
```

- **Obligatoriedad:** sin cambios. Antes el juzgado era opcional para todos los tipos de expediente (Principal, Incidente, Apelación, Ejecución) y lo sigue siendo. `null` u omitido equivale a "sin juzgado".
- `juzgadoId` debe ser > 0. Si no, la validación automática responde 400 con "El juzgado informado no es válido.".
- **Alta:** solo acepta juzgados **activos**.
- **Edición (`PUT`):** `juzgadoId` usa la semántica "informado", igual que `tipoBeneficioId` en la edición de casos:

| Lo que manda el front | Resultado |
|---|---|
| Campo omitido | Conserva el juzgado actual |
| `"juzgadoId": null` | Desvincula el juzgado |
| `"juzgadoId": <id del juzgado actual>` | Lo conserva **aunque esté inactivo** |
| `"juzgadoId": <otro id>` | Lo asigna solo si está activo |

  Para quitar el juzgado hay que mandar `null` explícito. Un cliente viejo que todavía mande `juzgado` (texto) no pierde el vínculo: el back ignora ese campo y conserva el juzgado actual.
- Si se desvincula un juzgado inactivo, después ya no se puede volver a asignar mientras siga inactivo.

### Errores de expediente

Los tres endpoints responden con estos errores:

| HTTP | `code` | Mensaje |
|---|---|---|
| 400 | `Expedientes.JuzgadoNoEncontrado` | El juzgado informado no existe. |
| 400 | `Expedientes.JuzgadoInactivo` | No se puede asignar un juzgado inactivo. |

## 3. Responses con juzgado

Cambian todas las responses que mostraban `juzgado`:

| Response | Endpoint |
|---|---|
| `ExpedienteResponse` | `POST`/`PUT /api/expedientes`, `GET /api/expedientes` |
| `ExpedienteDetalleResponse` | `GET /api/expedientes/{id}` |
| `ExpedienteCobroResponse` | `GET /api/expedientes/cobro` |
| `ExpedienteCasoDetalleResponse` | `GET /api/casos/{id}` → `expedientes[]` |
| `ExpedienteClienteDetalleResponse` | `GET /api/clientes/{id}` → `casos[].expedientes[]` |

```diff
- "juzgado": "JUZGADO FEDERAL N° 2 DE PARANÁ",
+ "juzgadoId": 5,
+ "juzgadoNombre": "JUZGADO FEDERAL N° 2 DE PARANÁ",
+ "juzgadoActivo": true,
```

Sin juzgado, los tres campos vienen en `null`.

### Juzgado inactivo en pantalla

- **Lectura:** un expediente con juzgado dado de baja lo sigue mostrando (`juzgadoNombre`, con `juzgadoActivo: false`). Conviene marcarlo, por ejemplo con "(inactivo)".
- **Selector del formulario de edición:** cargar `GET /api/juzgados?soloActivos=true`. Si el expediente tiene un juzgado inactivo, agregarlo como opción con los datos de la response (`juzgadoId` + `juzgadoNombre`), así se puede guardar sin cambiarlo. No ofrecer inactivos en el alta ni en otros expedientes.
- **Búsqueda:** `GET /api/expedientes?busqueda=` y `/cobro?busqueda=` siguen buscando por juzgado, ahora contra el nombre del catálogo (incluye juzgados inactivos).

## Migración de los datos anteriores

La migración es `20261125192733_AgregarCatalogoJuzgados` y se aplica con `dotnet ef database update`. Hace esto:

1. Crea un juzgado **activo** por cada texto distinto de `Expedientes.Juzgado`, con la misma normalización de la API (espacios y mayúsculas, sin depender del locale de la base). Por ejemplo, `"Juzgado Federal N° 2 de Paraná"`, `"  JUZGADO   FEDERAL n° 2 DE PARANÁ "` y `"juzgado federal n° 2 de paraná"` quedan en un único `JUZGADO FEDERAL N° 2 DE PARANÁ`. `UsuarioCreacion` = `Sistema`.
2. Vincula todos los expedientes, activos y dados de baja, con su juzgado.
3. Los valores vacíos, solo con espacios o `null` quedan con `juzgadoId: null`.
4. Antes de borrar la columna anterior verifica que no haya quedado ningún texto sin vincular. Si quedara alguno, la migración se revierte completa.
5. Elimina la columna `Expedientes.Juzgado`.

Consecuencias visibles:

- Los nombres migrados aparecen en mayúsculas.
- Variantes que no difieren solo en espacios o mayúsculas quedan como juzgados distintos, por ejemplo `N° 2` y `N.º 2`, o abreviaturas. Se pueden corregir desde el ABM: renombrar uno, o dar de baja el sobrante y reasignar sus expedientes. No hay fusión automática.
- La reversión (`Down`) vuelve a crear la columna de texto con el nombre del catálogo. No recupera la escritura original de cada expediente.

## Verificado contra la API real (base aislada)

Clon de la base de desarrollo más 10 expedientes de prueba: variantes de mayúsculas y espacios, espacio duro y tabulador, vacíos, `null`, expediente dado de baja y `ñ`.

- Migración: 7 juzgados, variantes unificadas, vacíos sin vínculo, columna anterior eliminada. `Down` y reaplicación correctas.
- 43/43 verificaciones HTTP de catálogo y expedientes (alta, duplicados, edición, baja/reactivación, inactivos en lectura y edición, desvinculación, errores y formato de validación).
- Tests automatizados: 42/42, incluida la equivalencia entre la normalización SQL de la migración y la de la API, también con collation `C`.

## Decisiones tomadas

- La ruta para restaurar es `PATCH /{id}/reactivar`, como en los demás catálogos, no `/restaurar` como en casos y expedientes.
- El nombre admite hasta 500 caracteres para no truncar ningún texto migrado.
- `juzgadoId` se mantiene opcional para todos los tipos de expediente, como estaba.
