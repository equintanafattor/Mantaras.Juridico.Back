# Brief front: origen de los avisos y texto del seguimiento de cobro

Estado back: integrado en `main` (rama `feat/origen-avisos`).
Incluye dos migraciones: `RegistrarOrigenAvisos` y `ActualizarTextoSeguimientoCobro`. Convenciones de siempre; requiere sesión.

## Contrato

`RecordatorioAgendaResponse` suma tres campos. Los existentes no cambian (fechas, estado, `atendido`, `fechaCreacion`, etc.). Aplica a todas las respuestas de aviso:

- `GET /api/recordatorios-agenda`
- `POST /api/recordatorios-agenda/entrada/{entradaAgendaId}`
- `PUT /api/recordatorios-agenda/{id}` (reprogramar)
- `PATCH /api/recordatorios-agenda/{id}/atender`

| Campo | Tipo | Descripción |
|---|---|---|
| `origen` | `"Manual" \| "Categoria" \| "Regla" \| "Desconocido"` | Procedencia registrada al crear el aviso. Siempre viene, nunca `null`. |
| `origenId` | `number \| null` | Id de la categoría (`TipoEntradaAgenda`) con `Categoria`, o de la regla (`ReglaVencimiento`) con `Regla`. `null` con `Manual` o `Desconocido`. |
| `origenNombre` | `string \| null` | Nombre **actual** de esa categoría o regla: si se renombra, se ve el nombre nuevo. Se devuelve aunque esté dada de baja. `null` con `Manual` o `Desconocido`. |

```json
{ "recordatorioAgendaId": 501, "entradaAgendaId": 120, "fechaProgramadaUtc": "2026-12-10T12:00:00Z",
  "estado": "Pendiente", "atendido": false,
  "origen": "Regla", "origenId": 7, "origenNombre": "Seguimiento de cobro judicial" }
```

Sugerencia de texto: "Agregado a mano", "Automático · categoría «X»", "Automático · regla «X»" y "Origen no disponible". Mostrá "Origen no disponible" con `Desconocido`, y también con `null` o con el campo ausente, por si el front corre contra un back anterior.

## De dónde sale cada origen

| Camino | `origen` | `origenId` / `origenNombre` |
|---|---|---|
| Aviso agregado a mano (`POST /entrada/{id}`) | `Manual` | `null` |
| Avisos predeterminados de la categoría, al crear una entrada | `Categoria` | categoría de la entrada |
| Cada ocurrencia de una entrada recurrente | `Categoria` | categoría de la entrada |
| Aplicar una regla de vencimiento: avisos de la categoría de la regla | `Categoria` | esa categoría |
| Aplicar una regla de vencimiento: avisos propios de la regla | `Regla` | la regla |
| Seguimiento de cobro (envío de cédula u oficio) | `Categoria` o `Regla` | la categoría "Seguimiento de cobro" o la regla "Seguimiento de cobro judicial" |

- El seguimiento de cobro usa la regla `SEGUIMIENTO_COBRO_JUDICIAL`, así que sus avisos se informan como los de cualquier regla, no con un origen aparte.
- **Coincidencias:** si un aviso de la regla cae en la misma fecha programada que uno de la categoría, se guarda uno solo y queda como `Categoria`, porque la categoría se agrega primero. Pasaba igual antes; ahora queda explícito.
- **Reprogramar y marcar como visto:** conservan el origen, su id y la `fechaCreacion`. Solo cambian la fecha programada o el estado, como hasta ahora.
- **Editar la entrada:** no crea, recalcula ni cambia el origen de sus avisos.
- **Categoría o regla dadas de baja:** el aviso conserva el vínculo y su nombre actual. No se pueden borrar físicamente mientras haya avisos que las referencien.

## Avisos anteriores a esta versión

Todos quedan con `origen: "Desconocido"`, `origenId: null` y `origenNombre: null`. No hay evidencia confiable de su procedencia.

No se deduce por coincidencia de fechas de creación: un aviso agregado a mano en el mismo instante, o una reprogramación, harían que esa deducción fallara. El front debe dejar de deducir el origen por `fechaCreacion`.

## Texto del seguimiento de cobro

- **Seguimientos nuevos:** la nota automática dice "Aviso interno a los 120 días hábiles desde el envío de la cédula u oficio de cobro." en lugar de "Alerta interna…". El texto sale de la descripción de la regla "Seguimiento de cobro judicial".
- **Datos anteriores:** la migración actualiza dos cosas.
  - La descripción de esa regla, solo si sigue con el texto original. Si el estudio la personalizó, no se toca.
  - La nota de las entradas generadas por esa regla, solo si es idéntica al texto automático original.
- **Lo que no se modifica:** las notas editadas, las de otras reglas o entradas manuales, ni ningún otro uso de "alerta".
- La migración no cambia fechas, estados ni campos de auditoría.

## Verificado (base aislada `mantaras_juridico_test_juzgados`)

- **Migración sobre datos existentes:** 114 avisos quedaron como `Desconocido`, con fechas, estado y auditoría iguales (mismo hash antes y después). Se actualizaron 24 notas de cobro y la regla. Las entradas conservan fechas, estados y auditoría, y las notas que no son de cobro no cambiaron. Down y Up dejan los datos idénticos.
- **QA del front contra esta rama:** 28/28. La base aislada puede contener datos de varias corridas.
- **Tests nuevos:** 12, cada uno en una transacción que se revierte, así que no dejan datos. Cubren origen manual, categoría, regla y cobro, la coincidencia regla/categoría, la recurrencia, reprogramar y marcar como visto, el nombre actual con la categoría dada de baja, el histórico desconocido y la restricción de consistencia en la base. También cubren la migración con notas automáticas (pendiente y completada) y con notas personalizadas, otra regla, entradas manuales, una regla personalizada, idempotencia y Down. Suite completa: 69/69.
