using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

public sealed class ReglasVencimientoService : IReglasVencimientoService
{
    private const int DiasProximoVencimiento = 7;

    private readonly IAgendaRepository _agendaRepository;
    private readonly ICurrentUserService _currentUser;

    public ReglasVencimientoService(
        IAgendaRepository agendaRepository,
        ICurrentUserService currentUser
    )
    {
        _agendaRepository = agendaRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ReglaVencimientoResponse>> ObtenerTodasAsync(
        bool soloActivas,
        CancellationToken cancellationToken = default
    )
    {
        var reglas = await _agendaRepository.ObtenerReglasAsync(
            soloActivas,
            cancellationToken
        );

        return reglas.Select(MapearResponse).ToArray();
    }

    public async Task<Result<ReglaVencimientoResponse>> ObtenerPorIdAsync(
        long reglaVencimientoId,
        CancellationToken cancellationToken = default
    )
    {
        var regla = await _agendaRepository.ObtenerReglaPorIdAsync(
            reglaVencimientoId,
            seguimiento: false,
            cancellationToken: cancellationToken
        );

        return regla is null
            ? Result<ReglaVencimientoResponse>.Failure(AgendaErrors.ReglaNoEncontrada)
            : Result<ReglaVencimientoResponse>.Success(MapearResponse(regla));
    }

    public async Task<Result<ReglaVencimientoResponse>> CrearAsync(
        GuardarReglaVencimientoRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var nombre = request.Nombre.Trim();

        if (
            await _agendaRepository.ExisteNombreReglaAsync(
                nombre,
                cancellationToken: cancellationToken
            )
        )
        {
            return Result<ReglaVencimientoResponse>.Failure(
                AgendaErrors.NombreReglaDuplicado
            );
        }

        var tipo = await _agendaRepository.ObtenerTipoPorIdAsync(
            request.TipoEntradaAgendaId,
            cancellationToken
        );

        if (tipo is null || !tipo.Activo)
        {
            return Result<ReglaVencimientoResponse>.Failure(
                AgendaErrors.TipoNoEncontradoOInactivo
            );
        }

        var regla = new ReglaVencimiento
        {
            TipoEntradaAgendaId = tipo.TipoEntradaAgendaId,
            TipoEntrada = tipo,
            Nombre = nombre,
            Descripcion = NormalizarOpcional(request.Descripcion),
            CantidadDias = request.CantidadDias,
            TipoComputo = request.TipoComputo,
            SentidoCalculo = request.SentidoCalculo,
            PrioridadGenerada = request.PrioridadGenerada,
            HoraSugerida = request.HoraSugerida,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = request.Activo,
        };

        await _agendaRepository.AgregarReglaAsync(regla, cancellationToken);
        await _agendaRepository.GuardarCambiosAsync(cancellationToken);

        return Result<ReglaVencimientoResponse>.Success(MapearResponse(regla));
    }

    public async Task<Result<ReglaVencimientoResponse>> ActualizarAsync(
        long reglaVencimientoId,
        GuardarReglaVencimientoRequest request,
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
            return Result<ReglaVencimientoResponse>.Failure(
                AgendaErrors.ReglaNoEncontrada
            );
        }

        var nombre = request.Nombre.Trim();

        if (
            await _agendaRepository.ExisteNombreReglaAsync(
                nombre,
                reglaVencimientoId,
                cancellationToken
            )
        )
        {
            return Result<ReglaVencimientoResponse>.Failure(
                AgendaErrors.NombreReglaDuplicado
            );
        }

        var tipo = await _agendaRepository.ObtenerTipoPorIdAsync(
            request.TipoEntradaAgendaId,
            cancellationToken
        );

        if (tipo is null || !tipo.Activo)
        {
            return Result<ReglaVencimientoResponse>.Failure(
                AgendaErrors.TipoNoEncontradoOInactivo
            );
        }

        regla.TipoEntradaAgendaId = tipo.TipoEntradaAgendaId;
        regla.TipoEntrada = tipo;
        regla.Nombre = nombre;
        regla.Descripcion = NormalizarOpcional(request.Descripcion);
        regla.CantidadDias = request.CantidadDias;
        regla.TipoComputo = request.TipoComputo;
        regla.SentidoCalculo = request.SentidoCalculo;
        regla.PrioridadGenerada = request.PrioridadGenerada;
        regla.HoraSugerida = request.HoraSugerida;
        regla.Activo = request.Activo;
        regla.FechaModificacion = DateTime.UtcNow;
        regla.UsuarioModificacion = _currentUser.Usuario;

        await _agendaRepository.GuardarCambiosAsync(cancellationToken);

        return Result<ReglaVencimientoResponse>.Success(MapearResponse(regla));
    }

    public async Task<Result<AplicacionReglaVencimientoResponse>> AplicarAsync(
        long reglaVencimientoId,
        AplicarReglaVencimientoRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var claveIdempotencia = ConstruirClaveIdempotencia(
            reglaVencimientoId,
            request.ClaveIdempotencia
        );
        var generacionExistente = await _agendaRepository
            .ObtenerGeneracionPorClaveAsync(
                claveIdempotencia,
                cancellationToken
            );

        if (generacionExistente is not null)
        {
            return Result<AplicacionReglaVencimientoResponse>.Success(
                MapearAplicacionResponse(generacionExistente)
            );
        }

        var regla = await _agendaRepository.ObtenerReglaPorIdAsync(
            reglaVencimientoId,
            seguimiento: true,
            cancellationToken: cancellationToken
        );

        if (regla is null)
        {
            return Result<AplicacionReglaVencimientoResponse>.Failure(
                AgendaErrors.ReglaNoEncontrada
            );
        }

        if (!regla.Activo)
        {
            return Result<AplicacionReglaVencimientoResponse>.Failure(
                AgendaErrors.ReglaInactiva
            );
        }

        if (!regla.TipoEntrada.Activo)
        {
            return Result<AplicacionReglaVencimientoResponse>.Failure(
                AgendaErrors.TipoNoEncontradoOInactivo
            );
        }

        var clienteIds = request.ClienteIds.ToHashSet();
        var casoIds = request.CasoIds.ToHashSet();
        var expedienteIds = request.ExpedienteIds.ToHashSet();
        var responsableIds = request.ResponsableIds.ToHashSet();

        var origenValido = await IncorporarContextoOrigenAsync(
            request,
            clienteIds,
            casoIds,
            expedienteIds,
            responsableIds,
            cancellationToken
        );

        if (!origenValido)
        {
            return Result<AplicacionReglaVencimientoResponse>.Failure(
                AgendaErrors.OrigenFechaBaseNoEncontrado
            );
        }

        var errorReferencias = await ValidarReferenciasAsync(
            clienteIds,
            casoIds,
            expedienteIds,
            responsableIds,
            cancellationToken
        );

        if (errorReferencias is not null)
        {
            return Result<AplicacionReglaVencimientoResponse>.Failure(
                errorReferencias
            );
        }

        var diasInhabiles = regla.TipoComputo == TipoComputoPlazo.DiasHabiles
            ? (
                await _agendaRepository.ObtenerDiasInhabilesActivosAsync(
                    cancellationToken
                )
            ).ToHashSet()
            : new HashSet<DateOnly>();
        var fechaCalculada = CalcularFecha(
            request.FechaBase,
            regla,
            diasInhabiles
        );
        var ahora = DateTime.UtcNow;
        var entrada = new EntradaAgenda
        {
            TipoEntradaAgendaId = regla.TipoEntradaAgendaId,
            TipoEntrada = regla.TipoEntrada,
            Titulo = regla.Nombre,
            Descripcion = regla.Descripcion,
            Estado = EstadoEntradaAgenda.Pendiente,
            Prioridad = regla.PrioridadGenerada,
            FechaInicio = fechaCalculada,
            HoraInicio = regla.HoraSugerida,
            FechaVencimiento = fechaCalculada,
            HoraVencimiento = regla.HoraSugerida,
            ZonaHoraria = EntradaAgenda.ZonaHorariaPredeterminada,
            FechaCreacion = ahora,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        foreach (var clienteId in clienteIds)
        {
            entrada.Clientes.Add(
                new EntradaAgendaCliente { ClienteId = clienteId }
            );
        }

        foreach (var casoId in casoIds)
        {
            entrada.Casos.Add(new EntradaAgendaCaso { CasoId = casoId });
        }

        foreach (var expedienteId in expedienteIds)
        {
            entrada.Expedientes.Add(
                new EntradaAgendaExpediente { ExpedienteId = expedienteId }
            );
        }

        foreach (var responsableId in responsableIds)
        {
            entrada.Responsables.Add(
                new EntradaAgendaResponsable { UsuarioId = responsableId }
            );
        }

        RecordatoriosAgendaFactory.AgregarPredeterminadosDeTipo(
            entrada,
            regla.TipoEntrada.RecordatoriosPredeterminados,
            ahora,
            _currentUser.Usuario
        );
        RecordatoriosAgendaFactory.AgregarPredeterminadosDeRegla(
            entrada,
            regla.RecordatoriosPredeterminados,
            ahora,
            _currentUser.Usuario
        );

        var generacion = new AgendaGeneracionRegla
        {
            ReglaVencimientoId = regla.ReglaVencimientoId,
            ReglaVencimiento = regla,
            EntradaAgenda = entrada,
            FechaBase = request.FechaBase,
            OrigenFechaBase = request.OrigenFechaBase,
            EntradaAgendaOrigenId = request.EntradaAgendaOrigenId,
            CasoOrigenId = request.CasoOrigenId,
            ExpedienteOrigenId = request.ExpedienteOrigenId,
            ObservacionOrigenId = request.ObservacionOrigenId,
            UsuarioGeneracionId = _currentUser.UsuarioId,
            ClaveIdempotencia = claveIdempotencia,
            FechaCreacion = ahora,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        await _agendaRepository.AgregarGeneracionAsync(
            generacion,
            cancellationToken
        );
        await _agendaRepository.GuardarCambiosAsync(cancellationToken);

        return Result<AplicacionReglaVencimientoResponse>.Success(
            MapearAplicacionResponse(generacion)
        );
    }

    private async Task<bool> IncorporarContextoOrigenAsync(
        AplicarReglaVencimientoRequest request,
        ISet<long> clienteIds,
        ISet<long> casoIds,
        ISet<long> expedienteIds,
        ISet<long> responsableIds,
        CancellationToken cancellationToken
    )
    {
        switch (request.OrigenFechaBase)
        {
            case OrigenFechaBaseAgenda.Manual:
                return true;

            case OrigenFechaBaseAgenda.EntradaAgenda:
            {
                var entradaOrigen = await _agendaRepository.ObtenerPorIdAsync(
                    request.EntradaAgendaOrigenId!.Value,
                    seguimiento: false,
                    cancellationToken: cancellationToken
                );

                if (entradaOrigen is null || !entradaOrigen.Activo)
                {
                    return false;
                }

                clienteIds.UnionWith(entradaOrigen.Clientes.Select(x => x.ClienteId));
                casoIds.UnionWith(entradaOrigen.Casos.Select(x => x.CasoId));
                expedienteIds.UnionWith(
                    entradaOrigen.Expedientes.Select(x => x.ExpedienteId)
                );
                responsableIds.UnionWith(
                    entradaOrigen.Responsables.Select(x => x.UsuarioId)
                );
                return true;
            }

            case OrigenFechaBaseAgenda.ExpedienteAdministrativo:
                casoIds.Add(request.CasoOrigenId!.Value);
                return await _agendaRepository.ExistenCasosActivosAsync(
                    new[] { request.CasoOrigenId.Value },
                    cancellationToken
                );

            case OrigenFechaBaseAgenda.ExpedienteJudicial:
                expedienteIds.Add(request.ExpedienteOrigenId!.Value);
                return await _agendaRepository.ExistenExpedientesActivosAsync(
                    new[] { request.ExpedienteOrigenId.Value },
                    cancellationToken
                );

            case OrigenFechaBaseAgenda.Movimiento:
            {
                var observacion = await _agendaRepository
                    .ObtenerObservacionPorIdAsync(
                        request.ObservacionOrigenId!.Value,
                        cancellationToken
                    );

                if (observacion is null)
                {
                    return false;
                }

                AgregarSiExiste(clienteIds, observacion.ClienteId);
                AgregarSiExiste(casoIds, observacion.CasoId);
                AgregarSiExiste(expedienteIds, observacion.ExpedienteId);
                return true;
            }

            default:
                return false;
        }
    }

    private async Task<Error?> ValidarReferenciasAsync(
        IReadOnlyCollection<long> clienteIds,
        IReadOnlyCollection<long> casoIds,
        IReadOnlyCollection<long> expedienteIds,
        IReadOnlyCollection<long> responsableIds,
        CancellationToken cancellationToken
    )
    {
        if (!await _agendaRepository.ExistenClientesActivosAsync(
            clienteIds,
            cancellationToken
        ))
        {
            return AgendaErrors.ClienteNoEncontradoOInactivo;
        }

        if (!await _agendaRepository.ExistenCasosActivosAsync(
            casoIds,
            cancellationToken
        ))
        {
            return AgendaErrors.CasoNoEncontradoOInactivo;
        }

        if (!await _agendaRepository.ExistenExpedientesActivosAsync(
            expedienteIds,
            cancellationToken
        ))
        {
            return AgendaErrors.ExpedienteNoEncontradoOInactivo;
        }

        if (!await _agendaRepository.ExistenUsuariosActivosAsync(
            responsableIds,
            cancellationToken
        ))
        {
            return AgendaErrors.ResponsableNoEncontradoOInactivo;
        }

        return null;
    }

    private static DateOnly CalcularFecha(
        DateOnly fechaBase,
        ReglaVencimiento regla,
        IReadOnlySet<DateOnly> diasInhabiles
    )
    {
        var paso = regla.SentidoCalculo == SentidoCalculoPlazo.Despues ? 1 : -1;

        if (regla.TipoComputo == TipoComputoPlazo.DiasCorridos)
        {
            return fechaBase.AddDays(paso * regla.CantidadDias);
        }

        var fecha = fechaBase;
        var diasComputados = 0;

        while (diasComputados < regla.CantidadDias)
        {
            fecha = fecha.AddDays(paso);

            if (
                fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                || diasInhabiles.Contains(fecha)
            )
            {
                continue;
            }

            diasComputados++;
        }

        return fecha;
    }

    private static AplicacionReglaVencimientoResponse MapearAplicacionResponse(
        AgendaGeneracionRegla generacion
    )
    {
        return new AplicacionReglaVencimientoResponse
        {
            AgendaGeneracionReglaId = generacion.AgendaGeneracionReglaId,
            ReglaVencimientoId = generacion.ReglaVencimientoId,
            ReglaNombre = generacion.ReglaVencimiento.Nombre,
            FechaBase = generacion.FechaBase,
            FechaCalculada = generacion.EntradaAgenda.FechaVencimiento!.Value,
            OrigenFechaBase = generacion.OrigenFechaBase,
            EntradaAgendaOrigenId = generacion.EntradaAgendaOrigenId,
            CasoOrigenId = generacion.CasoOrigenId,
            ExpedienteOrigenId = generacion.ExpedienteOrigenId,
            ObservacionOrigenId = generacion.ObservacionOrigenId,
            ClaveIdempotencia = generacion.ClaveIdempotencia,
            Entrada = MapearEntradaResponse(generacion.EntradaAgenda),
        };
    }

    private static EntradaAgendaResponse MapearEntradaResponse(
        EntradaAgenda entrada
    )
    {
        var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById(entrada.ZonaHoraria)
        );
        var hoy = DateOnly.FromDateTime(fechaLocal);
        var diasParaVencimiento = entrada.FechaVencimiento.HasValue
            ? entrada.FechaVencimiento.Value.DayNumber - hoy.DayNumber
            : (int?)null;

        return new EntradaAgendaResponse
        {
            EntradaAgendaId = entrada.EntradaAgendaId,
            TipoEntradaAgendaId = entrada.TipoEntradaAgendaId,
            TipoEntradaNombre = entrada.TipoEntrada.Nombre,
            TipoEntradaColor = entrada.TipoEntrada.Color,
            Titulo = entrada.Titulo,
            Descripcion = entrada.Descripcion,
            Estado = entrada.Estado,
            Prioridad = entrada.Prioridad,
            FechaInicio = entrada.FechaInicio,
            HoraInicio = entrada.HoraInicio,
            FechaVencimiento = entrada.FechaVencimiento,
            HoraVencimiento = entrada.HoraVencimiento,
            DiasParaVencimiento = diasParaVencimiento,
            EstaVencida = diasParaVencimiento < 0,
            ProximaAVencer = diasParaVencimiento is >= 0 and <= DiasProximoVencimiento,
            ZonaHoraria = entrada.ZonaHoraria,
            ClienteIds = entrada.Clientes.Select(x => x.ClienteId).ToArray(),
            CasoIds = entrada.Casos.Select(x => x.CasoId).ToArray(),
            ExpedienteIds = entrada.Expedientes
                .Select(x => x.ExpedienteId)
                .ToArray(),
            ResponsableIds = entrada.Responsables
                .Select(x => x.UsuarioId)
                .ToArray(),
            FechaCreacion = entrada.FechaCreacion,
            FechaModificacion = entrada.FechaModificacion,
            Activo = entrada.Activo,
        };
    }

    private static string ConstruirClaveIdempotencia(
        long reglaVencimientoId,
        string claveSolicitud
    ) => $"{reglaVencimientoId}:{claveSolicitud.Trim()}";

    private static void AgregarSiExiste(ISet<long> ids, long? id)
    {
        if (id.HasValue)
        {
            ids.Add(id.Value);
        }
    }

    private static ReglaVencimientoResponse MapearResponse(
        ReglaVencimiento regla
    )
    {
        return new ReglaVencimientoResponse
        {
            ReglaVencimientoId = regla.ReglaVencimientoId,
            TipoEntradaAgendaId = regla.TipoEntradaAgendaId,
            TipoEntradaNombre = regla.TipoEntrada.Nombre,
            TipoEntradaColor = regla.TipoEntrada.Color,
            Nombre = regla.Nombre,
            Descripcion = regla.Descripcion,
            CantidadDias = regla.CantidadDias,
            TipoComputo = regla.TipoComputo,
            SentidoCalculo = regla.SentidoCalculo,
            PrioridadGenerada = regla.PrioridadGenerada,
            HoraSugerida = regla.HoraSugerida,
            FechaCreacion = regla.FechaCreacion,
            FechaModificacion = regla.FechaModificacion,
            Activo = regla.Activo,
        };
    }

    private static string? NormalizarOpcional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
