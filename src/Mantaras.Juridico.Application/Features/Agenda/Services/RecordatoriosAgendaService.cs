using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

public sealed class RecordatoriosAgendaService : IRecordatoriosAgendaService
{
    private static readonly TimeOnly HoraPredeterminada = new(9, 0);

    private readonly IAgendaRepository _agendaRepository;
    private readonly IRecordatoriosAgendaRepository _recordatoriosRepository;
    private readonly ICurrentUserService _currentUser;

    public RecordatoriosAgendaService(
        IAgendaRepository agendaRepository,
        IRecordatoriosAgendaRepository recordatoriosRepository,
        ICurrentUserService currentUser
    )
    {
        _agendaRepository = agendaRepository;
        _recordatoriosRepository = recordatoriosRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<RecordatorioAgendaResponse>> CrearAsync(
        long entradaAgendaId,
        CrearRecordatorioAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (!DatosValidos(request))
            return Result<RecordatorioAgendaResponse>.Failure(AgendaErrors.RecordatorioDatosInvalidos);

        var entrada = await _agendaRepository.ObtenerPorIdAsync(
            entradaAgendaId,
            seguimiento: false,
            cancellationToken: cancellationToken
        );

        if (entrada is null || !entrada.Activo)
        {
            return Result<RecordatorioAgendaResponse>.Failure(
                AgendaErrors.NoEncontrada
            );
        }

        var fechaBaseLocal = ObtenerFechaBaseLocal(
            entrada,
            request.BaseCalculo
        );

        if (!fechaBaseLocal.HasValue)
        {
            return Result<RecordatorioAgendaResponse>.Failure(
                AgendaErrors.FechaBaseRecordatorioNoDisponible
            );
        }

        DateTime fechaProgramadaUtc;
        try { fechaProgramadaUtc = CalcularFechaProgramadaUtc(entrada, fechaBaseLocal.Value, request.MinutosAnticipacion); }
        catch (ArgumentException) { return Result<RecordatorioAgendaResponse>.Failure(AgendaErrors.RecordatorioFechaFueraDeRango); }

        if (
            await _recordatoriosRepository.ExisteAsync(
                entradaAgendaId,
                fechaProgramadaUtc,
                cancellationToken
            )
        )
        {
            return Result<RecordatorioAgendaResponse>.Failure(
                AgendaErrors.RecordatorioDuplicado
            );
        }

        var recordatorio = new RecordatorioAgenda
        {
            EntradaAgendaId = entrada.EntradaAgendaId,
            Canal = CanalRecordatorioAgenda.Interno,
            FechaProgramadaUtc = fechaProgramadaUtc,
            Atendido = false,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        await _recordatoriosRepository.AgregarAsync(
            recordatorio,
            cancellationToken
        );
        await _recordatoriosRepository.GuardarCambiosAsync(cancellationToken);

        recordatorio.EntradaAgenda = entrada;

        return Result<RecordatorioAgendaResponse>.Success(
            MapearResponse(recordatorio)
        );
    }

    public async Task<Result<RecordatorioAgendaResponse>> ReprogramarAsync(
        long recordatorioAgendaId,
        CrearRecordatorioAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (!DatosValidos(request))
            return Result<RecordatorioAgendaResponse>.Failure(AgendaErrors.RecordatorioDatosInvalidos);
        var recordatorio = await _recordatoriosRepository.ObtenerPorIdAsync(recordatorioAgendaId, seguimiento: true, cancellationToken: cancellationToken);
        if (recordatorio is null || !recordatorio.EntradaAgenda.Activo)
            return Result<RecordatorioAgendaResponse>.Failure(AgendaErrors.RecordatorioNoEncontrado);
        if (recordatorio.Atendido)
            return Result<RecordatorioAgendaResponse>.Failure(AgendaErrors.RecordatorioAtendido);
        var fechaBase = ObtenerFechaBaseLocal(recordatorio.EntradaAgenda, request.BaseCalculo);
        if (!fechaBase.HasValue)
            return Result<RecordatorioAgendaResponse>.Failure(AgendaErrors.FechaBaseRecordatorioNoDisponible);
        DateTime fechaProgramada;
        try { fechaProgramada = CalcularFechaProgramadaUtc(recordatorio.EntradaAgenda, fechaBase.Value, request.MinutosAnticipacion); }
        catch (ArgumentException) { return Result<RecordatorioAgendaResponse>.Failure(AgendaErrors.RecordatorioFechaFueraDeRango); }
        if (await _recordatoriosRepository.ExisteAsync(recordatorio.EntradaAgendaId, fechaProgramada, cancellationToken, recordatorioAgendaId))
            return Result<RecordatorioAgendaResponse>.Failure(AgendaErrors.RecordatorioDuplicado);
        if (recordatorio.FechaProgramadaUtc == fechaProgramada)
            return Result<RecordatorioAgendaResponse>.Success(MapearResponse(recordatorio));
        recordatorio.FechaProgramadaUtc = fechaProgramada;
        recordatorio.FechaModificacion = DateTime.UtcNow;
        recordatorio.UsuarioModificacion = _currentUser.Usuario;
        await _recordatoriosRepository.GuardarCambiosAsync(cancellationToken);
        return Result<RecordatorioAgendaResponse>.Success(MapearResponse(recordatorio));
    }

    public async Task<Result<bool>> QuitarAsync(long recordatorioAgendaId, CancellationToken cancellationToken = default)
    {
        var recordatorio = await _recordatoriosRepository.ObtenerPorIdAsync(recordatorioAgendaId, seguimiento: true, cancellationToken: cancellationToken);
        if (recordatorio is null || !recordatorio.EntradaAgenda.Activo)
            return Result<bool>.Failure(AgendaErrors.RecordatorioNoEncontrado);
        if (recordatorio.Atendido)
            return Result<bool>.Failure(AgendaErrors.RecordatorioAtendido);
        recordatorio.Activo = false;
        recordatorio.FechaModificacion = DateTime.UtcNow;
        recordatorio.UsuarioModificacion = _currentUser.Usuario;
        await _recordatoriosRepository.GuardarCambiosAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    private static bool DatosValidos(CrearRecordatorioAgendaRequest request) =>
        Enum.IsDefined(request.BaseCalculo) && request.MinutosAnticipacion >= 0 && request.MinutosAnticipacion <= 525600;

    private static DateTime CalcularFechaProgramadaUtc(EntradaAgenda entrada, DateTime fechaBaseLocal, int minutosAnticipacion)
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(entrada.ZonaHoraria);
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(fechaBaseLocal, DateTimeKind.Unspecified), zona)
            .AddMinutes(-minutosAnticipacion);
    }

    public async Task<PagedResponse<RecordatorioAgendaResponse>> BuscarAsync(
        BuscarRecordatoriosAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var recordatorios = await _recordatoriosRepository.BuscarAsync(
            request.EntradaAgendaId,
            request.Estado,
            request.Page,
            request.PageSize,
            cancellationToken
        );
        var totalItems = await _recordatoriosRepository.ContarAsync(
            request.EntradaAgendaId,
            request.Estado,
            cancellationToken
        );

        return new PagedResponse<RecordatorioAgendaResponse>
        {
            Items = recordatorios.Select(MapearResponse).ToArray(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
        };
    }

    public async Task<Result<RecordatorioAgendaResponse>> AtenderAsync(
        long recordatorioAgendaId,
        CancellationToken cancellationToken = default
    )
    {
        var recordatorio = await _recordatoriosRepository.ObtenerPorIdAsync(
            recordatorioAgendaId,
            seguimiento: true,
            cancellationToken: cancellationToken
        );

        if (recordatorio is null)
        {
            return Result<RecordatorioAgendaResponse>.Failure(
                AgendaErrors.RecordatorioNoEncontrado
            );
        }

        if (recordatorio.Atendido)
        {
            return Result<RecordatorioAgendaResponse>.Success(
                MapearResponse(recordatorio)
            );
        }

        var ahora = DateTime.UtcNow;
        ahora = new DateTime(
            ahora.Ticks - ahora.Ticks % 10,
            DateTimeKind.Utc
        );

        recordatorio.Atendido = true;
        recordatorio.FechaAtendidoUtc = ahora;
        recordatorio.UsuarioAtendioId = _currentUser.UsuarioId;
        recordatorio.FechaModificacion = ahora;
        recordatorio.UsuarioModificacion = _currentUser.Usuario;

        await _recordatoriosRepository.GuardarCambiosAsync(cancellationToken);

        return Result<RecordatorioAgendaResponse>.Success(
            MapearResponse(recordatorio)
        );
    }

    private static DateTime? ObtenerFechaBaseLocal(
        EntradaAgenda entrada,
        BaseCalculoRecordatorioAgenda baseCalculo
    )
    {
        return baseCalculo switch
        {
            BaseCalculoRecordatorioAgenda.Inicio => entrada.FechaInicio.ToDateTime(
                entrada.HoraInicio ?? HoraPredeterminada
            ),
            BaseCalculoRecordatorioAgenda.Vencimiento when
                entrada.FechaVencimiento.HasValue =>
                    entrada.FechaVencimiento.Value.ToDateTime(
                        entrada.HoraVencimiento ?? HoraPredeterminada
                    ),
            _ => null,
        };
    }

    private static RecordatorioAgendaResponse MapearResponse(
        RecordatorioAgenda recordatorio
    )
    {
        var estado = recordatorio.Atendido
            ? EstadoRecordatorioAgenda.Atendido
            : recordatorio.FechaProgramadaUtc <= DateTime.UtcNow
                ? EstadoRecordatorioAgenda.Vencido
                : EstadoRecordatorioAgenda.Pendiente;

        return new RecordatorioAgendaResponse
        {
            RecordatorioAgendaId = recordatorio.RecordatorioAgendaId,
            EntradaAgendaId = recordatorio.EntradaAgendaId,
            EntradaTitulo = recordatorio.EntradaAgenda.Titulo,
            TipoEntradaNombre = recordatorio.EntradaAgenda.TipoEntrada.Nombre,
            TipoEntradaColor = recordatorio.EntradaAgenda.TipoEntrada.Color,
            Canal = recordatorio.Canal,
            FechaProgramadaUtc = recordatorio.FechaProgramadaUtc,
            Estado = estado,
            Atendido = recordatorio.Atendido,
            FechaAtendidoUtc = recordatorio.FechaAtendidoUtc,
            UsuarioAtendioId = recordatorio.UsuarioAtendioId,
            FechaCreacion = recordatorio.FechaCreacion,
            Activo = recordatorio.Activo,
        };
    }
}
