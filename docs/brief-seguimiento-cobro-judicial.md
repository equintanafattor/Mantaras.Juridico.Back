# Seguimiento de cobro desde el expediente judicial — brief

Rama: `feat/agenda-entregable-2` (Entregable 2). **No está desplegado.**
Lo que se describe acá **solo se probó en local** contra una base aislada. No está validado en staging ni en producción.

Para el front: estos contratos son una **propuesta**. Conviene confirmarlos antes de integrar, sobre todo los puntos de la sección "Decisiones pendientes", porque pueden cambiar el comportamiento.

## Qué hace

Desde el judicial se registra que se envió una **cédula** u **oficio** de cobro, con su fecha de envío. Al guardarlo, en una sola transacción:

1. Se guarda el envío (`EnviosCobroExpedientes`).
2. El judicial pasa a aparecer en el **listado de expedientes de cobro**.
3. El motor de reglas de agenda que ya existe genera una entrada de agenda con vencimiento a los **120 días hábiles** desde la fecha de envío, más una **alerta interna** (recordatorio) para ese día a las 09:00 hora de Argentina.

Los días hábiles excluyen sábados, domingos y los días inhábiles **activos** del calendario (`/api/dias-inhabiles`). No hay un calculador nuevo: se reutiliza el mismo cálculo de `POST /api/reglas-vencimiento/{id}/aplicar`.

Registrar el envío **no** genera ni envía documentos: solo deja constancia de que se envió.

Los judiciales que ya existen y no tienen envío registrado no generan alertas y no entran al listado.

## Endpoints

### 1. Registrar el envío — `PUT /api/expedientes/{expedienteId}` (existente, campo nuevo)

Se agrega el campo opcional `envioCobro` al body actual:

```json
{
  "casoIds": [1],
  "tipoExpediente": "Principal",
  "expedientePadreId": null,
  "numeroExpediente": "J-1",
  "caratula": "Pérez c/ ANSES",
  "juzgado": null,
  "fechaInicio": null,
  "estadoLegal": null,
  "tipoTramite": null,
  "envioCobro": {
    "tipoDocumento": "Cedula",
    "fechaEnvio": "2026-10-27"
  }
}
```

| Campo | Regla |
|---|---|
| `envioCobro` | Opcional. `null` u omitido = **no cambia nada** del envío (no lo borra). |
| `envioCobro.tipoDocumento` | `"Cedula"` u `"Oficio"`. |
| `envioCobro.fechaEnvio` | Obligatoria, `yyyy-MM-dd`. No puede ser posterior a hoy (hora de Argentina). |

Comportamiento:

| Situación | Resultado |
|---|---|
| El judicial no tiene envío | Registra el envío, genera la entrada y la alerta. `200`. |
| Se vuelve a enviar el **mismo** tipo y la misma fecha (reintento, doble clic, guardar sin cambios) | No hace nada nuevo y devuelve el mismo envío. `200`. Sin duplicados. |
| Ya tiene envío y llegan **otros** datos (otra fecha u otro documento) | `400` `Expedientes.EnvioCobroYaRegistrado`. Ver "Decisiones pendientes". |
| El judicial está dado de baja | `400` `Expedientes.EnvioCobroExpedienteInactivo`. |
| La regla de seguimiento está inactiva | `400` `Agenda.ReglaInactiva`. |

Si la respuesta es un error, **no se guarda nada** de la solicitud, tampoco los demás campos del judicial.

### 2. Consultar el judicial — `GET /api/expedientes/{id}` y `GET /api/expedientes` (existentes, campo nuevo)

Las dos respuestas (y la del `PUT`) traen `envioCobro`, que es `null` si el judicial no tiene envío:

```json
"envioCobro": {
  "envioCobroExpedienteId": 1,
  "tipoDocumento": "Cedula",
  "fechaEnvio": "2026-10-27",
  "agendaGeneracionReglaId": 1,
  "reglaVencimientoId": 3,
  "entradaAgendaId": 1,
  "fechaSeguimiento": "2027-04-21",
  "estadoSeguimiento": "Pendiente",
  "diasParaSeguimiento": 176,
  "seguimientoVencido": false,
  "fechaRegistro": "2026-10-27T23:40:12.123Z",
  "usuarioRegistro": "usuario@estudio.com"
}
```

- `fechaSeguimiento`: el día 120 hábil, es decir, el vencimiento de la entrada de agenda.
- `estadoSeguimiento`: el estado de esa entrada (`Pendiente`, `EnCurso`, `Pospuesta`, `Completada`, `Cancelada`).
- `diasParaSeguimiento`: días **corridos** que faltan hasta `fechaSeguimiento`. Es negativo si ya pasó.
- `seguimientoVencido`: `true` si el seguimiento sigue abierto y `fechaSeguimiento` es anterior a hoy. Con `Completada` o `Cancelada` es `false` (ver `docs/brief-agenda-simplificacion-backend.md`).
- `entradaAgendaId`: sirve para abrir la entrada con `GET /api/agenda/{id}` y su alerta con `GET /api/recordatorios-agenda?EntradaAgendaId={id}`.

### 3. Listado de expedientes de cobro — `GET /api/expedientes/cobro` (nuevo)

Query params:

| Param | Default | Descripción |
|---|---|---|
| `busqueda` | — | La misma búsqueda que `GET /api/expedientes`: número, carátula, juzgado, estado legal, observaciones, administrativo y cliente. |
| `soloActivos` | `true` | Excluye los judiciales dados de baja. |
| `seguimientoVencido` | — | `true`: solo vencidos. `false`: solo no vencidos. Omitido: todos. |
| `page` / `pageSize` | `1` / `20` | Máximo 100 por página. |

Está ordenado por `fechaSeguimiento`, de la más próxima o vencida a la más lejana. La respuesta es `PagedResponse<ExpedienteCobroResponse>`:

```json
{
  "items": [
    {
      "expedienteId": 2,
      "casos": [{ "casoId": 2, "titulo": "…", "numeroExpedienteAnses": null, "numeroBeneficio": null, "tipoBeneficioId": null, "tipoBeneficioNombre": null, "tipoBeneficioActivo": null, "activo": true }],
      "tipoExpediente": "Principal",
      "numeroExpediente": "J-2",
      "caratula": "…",
      "juzgado": null,
      "estadoLegal": null,
      "activo": true,
      "envioCobro": { "…": "misma forma que en el punto 2" }
    }
  ],
  "page": 1, "pageSize": 20, "totalItems": 2, "totalPages": 1,
  "hasPreviousPage": false, "hasNextPage": false
}
```

Un judicial aparece en el listado **mientras tenga un envío activo**. Todavía no hay una acción que lo saque del listado (ver "Decisiones pendientes").

### 4. Alerta — endpoints de agenda existentes

No hay endpoints nuevos para la alerta. La alerta es un `RecordatorioAgenda` con canal `Interno` y se consulta con los endpoints de siempre:

- `GET /api/recordatorios-agenda?EntradaAgendaId={entradaAgendaId}` devuelve la alerta en estado `Pendiente`, `Vencido` o `Atendido`.
- `GET /api/agenda/{entradaAgendaId}` devuelve la entrada, con tipo **"Seguimiento de cobro"**, vinculada al judicial y a sus administrativos.

La regla se ve y se puede editar en `GET /api/reglas-vencimiento`. Se identifica por el campo nuevo `codigo = "SEGUIMIENTO_COBRO_JUDICIAL"`, que es `null` en las reglas que cargan los usuarios. Si se edita (por ejemplo, la cantidad de días), el cambio se aplica a los envíos que se registren desde ese momento. Las alertas ya generadas no se recalculan.

## Modelo y migración

Migración: `20261027230918_AgregarSeguimientoCobroJudicial`. Solo agrega; no modifica ni borra datos existentes.

- `ReglasVencimiento.Codigo`: `varchar(50)`, nullable, único cuando no es nulo.
- Tabla nueva `EnviosCobroExpedientes`: `ExpedienteId` (FK), `TipoDocumento` (1 = Cédula, 2 = Oficio), `FechaEnvio` (`date`), `AgendaGeneracionReglaId` (FK, único) y los campos de auditoría.
  - Índice único parcial `ExpedienteId WHERE Activo`: un solo envío activo por judicial a nivel base de datos.
- Siembra idempotente (`ON CONFLICT DO NOTHING`, sin ids explícitos para no chocar con datos cargados por usuarios):
  - Tipo de entrada "Seguimiento de cobro".
  - Regla "Seguimiento de cobro judicial": 120 días hábiles, posteriores a la fecha de envío, prioridad normal.
  - Recordatorio predeterminado de la regla, el día del vencimiento.
- El `Down` borra la tabla y la columna. La regla sembrada se borra solo si nunca generó entradas.

Trazabilidad: envío → `AgendaGeneracionesReglas` (regla, `FechaBase` = fecha de envío, origen `ExpedienteJudicial` con `ExpedienteOrigenId`, usuario, clave `cobro-judicial:{expedienteId}:{tipo}:{fecha}`) → `EntradasAgenda` (vinculada al judicial y a sus administrativos) → `RecordatoriosAgenda`.

## Decisiones pendientes (a acordar)

Para cada punto se indica qué hace hoy el backend y qué se propone.

1. **Fecha base.** Hoy se cuenta desde `fechaEnvio`. Propuesta: mantenerlo. Si se define otra fecha (por ejemplo, la de diligenciamiento), el cambio es acotado.
2. **Corrección de fecha.** Hoy se rechaza con `400`. Propuesta: permitirla mientras el seguimiento esté `Pendiente`. El envío anterior se desactiva, su entrada pasa a `Cancelada` y se genera una nueva desde la fecha corregida. Queda el historial.
3. **Anulación / desmarcado.** Hoy no existe, y mandar `envioCobro: null` no borra el envío. Propuesta: `DELETE /api/expedientes/{id}/envio-cobro`, que desactiva el envío, cancela la entrada y saca el judicial del listado.
4. **Reenvío.** Hoy se rechaza. Propuesta: el reenvío reemplaza al envío vigente (igual que la corrección) y los 120 días hábiles corren desde la nueva fecha.
5. **Envío de ambos documentos.** Hoy se permite un solo envío activo por judicial. Propuesta: un único seguimiento por judicial, contado desde el primer envío, y el segundo documento se registra sin generar otra alerta. Esto implica ajustar el índice único.
6. **Envío retroactivo con los 120 días ya cumplidos.** Hoy se registra igual: la entrada y la alerta quedan en estado **vencido** desde el primer momento y el judicial aparece en el listado con `seguimientoVencido: true`. Propuesta: mantenerlo y que el front avise antes de guardar cuando `fechaEnvio` sea muy antigua.
7. **Cierre del seguimiento.** Hoy no hay cierre: el judicial sigue en el listado aunque la entrada se complete o se cancele (el front puede mostrar `estadoSeguimiento`). Propuesta: que completar o cancelar la entrada de agenda cierre el seguimiento y saque el judicial del listado. La otra opción es una acción explícita del tipo "cobro percibido".

Otros puntos a tener en cuenta:

- Si se cargan días inhábiles **después** de registrar un envío, la fecha ya generada no se recalcula. El motor actual se comporta igual para todas las reglas.
- Si la regla está inactiva, el registro del envío falla con `400` en lugar de guardar un envío sin alerta. Hay que confirmar si ese es el comportamiento buscado.
- Ante dos guardados **simultáneos** del mismo envío, la base de datos garantiza que no haya duplicados. En la prueba con 6 solicitudes en paralelo, todas devolvieron `200` y quedó un solo envío. La solicitud perdedora de una carrera ahora se reintenta y responde `200` con el envío existente, o `400` si los datos difieren (ver `docs/brief-agenda-simplificacion-backend.md`).

## Verificaciones realizadas (solo local)

Entorno: Postgres 16 local, base aislada `MantarasJuridico_cobro_test` (no se tocó la base de desarrollo) y API levantada contra esa base. El repo no tiene proyecto de tests automatizados (`tests/` está vacío), así que la verificación se hizo con un script end-to-end contra la API: **45 de 45 controles OK**.

- **Integración de `origin/main`** en la rama: sin conflictos. Compila con 0 errores y 0 advertencias, y `has-pending-model-changes` no informa cambios pendientes.
- **Migración**: se aplicó sobre datos cargados antes con la migración anterior. Los hashes de `Expedientes`, `CasosExpedientes`, `TiposEntradaAgenda` y de la regla preexistente son idénticos antes y después. También se probó `Down` → `Up`.
- **Cálculo**, comparado contra un cálculo de referencia independiente:
  - Envío del 27/10/2026 → **21/04/2027**: cruza meses y año, con 7 inhábiles en el rango. Sin esos inhábiles la fecha habría sido el 13/04, así que la prueba efectivamente los tiene en cuenta.
  - Un inhábil **inactivo** (20/11/2026) se cuenta como hábil.
  - Un inhábil que cae en **sábado** no se descuenta dos veces.
  - Envío retroactivo del 01/12/2025 → **25/05/2026**: cruza 2025 → 2026 y queda vencido.
  - Regresión de `/aplicar` con 10 días hábiles desde el 24/12/2026 → 11/01/2027.
- **Persistencia y asociaciones**: envío → generación (origen judicial) → regla de sistema → entrada vinculada al judicial y a su administrativo → 1 alerta a las 09:00 de Argentina.
- **Sin duplicados**: 3 reintentos idénticos, un guardado sin `envioCobro` y 6 solicitudes concurrentes dejaron siempre 1 envío, 1 generación, 1 entrada y 1 alerta.
- **Circuito completo**: registro → detalle del judicial → alerta consultable → inclusión en el listado (orden, filtros de vencido y no vencido, búsqueda y paginación). Los judiciales sin envío no aparecen.
- **Validaciones**: fecha futura, tipo de documento inválido, judicial dado de baja y cambio de datos sobre un envío existente devuelven `400`. Cuando una solicitud se rechaza, no se persiste ningún cambio.
- **Regresión** de `POST /api/reglas-vencimiento/{id}/aplicar` después del refactor del motor: cálculo, idempotencia y `404` para una regla inexistente funcionan como antes.

**Pendiente:** validar en staging/producción con datos reales y con el calendario de inhábiles real.
