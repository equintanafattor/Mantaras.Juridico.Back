using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Casos.Requests;
using Mantaras.Juridico.Application.Features.Casos.Responses;

namespace Mantaras.Juridico.Application.Features.Casos.Services;

public interface ICasosService
{
    Task<Result<CasoResponse>> CrearAsync(
        CrearCasoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<CasoDetalleResponse>> ObtenerPorIdAsync(
        long casoId,
        CancellationToken cancellationToken = default
    );

    Task<PagedResponse<CasoResponse>> BuscarAsync(
        BuscarCasosRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<CasoResponse>> ActualizarAsync(
        long casoId,
        ActualizarCasoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(
        long casoId,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> RestaurarAsync(
        long casoId,
        CancellationToken cancellationToken = default
    );
}
