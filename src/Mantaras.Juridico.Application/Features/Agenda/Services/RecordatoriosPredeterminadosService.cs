using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

public sealed class RecordatoriosPredeterminadosService
    : IRecordatoriosPredeterminadosService
{
    private readonly IAgendaRepository _agendaRepository;
    private readonly ICurrentUserService _currentUser;

    public RecordatoriosPredeterminadosService(
        IAgendaRepository agendaRepository,
        ICurrentUserService currentUser
    )
    {
        _agendaRepository = agendaRepository;
        _currentUser = currentUser;
    }

    public async Task<
        Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
    > ObtenerPorTipoAsync(
        long tipoEntradaAgendaId,
        CancellationToken cancellationToken = default
    )
    {
        var tipo = await _agendaRepository.ObtenerTipoPorIdAsync(
            tipoEntradaAgendaId,
            cancellationToken
        );

        if (tipo is null)
        {
            return Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
                .Failure(AgendaErrors.TipoNoEncontradoOInactivo);
        }

        return Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
            .Success(Mapear(tipo.RecordatoriosPredeterminados));
    }

    public async Task<
        Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
    > GuardarPorTipoAsync(
        long tipoEntradaAgendaId,
        GuardarRecordatoriosPredeterminadosRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var tipo = await _agendaRepository.ObtenerTipoPorIdAsync(
            tipoEntradaAgendaId,
            cancellationToken
        );

        if (tipo is null)
        {
            return Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
                .Failure(AgendaErrors.TipoNoEncontradoOInactivo);
        }

        var ahora = AhoraUtcConPrecisionPostgres();
        var solicitados = request.Recordatorios
            .Select(x => (x.BaseCalculo, x.MinutosAnticipacion))
            .ToHashSet();

        foreach (var existente in tipo.RecordatoriosPredeterminados)
        {
            var debeEstarActivo = solicitados.Remove(
                (existente.BaseCalculo, existente.MinutosAnticipacion)
            );

            if (existente.Activo == debeEstarActivo)
            {
                continue;
            }

            existente.Activo = debeEstarActivo;
            existente.FechaModificacion = ahora;
            existente.UsuarioModificacion = _currentUser.Usuario;
        }

        foreach (var nuevo in solicitados)
        {
            tipo.RecordatoriosPredeterminados.Add(
                new RecordatorioPredeterminadoTipoAgenda
                {
                    TipoEntradaAgendaId = tipo.TipoEntradaAgendaId,
                    BaseCalculo = nuevo.BaseCalculo,
                    MinutosAnticipacion = nuevo.MinutosAnticipacion,
                    FechaCreacion = ahora,
                    UsuarioCreacion = _currentUser.Usuario,
                    Activo = true,
                }
            );
        }

        await _agendaRepository.GuardarCambiosAsync(cancellationToken);

        return Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
            .Success(Mapear(tipo.RecordatoriosPredeterminados));
    }

    public async Task<
        Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
    > ObtenerPorReglaAsync(
        long reglaVencimientoId,
        CancellationToken cancellationToken = default
    )
    {
        var regla = await _agendaRepository.ObtenerReglaPorIdAsync(
            reglaVencimientoId,
            seguimiento: true,
            cancellationToken: cancellationToken
        );

        if (regla is null)
        {
            return Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
                .Failure(AgendaErrors.ReglaNoEncontrada);
        }

        return Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
            .Success(Mapear(regla.RecordatoriosPredeterminados));
    }

    public async Task<
        Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
    > GuardarPorReglaAsync(
        long reglaVencimientoId,
        GuardarRecordatoriosPredeterminadosRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var regla = await _agendaRepository.ObtenerReglaPorIdAsync(
            reglaVencimientoId,
            seguimiento: true,
            cancellationToken: cancellationToken
        );

        if (regla is null)
        {
            return Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
                .Failure(AgendaErrors.ReglaNoEncontrada);
        }

        var ahora = AhoraUtcConPrecisionPostgres();
        var solicitados = request.Recordatorios
            .Select(x => (x.BaseCalculo, x.MinutosAnticipacion))
            .ToHashSet();

        foreach (var existente in regla.RecordatoriosPredeterminados)
        {
            var debeEstarActivo = solicitados.Remove(
                (existente.BaseCalculo, existente.MinutosAnticipacion)
            );

            if (existente.Activo == debeEstarActivo)
            {
                continue;
            }

            existente.Activo = debeEstarActivo;
            existente.FechaModificacion = ahora;
            existente.UsuarioModificacion = _currentUser.Usuario;
        }

        foreach (var nuevo in solicitados)
        {
            regla.RecordatoriosPredeterminados.Add(
                new RecordatorioPredeterminadoReglaVencimiento
                {
                    ReglaVencimientoId = regla.ReglaVencimientoId,
                    BaseCalculo = nuevo.BaseCalculo,
                    MinutosAnticipacion = nuevo.MinutosAnticipacion,
                    FechaCreacion = ahora,
                    UsuarioCreacion = _currentUser.Usuario,
                    Activo = true,
                }
            );
        }

        await _agendaRepository.GuardarCambiosAsync(cancellationToken);

        return Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
            .Success(Mapear(regla.RecordatoriosPredeterminados));
    }

    private static IReadOnlyCollection<RecordatorioPredeterminadoResponse> Mapear(
        IEnumerable<RecordatorioPredeterminadoTipoAgenda> recordatorios
    )
    {
        return recordatorios
            .Where(x => x.Activo)
            .OrderBy(x => x.BaseCalculo)
            .ThenBy(x => x.MinutosAnticipacion)
            .Select(x => new RecordatorioPredeterminadoResponse
            {
                RecordatorioPredeterminadoId =
                    x.RecordatorioPredeterminadoTipoAgendaId,
                BaseCalculo = x.BaseCalculo,
                MinutosAnticipacion = x.MinutosAnticipacion,
                Activo = x.Activo,
            })
            .ToArray();
    }

    private static IReadOnlyCollection<RecordatorioPredeterminadoResponse> Mapear(
        IEnumerable<RecordatorioPredeterminadoReglaVencimiento> recordatorios
    )
    {
        return recordatorios
            .Where(x => x.Activo)
            .OrderBy(x => x.BaseCalculo)
            .ThenBy(x => x.MinutosAnticipacion)
            .Select(x => new RecordatorioPredeterminadoResponse
            {
                RecordatorioPredeterminadoId =
                    x.RecordatorioPredeterminadoReglaVencimientoId,
                BaseCalculo = x.BaseCalculo,
                MinutosAnticipacion = x.MinutosAnticipacion,
                Activo = x.Activo,
            })
            .ToArray();
    }

    private static DateTime AhoraUtcConPrecisionPostgres()
    {
        var ahora = DateTime.UtcNow;

        return new DateTime(
            ahora.Ticks - ahora.Ticks % 10,
            DateTimeKind.Utc
        );
    }
}
