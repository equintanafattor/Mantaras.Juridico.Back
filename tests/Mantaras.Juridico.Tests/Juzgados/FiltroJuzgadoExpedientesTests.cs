using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Mantaras.Juridico.Infrastructure.Persistence;
using Mantaras.Juridico.Infrastructure.Persistence.Repositories;
using Mantaras.Juridico.Tests.DiasInhabiles;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Tests.Juzgados;

// Filtro juzgadoId de GET /api/expedientes (ExpedienteRepository.BuscarAsync/ContarAsync).
// Cada test crea sus propios datos con una marca única y los borra al terminar,
// así no depende de lo que haya en la base aislada ni la deja modificada.
[Collection(ColeccionBaseAislada.Nombre)]
public sealed class FiltroJuzgadoExpedientesTests : IAsyncLifetime
{
    private readonly BaseAislada _base;
    private readonly string _marca = "FILTRO " + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    private Escenario _e = null!;

    public FiltroJuzgadoExpedientesTests(BaseAislada baseAislada)
    {
        _base = baseAislada;
    }

    private sealed record Escenario(
        long JuzgadoActivo,
        long JuzgadoInactivo,
        long JuzgadoSinExpedientes,
        long Caso1,
        long Caso2,
        IReadOnlyDictionary<string, long> Expedientes,
        IReadOnlyCollection<long> CasoIds,
        IReadOnlyCollection<long> JuzgadoIds
    );

    public async Task InitializeAsync()
    {
        if (BaseAislada.CadenaConexion is null)
        {
            return;
        }

        await using var db = _base.CrearContexto();
        var ahora = DateTime.UtcNow;

        Juzgado NuevoJuzgado(string sufijo, bool activo) => new()
        {
            Nombre = $"{_marca} {sufijo}", Activo = activo, FechaCreacion = ahora, UsuarioCreacion = "test",
        };

        var jA = NuevoJuzgado("A", true);
        var jB = NuevoJuzgado("B", false);
        var jC = NuevoJuzgado("C", true);
        var k1 = new Caso { Titulo = $"{_marca} caso 1", FaseInterna = "PRUEBA", FechaCreacion = ahora, Activo = true };
        var k2 = new Caso { Titulo = $"{_marca} caso 2", FaseInterna = "PRUEBA", FechaCreacion = ahora, Activo = true };
        db.AddRange(jA, jB, jC, k1, k2);

        // (clave, juzgado, caso, activo). FechaCreacion distinta: el orden del listado es determinista.
        var definiciones = new (string Clave, Juzgado? Juzgado, Caso Caso, bool Activo)[]
        {
            ("uno", jA, k1, true),
            ("dos", jA, k1, true),
            ("tres especial", jA, k2, true),
            ("cuatro", jA, k2, false),
            ("cinco", jB, k1, true),
            ("seis", jB, k2, false),
            ("siete", null, k1, true),
        };

        var expedientes = new Dictionary<string, Expediente>();
        var minuto = 0;

        foreach (var (clave, juzgado, caso, activo) in definiciones)
        {
            var expediente = new Expediente
            {
                Caratula = $"{_marca} {clave}",
                TipoExpediente = TipoExpediente.Principal,
                Juzgado = juzgado,
                Activo = activo,
                FechaCreacion = ahora.AddMinutes(-(minuto++)),
                UsuarioCreacion = "test",
            };
            expediente.Casos.Add(new CasoExpediente { Caso = caso, Expediente = expediente });
            expedientes[clave] = expediente;
            db.Add(expediente);
        }

        await db.SaveChangesAsync();

        _e = new Escenario(
            jA.JuzgadoId, jB.JuzgadoId, jC.JuzgadoId, k1.CasoId, k2.CasoId,
            expedientes.ToDictionary(x => x.Key, x => x.Value.ExpedienteId),
            new[] { k1.CasoId, k2.CasoId },
            new[] { jA.JuzgadoId, jB.JuzgadoId, jC.JuzgadoId }
        );
    }

    public async Task DisposeAsync()
    {
        if (BaseAislada.CadenaConexion is null || _e is null)
        {
            return;
        }

        await using var db = _base.CrearContexto();
        var ids = _e.Expedientes.Values.ToArray();
        await db.CasosExpedientes.Where(x => ids.Contains(x.ExpedienteId)).ExecuteDeleteAsync();
        await db.Expedientes.Where(x => ids.Contains(x.ExpedienteId)).ExecuteDeleteAsync();
        await db.Casos.Where(x => _e.CasoIds.Contains(x.CasoId)).ExecuteDeleteAsync();
        await db.Juzgados.Where(x => _e.JuzgadoIds.Contains(x.JuzgadoId)).ExecuteDeleteAsync();
    }

    private async Task<(long[] Ids, int Total)> Buscar(
        long? juzgadoId,
        long? casoId = null,
        string? busqueda = null,
        bool soloActivos = true,
        int page = 1,
        int pageSize = 20
    )
    {
        await using var db = _base.CrearContexto();
        var repo = new ExpedienteRepository(db);
        var items = await repo.BuscarAsync(casoId, juzgadoId, busqueda, soloActivos, page, pageSize);
        var total = await repo.ContarAsync(casoId, juzgadoId, busqueda, soloActivos);
        return (items.Select(x => x.ExpedienteId).ToArray(), total);
    }

    private long[] Ids(params string[] claves) => claves.Select(x => _e.Expedientes[x]).ToArray();

    [FactConBase]
    public async Task Solo_juzgadoId_filtra_por_identificador_y_respeta_soloActivos()
    {
        var activos = await Buscar(_e.JuzgadoActivo);
        Assert.Equal(3, activos.Total);
        Assert.Equal(Ids("uno", "dos", "tres especial"), activos.Ids);

        var todos = await Buscar(_e.JuzgadoActivo, soloActivos: false);
        Assert.Equal(4, todos.Total);
        Assert.Equal(Ids("uno", "dos", "tres especial", "cuatro"), todos.Ids);
    }

    [FactConBase]
    public async Task Juzgado_inactivo_se_puede_consultar()
    {
        var activos = await Buscar(_e.JuzgadoInactivo);
        Assert.Equal(1, activos.Total);
        Assert.Equal(Ids("cinco"), activos.Ids);

        var todos = await Buscar(_e.JuzgadoInactivo, soloActivos: false);
        Assert.Equal(2, todos.Total);
        Assert.Equal(Ids("cinco", "seis"), todos.Ids);
    }

    [FactConBase]
    public async Task Juzgado_sin_expedientes_o_inexistente_devuelve_vacio()
    {
        var sinExpedientes = await Buscar(_e.JuzgadoSinExpedientes, soloActivos: false);
        Assert.Equal(0, sinExpedientes.Total);
        Assert.Empty(sinExpedientes.Ids);

        var inexistente = await Buscar(long.MaxValue, soloActivos: false);
        Assert.Equal(0, inexistente.Total);
        Assert.Empty(inexistente.Ids);
    }

    [FactConBase]
    public async Task Se_combina_con_casoId_busqueda_y_soloActivos()
    {
        var conCaso = await Buscar(_e.JuzgadoActivo, casoId: _e.Caso1);
        Assert.Equal(2, conCaso.Total);
        Assert.Equal(Ids("uno", "dos"), conCaso.Ids);

        var conCasoTodos = await Buscar(_e.JuzgadoActivo, casoId: _e.Caso2, soloActivos: false);
        Assert.Equal(2, conCasoTodos.Total);
        Assert.Equal(Ids("tres especial", "cuatro"), conCasoTodos.Ids);

        var conBusqueda = await Buscar(_e.JuzgadoActivo, busqueda: "especial");
        Assert.Equal(1, conBusqueda.Total);
        Assert.Equal(Ids("tres especial"), conBusqueda.Ids);

        // La búsqueda coincide con el nombre del juzgado B, pero el filtro pide el A: AND.
        var cruzada = await Buscar(_e.JuzgadoActivo, busqueda: $"{_marca} B", soloActivos: false);
        Assert.Equal(0, cruzada.Total);
        Assert.Empty(cruzada.Ids);

        var todoCombinado = await Buscar(_e.JuzgadoInactivo, casoId: _e.Caso2, busqueda: "seis", soloActivos: false);
        Assert.Equal(1, todoCombinado.Total);
        Assert.Equal(Ids("seis"), todoCombinado.Ids);
    }

    [FactConBase]
    public async Task Sin_juzgadoId_mantiene_el_comportamiento_actual()
    {
        var activos = await Buscar(null, busqueda: _marca);
        Assert.Equal(5, activos.Total);
        Assert.Equal(Ids("uno", "dos", "tres especial", "cinco", "siete"), activos.Ids);

        var todos = await Buscar(null, busqueda: _marca, soloActivos: false);
        Assert.Equal(7, todos.Total);
        Assert.Equal(_e.Expedientes.Count, todos.Ids.Distinct().Count());

        var porCaso = await Buscar(null, casoId: _e.Caso1, soloActivos: false);
        Assert.Equal(4, porCaso.Total);
    }

    [FactConBase]
    public async Task Total_y_paginacion_se_calculan_sobre_el_resultado_filtrado()
    {
        var pagina1 = await Buscar(_e.JuzgadoActivo, soloActivos: false, page: 1, pageSize: 3);
        var pagina2 = await Buscar(_e.JuzgadoActivo, soloActivos: false, page: 2, pageSize: 3);
        var pagina3 = await Buscar(_e.JuzgadoActivo, soloActivos: false, page: 3, pageSize: 3);

        Assert.Equal(4, pagina1.Total);
        Assert.Equal(4, pagina2.Total);
        Assert.Equal(4, pagina3.Total);
        Assert.Equal(Ids("uno", "dos", "tres especial"), pagina1.Ids);
        Assert.Equal(Ids("cuatro"), pagina2.Ids);
        Assert.Empty(pagina3.Ids);

        var unidas = pagina1.Ids.Concat(pagina2.Ids).ToArray();
        Assert.Equal(unidas.Length, unidas.Distinct().Count());
    }
}
