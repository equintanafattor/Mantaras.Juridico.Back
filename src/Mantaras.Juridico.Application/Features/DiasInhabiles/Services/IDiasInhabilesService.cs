using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Services;

public interface IDiasInhabilesService
{
    Task<Result<DiaInhabilResponse>> CrearAsync(
        GuardarDiaInhabilRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<DiaInhabilResponse>> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<Result<DiaInhabilResponse>> ActualizarAsync(
        long id,
        GuardarDiaInhabilRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<bool>> ReactivarAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<PagedResponse<DiaInhabilResponse>>> BuscarAsync(
        BuscarDiasInhabilesRequest request,
        CancellationToken cancellationToken = default
    );
}
