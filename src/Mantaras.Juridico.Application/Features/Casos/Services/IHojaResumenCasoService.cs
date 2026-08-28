using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Casos.Requests;
using Mantaras.Juridico.Application.Features.Casos.Responses;

namespace Mantaras.Juridico.Application.Features.Casos.Services;

public interface IHojaResumenCasoService
{
    Task<Result<HojaResumenCasoResponse>> ObtenerAsync(
        long casoId,
        CancellationToken cancellationToken = default
    );

    Task<Result<HojaResumenCasoResponse>> GuardarAsync(
        long casoId,
        GuardarHojaResumenCasoRequest request,
        CancellationToken cancellationToken = default
    );
}