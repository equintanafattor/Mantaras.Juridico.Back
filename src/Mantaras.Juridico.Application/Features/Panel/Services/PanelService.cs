using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Panel.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Panel.Services;

public sealed class PanelService : IPanelService
{
    private const int CantidadActividadReciente = 8;
    private const int CantidadAgendaProxima = 8;

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

        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
            EntradaAgenda.ZonaHorariaPredeterminada
        );
        var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            zonaHoraria
        );
        var hoy = DateOnly.FromDateTime(fechaLocal);
        var agenda = await _panelRepository.ObtenerResumenAgendaAsync(
            hoy,
            CantidadAgendaProxima,
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
            Agenda = new PanelAgendaResponse
            {
                Hoy = agenda.Hoy,
                Proximos = agenda.Proximos,
                Vencidos = agenda.Vencidos,
                ElementosProximos = agenda.ElementosProximos
                    .Select(x => MapearAgenda(x, hoy))
                    .ToArray(),
            },
        };
    }

    private static PanelAgendaItemResponse MapearAgenda(
        PanelAgendaItemData entrada,
        DateOnly hoy
    )
    {
        var contextos = entrada.Clientes
            .Select(x => MapearContexto("Cliente", x, $"/clientes/{x.Id}"))
            .Concat(
                entrada.Casos.Select(x =>
                    MapearContexto(
                        "ExpedienteAdministrativo",
                        x,
                        $"/casos/{x.Id}"
                    )
                )
            )
            .Concat(
                entrada.Expedientes.Select(x =>
                    MapearContexto(
                        "ExpedienteJudicial",
                        x,
                        $"/expedientes/{x.Id}"
                    )
                )
            )
            .ToArray();

        return new PanelAgendaItemResponse
        {
            EntradaAgendaId = entrada.EntradaAgendaId,
            Titulo = entrada.Titulo,
            TipoEntradaNombre = entrada.TipoEntradaNombre,
            TipoEntradaColor = entrada.TipoEntradaColor,
            Estado = entrada.Estado,
            Prioridad = entrada.Prioridad,
            FechaReferencia = entrada.FechaReferencia,
            HoraReferencia = entrada.HoraReferencia,
            EsDeHoy = entrada.FechaReferencia == hoy,
            Contextos = contextos,
        };
    }

    private static PanelAgendaContextoResponse MapearContexto(
        string tipo,
        PanelAgendaContextoData contexto,
        string url
    )
    {
        return new PanelAgendaContextoResponse
        {
            Tipo = tipo,
            Id = contexto.Id,
            Nombre = contexto.Nombre,
            Url = url,
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
            CasoId = expediente.Casos
                .OrderBy(x => x.CasoId)
                .Select(x => x.CasoId)
                .First(),
            ExpedienteId = expediente.ExpedienteId,
            Titulo = expediente.Caratula,
            Referencia =
                expediente.NumeroExpediente
                ?? expediente.Casos
                    .OrderBy(x => x.CasoId)
                    .Select(x => x.Caso.Titulo)
                    .First(),
            FechaActividad =
                expediente.FechaModificacion
                ?? expediente.FechaCreacion,
        };
    }
}
