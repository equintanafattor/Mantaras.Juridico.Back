using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Application.Features.Agenda.Services;
using Mantaras.Juridico.Application.Features.Expedientes.Requests;
using Mantaras.Juridico.Application.Features.Expedientes.Services;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Mantaras.Juridico.Infrastructure.Persistence;
using Mantaras.Juridico.Infrastructure.Persistence.Repositories;
using Mantaras.Juridico.Tests.DiasInhabiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Mantaras.Juridico.Tests.Agenda;

// Origen de los avisos por los caminos reales de creación (servicios + repositorios).
// Cada test corre en una transacción que se revierte al terminar: no deja datos en la base aislada.
[Collection(ColeccionBaseAislada.Nombre)]
public sealed class OrigenAvisosTests : IAsyncLifetime
{
    private static readonly TimeOnly Diez = new(10, 0);

    private readonly BaseAislada _base;
    private readonly string _marca = "ORIGEN " + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    private JuridicoDbContext _db = null!;
    private IDbContextTransaction _tx = null!;

    private AgendaService _agenda = null!;
    private RecordatoriosAgendaService _recordatorios = null!;
    private ReglasVencimientoService _reglas = null!;
    private ExpedientesService _expedientes = null!;

    private TipoEntradaAgenda _categoria = null!;
    private ReglaVencimiento _regla = null!;

    public OrigenAvisosTests(BaseAislada baseAislada)
    {
        _base = baseAislada;
    }

    public async Task InitializeAsync()
    {
        if (BaseAislada.CadenaConexion is null)
        {
            return;
        }

        _db = _base.CrearContexto();
        _tx = await _db.Database.BeginTransactionAsync();

        var usuario = new UsuarioFalso();
        var agendaRepo = new AgendaRepository(_db);
        _agenda = new AgendaService(agendaRepo, usuario);
        _recordatorios = new RecordatoriosAgendaService(agendaRepo, new RecordatoriosAgendaRepository(_db), usuario);
        _reglas = new ReglasVencimientoService(agendaRepo, usuario);
        _expedientes = new ExpedientesService(
            new ExpedienteRepository(_db), new CasoRepository(_db), new JuzgadoRepository(_db), _reglas, usuario
        );

        var ahora = DateTime.UtcNow;
        _categoria = new TipoEntradaAgenda { Nombre = $"{_marca} CATEGORIA", FechaCreacion = ahora, UsuarioCreacion = "test", Activo = true };
        _categoria.RecordatoriosPredeterminados.Add(Predeterminado(BaseCalculoRecordatorioAgenda.Inicio, 60));
        _categoria.RecordatoriosPredeterminados.Add(Predeterminado(BaseCalculoRecordatorioAgenda.Inicio, 1440));

        _regla = new ReglaVencimiento
        {
            TipoEntrada = _categoria, Nombre = $"{_marca} REGLA", CantidadDias = 10,
            TipoComputo = TipoComputoPlazo.DiasCorridos, HoraSugerida = Diez,
            FechaCreacion = ahora, UsuarioCreacion = "test", Activo = true,
        };
        // Vencimiento -60 coincide con Inicio -60 de la categoría (la entrada de regla tiene inicio = vencimiento).
        _regla.RecordatoriosPredeterminados.Add(PredeterminadoRegla(BaseCalculoRecordatorioAgenda.Vencimiento, 60));
        _regla.RecordatoriosPredeterminados.Add(PredeterminadoRegla(BaseCalculoRecordatorioAgenda.Vencimiento, 120));

        _db.AddRange(_categoria, _regla);
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (BaseAislada.CadenaConexion is null)
        {
            return;
        }

        await _tx.RollbackAsync();
        await _tx.DisposeAsync();
        await _db.DisposeAsync();
    }

    private static RecordatorioPredeterminadoTipoAgenda Predeterminado(BaseCalculoRecordatorioAgenda baseCalculo, int minutos) => new()
    {
        BaseCalculo = baseCalculo, MinutosAnticipacion = minutos, FechaCreacion = DateTime.UtcNow, UsuarioCreacion = "test", Activo = true,
    };

    private static RecordatorioPredeterminadoReglaVencimiento PredeterminadoRegla(BaseCalculoRecordatorioAgenda baseCalculo, int minutos) => new()
    {
        BaseCalculo = baseCalculo, MinutosAnticipacion = minutos, FechaCreacion = DateTime.UtcNow, UsuarioCreacion = "test", Activo = true,
    };

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.UtcNow);

    // 10:00 en Buenos Aires (UTC-3, sin horario de verano) menos la anticipación.
    private static DateTime Programada(DateOnly fecha, int minutos) =>
        DateTime.SpecifyKind(fecha.ToDateTime(Diez).AddHours(3).AddMinutes(-minutos), DateTimeKind.Utc);

    private async Task<long> CrearEntradaAsync(RecurrenciaEntradaRequest? recurrencia = null, DateOnly? fecha = null)
    {
        var result = await _agenda.CrearAsync(new GuardarEntradaAgendaRequest
        {
            TipoEntradaAgendaId = _categoria.TipoEntradaAgendaId, Titulo = $"{_marca} entrada",
            FechaInicio = fecha ?? Hoy.AddDays(30), HoraInicio = Diez, Recurrencia = recurrencia,
        });
        Assert.True(result.IsSuccess, string.Join(", ", result.Errors.Select(x => x.Code)));
        return result.Value!.EntradaAgendaId;
    }

    private async Task<RecordatorioAgendaResponse[]> AvisosAsync(long entradaAgendaId)
    {
        _db.ChangeTracker.Clear();
        var pagina = await _recordatorios.BuscarAsync(new BuscarRecordatoriosAgendaRequest
        {
            EntradaAgendaId = entradaAgendaId, PageSize = 100,
        });
        return pagina.Items.OrderBy(x => x.FechaProgramadaUtc).ToArray();
    }

    private void AssertCategoria(RecordatorioAgendaResponse aviso, TipoEntradaAgenda categoria)
    {
        Assert.Equal(OrigenRecordatorioAgenda.Categoria, aviso.Origen);
        Assert.Equal(categoria.TipoEntradaAgendaId, aviso.OrigenId);
        Assert.Equal(categoria.Nombre, aviso.OrigenNombre);
    }

    private static void AssertRegla(RecordatorioAgendaResponse aviso, ReglaVencimiento regla)
    {
        Assert.Equal(OrigenRecordatorioAgenda.Regla, aviso.Origen);
        Assert.Equal(regla.ReglaVencimientoId, aviso.OrigenId);
        Assert.Equal(regla.Nombre, aviso.OrigenNombre);
    }

    [FactConBase]
    public async Task Alta_de_entrada_crea_avisos_de_categoria_con_sus_fechas()
    {
        var fecha = Hoy.AddDays(30);
        var entrada = await CrearEntradaAsync(fecha: fecha);

        var avisos = await AvisosAsync(entrada);

        Assert.Equal(2, avisos.Length);
        Assert.All(avisos, x => AssertCategoria(x, _categoria));
        // Mismo cálculo de siempre: el origen no cambia las fechas programadas.
        Assert.Equal(new[] { Programada(fecha, 1440), Programada(fecha, 60) }, avisos.Select(x => x.FechaProgramadaUtc));
        Assert.All(avisos, x => Assert.Equal(EstadoRecordatorioAgenda.Pendiente, x.Estado));
    }

    [FactConBase]
    public async Task Aviso_agregado_a_mano_es_manual()
    {
        var entrada = await CrearEntradaAsync();

        var result = await _recordatorios.CrearAsync(entrada, new CrearRecordatorioAgendaRequest
        {
            BaseCalculo = BaseCalculoRecordatorioAgenda.Inicio, MinutosAnticipacion = 15,
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(OrigenRecordatorioAgenda.Manual, result.Value!.Origen);
        Assert.Null(result.Value.OrigenId);
        Assert.Null(result.Value.OrigenNombre);

        var manual = (await AvisosAsync(entrada)).Single(x => x.RecordatorioAgendaId == result.Value.RecordatorioAgendaId);
        Assert.Equal(OrigenRecordatorioAgenda.Manual, manual.Origen);
    }

    [FactConBase]
    public async Task Regla_crea_avisos_de_regla_y_de_su_categoria_y_la_coincidencia_queda_de_categoria()
    {
        var result = await _reglas.AplicarAsync(_regla.ReglaVencimientoId, new AplicarReglaVencimientoRequest
        {
            FechaBase = Hoy, OrigenFechaBase = OrigenFechaBaseAgenda.Manual, ClaveIdempotencia = _marca,
        });
        Assert.True(result.IsSuccess, string.Join(", ", result.Errors.Select(x => x.Code)));
        var fecha = result.Value!.FechaCalculada;

        var avisos = await AvisosAsync(result.Value.Entrada.EntradaAgendaId);

        // Categoría: -1440 y -60. Regla: -120 (su -60 coincide con el de la categoría y no se duplica).
        Assert.Equal(3, avisos.Length);
        Assert.Equal(new[] { Programada(fecha, 1440), Programada(fecha, 120), Programada(fecha, 60) }, avisos.Select(x => x.FechaProgramadaUtc));
        AssertCategoria(avisos[0], _categoria);
        AssertRegla(avisos[1], _regla);
        AssertCategoria(avisos[2], _categoria);
    }

    [FactConBase]
    public async Task Seguimiento_de_cobro_crea_avisos_de_la_regla_de_cobro()
    {
        var reglaCobro = await _db.ReglasVencimiento
            .Include(x => x.TipoEntrada).ThenInclude(x => x.RecordatoriosPredeterminados)
            .Include(x => x.RecordatoriosPredeterminados)
            .SingleAsync(x => x.Codigo == ReglaVencimiento.CodigoSeguimientoCobroJudicial);

        // Configuración controlada dentro de la transacción: la base aislada puede tener cambios de QA.
        reglaCobro.Activo = true;
        reglaCobro.TipoEntrada.Activo = true;
        // Solo queda activo vencimiento -1440 en la categoría y vencimiento -0 en la regla (el sembrado).
        foreach (var p in reglaCobro.TipoEntrada.RecordatoriosPredeterminados)
            p.Activo = p.BaseCalculo == BaseCalculoRecordatorioAgenda.Vencimiento && p.MinutosAnticipacion == 1440;
        foreach (var p in reglaCobro.RecordatoriosPredeterminados)
            p.Activo = p.BaseCalculo == BaseCalculoRecordatorioAgenda.Vencimiento && p.MinutosAnticipacion == 0;
        if (!reglaCobro.TipoEntrada.RecordatoriosPredeterminados.Any(x => x.Activo))
            reglaCobro.TipoEntrada.RecordatoriosPredeterminados.Add(Predeterminado(BaseCalculoRecordatorioAgenda.Vencimiento, 1440));
        if (!reglaCobro.RecordatoriosPredeterminados.Any(x => x.Activo))
            reglaCobro.RecordatoriosPredeterminados.Add(PredeterminadoRegla(BaseCalculoRecordatorioAgenda.Vencimiento, 0));

        var caso = new Caso { Titulo = $"{_marca} caso", FaseInterna = "PRUEBA", FechaCreacion = DateTime.UtcNow, Activo = true };
        var expediente = new Expediente
        {
            Caratula = $"{_marca} cobro", TipoExpediente = TipoExpediente.Principal, Activo = true,
            FechaCreacion = DateTime.UtcNow, UsuarioCreacion = "test",
        };
        expediente.Casos.Add(new CasoExpediente { Caso = caso, Expediente = expediente });
        _db.Add(expediente);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var result = await _expedientes.ActualizarAsync(expediente.ExpedienteId, new ActualizarExpedienteRequest
        {
            CasoIds = new[] { caso.CasoId }, TipoExpediente = TipoExpediente.Principal, Caratula = expediente.Caratula,
            EnvioCobro = new RegistrarEnvioCobroRequest { TipoDocumento = TipoDocumentoCobro.Cedula, FechaEnvio = Hoy },
        });
        Assert.True(result.IsSuccess, string.Join(", ", result.Errors.Select(x => x.Code)));

        var entradaId = await _db.EnviosCobroExpedientes
            .Where(x => x.ExpedienteId == expediente.ExpedienteId && x.Activo)
            .Select(x => x.AgendaGeneracionRegla.EntradaAgendaId)
            .SingleAsync();
        var avisos = await AvisosAsync(entradaId);

        Assert.Equal(2, avisos.Length);
        AssertCategoria(avisos[0], reglaCobro.TipoEntrada);
        AssertRegla(avisos[1], reglaCobro);
    }

    [FactConBase]
    public async Task Cada_ocurrencia_de_una_recurrencia_tiene_avisos_de_categoria()
    {
        var primera = await CrearEntradaAsync(new RecurrenciaEntradaRequest
        {
            Frecuencia = FrecuenciaRecurrenciaAgenda.Diaria, Intervalo = 1, CantidadOcurrencias = 3,
        });
        var serie = await _db.EntradasAgenda.Where(x => x.EntradaAgendaId == primera).Select(x => x.RecurrenciaAgendaId).SingleAsync();
        var ocurrencias = await _db.EntradasAgenda.Where(x => x.RecurrenciaAgendaId == serie).Select(x => x.EntradaAgendaId).ToListAsync();

        Assert.Equal(3, ocurrencias.Count);
        foreach (var ocurrencia in ocurrencias)
        {
            var avisos = await AvisosAsync(ocurrencia);
            Assert.Equal(2, avisos.Length);
            Assert.All(avisos, x => AssertCategoria(x, _categoria));
        }
    }

    [FactConBase]
    public async Task Reprogramar_y_marcar_como_visto_conservan_el_origen()
    {
        var entrada = await CrearEntradaAsync();
        var aviso = (await AvisosAsync(entrada)).First();

        var reprogramado = await _recordatorios.ReprogramarAsync(aviso.RecordatorioAgendaId, new CrearRecordatorioAgendaRequest
        {
            BaseCalculo = BaseCalculoRecordatorioAgenda.Inicio, MinutosAnticipacion = 30,
        });
        Assert.True(reprogramado.IsSuccess);
        Assert.NotEqual(aviso.FechaProgramadaUtc, reprogramado.Value!.FechaProgramadaUtc);
        AssertCategoria(reprogramado.Value, _categoria);
        Assert.Equal(aviso.FechaCreacion, reprogramado.Value.FechaCreacion);

        var visto = await _recordatorios.AtenderAsync(aviso.RecordatorioAgendaId);
        Assert.True(visto.IsSuccess);
        AssertCategoria(visto.Value!, _categoria);

        var releido = (await AvisosAsync(entrada)).Single(x => x.RecordatorioAgendaId == aviso.RecordatorioAgendaId);
        AssertCategoria(releido, _categoria);
        Assert.Equal(reprogramado.Value.FechaProgramadaUtc, releido.FechaProgramadaUtc);
        Assert.True(releido.Atendido);
    }

    [FactConBase]
    public async Task El_nombre_es_el_actual_aunque_la_categoria_este_dada_de_baja()
    {
        var entrada = await CrearEntradaAsync();
        _categoria.Nombre = $"{_marca} RENOMBRADA";
        _categoria.Activo = false;
        _db.Update(_categoria);
        await _db.SaveChangesAsync();

        var avisos = await AvisosAsync(entrada);

        Assert.All(avisos, x => Assert.Equal($"{_marca} RENOMBRADA", x.OrigenNombre));
        Assert.All(avisos, x => Assert.Equal(_categoria.TipoEntradaAgendaId, x.OrigenId));
    }

    [FactConBase]
    public async Task Historico_sin_origen_registrado_es_desconocido()
    {
        var entrada = await CrearEntradaAsync();

        // Igual que una fila anterior a la migración: sin columnas de origen (toman el default 0).
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "RecordatoriosAgenda" ("EntradaAgendaId", "Canal", "FechaProgramadaUtc", "Atendido", "FechaCreacion", "UsuarioCreacion", "Activo")
            VALUES ({entrada}, 1, {Programada(Hoy.AddDays(30), 5)}, FALSE, {DateTime.UtcNow}, 'historico', TRUE)
            """);

        var historico = (await AvisosAsync(entrada)).Single(x => x.FechaProgramadaUtc == Programada(Hoy.AddDays(30), 5));

        Assert.Equal(OrigenRecordatorioAgenda.Desconocido, historico.Origen);
        Assert.Null(historico.OrigenId);
        Assert.Null(historico.OrigenNombre);
    }

    [FactConBase]
    public async Task La_base_rechaza_un_origen_inconsistente()
    {
        var entrada = await CrearEntradaAsync();

        async Task<string?> InsertarAsync(int origen, long? tipoId, long? reglaId)
        {
            await _tx.CreateSavepointAsync("origen");
            try
            {
                await _db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO "RecordatoriosAgenda" ("EntradaAgendaId", "Canal", "FechaProgramadaUtc", "Atendido", "FechaCreacion", "Activo", "Origen", "TipoEntradaAgendaOrigenId", "ReglaVencimientoOrigenId")
                    VALUES ({entrada}, 1, {DateTime.UtcNow.AddDays(40).AddMinutes(origen)}, FALSE, {DateTime.UtcNow}, TRUE, {origen}, {tipoId}, {reglaId})
                    """);
                await _tx.RollbackToSavepointAsync("origen");
                return null;
            }
            catch (PostgresException ex)
            {
                await _tx.RollbackToSavepointAsync("origen");
                return ex.ConstraintName;
            }
        }

        var categoria = _categoria.TipoEntradaAgendaId;
        var regla = _regla.ReglaVencimientoId;

        Assert.Null(await InsertarAsync(2, categoria, null));
        Assert.Null(await InsertarAsync(3, null, regla));
        Assert.Equal("CK_RecordatoriosAgenda_Origen", await InsertarAsync(2, null, null));
        Assert.Equal("CK_RecordatoriosAgenda_Origen", await InsertarAsync(3, categoria, null));
        Assert.Equal("CK_RecordatoriosAgenda_Origen", await InsertarAsync(1, categoria, null));
        Assert.Equal("CK_RecordatoriosAgenda_Origen", await InsertarAsync(0, null, regla));
    }

    private sealed class UsuarioFalso : ICurrentUserService
    {
        public long? UsuarioId => null;

        public string Usuario => "pruebas-origen-avisos";
    }
}
