using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Mantaras.Juridico.Infrastructure.Persistence;
using Mantaras.Juridico.Infrastructure.Persistence.Migrations;
using Mantaras.Juridico.Tests.DiasInhabiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mantaras.Juridico.Tests.Agenda;

// SQL de la migración ActualizarTextoSeguimientoCobro sobre notas automáticas y personalizadas.
// Corre en una transacción que se revierte: la regla de cobro real vuelve a su estado al terminar.
[Collection(ColeccionBaseAislada.Nombre)]
public sealed class TextoSeguimientoCobroMigracionTests : IAsyncLifetime
{
    private readonly BaseAislada _base;
    private readonly string _marca = "TEXTOCOBRO " + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    private JuridicoDbContext _db = null!;
    private IDbContextTransaction _tx = null!;
    private ReglaVencimiento _reglaCobro = null!;
    private readonly Dictionary<string, long> _entradas = new();

    private const string Personalizada =
        "Alerta interna a los 120 días hábiles desde el envío de la cédula u oficio de cobro. Llamar al juzgado.";

    public TextoSeguimientoCobroMigracionTests(BaseAislada baseAislada)
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
        var ahora = DateTime.UtcNow;

        _reglaCobro = await _db.ReglasVencimiento.SingleAsync(x => x.Codigo == ReglaVencimiento.CodigoSeguimientoCobroJudicial);
        _reglaCobro.Descripcion = ActualizarTextoSeguimientoCobro.TextoAnterior;

        var otraCategoria = new TipoEntradaAgenda { Nombre = $"{_marca} CATEGORIA", FechaCreacion = ahora, UsuarioCreacion = "test", Activo = true };
        var otraRegla = new ReglaVencimiento
        {
            TipoEntrada = otraCategoria, Nombre = $"{_marca} OTRA REGLA", CantidadDias = 5,
            TipoComputo = TipoComputoPlazo.DiasCorridos, FechaCreacion = ahora, UsuarioCreacion = "test", Activo = true,
        };
        _db.AddRange(otraCategoria, otraRegla);
        await _db.SaveChangesAsync();

        EntradaAgenda Entrada(string clave, long tipoId, string? descripcion, EstadoEntradaAgenda estado, ReglaVencimiento? generadaPor)
        {
            var entrada = new EntradaAgenda
            {
                TipoEntradaAgendaId = tipoId, Titulo = $"{_marca} {clave}", Descripcion = descripcion, Estado = estado,
                FechaInicio = new DateOnly(2026, 3, 2), FechaVencimiento = new DateOnly(2026, 8, 20),
                HoraVencimiento = new TimeOnly(9, 30), FechaCreacion = ahora.AddDays(-30), UsuarioCreacion = "test",
                FechaModificacion = ahora.AddDays(-10), UsuarioModificacion = "test", Activo = true,
            };

            if (generadaPor is not null)
            {
                _db.Add(new AgendaGeneracionRegla
                {
                    ReglaVencimiento = generadaPor, EntradaAgenda = entrada, FechaBase = new DateOnly(2026, 3, 2),
                    OrigenFechaBase = OrigenFechaBaseAgenda.Manual, ClaveIdempotencia = $"{_marca} {clave}",
                    FechaCreacion = ahora, UsuarioCreacion = "test",
                });
            }
            else
            {
                _db.Add(entrada);
            }

            return entrada;
        }

        var tipoCobro = _reglaCobro.TipoEntradaAgendaId;
        var entradas = new Dictionary<string, EntradaAgenda>
        {
            ["cobro automatica pendiente"] = Entrada("cobro automatica pendiente", tipoCobro, ActualizarTextoSeguimientoCobro.TextoAnterior, EstadoEntradaAgenda.Pendiente, _reglaCobro),
            ["cobro automatica completada"] = Entrada("cobro automatica completada", tipoCobro, ActualizarTextoSeguimientoCobro.TextoAnterior, EstadoEntradaAgenda.Completada, _reglaCobro),
            ["cobro personalizada"] = Entrada("cobro personalizada", tipoCobro, Personalizada, EstadoEntradaAgenda.Pendiente, _reglaCobro),
            ["cobro sin nota"] = Entrada("cobro sin nota", tipoCobro, null, EstadoEntradaAgenda.Pendiente, _reglaCobro),
            ["otra regla mismo texto"] = Entrada("otra regla mismo texto", otraCategoria.TipoEntradaAgendaId, ActualizarTextoSeguimientoCobro.TextoAnterior, EstadoEntradaAgenda.Pendiente, otraRegla),
            ["manual mismo texto"] = Entrada("manual mismo texto", tipoCobro, ActualizarTextoSeguimientoCobro.TextoAnterior, EstadoEntradaAgenda.Pendiente, null),
            ["manual con alerta"] = Entrada("manual con alerta", otraCategoria.TipoEntradaAgendaId, "Alerta: revisar el expediente.", EstadoEntradaAgenda.Cancelada, null),
        };

        await _db.SaveChangesAsync();

        foreach (var (clave, entrada) in entradas)
        {
            _entradas[clave] = entrada.EntradaAgendaId;
        }
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

    private sealed record Foto(string? Descripcion, DateOnly FechaInicio, DateOnly? FechaVencimiento, TimeOnly? HoraVencimiento,
        EstadoEntradaAgenda Estado, DateTime FechaCreacion, DateTime? FechaModificacion, string? UsuarioModificacion);

    private async Task<Dictionary<string, Foto>> FotografiarAsync()
    {
        _db.ChangeTracker.Clear();
        var ids = _entradas.Values.ToArray();
        var filas = await _db.EntradasAgenda.AsNoTracking().Where(x => ids.Contains(x.EntradaAgendaId)).ToListAsync();

        return _entradas.ToDictionary(
            x => x.Key,
            x =>
            {
                var e = filas.Single(f => f.EntradaAgendaId == x.Value);
                return new Foto(e.Descripcion, e.FechaInicio, e.FechaVencimiento, e.HoraVencimiento, e.Estado,
                    e.FechaCreacion, e.FechaModificacion, e.UsuarioModificacion);
            });
    }

    private async Task<string?> DescripcionReglaAsync()
    {
        _db.ChangeTracker.Clear();
        return await _db.ReglasVencimiento.AsNoTracking()
            .Where(x => x.ReglaVencimientoId == _reglaCobro.ReglaVencimientoId)
            .Select(x => x.Descripcion)
            .SingleAsync();
    }

    private Task Aplicar(string desde, string hacia) =>
        _db.Database.ExecuteSqlRawAsync(ActualizarTextoSeguimientoCobro.Reemplazar(desde, hacia));

    [FactConBase]
    public async Task Actualiza_solo_el_texto_automatico_original_del_seguimiento_de_cobro()
    {
        var antes = await FotografiarAsync();

        await Aplicar(ActualizarTextoSeguimientoCobro.TextoAnterior, ActualizarTextoSeguimientoCobro.TextoNuevo);
        var despues = await FotografiarAsync();

        Assert.Equal(ActualizarTextoSeguimientoCobro.TextoNuevo, despues["cobro automatica pendiente"].Descripcion);
        Assert.Equal(ActualizarTextoSeguimientoCobro.TextoNuevo, despues["cobro automatica completada"].Descripcion);
        Assert.Equal(Personalizada, despues["cobro personalizada"].Descripcion);
        Assert.Null(despues["cobro sin nota"].Descripcion);
        Assert.Equal(ActualizarTextoSeguimientoCobro.TextoAnterior, despues["otra regla mismo texto"].Descripcion);
        Assert.Equal(ActualizarTextoSeguimientoCobro.TextoAnterior, despues["manual mismo texto"].Descripcion);
        Assert.Equal("Alerta: revisar el expediente.", despues["manual con alerta"].Descripcion);

        // Fechas, estados y auditoría quedan iguales en todas.
        foreach (var clave in _entradas.Keys)
        {
            Assert.Equal(antes[clave] with { Descripcion = null }, despues[clave] with { Descripcion = null });
        }

        // Fuente del texto de los seguimientos nuevos.
        Assert.Equal(ActualizarTextoSeguimientoCobro.TextoNuevo, await DescripcionReglaAsync());
    }

    [FactConBase]
    public async Task Conserva_una_descripcion_de_regla_personalizada()
    {
        _reglaCobro.Descripcion = "Alerta interna personalizada por el estudio.";
        await _db.SaveChangesAsync();

        await Aplicar(ActualizarTextoSeguimientoCobro.TextoAnterior, ActualizarTextoSeguimientoCobro.TextoNuevo);

        Assert.Equal("Alerta interna personalizada por el estudio.", await DescripcionReglaAsync());
    }

    [FactConBase]
    public async Task Es_idempotente_y_el_down_revierte_solo_lo_que_cambio()
    {
        await Aplicar(ActualizarTextoSeguimientoCobro.TextoAnterior, ActualizarTextoSeguimientoCobro.TextoNuevo);
        var unaVez = await FotografiarAsync();
        await Aplicar(ActualizarTextoSeguimientoCobro.TextoAnterior, ActualizarTextoSeguimientoCobro.TextoNuevo);
        Assert.Equal(unaVez, await FotografiarAsync());

        await Aplicar(ActualizarTextoSeguimientoCobro.TextoNuevo, ActualizarTextoSeguimientoCobro.TextoAnterior);
        var revertido = await FotografiarAsync();

        Assert.Equal(ActualizarTextoSeguimientoCobro.TextoAnterior, revertido["cobro automatica pendiente"].Descripcion);
        Assert.Equal(Personalizada, revertido["cobro personalizada"].Descripcion);
        Assert.Equal(ActualizarTextoSeguimientoCobro.TextoAnterior, await DescripcionReglaAsync());
    }
}
