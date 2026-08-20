using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Observaciones.Requests;
using Mantaras.Juridico.Application.Features.Observaciones.Responses;

namespace Mantaras.Juridico.Application.Features.Observaciones.Services;

public interface IObservacionesService
{
    Task<Result<IReadOnlyCollection<ObservacionResponse>>>
        ObtenerPorClienteAsync(
            long clienteId,
            CancellationToken cancellationToken = default
        );

    Task<Result<IReadOnlyCollection<ObservacionResponse>>>
        ObtenerPorCasoAsync(
            long casoId,
            CancellationToken cancellationToken = default
        );

    Task<Result<IReadOnlyCollection<ObservacionResponse>>>
        ObtenerPorExpedienteAsync(
            long expedienteId,
            CancellationToken cancellationToken = default
        );

    Task<Result<ObservacionResponse>> CrearParaClienteAsync(
        long clienteId,
        CrearObservacionRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ObservacionResponse>> CrearParaCasoAsync(
        long casoId,
        CrearObservacionRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ObservacionResponse>> CrearParaExpedienteAsync(
        long expedienteId,
        CrearObservacionRequest request,
        CancellationToken cancellationToken = default
    );
}