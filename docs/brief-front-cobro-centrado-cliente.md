# Brief front: clientes en el listado de cobro

Estado back: integrado en `main` (rama `feat/cobro-centrado-cliente`). Sin migraciones.
Convenciones de siempre; requiere sesión.

## Contrato

`GET /api/expedientes/cobro` mantiene los mismos parámetros (`busqueda`, `soloActivos`, `seguimientoVencido`, `page`, `pageSize`), el mismo orden y el mismo conteo. Cada ítem de `ExpedienteCobroResponse` suma dos campos; los existentes no cambian.

| Campo | Tipo | Descripción |
|---|---|---|
| `clientes` | `ClienteExpedienteCobroResponse[]` | Clientes distintos (por `clienteId`) de todos los casos vinculados al judicial. Nunca `null`: `[]` si no hay casos o clientes. |
| `clientePrincipalId` | `number \| null` | Cliente principal resuelto por el back. `null` si no hay uno inequívoco. |

`ClienteExpedienteCobroResponse`:

```json
{ "clienteId": 12, "nombre": "Ana", "apellido": "Pérez", "nombreCompleto": "Pérez, Ana",
  "dni": "30111222", "cuil": null, "activo": true, "esPrincipal": true }
```

- `nombreCompleto`: "Apellido, Nombre", igual que `CasoClienteResponse`.
- `esPrincipal`: el cliente está marcado como principal (`CasoCliente.EsPrincipal`) en al menos uno de los casos del judicial.
- Orden: primero los principales y después por apellido, nombre e id.
- Se incluyen los casos y clientes dados de baja, para no ocultar la asociación. Usá `activo` para marcarlos.

## Cómo se determina el cliente principal

La relación usada es judicial → `CasosExpedientes` → caso → `CasosClientes` → cliente.

1. Si el judicial tiene un único cliente distinto, ese es el principal, aunque no esté marcado (puede pasar con datos históricos).
2. Si tiene varios, el principal es el único cliente distinto marcado `esPrincipal` en alguno de sus casos. Si el mismo cliente es principal en varios casos, cuenta una sola vez.
3. En cualquier otro caso `clientePrincipalId` vale `null`. Eso pasa cuando no hay clientes, o cuando hay varios clientes y 0 principales, o más de un principal distinto (por ejemplo, dos casos con titulares diferentes).

Con `null` y clientes en la lista, el front muestra los enlaces individuales para elegir. Cada cliente se abre con `/clientes/{clienteId}` (`GET /api/clientes/{clienteId}`).

## Sin cambios

- Una fila por judicial, aunque tenga varios casos o clientes.
- Filtros, orden por vencimiento del seguimiento, paginación y `totalItems` iguales que antes.
- Los clientes vienen en la misma consulta del listado. No hay consultas por fila.
- La búsqueda (`busqueda`) ya miraba los clientes y no cambia. Coincide por:
  - Cliente: nombre, apellido, "Apellido, Nombre", "Nombre Apellido", DNI y CUIL.
  - Expediente: número, carátula, nombre del juzgado, estado legal y observaciones del expediente.
  - Caso: título.

  No busca en las observaciones de casos o clientes.

## Verificado (base aislada)

- Tests de repositorio con datos propios que se borran al terminar. Cubren principal con secundario, ambiguo, el mismo principal en dos casos, sin clientes y sin casos, cliente único sin marca, cliente inactivo, un cliente en varios judiciales, paginación en 3 páginas con total y sin repetidos, y que el listado sea una sola consulta SQL. Suite completa: 57/57.
- Por HTTP: judicial "PRUEBA COBRO SIN CLIENTE" con `clientes: []` y `clientePrincipalId: null`.
- QA del front contra la API real: 86/86. La base aislada puede contener datos de varias corridas.
