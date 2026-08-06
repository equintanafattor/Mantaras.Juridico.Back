using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Expedientes.Requests;
using Mantaras.Juridico.Application.Features.Expedientes.Responses;

namespace Mantaras.Juridico.Application.Features.Expedientes.Services;

public interface IExpedientesService
{
    Task<Result<ExpedienteResponse>> CrearAsync(
        CrearExpedienteRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ExpedienteDetalleResponse>> ObtenerPorIdAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    );

    Task<PagedResponse<ExpedienteResponse>> BuscarAsync(
        BuscarExpedientesRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ExpedienteResponse>> ActualizarAsync(
        long expedienteId,
        ActualizarExpedienteRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> RestaurarAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    );
}