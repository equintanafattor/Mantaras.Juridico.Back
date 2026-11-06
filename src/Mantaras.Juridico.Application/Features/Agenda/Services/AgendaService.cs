using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

public sealed class AgendaService : IAgendaService
{
    private readonly IAgendaRepository _agendaRepository;
    private readonly ICurrentUserService _currentUser;

    public AgendaService(
        IAgendaRepository agendaRepository,
        ICurrentUserService currentUser
    )
    {
        _agendaRepository = agendaRepository;
        _currentUser = currentUser;
    }

    public Task<OpcionesAgendaResponse> ObtenerOpcionesAsync(CancellationToken cancellationToken = default)
    {
        return _agendaRepository.ObtenerOpcionesAsync(cancellationToken);
    }

    public async Task<Result<EntradaAgendaResponse>> CrearAsync(
        GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await ValidarReferenciasAsync(
            request,
            cancellationToken
        );

        if (validacion.IsFailure)
        {
            return Result<EntradaAgendaResponse>.Failure(validacion.Errors);
        }

        var ahora = DateTime.UtcNow;
        var entrada = new EntradaAgenda
        {
            TipoEntradaAgendaId = request.TipoEntradaAgendaId,
            TipoEntrada = validacion.Value!,
            Titulo = request.Titulo.Trim(),
            Descripcion = NormalizarOpcional(request.Descripcion),
            Prioridad = request.Prioridad,
            FechaInicio = request.FechaInicio,
            HoraInicio = request.HoraInicio,
            FechaFin = request.FechaFin,
            HoraFin = request.HoraFin,
            FechaVencimiento = request.FechaVencimiento,
            HoraVencimiento = request.HoraVencimiento,
            ZonaHoraria = EntradaAgenda.ZonaHorariaPredeterminada,
            FechaCreacion = ahora,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        ReemplazarRelaciones(entrada, request);
        RecordatoriosAgendaFactory.AgregarPredeterminadosDeTipo(
            entrada,
            validacion.Value!.RecordatoriosPredeterminados,
            ahora,
            _currentUser.Usuario
        );

        if (request.Recurrencia is { } repeticion)
        {
            var fechas = GenerarFechas(request.FechaInicio, repeticion);
            var serie = new RecurrenciaAgenda
            {
                Frecuencia = repeticion.Frecuencia,
                Intervalo = repeticion.Intervalo,
                FechaInicio = request.FechaInicio,
                FechaFin = fechas[^1],
                MaximoOcurrencias = fechas.Count,
                OcurrenciasGeneradas = fechas.Count,
                ZonaHoraria = entrada.ZonaHoraria,
                FechaCreacion = ahora,
                UsuarioCreacion = _currentUser.Usuario,
                Activo = true,
            };
            entrada.Recurrencia = serie;
            for (var i = 1; i < fechas.Count; i++)
            {
                var desplazamiento = fechas[i].DayNumber - request.FechaInicio.DayNumber;
                var ocurrencia = new EntradaAgenda
                {
                    Recurrencia = serie,
                    TipoEntradaAgendaId = entrada.TipoEntradaAgendaId,
                    TipoEntrada = validacion.Value!,
                    Titulo = entrada.Titulo,
                    Descripcion = entrada.Descripcion,
                    Prioridad = entrada.Prioridad,
                    FechaInicio = fechas[i],
                    HoraInicio = entrada.HoraInicio,
                    FechaFin = entrada.FechaFin?.AddDays(desplazamiento),
                    HoraFin = entrada.HoraFin,
                    FechaVencimiento = entrada.FechaVencimiento?.AddDays(desplazamiento),
                    HoraVencimiento = entrada.HoraVencimiento,
                    ZonaHoraria = entrada.ZonaHoraria,
                    FechaCreacion = ahora,
                    UsuarioCreacion = _currentUser.Usuario,
                    Activo = true,
                };
                ReemplazarRelaciones(ocurrencia, request);
                RecordatoriosAgendaFactory.AgregarPredeterminadosDeTipo(
                    ocurrencia, validacion.Value!.RecordatoriosPredeterminados, ahora, _currentUser.Usuario
                );
                await _agendaRepository.AgregarAsync(ocurrencia, cancellationToken);
            }
        }
        await _agendaRepository.AgregarAsync(entrada, cancellationToken);
        await _agendaRepository.GuardarCambiosAsync(cancellationToken);

        return await ResponderAsync(entrada.EntradaAgendaId, cancellationToken);
    }

    public Task<Result<EntradaAgendaResponse>> CrearVencimientoManualAsync(
        GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (!request.FechaVencimiento.HasValue)
        {
            return Task.FromResult(
                Result<EntradaAgendaResponse>.Failure(
                    AgendaErrors.FechaVencimientoObligatoria
                )
            );
        }

        return CrearAsync(request, cancellationToken);
    }

    public async Task<Result<EntradaAgendaResponse>> ObtenerPorIdAsync(
        long entradaAgendaId,
        CancellationToken cancellationToken = default
    )
    {
        return await ResponderAsync(entradaAgendaId, cancellationToken);
    }

    public async Task<Result<EntradaAgendaResponse>> ActualizarAsync(
        long entradaAgendaId,
        GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var entrada = await _agendaRepository.ObtenerPorIdAsync(
            entradaAgendaId,
            seguimiento: true,
            cancellationToken: cancellationToken
        );

        if (entrada is null)
        {
            return Result<EntradaAgendaResponse>.Failure(AgendaErrors.NoEncontrada);
        }

        // Una edición siempre afecta sólo esta ocurrencia.
        if (request.Recurrencia is not null)
        {
            return Result<EntradaAgendaResponse>.Failure(
                new Error("Agenda.RecurrenciaEdicion", "La recurrencia sólo puede definirse al crear una entrada.")
            );
        }

        var validacion = await ValidarReferenciasAsync(
            request,
            cancellationToken
        );

        if (validacion.IsFailure)
        {
            return Result<EntradaAgendaResponse>.Failure(validacion.Errors);
        }

        entrada.TipoEntradaAgendaId = request.TipoEntradaAgendaId;
        entrada.TipoEntrada = validacion.Value!;
        entrada.Titulo = request.Titulo.Trim();
        entrada.Descripcion = NormalizarOpcional(request.Descripcion);
        entrada.Prioridad = request.Prioridad;
        entrada.FechaInicio = request.FechaInicio;
        entrada.HoraInicio = request.HoraInicio;
        entrada.FechaFin = request.FechaFin;
        entrada.HoraFin = request.HoraFin;
        entrada.FechaVencimiento = request.FechaVencimiento;
        entrada.HoraVencimiento = request.HoraVencimiento;
        entrada.FechaModificacion = DateTime.UtcNow;
        entrada.UsuarioModificacion = _currentUser.Usuario;

        ReemplazarRelaciones(entrada, request);

        await _agendaRepository.GuardarCambiosAsync(cancellationToken);

        return await ResponderAsync(entrada.EntradaAgendaId, cancellationToken);
    }

    public async Task<PagedResponse<EntradaAgendaListadoResponse>> BuscarAsync(
        BuscarAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var entradas = await _agendaRepository.BuscarAsync(
            request,
            cancellationToken
        );

        var totalItems = await _agendaRepository.ContarAsync(
            request,
            cancellationToken
        );

        var responsableIds = entradas
            .SelectMany(x => x.Responsables)
            .Select(x => x.UsuarioId)
            .Distinct()
            .ToArray();

        var nombresResponsables = await _agendaRepository.ObtenerNombresUsuariosAsync(
            responsableIds,
            cancellationToken
        );

        return new PagedResponse<EntradaAgendaListadoResponse>
        {
            Items = entradas
                .Select(x => EntradaAgendaMapper.MapearListado(x, nombresResponsables))
                .ToArray(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
        };
    }

    public async Task<Result<EntradaAgendaResponse>> CambiarEstadoAsync(
        long entradaAgendaId,
        CambiarEstadoAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var entrada = await _agendaRepository.ObtenerPorIdAsync(
            entradaAgendaId,
            seguimiento: true,
            cancellationToken: cancellationToken
        );

        if (entrada is null)
        {
            return Result<EntradaAgendaResponse>.Failure(AgendaErrors.NoEncontrada);
        }

        if (entrada.Estado == request.Estado)
        {
            return await ResponderAsync(entrada.EntradaAgendaId, cancellationToken);
        }

        if (!CriteriosAgenda.TransicionesPermitidas(entrada.Estado).Contains(request.Estado))
        {
            return Result<EntradaAgendaResponse>.Failure(
                AgendaErrors.TransicionEstadoInvalida(
                    entrada.Estado,
                    request.Estado
                )
            );
        }

        entrada.Estado = request.Estado;
        entrada.FechaModificacion = DateTime.UtcNow;
        entrada.UsuarioModificacion = _currentUser.Usuario;

        await _agendaRepository.GuardarCambiosAsync(cancellationToken);

        return await ResponderAsync(entrada.EntradaAgendaId, cancellationToken);
    }

    private async Task<Result<EntradaAgendaResponse>> ResponderAsync(
        long entradaAgendaId,
        CancellationToken cancellationToken
    )
    {
        var response = await EntradaAgendaMapper.ObtenerDetalleAsync(
            _agendaRepository,
            entradaAgendaId,
            cancellationToken
        );

        return response is null
            ? Result<EntradaAgendaResponse>.Failure(AgendaErrors.NoEncontrada)
            : Result<EntradaAgendaResponse>.Success(response);
    }

    private async Task<Result<TipoEntradaAgenda>> ValidarReferenciasAsync(
        GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken
    )
    {
        var tipo = await _agendaRepository.ObtenerTipoPorIdAsync(
            request.TipoEntradaAgendaId,
            cancellationToken
        );

        if (tipo is null || !tipo.Activo)
        {
            return Result<TipoEntradaAgenda>.Failure(
                AgendaErrors.TipoNoEncontradoOInactivo
            );
        }

        if (
            !await _agendaRepository.ExistenClientesActivosAsync(
                request.ClienteIds,
                cancellationToken
            )
        )
        {
            return Result<TipoEntradaAgenda>.Failure(
                AgendaErrors.ClienteNoEncontradoOInactivo
            );
        }

        if (
            !await _agendaRepository.ExistenCasosActivosAsync(
                request.CasoIds,
                cancellationToken
            )
        )
        {
            return Result<TipoEntradaAgenda>.Failure(
                AgendaErrors.CasoNoEncontradoOInactivo
            );
        }

        if (
            !await _agendaRepository.ExistenExpedientesActivosAsync(
                request.ExpedienteIds,
                cancellationToken
            )
        )
        {
            return Result<TipoEntradaAgenda>.Failure(
                AgendaErrors.ExpedienteNoEncontradoOInactivo
            );
        }

        if (
            !await _agendaRepository.ExistenUsuariosActivosAsync(
                request.ResponsableIds,
                cancellationToken
            )
        )
        {
            return Result<TipoEntradaAgenda>.Failure(
                AgendaErrors.ResponsableNoEncontradoOInactivo
            );
        }

        return Result<TipoEntradaAgenda>.Success(tipo);
    }

    private static void ReemplazarRelaciones(
        EntradaAgenda entrada,
        GuardarEntradaAgendaRequest request
    )
    {
        var clienteIds = request.ClienteIds.ToHashSet();
        var clientesEliminados = entrada.Clientes
            .Where(x => !clienteIds.Contains(x.ClienteId))
            .ToArray();

        foreach (var relacion in clientesEliminados)
        {
            entrada.Clientes.Remove(relacion);
        }

        var clientesExistentes = entrada.Clientes
            .Select(x => x.ClienteId)
            .ToHashSet();

        foreach (var clienteId in clienteIds.Where(id => !clientesExistentes.Contains(id)))
        {
            entrada.Clientes.Add(
                new EntradaAgendaCliente { ClienteId = clienteId }
            );
        }

        var casoIds = request.CasoIds.ToHashSet();
        var casosEliminados = entrada.Casos
            .Where(x => !casoIds.Contains(x.CasoId))
            .ToArray();

        foreach (var relacion in casosEliminados)
        {
            entrada.Casos.Remove(relacion);
        }

        var casosExistentes = entrada.Casos
            .Select(x => x.CasoId)
            .ToHashSet();

        foreach (var casoId in casoIds.Where(id => !casosExistentes.Contains(id)))
        {
            entrada.Casos.Add(
                new EntradaAgendaCaso { CasoId = casoId }
            );
        }

        var expedienteIds = request.ExpedienteIds.ToHashSet();
        var expedientesEliminados = entrada.Expedientes
            .Where(x => !expedienteIds.Contains(x.ExpedienteId))
            .ToArray();

        foreach (var relacion in expedientesEliminados)
        {
            entrada.Expedientes.Remove(relacion);
        }

        var expedientesExistentes = entrada.Expedientes
            .Select(x => x.ExpedienteId)
            .ToHashSet();

        foreach (
            var expedienteId in expedienteIds.Where(
                id => !expedientesExistentes.Contains(id)
            )
        )
        {
            entrada.Expedientes.Add(
                new EntradaAgendaExpediente { ExpedienteId = expedienteId }
            );
        }

        var responsableIds = request.ResponsableIds.ToHashSet();
        var responsablesEliminados = entrada.Responsables
            .Where(x => !responsableIds.Contains(x.UsuarioId))
            .ToArray();

        foreach (var relacion in responsablesEliminados)
        {
            entrada.Responsables.Remove(relacion);
        }

        var responsablesExistentes = entrada.Responsables
            .Select(x => x.UsuarioId)
            .ToHashSet();

        foreach (
            var usuarioId in responsableIds.Where(
                id => !responsablesExistentes.Contains(id)
            )
        )
        {
            entrada.Responsables.Add(
                new EntradaAgendaResponsable { UsuarioId = usuarioId }
            );
        }
    }

    private static string? NormalizarOpcional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<DateOnly> GenerarFechas(DateOnly inicio, RecurrenciaEntradaRequest regla)
    {
        var fechas = new List<DateOnly>(regla.CantidadOcurrencias);
        for (var i = 0; i < regla.CantidadOcurrencias; i++)
        {
            var salto = checked(i * regla.Intervalo);
            var fecha = regla.Frecuencia switch
            {
                FrecuenciaRecurrenciaAgenda.Diaria => inicio.AddDays(salto),
                FrecuenciaRecurrenciaAgenda.Semanal => inicio.AddDays(checked(salto * 7)),
                FrecuenciaRecurrenciaAgenda.Mensual => FechaMes(inicio, salto),
                FrecuenciaRecurrenciaAgenda.Anual => FechaMes(inicio, checked(salto * 12)),
                _ => throw new ArgumentOutOfRangeException(nameof(regla.Frecuencia)),
            };
            fechas.Add(fecha);
        }
        return fechas;
    }

    private static DateOnly FechaMes(DateOnly inicio, int meses)
    {
        var primerDia = new DateOnly(inicio.Year, inicio.Month, 1).AddMonths(meses);
        return new DateOnly(primerDia.Year, primerDia.Month,
            Math.Min(inicio.Day, DateTime.DaysInMonth(primerDia.Year, primerDia.Month)));
    }
}
