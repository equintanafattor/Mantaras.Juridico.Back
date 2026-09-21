using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Requests;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Responses;

namespace Mantaras.Juridico.Application.Features.OpcionesCatalogo.Services;

public interface IOpcionesCatalogoService
{
    Task<Result<OpcionCatalogoResponse>> CrearAsync(
        string tipo,
        GuardarOpcionCatalogoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<OpcionCatalogoResponse>> ObtenerPorIdAsync(
        string tipo,
        long id,
        CancellationToken cancellationToken = default
    );

    Task<Result<OpcionCatalogoResponse>> ActualizarAsync(
        string tipo,
        long id,
        GuardarOpcionCatalogoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> CambiarEstadoAsync(
        string tipo,
        long id,
        bool activar,
        CancellationToken cancellationToken = default
    );

    Task<Result<PagedResponse<OpcionCatalogoResponse>>> BuscarAsync(
        string tipo,
        BuscarCatalogosRequest request,
        CancellationToken cancellationToken = default
    );
}
