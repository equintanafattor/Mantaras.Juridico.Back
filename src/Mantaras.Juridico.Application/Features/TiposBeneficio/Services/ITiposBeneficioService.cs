using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.TiposBeneficio.Requests;
using Mantaras.Juridico.Application.Features.TiposBeneficio.Responses;

namespace Mantaras.Juridico.Application.Features.TiposBeneficio.Services;

public interface ITiposBeneficioService
{
    Task<Result<TipoBeneficioResponse>> CrearAsync(
        GuardarTipoBeneficioRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<TipoBeneficioResponse>> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<Result<TipoBeneficioResponse>> ActualizarAsync(
        long id,
        GuardarTipoBeneficioRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<bool>> ReactivarAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<PagedResponse<TipoBeneficioResponse>>> BuscarAsync(
        BuscarCatalogosRequest request,
        CancellationToken cancellationToken = default
    );
}
