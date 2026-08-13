using Mantaras.Juridico.Application.Features.Panel.Responses;

namespace Mantaras.Juridico.Application.Features.Panel.Services;

public interface IPanelService
{
    Task<PanelResumenResponse> ObtenerResumenAsync(
        CancellationToken cancellationToken = default
    );
}