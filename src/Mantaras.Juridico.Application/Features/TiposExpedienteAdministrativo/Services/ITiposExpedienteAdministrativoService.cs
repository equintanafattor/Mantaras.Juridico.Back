using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo.Requests;
using Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo.Responses;

namespace Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo.Services;

public interface ITiposExpedienteAdministrativoService
{
    Task<Result<TipoExpedienteAdministrativoResponse>> CrearAsync(
        GuardarTipoExpedienteAdministrativoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<TipoExpedienteAdministrativoResponse>> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<Result<TipoExpedienteAdministrativoResponse>> ActualizarAsync(
        long id,
        GuardarTipoExpedienteAdministrativoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<bool>> ReactivarAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<PagedResponse<TipoExpedienteAdministrativoResponse>>> BuscarAsync(
        BuscarCatalogosRequest request,
        CancellationToken cancellationToken = default
    );
}
