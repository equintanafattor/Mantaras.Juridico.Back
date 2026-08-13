using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Panel.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Panel.Services;

public sealed class PanelService : IPanelService
{
    private const int CantidadActividadReciente = 8;

    private readonly IPanelRepository _panelRepository;

    public PanelService(IPanelRepository panelRepository)
    {
        _panelRepository = panelRepository;
    }

    public async Task<PanelResumenResponse> ObtenerResumenAsync(
        CancellationToken cancellationToken = default
    )
    {
        // Las consultas son secuenciales porque comparten el mismo DbContext.
        var clientesActivos =
            await _panelRepository.ContarClientesActivosAsync(
                cancellationToken
            );

        var casosActivos =
            await _panelRepository.ContarCasosActivosAsync(
                cancellationToken
            );

        var expedientesActivos =
            await _panelRepository.ContarExpedientesActivosAsync(
                cancellationToken
            );

        var casosRecientes =
            await _panelRepository.ObtenerCasosRecientesAsync(
                CantidadActividadReciente,
                cancellationToken
            );

        var expedientesRecientes =
            await _panelRepository.ObtenerExpedientesRecientesAsync(
                CantidadActividadReciente,
                cancellationToken
            );

        var actividadReciente = casosRecientes
            .Select(MapearCaso)
            .Concat(expedientesRecientes.Select(MapearExpediente))
            .OrderByDescending(x => x.FechaActividad)
            .Take(CantidadActividadReciente)
            .ToArray();

        return new PanelResumenResponse
        {
            Metricas = new PanelMetricasResponse
            {
                ClientesActivos = clientesActivos,
                CasosActivos = casosActivos,
                ExpedientesActivos = expedientesActivos,
            },
            ActividadReciente = actividadReciente,
            Alertas = new PanelAlertasResponse
            {
                Disponible = false,
                TotalPendientes = 0,
            },
        };
    }

    private static ActividadRecienteResponse MapearCaso(Caso caso)
    {
        return new ActividadRecienteResponse
        {
            Tipo = "Caso",
            CasoId = caso.CasoId,
            ExpedienteId = null,
            Titulo = caso.Titulo,
            Referencia = caso.TipoTramite,
            FechaActividad =
                caso.FechaModificacion ?? caso.FechaCreacion,
        };
    }

    private static ActividadRecienteResponse MapearExpediente(
        Expediente expediente
    )
    {
        return new ActividadRecienteResponse
        {
            Tipo = "Expediente",
            CasoId = expediente.CasoId,
            ExpedienteId = expediente.ExpedienteId,
            Titulo = expediente.Caratula,
            Referencia =
                expediente.NumeroExpediente ?? expediente.Caso.Titulo,
            FechaActividad =
                expediente.FechaModificacion
                ?? expediente.FechaCreacion,
        };
    }
}