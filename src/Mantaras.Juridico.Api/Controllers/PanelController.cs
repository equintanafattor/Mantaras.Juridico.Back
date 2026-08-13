using Mantaras.Juridico.Application.Features.Panel.Responses;
using Mantaras.Juridico.Application.Features.Panel.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/panel")]
public sealed class PanelController : ControllerBase
{
    private readonly IPanelService _panelService;

    public PanelController(IPanelService panelService)
    {
        _panelService = panelService;
    }

    [HttpGet("resumen")]
    [ProducesResponseType(
        typeof(PanelResumenResponse),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<PanelResumenResponse>> ObtenerResumen(
        CancellationToken cancellationToken
    )
    {
        var response = await _panelService.ObtenerResumenAsync(
            cancellationToken
        );

        return Ok(response);
    }
}