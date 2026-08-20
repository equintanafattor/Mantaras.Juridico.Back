using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Casos;
using Mantaras.Juridico.Application.Features.Clientes;
using Mantaras.Juridico.Application.Features.Expedientes;
using Mantaras.Juridico.Application.Features.Observaciones.Requests;
using Mantaras.Juridico.Application.Features.Observaciones.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Observaciones.Services;

public sealed class ObservacionesService : IObservacionesService
{
    private readonly IObservacionRepository _observacionRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICasoRepository _casoRepository;
    private readonly IExpedienteRepository _expedienteRepository;
    private readonly ICurrentUserService _currentUser;

    public ObservacionesService(
        IObservacionRepository observacionRepository,
        IClienteRepository clienteRepository,
        ICasoRepository casoRepository,
        IExpedienteRepository expedienteRepository,
        ICurrentUserService currentUser
    )
    {
        _observacionRepository = observacionRepository;
        _clienteRepository = clienteRepository;
        _casoRepository = casoRepository;
        _expedienteRepository = expedienteRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyCollection<ObservacionResponse>>>
        ObtenerPorClienteAsync(
            long clienteId,
            CancellationToken cancellationToken = default
        )
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(
            clienteId,
            cancellationToken
        );

        if (cliente is null)
        {
            return Result<IReadOnlyCollection<ObservacionResponse>>
                .Failure(ClienteErrors.NoEncontrado);
        }

        var observaciones =
            await _observacionRepository.ObtenerPorClienteIdAsync(
                clienteId,
                cancellationToken
            );

        return CrearResultadoListado(observaciones);
    }

    public async Task<Result<IReadOnlyCollection<ObservacionResponse>>>
        ObtenerPorCasoAsync(
            long casoId,
            CancellationToken cancellationToken = default
        )
    {
        var caso = await _casoRepository.ObtenerPorIdAsync(
            casoId,
            cancellationToken
        );

        if (caso is null)
        {
            return Result<IReadOnlyCollection<ObservacionResponse>>
                .Failure(CasoErrors.NoEncontrado);
        }

        var observaciones =
            await _observacionRepository.ObtenerPorCasoIdAsync(
                casoId,
                cancellationToken
            );

        return CrearResultadoListado(observaciones);
    }

    public async Task<Result<IReadOnlyCollection<ObservacionResponse>>>
        ObtenerPorExpedienteAsync(
            long expedienteId,
            CancellationToken cancellationToken = default
        )
    {
        var expediente = await _expedienteRepository.ObtenerPorIdAsync(
            expedienteId,
            cancellationToken
        );

        if (expediente is null)
        {
            return Result<IReadOnlyCollection<ObservacionResponse>>
                .Failure(ExpedienteErrors.NoEncontrado);
        }

        var observaciones =
            await _observacionRepository.ObtenerPorExpedienteIdAsync(
                expedienteId,
                cancellationToken
            );

        return CrearResultadoListado(observaciones);
    }

    public async Task<Result<ObservacionResponse>> CrearParaClienteAsync(
        long clienteId,
        CrearObservacionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(
            clienteId,
            cancellationToken
        );

        if (cliente is null)
        {
            return Result<ObservacionResponse>.Failure(
                ClienteErrors.NoEncontrado
            );
        }

        var observacion = CrearObservacion(request);
        observacion.ClienteId = clienteId;

        return await GuardarAsync(observacion, cancellationToken);
    }

    public async Task<Result<ObservacionResponse>> CrearParaCasoAsync(
        long casoId,
        CrearObservacionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var caso = await _casoRepository.ObtenerPorIdAsync(
            casoId,
            cancellationToken
        );

        if (caso is null)
        {
            return Result<ObservacionResponse>.Failure(
                CasoErrors.NoEncontrado
            );
        }

        var observacion = CrearObservacion(request);
        observacion.CasoId = casoId;

        return await GuardarAsync(observacion, cancellationToken);
    }

    public async Task<Result<ObservacionResponse>>
        CrearParaExpedienteAsync(
            long expedienteId,
            CrearObservacionRequest request,
            CancellationToken cancellationToken = default
        )
    {
        var expediente = await _expedienteRepository.ObtenerPorIdAsync(
            expedienteId,
            cancellationToken
        );

        if (expediente is null)
        {
            return Result<ObservacionResponse>.Failure(
                ExpedienteErrors.NoEncontrado
            );
        }

        var observacion = CrearObservacion(request);
        observacion.ExpedienteId = expedienteId;

        return await GuardarAsync(observacion, cancellationToken);
    }

    private Observacion CrearObservacion(
        CrearObservacionRequest request
    )
    {
        return new Observacion
        {
            Texto = request.Texto.Trim(),
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
        };
    }

    private async Task<Result<ObservacionResponse>> GuardarAsync(
        Observacion observacion,
        CancellationToken cancellationToken
    )
    {
        await _observacionRepository.AgregarAsync(
            observacion,
            cancellationToken
        );

        await _observacionRepository.GuardarCambiosAsync(
            cancellationToken
        );

        return Result<ObservacionResponse>.Success(
            MapearResponse(observacion)
        );
    }

    private static Result<IReadOnlyCollection<ObservacionResponse>>
        CrearResultadoListado(
            IReadOnlyCollection<Observacion> observaciones
        )
    {
        IReadOnlyCollection<ObservacionResponse> response =
            observaciones.Select(MapearResponse).ToArray();

        return Result<IReadOnlyCollection<ObservacionResponse>>
            .Success(response);
    }

    private static ObservacionResponse MapearResponse(
        Observacion observacion
    )
    {
        return new ObservacionResponse
        {
            ObservacionId = observacion.ObservacionId,
            Texto = observacion.Texto,
            FechaCreacion = observacion.FechaCreacion,
            UsuarioCreacion =
                observacion.UsuarioCreacion ?? "Sistema",
        };
    }
}