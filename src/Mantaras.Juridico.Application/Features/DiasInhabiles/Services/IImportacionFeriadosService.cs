using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Services;

public interface IImportacionFeriadosService
{
    Task<Result<VistaPreviaImportacionFeriadosResponse>> ObtenerVistaPreviaAsync(
        int anio,
        CancellationToken cancellationToken = default
    );

    Task<Result<ConfirmacionImportacionFeriadosResponse>> ConfirmarAsync(
        int anio,
        ConfirmarImportacionFeriadosRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<EstadoCalendarioFeriadosResponse>> ObtenerEstadoAsync(
        int anio,
        CancellationToken cancellationToken = default
    );
}
