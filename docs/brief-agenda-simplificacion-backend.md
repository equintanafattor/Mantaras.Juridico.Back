# Simplificación de Agenda, alertas y seguimiento de cobro — brief de backend

Rama: `feat/agenda-entregable-2` (Entregable 2). **No está desplegado ni integrado en `main`.**
Responde al brief de frontend `docs/brief-backend-agenda-simplificacion.md` (repo front, puntos 1 a 6).
Todo se probó **solo en local**, contra una base aislada (ver "Pruebas").

Resumen: no hay endpoints nuevos, no hay migraciones y no cambia el modelo. Todos los cambios de contrato son **aditivos** (campos o parámetros opcionales nuevos), salvo tres cambios de semántica que se piden explícitamente y se detallan en "Cambios de comportamiento".

---

## 1. Contratos

### 1.1 `GET /api/agenda` — parámetros nuevos (opcionales)

| Param | Default | Descripción |
|---|---|---|
| `soloAbiertas` | `false` | `true`: solo `Pendiente`, `EnCurso` y `Pospuesta`. Se combina con `estado` si también se envía. |
| `orden` | `Inicio` | `Inicio`: orden original (FechaInicio, HoraInicio, id). `Referencia`: `FechaVencimiento ?? FechaFin ?? FechaInicio` ascendente, después su hora (vencimiento, fin o inicio; sin hora al final), después prioridad (Urgente → Baja), después id. Un valor inválido devuelve `400`. |

Si no se envían, la respuesta es igual a la anterior.

Consulta única para la vista "Pendientes" (reemplaza las tres consultas por estado):

```
GET /api/agenda?soloAbiertas=true&orden=Referencia&incluirVencimientos=true&hasta=2026-12-06&pageSize=100
```

Para las fichas de cliente o expediente: lo mismo, con `clienteId` o `expedienteId` y sin `hasta`.

### 1.2 Entradas — campos nuevos

En `EntradaAgendaListadoResponse` (filas del listado) y en `EntradaAgendaResponse` (detalle y respuestas de `POST`, `PUT` y `PATCH /estado`):

| Campo | Descripción |
|---|---|
| `estaAtrasada` | Entrada abierta, **sin** `fechaVencimiento`, con `(fechaFin ?? fechaInicio)` anterior a hoy (hora de Argentina). |
| `transicionesPermitidas` | Estados a los que se puede pasar con `PATCH /api/agenda/{id}/estado`. Con esto el front puede dejar de mantener su propia tabla de transiciones. |

Solo en el detalle (`EntradaAgendaResponse`): `clientes`, `casos`, `expedientes` y `responsables`, con la misma forma `RelacionAgendaResponse` del listado (`id`, `nombre`, `referencia`). Los `*Ids` se mantienen por compatibilidad.

Ejemplo de detalle (entrada completada con el vencimiento pasado):

```json
{
  "entradaAgendaId": 79,
  "tipoEntradaNombre": "Audiencia o turno",
  "titulo": "Presentar escrito",
  "estado": "Completada",
  "prioridad": "Normal",
  "fechaInicio": "2026-10-31",
  "fechaVencimiento": "2026-11-02",
  "diasParaVencimiento": -4,
  "estaVencida": false,
  "proximaAVencer": false,
  "estaAtrasada": false,
  "transicionesPermitidas": ["Pendiente"],
  "clienteIds": [5], "casoIds": [5], "expedienteIds": [13], "responsableIds": [1],
  "clientes": [{ "id": 5, "nombre": "Pérez, Ana", "referencia": "38493765" }],
  "casos": [{ "id": 5, "nombre": "Jubilación Pérez", "referencia": null }],
  "expedientes": [{ "id": 13, "nombre": "Pérez c/ ANSES", "referencia": "J-123" }],
  "responsables": [{ "id": 1, "nombre": "Usuario de prueba", "referencia": null }],
  "...": "resto de campos sin cambios"
}
```

Ejemplo de fila (pospuesta y atrasada):

```json
{
  "entradaAgendaId": 76,
  "titulo": "Llamar a la clienta",
  "estado": "Pospuesta",
  "fechaInicio": "2026-11-04",
  "fechaVencimiento": null,
  "diasParaVencimiento": null,
  "estaVencida": false,
  "proximaAVencer": false,
  "estaAtrasada": true,
  "transicionesPermitidas": ["Pendiente", "EnCurso", "Completada", "Cancelada"],
  "clientes": [ ... ], "casos": [ ... ], "expedientes": [ ... ], "responsables": [ ... ]
}
```

Alta mínima (sin cambios de contrato; se confirma qué es obligatorio): `POST /api/agenda` con `tipoEntradaAgendaId`, `titulo` y `fechaInicio`. Prioridad `Normal`, estado `Pendiente`, relaciones vacías y los avisos predeterminados del tipo se aplican solos.

```json
{ "tipoEntradaAgendaId": 3, "titulo": "Llamar a cliente", "fechaInicio": "2026-11-16" }
```

### 1.3 `GET /api/recordatorios-agenda` — parámetro y campo nuevos

| | Descripción |
|---|---|
| `soloEntradasAbiertas` (query, default `false`) | `true`: excluye los avisos de entradas `Completada` o `Cancelada`. **No modifica** los avisos (opción "b" del brief de front). |
| `entradaEstado` (respuesta) | Estado de trabajo de la entrada del aviso. |

Para "Alertas sin atender": `GET /api/recordatorios-agenda?estado=Vencido&soloEntradasAbiertas=true`.

```json
{
  "recordatorioAgendaId": 25,
  "entradaAgendaId": 79,
  "entradaTitulo": "Presentar escrito",
  "entradaEstado": "Completada",
  "estado": "Vencido",
  "atendido": false,
  "fechaProgramadaUtc": "2026-10-31T12:00:00Z",
  "...": "resto sin cambios"
}
```

### 1.4 `GET /api/panel/resumen` — `agenda`

```json
"agenda": { "hoy": 1, "proximos": 2, "vencidos": 1, "atrasados": 1, "elementosProximos": [ ... ] }
```

Las cuatro cifras cuentan **solo entradas abiertas y activas** y no se superponen:

| Cifra | Criterio |
|---|---|
| `vencidos` | Con vencimiento anterior a hoy (igual que `estaVencida`). |
| `atrasados` (nuevo) | Sin vencimiento y con `(fechaFin ?? fechaInicio)` anterior a hoy (igual que `estaAtrasada`). |
| `hoy` | Vencen hoy, o sin vencimiento ocurren hoy. Incluye los eventos de varios días en curso. |
| `proximos` | Vencen, o sin vencimiento empiezan, después de hoy (sin límite de días, como antes). |

`elementosProximos`: ahora incluye los eventos de varios días en curso; `esDeHoy` sigue el mismo criterio que `hoy`. `fechaReferencia` no cambia (`fechaVencimiento ?? fechaInicio`).

`alertas` sigue con `disponible: false` (no se tocó; ver decisiones).

### 1.5 Seguimiento de cobro — `envioCobro`

Mismo contrato que el brief de cobro (`docs/brief-seguimiento-cobro-judicial.md`). Lo que cambia es la semántica de un campo y de un filtro:

| Campo / filtro | Semántica |
|---|---|
| `fechaSeguimiento` | Día 120 **hábil** desde `fechaEnvio`. Excluye sábados, domingos y los inhábiles **activos** del calendario. |
| `diasParaSeguimiento` | Días **corridos** desde hoy hasta `fechaSeguimiento`. Negativo si ya pasó. **No son días hábiles**: rotular como "Faltan N días" o "Venció hace N días", no como "días hábiles". |
| `seguimientoVencido` | `true` solo si el seguimiento está **abierto** (`Pendiente`, `EnCurso`, `Pospuesta`) y `fechaSeguimiento` es anterior a hoy. Con `Completada` o `Cancelada` es siempre `false`, aunque `diasParaSeguimiento` sea negativo. |
| `GET /api/expedientes/cobro?seguimientoVencido=true` | Abiertos con la fecha pasada. |
| `GET /api/expedientes/cobro?seguimientoVencido=false` | Abiertos sin vencer **y** cerrados. |
| Sin `seguimientoVencido` | Todos los envíos activos, como antes. |

```json
"envioCobro": {
  "envioCobroExpedienteId": 17,
  "tipoDocumento": "Cedula",
  "fechaEnvio": "2025-12-01",
  "entradaAgendaId": 84,
  "fechaSeguimiento": "2026-05-18",
  "estadoSeguimiento": "Completada",
  "diasParaSeguimiento": -172,
  "seguimientoVencido": false,
  "...": "resto sin cambios"
}
```

### 1.6 Errores

Todas las respuestas de error usan ahora la misma forma (antes el `500` devolvía `{ statusCode, mensaje }`):

```json
{ "errors": [ { "code": "Agenda.ReglaDeSistema", "message": "Esta regla la aplica el sistema..." } ] }
```

Los errores de validación automática (`400` de FluentValidation) siguen con la forma `ProblemDetails` (`errors` como diccionario), que el `apiClient` del front ya interpreta.

Códigos nuevos:

| Código | HTTP | Cuándo |
|---|---|---|
| `Agenda.ReglaDeSistema` | 400 | `POST /api/reglas-vencimiento/{id}/aplicar` sobre una regla con `codigo` (por ejemplo `SEGUIMIENTO_COBRO_JUDICIAL`). Editarla desde Configuración sigue permitido. |
| `General.RegistroConcurrente` | 409 | Un índice único rechazó un guardado y el servicio no pudo resolverlo solo. Sugerido: "Actualice la pantalla y vuelva a intentarlo". |
| `General.ErrorInterno` | 500 | Error no controlado (antes sin código). |
| `General.ReglaDeNegocio` | 400 | `BusinessException` (antes sin código). |

---

## 2. Cambios implementados y motivo

| # | Cambio | Motivo |
|---|---|---|
| 1 | Criterio único `CriteriosAgenda` (abierta, vencida, atrasada, próxima, transiciones) usado por Agenda, panel, motor de reglas y cobro. | Antes había tres criterios distintos: Agenda ignoraba lo cerrado, el panel contaba como vencido lo que no tenía vencimiento y cobro no miraba el estado. |
| 2 | `soloAbiertas` y `orden=Referencia` en `GET /api/agenda`. | La vista "Pendientes" hacía 3 consultas (una por estado) y leía todas las páginas. Ahora es una sola consulta paginada y ordenada. |
| 3 | `estaAtrasada` y `transicionesPermitidas` en entradas. | El front calculaba "atrasada" y repetía la tabla de transiciones. |
| 4 | Relaciones con nombre en el detalle. | Al abrir desde una alerta o desde cobro se mostraba "ID 123" o "Usuario no disponible". |
| 5 | Panel: `vencidos` y `atrasados` separados; eventos en curso cuentan como `hoy`. | El panel y la Agenda daban números distintos. |
| 6 | `soloEntradasAbiertas` y `entradaEstado` en avisos. | "Alertas sin atender" mostraba avisos de cosas ya resueltas, sin tocar datos ni trazabilidad. |
| 7 | `seguimientoVencido` y su filtro siguen el criterio de Agenda. | Un seguimiento completado aparecía en "Plazo vencido". |
| 8 | `/aplicar` rechaza reglas de sistema. | Aplicarla a mano creaba un segundo seguimiento con alerta y sin envío. |
| 9 | Concurrencia: los guardados que chocan con un índice único se reintentan una vez con datos frescos. | Riesgo señalado en el brief de cobro: una carrera podía terminar en `500`. Ver sección 3. |
| 10 | Middleware de errores con `errors[].code`. | Códigos estables también para `409` y `500`. |

Sin cambios: reglas de cálculo, recurrencias, recordatorios predeterminados y personalizados, reglas configurables, calendario de inhábiles, asociaciones múltiples, responsables, auditoría.

---

## 3. Estados, vencidos, avisos y concurrencia

**Estado de trabajo** (`estado`): `Pendiente`, `EnCurso`, `Pospuesta` (abiertas) y `Completada`, `Cancelada` (cerradas). Transiciones (sin cambios): entre abiertas, todas; de abierta a cerrada, sí; de cerrada solo se puede volver a `Pendiente` ("Reabrir"). Pedir el mismo estado que ya tiene devuelve `200` sin cambios.

**Condición** (calculada, en hora de Argentina; solo para abiertas):

- `estaVencida`: tiene vencimiento y ya pasó.
- `estaAtrasada`: no tiene vencimiento y su fecha (fin o inicio) ya pasó.
- `proximaAVencer`: vence entre hoy y 7 días.
- Cerradas: las tres en `false`. `diasParaVencimiento` se informa igual (puede ser negativo).

**Posponer**: `PATCH /estado` con `Pospuesta` no cambia fechas. Si además se quiere mover la fecha, hoy son dos llamadas (`PUT` y `PATCH /estado`). Una entrada pospuesta con la fecha pasada sigue vencida o atrasada.

**Avisos**: su estado (`Pendiente`, `Vencido`, `Atendido`) depende solo de la hora programada y de si se atendió; **no cambia** al cerrar la entrada. Por defecto se siguen listando los avisos de entradas cerradas (como antes); con `soloEntradasAbiertas=true` se ocultan. Si deben marcarse como atendidos al cerrar la entrada es una decisión pendiente (sección 4).

**Concurrencia y duplicados**:

- `PUT /api/expedientes/{id}` con `envioCobro`: si dos guardados simultáneos chocan, el perdedor se reintenta. Si el envío ya registrado coincide, devuelve `200` con ese envío; si no coincide, `400 Expedientes.EnvioCobroYaRegistrado`. Probado con 8 solicitudes simultáneas idénticas (todas `200`, un solo envío) y 8 con datos distintos (`200` o `400`, nunca `500`). Antes de este ajuste se reprodujo un `400 SeguimientoCobroYaGenerado` en la carrera idéntica; ya no ocurre.
- `POST /api/reglas-vencimiento/{id}/aplicar` con la misma `claveIdempotencia`: 8 simultáneas dan una sola generación y todas responden con ella.
- `POST /api/agenda` **no tiene clave de idempotencia**: un doble clic crea dos entradas. Agregarla implica una columna nueva; se propone deshabilitar el botón mientras se guarda (ver decisiones).

---

## 4. Decisiones pendientes

Ninguna se resolvió en este trabajo. Se informa qué hace hoy el backend.

**Cobro** (las mismas del brief de cobro, sin cambios):

1. Fecha base definitiva. Hoy: `fechaEnvio`.
2. Corrección de fecha. Hoy: `400 Expedientes.EnvioCobroYaRegistrado`.
3. Anulación. Hoy no existe; `envioCobro: null` no borra.
4. Reenvío. Hoy se rechaza igual que la corrección.
5. Registro de ambos documentos. Hoy: un solo envío activo por judicial.
6. Cierre del seguimiento y salida del listado. Hoy: completar o cancelar la entrada **no** saca al judicial del listado. Solo deja de figurar como vencido (`seguimientoVencido: false`) y aparece con `seguimientoVencido=false`.
7. Cambios de la regla o del calendario sobre seguimientos existentes. Hoy no se recalculan; solo afectan envíos nuevos.

**Agenda y avisos** (nuevas, surgidas de esta revisión):

8. Avisos de entradas cerradas: ocultarlos (hoy, con `soloEntradasAbiertas=true`) o marcarlos como atendidos al cerrar (opción "a", no implementada).
9. Panel `alertas`: sigue `disponible: false`. Propuesta: contar los avisos vencidos sin atender de entradas abiertas (depende del punto 8).
10. Idempotencia del alta de entradas (`POST /api/agenda`): mitigación en el front o clave de idempotencia en backend (requiere migración).
11. `proximos` del panel no tiene límite de días y `proximaAVencer` usa 7 días. Se mantuvo así; confirmar si el panel debería limitarse.

---

## 5. Migraciones y configuración

- **Sin migraciones nuevas.** `dotnet ef migrations has-pending-model-changes` informa que no hay cambios de modelo. La última sigue siendo `20261027230918_AgregarSeguimientoCobroJudicial`.
- Sin configuración nueva. La API sigue aplicando migraciones al iniciar: antes de levantarla, confirmar a qué base apunta `ConnectionStrings__DefaultConnection`.
- Usuario de prueba para el front: se crea con `BootstrapAdmin__Email`, `BootstrapAdmin__Password` y `BootstrapAdmin__Nombre` como variables de entorno al levantar la API. **No hay credenciales en el repo**; la contraseña se pasa por un canal privado.

---

## 6. Pruebas y límites

Entorno: PostgreSQL 18 local, base aislada `MantarasJuridico_cobro_test` creada vacía para esta prueba. API en `http://localhost:5099` con la cadena de conexión pasada por variable de entorno.

Conexión efectiva confirmada: la base de prueba quedó con 21 migraciones aplicadas; la base de desarrollo `mantaras_juridico` siguió con 20 (no se tocó).

- Compilación: 0 errores, 0 advertencias. El repo no tiene proyecto de tests automatizados.
- Script end-to-end contra la API (78 controles), corrido **3 veces seguidas sin fallas**: alta mínima; vencida, atrasada, en curso, próxima y cerradas; transiciones y transición inválida; reabrir y posponer; listado con `soloAbiertas`, `orden=Referencia`, paginación, combinación con `estado` y compatibilidad sin parámetros; avisos con y sin `soloEntradasAbiertas`; cifras del panel; regla de sistema rechazada; `/aplicar` concurrente; envío de cobro retroactivo, completado y filtros; 8 `PUT` concurrentes idénticos y 8 distintos; forma de los errores `404`.
- Cálculo de 120 días hábiles comparado con un cálculo independiente: envío del 27/10/2026 → 28/04/2027 con los inhábiles cargados en la prueba. Además, 10 hábiles desde el 22/12/2026 → 11/01/2027.
- Regresión de recurrencia y avisos predeterminados: una serie semanal de 3 ocurrencias generó un aviso en cada una, a la hora esperada.
- Scripts existentes del repo, contra la misma API: `Probar-CasosAdministrativos` (414), `Probar-HojaResumenCaso` (313), `Probar-Catalogos` (78) y `Probar-Familiares` (627). Todos OK.
- En el log de la API, los únicos `fail:` son registros de EF de las violaciones de unicidad que el código captura y resuelve. No hubo errores no controlados.

Límites:

- Solo local, con datos sintéticos. No se validó en staging ni en producción, ni con el calendario real de inhábiles.
- No se probó el circuito completo con el frontend en el navegador. Queda para coordinar con front usando el usuario de prueba.
- La concurrencia se probó con 8 solicitudes en paralelo contra una sola instancia de la API.

---

## 7. Rama, commits y push

- Rama: `feat/agenda-entregable-2`, al día con `origin/feat/agenda-entregable-2` y `origin/main` (sin conflictos).
- Commit: ver `git log` (un commit con estos cambios, sobre `c16914e`).
- Push: ver el estado informado al entregar.
- No se integró en `main` ni se desplegó.
