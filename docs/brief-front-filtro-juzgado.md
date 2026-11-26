# Brief front: filtro por juzgado en el listado de expedientes

Estado back: integrado en `main` (rama `feat/filtro-juzgado`).
Requiere el catálogo de Juzgados (ver `brief-front-catalogo-juzgados.md`). Convenciones de siempre; todos los endpoints requieren sesión.

## Contrato

`GET /api/expedientes` acepta un parámetro nuevo y opcional:

| Param | Tipo | Default | Descripción |
|---|---|---|---|
| `juzgadoId` | number (long) | — | Devuelve solo los expedientes vinculados a ese juzgado del catálogo. |

Los demás parámetros no cambian: `casoId`, `busqueda`, `soloActivos` (default `true`), `page` (default 1) y `pageSize` (default 20, máx. 100). La respuesta sigue siendo `PagedResponse<ExpedienteResponse>`, con los campos `juzgadoId`, `juzgadoNombre` y `juzgadoActivo` en cada ítem.

```
GET /api/expedientes?juzgadoId=5&soloActivos=false&page=1&pageSize=20
```

## Reglas

- **Por identificador:** filtra por `juzgadoId`, no por nombre. Renombrar un juzgado no cambia el resultado.
- **Combinación:** se combina con `casoId`, `busqueda` y `soloActivos` con AND. Por ejemplo, `juzgadoId=5&busqueda=texto` devuelve solo expedientes del juzgado 5 que además coinciden con `texto`. La búsqueda de texto sigue mirando también el nombre del juzgado, pero no reemplaza al filtro.
- **Totales y paginación:** el filtro se aplica antes de contar y paginar. `totalItems`, `totalPages` y `hasNextPage` reflejan el resultado filtrado.
- **Juzgados inactivos:** se pueden usar en el filtro y devuelven sus expedientes vinculados. Ojo: `soloActivos` se refiere al **expediente**, no al juzgado. Para ver también los expedientes dados de baja de un juzgado, mandá `soloActivos=false`.
- **Sin `juzgadoId`:** el comportamiento es el mismo de antes.
- **`juzgadoId` inexistente:** 200 con lista vacía (`items: []`, `totalItems: 0`), igual que `casoId`.
- **`juzgadoId` <= 0:** 400 con formato `ValidationProblemDetails`:

  ```json
  { "status": 400, "title": "One or more validation errors occurred.",
    "errors": { "JuzgadoId": ["El juzgado informado no es válido."] } }
  ```

- **Valor no numérico** (`juzgadoId=abc`): 400 del binding de ASP.NET, con el mismo formato `ValidationProblemDetails`.
- **`/api/expedientes/cobro`:** no recibe este filtro. Si se manda, se ignora.

## Selector del filtro

- Cargar las opciones con `GET /api/juzgados?soloActivos=false&pageSize=100`, porque el filtro admite inactivos. El máximo de `pageSize` es 100: si `totalItems` > 100, paginar con `page` o usar `busqueda` para filtrar por texto.
- Conviene marcar los inactivos, por ejemplo con "(inactivo)", usando `activo` de la respuesta.
- Si el filtro viene de la URL con un id que ya no aparece en el catálogo, el listado responde vacío, no con error.

## Verificado (base aislada)

- Tests de repositorio, cada uno con datos propios que se borran al terminar: filtro solo, juzgado inactivo, juzgado sin expedientes e inexistente, combinaciones con `casoId`, `busqueda` y `soloActivos`, sin filtro, y totales y paginación en 3 páginas. Suite completa: 48/48.
- Por HTTP contra la API real: 19/19, incluidos `totalPages`/`hasNextPage`, valores inválidos (0, negativo, no numérico) y el comportamiento sin filtro.
