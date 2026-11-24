using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Services;
using Mantaras.Juridico.Application.Features.DiasInhabiles;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Importacion;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Services;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Validators;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Mantaras.Juridico.Infrastructure.Persistence;
using Mantaras.Juridico.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Tests.DiasInhabiles;

[Collection(ColeccionBaseAislada.Nombre)]
public sealed class ImportacionFeriadosServiceTests : IAsyncLifetime
{
    private readonly BaseAislada _base;
    private readonly FuenteFalsa _fuente = new();
    private readonly List<JuridicoDbContext> _contextos = new();

    public ImportacionFeriadosServiceTests(BaseAislada baseAislada)
    {
        _base = baseAislada;
    }

    public Task InitializeAsync() =>
        BaseAislada.CadenaConexion is null ? Task.CompletedTask : _base.VaciarAsync();

    public async Task DisposeAsync()
    {
        foreach (var contexto in _contextos)
        {
            await contexto.DisposeAsync();
        }
    }

    [FactConBase]
    public async Task Primera_importacion_crea_solo_feriados_nacionales_y_marca_cobertura()
    {
        var servicio = CrearServicio();

        Assert.False((await servicio.ObtenerEstadoAsync(2026)).Value!.CalendarioCargado);

        var vista = (await servicio.ObtenerVistaPreviaAsync(2026)).Value!;
        var confirmacion = await Confirmar(servicio, vista.Huella);

        Assert.Equal(17, vista.Resumen.Nuevos);
        Assert.Equal(14, vista.Resumen.RequierenRevision);
        Assert.Equal(1, vista.Resumen.Omitidos);
        Assert.Equal(17, confirmacion.Creados);
        Assert.False(confirmacion.YaAplicada);

        var dias = await LeerDiasAsync();
        Assert.Equal(17, dias.Count);
        Assert.All(dias, x =>
        {
            Assert.Equal(OrigenDiaInhabil.FeriadoNacional, x.Origen);
            Assert.Equal("argentina.gob.ar", x.Fuente);
            Assert.Equal(x.Fecha, x.FechaFuente);
            Assert.NotNull(x.FechaSincronizacion);
        });
        // Fecha repetida en la fuente (feriado + no laborables): un solo registro.
        Assert.Single(dias, x => x.Fecha == new DateOnly(2026, 4, 2));
        // Turísticos, no laborables y especiales no se activan sin revisión.
        Assert.DoesNotContain(dias, x => x.Fecha == new DateOnly(2026, 3, 23));
        Assert.DoesNotContain(dias, x => x.Fecha == new DateOnly(2026, 11, 10));
        // El 1/1/2027 del archivo 2026 queda para el calendario 2027.
        Assert.DoesNotContain(dias, x => x.Fecha.Year == 2027);

        var estado = (await CrearServicio().ObtenerEstadoAsync(2026)).Value!;
        Assert.True(estado.CalendarioCargado);
        Assert.Equal(17, estado.FeriadosImportadosActivos);
        Assert.Equal("https://fixture/2026", estado.Fuente!.Url);
    }

    [FactConBase]
    public async Task Repetir_la_importacion_no_genera_cambios_ni_duplicados()
    {
        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;
        await Confirmar(CrearServicio(), vista.Huella);
        var antes = await LeerDiasAsync();

        // Reintento de la misma confirmación (por ejemplo, doble clic o timeout).
        var reintento = await Confirmar(CrearServicio(), vista.Huella);
        Assert.True(reintento.YaAplicada);
        Assert.Equal(17, reintento.Creados);

        // Nueva vista previa y confirmación sobre datos idénticos.
        var segunda = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;
        Assert.Equal(0, segunda.Resumen.Nuevos);
        Assert.Equal(17, segunda.Resumen.Existentes);
        var confirmacion = await Confirmar(CrearServicio(), segunda.Huella);
        Assert.Equal(0, confirmacion.Creados);
        Assert.Equal(0, confirmacion.Actualizados);

        var despues = await LeerDiasAsync();
        Assert.Equal(
            antes.Select(x => (x.DiaInhabilId, x.Fecha, x.Descripcion, x.Activo, x.FechaModificacion, x.FechaSincronizacion)),
            despues.Select(x => (x.DiaInhabilId, x.Fecha, x.Descripcion, x.Activo, x.FechaModificacion, x.FechaSincronizacion))
        );
    }

    [FactConBase]
    public async Task Respeta_registros_manuales_y_ferias()
    {
        var manuales = CrearServicioManual();
        await manuales.CrearAsync(new GuardarDiaInhabilRequest { Fecha = new DateOnly(2026, 5, 1), Descripcion = "Trabajador (cargado a mano)" });
        await manuales.CrearAsync(new GuardarDiaInhabilRequest { Fecha = new DateOnly(2026, 1, 15), Descripcion = "Feria judicial de enero" });

        // Fechas manuales no cuentan como calendario cargado.
        Assert.False((await CrearServicio().ObtenerEstadoAsync(2026)).Value!.CalendarioCargado);

        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;
        var primeroDeMayo = vista.Items.Single(x => x.Fecha == new DateOnly(2026, 5, 1));
        Assert.Equal(EstadoImportacionFeriado.Existente, primeroDeMayo.Estado);
        Assert.Equal(OrigenDiaInhabil.Manual, primeroDeMayo.OrigenExistente);

        await Confirmar(CrearServicio(), vista.Huella);

        var dias = await LeerDiasAsync();
        var manualMayo = dias.Single(x => x.Fecha == new DateOnly(2026, 5, 1));
        Assert.Equal(OrigenDiaInhabil.Manual, manualMayo.Origen);
        Assert.Equal("Trabajador (cargado a mano)", manualMayo.Descripcion);
        Assert.Contains(dias, x => x.Fecha == new DateOnly(2026, 1, 15) && x.Origen == OrigenDiaInhabil.Manual);
        Assert.Equal(18, dias.Count);
    }

    [FactConBase]
    public async Task No_reactiva_desactivados_ni_pisa_ediciones_manuales()
    {
        await Confirmar(CrearServicio(), (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!.Huella);
        var dias = await LeerDiasAsync();
        var mayo = dias.Single(x => x.Fecha == new DateOnly(2026, 5, 25));
        var julio = dias.Single(x => x.Fecha == new DateOnly(2026, 7, 9));
        var agosto = dias.Single(x => x.Fecha == new DateOnly(2026, 8, 17));

        Assert.True((await CrearServicioManual().DarDeBajaAsync(mayo.DiaInhabilId)).IsSuccess);
        await CrearServicioManual().ActualizarAsync(julio.DiaInhabilId, new GuardarDiaInhabilRequest { Fecha = julio.Fecha, Descripcion = "Independencia (editado)" });
        await CrearServicioManual().ActualizarAsync(agosto.DiaInhabilId, new GuardarDiaInhabilRequest { Fecha = new DateOnly(2026, 8, 18), Descripcion = agosto.Descripcion });

        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;
        Assert.Equal(EstadoImportacionFeriado.Conflicto, vista.Items.Single(x => x.Fecha == mayo.Fecha).Estado);
        Assert.Equal(EstadoImportacionFeriado.Existente, vista.Items.Single(x => x.Fecha == julio.Fecha).Estado);
        Assert.Equal(EstadoImportacionFeriado.Conflicto, vista.Items.Single(x => x.Fecha == agosto.Fecha).Estado);
        Assert.Equal(2, vista.Resumen.Conflictos);

        var confirmacion = await Confirmar(CrearServicio(), vista.Huella);
        Assert.Equal(0, confirmacion.Creados);

        var despues = await LeerDiasAsync();
        Assert.False(despues.Single(x => x.DiaInhabilId == mayo.DiaInhabilId).Activo);
        Assert.Equal("Independencia (editado)", despues.Single(x => x.DiaInhabilId == julio.DiaInhabilId).Descripcion);
        Assert.Equal(new DateOnly(2026, 8, 18), despues.Single(x => x.DiaInhabilId == agosto.DiaInhabilId).Fecha);
        Assert.DoesNotContain(despues, x => x.Fecha == new DateOnly(2026, 8, 17));
        Assert.All(
            despues.Where(x => x.DiaInhabilId is var id && (id == mayo.DiaInhabilId || id == julio.DiaInhabilId || id == agosto.DiaInhabilId)),
            x => Assert.True(x.ModificadoManualmente)
        );
    }

    [FactConBase]
    public async Task Actualiza_descripcion_de_importados_sin_cambios_manuales()
    {
        await Confirmar(CrearServicio(), (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!.Huella);

        _fuente.Publicacion = ReemplazarNombre(Fixtures.Publicacion(2026), new DateOnly(2026, 7, 9), "Declaración de la Independencia");
        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;
        Assert.Equal(1, vista.Resumen.Actualizables);

        var confirmacion = await Confirmar(CrearServicio(), vista.Huella);

        Assert.Equal(1, confirmacion.Actualizados);
        Assert.Equal("Declaración de la Independencia", (await LeerDiasAsync()).Single(x => x.Fecha == new DateOnly(2026, 7, 9)).Descripcion);
    }

    [FactConBase]
    public async Task No_borra_lo_que_desaparece_de_la_fuente()
    {
        await Confirmar(CrearServicio(), (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!.Huella);

        var original = Fixtures.Publicacion(2026);
        _fuente.Publicacion = original with
        {
            Registros = original.Registros.Where(x => x.Fecha != new DateOnly(2026, 11, 9)).ToArray(),
        };
        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;
        Assert.Equal(new DateOnly(2026, 11, 9), Assert.Single(vista.AusentesEnFuente).Fecha);

        await Confirmar(CrearServicio(), vista.Huella);

        Assert.Contains(await LeerDiasAsync(), x => x.Fecha == new DateOnly(2026, 11, 9) && x.Activo);
    }

    [FactConBase]
    public async Task Si_la_fuente_falla_conserva_el_calendario_actual()
    {
        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;

        _fuente.Error = DiaInhabilErrors.FuenteNoDisponible;
        var preview = await CrearServicio().ObtenerVistaPreviaAsync(2026);
        var confirmacion = await CrearServicio().ConfirmarAsync(2026, new ConfirmarImportacionFeriadosRequest { Huella = vista.Huella });

        Assert.Equal(DiaInhabilErrors.FuenteNoDisponible, preview.Errors.Single());
        Assert.Equal(DiaInhabilErrors.FuenteNoDisponible, confirmacion.Errors.Single());
        Assert.Empty(await LeerDiasAsync());
        Assert.False((await CrearServicio().ObtenerEstadoAsync(2026)).Value!.CalendarioCargado);

        _fuente.Error = DiaInhabilErrors.FuenteInvalida;
        Assert.Equal(DiaInhabilErrors.FuenteInvalida, (await CrearServicio().ConfirmarAsync(2026, new ConfirmarImportacionFeriadosRequest { Huella = vista.Huella })).Errors.Single());
        Assert.Empty(await LeerDiasAsync());
    }

    [FactConBase]
    public async Task Rechaza_confirmar_si_el_calendario_cambio_desde_la_vista_previa()
    {
        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;
        await CrearServicioManual().CrearAsync(new GuardarDiaInhabilRequest { Fecha = new DateOnly(2026, 12, 25), Descripcion = "Navidad manual" });

        var result = await CrearServicio().ConfirmarAsync(2026, new ConfirmarImportacionFeriadosRequest { Huella = vista.Huella });

        Assert.Equal(DiaInhabilErrors.VistaPreviaDesactualizada, result.Errors.Single());
        Assert.Single(await LeerDiasAsync());
    }

    [FactConBase]
    public async Task Rechaza_confirmar_si_la_fuente_cambio_desde_la_vista_previa()
    {
        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;
        _fuente.Publicacion = ReemplazarNombre(Fixtures.Publicacion(2026), new DateOnly(2026, 12, 25), "Navidad 2026");

        var result = await CrearServicio().ConfirmarAsync(2026, new ConfirmarImportacionFeriadosRequest { Huella = vista.Huella });

        Assert.Equal(DiaInhabilErrors.VistaPreviaDesactualizada, result.Errors.Single());
        Assert.Empty(await LeerDiasAsync());
    }

    [FactConBase]
    public async Task Incluye_solo_las_fechas_a_revisar_elegidas()
    {
        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;

        var invalida = await CrearServicio().ConfirmarAsync(2026, new ConfirmarImportacionFeriadosRequest
        {
            Huella = vista.Huella,
            FechasRevisionIncluidas = new[] { new DateOnly(2026, 5, 1) },
        });
        Assert.Equal("DiasInhabiles.DatosInvalidos", invalida.Errors.Single().Code);
        Assert.Empty(await LeerDiasAsync());

        var confirmacion = await Confirmar(CrearServicio(), vista.Huella, new DateOnly(2026, 3, 23));

        Assert.Equal(18, confirmacion.Creados);
        Assert.Equal(1, confirmacion.RevisionIncluidos);
        Assert.Contains(await LeerDiasAsync(), x => x.Fecha == new DateOnly(2026, 3, 23) && x.Origen == OrigenDiaInhabil.FeriadoNacional);
    }

    [FactConBase]
    public async Task Confirmaciones_simultaneas_no_duplican()
    {
        var vista = (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!;

        var resultados = await Task.WhenAll(
            Enumerable.Range(0, 4).Select(_ => CrearServicio().ConfirmarAsync(
                2026,
                new ConfirmarImportacionFeriadosRequest { Huella = vista.Huella }
            ))
        );

        Assert.All(resultados, x => Assert.True(x.IsSuccess, string.Join(" ", x.Errors.Select(e => e.Code))));
        Assert.Single(resultados, x => !x.Value!.YaAplicada);
        Assert.Equal(17, (await LeerDiasAsync()).Count);
    }

    [FactConBase]
    public async Task Calculo_que_cruza_de_anio_advierte_el_anio_sin_calendario_y_no_se_recalcula()
    {
        await Confirmar(CrearServicio(), (await CrearServicio().ObtenerVistaPreviaAsync(2026)).Value!.Huella);
        var reglaId = await CrearReglaAsync(cantidadDias: 10, TipoComputoPlazo.DiasHabiles);
        var reglasCorridos = await CrearReglaAsync(cantidadDias: 30, TipoComputoPlazo.DiasCorridos);

        var aplicacion = (await CrearServicioReglas().AplicarAsync(reglaId, new AplicarReglaVencimientoRequest
        {
            FechaBase = new DateOnly(2026, 12, 21),
            OrigenFechaBase = OrigenFechaBaseAgenda.Manual,
            ClaveIdempotencia = Guid.NewGuid().ToString("N"),
        })).Value!;

        // Hábiles desde el lunes 21/12/2026: 22, 23, 24, (25 Navidad), 28, 29, 30, 31,
        // (1/1/2027 no está cargado) 1, 4, 5 => 5/1/2027.
        Assert.Equal(new DateOnly(2027, 1, 5), aplicacion.FechaCalculada);
        var advertencia = Assert.Single(aplicacion.Advertencias);
        Assert.Equal(2027, advertencia.Anio);
        Assert.Equal(DiaInhabilErrors.CodigoAnioSinCalendario, advertencia.Codigo);

        var corridos = (await CrearServicioReglas().AplicarAsync(reglasCorridos, new AplicarReglaVencimientoRequest
        {
            FechaBase = new DateOnly(2026, 12, 21),
            OrigenFechaBase = OrigenFechaBaseAgenda.Manual,
            ClaveIdempotencia = Guid.NewGuid().ToString("N"),
        })).Value!;
        Assert.Empty(corridos.Advertencias);

        // Cargar 2027 después (con el 1/1) no modifica lo ya generado.
        await using (var contexto = _base.CrearContexto())
        {
            contexto.DiasInhabiles.Add(new DiaInhabil { Fecha = new DateOnly(2027, 1, 1), Descripcion = "Año nuevo", FechaCreacion = DateTime.UtcNow });
            contexto.CalendariosFeriadosImportados.Add(new CalendarioFeriadosImportado
            {
                Anio = 2027, Fuente = "argentina.gob.ar", UrlFuente = "https://fixture/2027",
                FechaPrimeraSincronizacion = DateTime.UtcNow, FechaSincronizacion = DateTime.UtcNow,
                UltimaClaveConfirmacion = new string('0', 64),
            });
            await contexto.SaveChangesAsync();
        }

        await using (var contexto = _base.CrearContexto())
        {
            var generacion = await contexto.AgendaGeneracionesReglas
                .Include(x => x.EntradaAgenda)
                .SingleAsync(x => x.AgendaGeneracionReglaId == aplicacion.AgendaGeneracionReglaId);
            Assert.Equal(new DateOnly(2027, 1, 5), generacion.EntradaAgenda.FechaVencimiento);
            Assert.Equal(new[] { 2027 }, generacion.AniosSinCalendario);
        }

        var nueva = (await CrearServicioReglas().AplicarAsync(reglaId, new AplicarReglaVencimientoRequest
        {
            FechaBase = new DateOnly(2026, 12, 21),
            OrigenFechaBase = OrigenFechaBaseAgenda.Manual,
            ClaveIdempotencia = Guid.NewGuid().ToString("N"),
        })).Value!;
        Assert.Equal(new DateOnly(2027, 1, 6), nueva.FechaCalculada);
        Assert.Empty(nueva.Advertencias);
    }

    private async Task<long> CrearReglaAsync(int cantidadDias, TipoComputoPlazo computo)
    {
        await using var contexto = _base.CrearContexto();
        var marca = Guid.NewGuid().ToString("N");
        var regla = new ReglaVencimiento
        {
            Nombre = $"PRUEBA FERIADOS {marca}",
            CantidadDias = cantidadDias,
            TipoComputo = computo,
            FechaCreacion = DateTime.UtcNow,
            TipoEntrada = new TipoEntradaAgenda { Nombre = $"PRUEBA FERIADOS {marca}", FechaCreacion = DateTime.UtcNow },
        };
        contexto.ReglasVencimiento.Add(regla);
        await contexto.SaveChangesAsync();
        return regla.ReglaVencimientoId;
    }

    private static async Task<Application.Features.DiasInhabiles.Responses.ConfirmacionImportacionFeriadosResponse> Confirmar(
        IImportacionFeriadosService servicio,
        string huella,
        params DateOnly[] revision
    )
    {
        var result = await servicio.ConfirmarAsync(2026, new ConfirmarImportacionFeriadosRequest
        {
            Huella = huella,
            FechasRevisionIncluidas = revision,
        });

        Assert.True(result.IsSuccess, string.Join(" ", result.Errors.Select(x => x.Code)));
        return result.Value!;
    }

    private async Task<List<DiaInhabil>> LeerDiasAsync()
    {
        await using var contexto = _base.CrearContexto();
        return await contexto.DiasInhabiles.AsNoTracking().OrderBy(x => x.Fecha).ToListAsync();
    }

    private JuridicoDbContext NuevoContexto()
    {
        var contexto = _base.CrearContexto();
        _contextos.Add(contexto);
        return contexto;
    }

    private ImportacionFeriadosService CrearServicio() => new(
        new DiaInhabilRepository(NuevoContexto()),
        _fuente,
        new UsuarioFalso(),
        new ConfirmarImportacionFeriadosRequestValidator()
    );

    private DiasInhabilesService CrearServicioManual() => new(
        new DiaInhabilRepository(NuevoContexto()),
        new UsuarioFalso(),
        new GuardarDiaInhabilRequestValidator(),
        new BuscarDiasInhabilesRequestValidator()
    );

    private ReglasVencimientoService CrearServicioReglas() => new(
        new AgendaRepository(NuevoContexto()),
        new UsuarioFalso()
    );

    private static PublicacionFeriadosNacionales ReemplazarNombre(
        PublicacionFeriadosNacionales publicacion,
        DateOnly fecha,
        string nombre
    ) => publicacion with
    {
        Registros = publicacion.Registros
            .Select(x => x.Fecha == fecha && PlanificadorImportacionFeriados.EsFeriadoNacional(x.Tipo) ? x with { Nombre = nombre } : x)
            .ToArray(),
    };

    private sealed class FuenteFalsa : IFuenteFeriadosNacionales
    {
        public PublicacionFeriadosNacionales Publicacion { get; set; } = Fixtures.Publicacion(2026);

        public Error? Error { get; set; }

        public string Codigo => "argentina.gob.ar";

        public string Nombre => "Fuente de prueba";

        public string ConstruirUrl(int anio) => $"https://fixture/{anio}";

        public Task<Result<PublicacionFeriadosNacionales>> ObtenerAsync(
            int anio,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(
            Error is not null
                ? Result<PublicacionFeriadosNacionales>.Failure(Error)
                : Result<PublicacionFeriadosNacionales>.Success(Publicacion)
        );
    }

    private sealed class UsuarioFalso : ICurrentUserService
    {
        public long? UsuarioId => null;

        public string Usuario => "pruebas-feriados";
    }
}
