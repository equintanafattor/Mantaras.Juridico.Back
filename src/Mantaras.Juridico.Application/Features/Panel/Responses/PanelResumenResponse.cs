namespace Mantaras.Juridico.Application.Features.Panel.Responses;

public sealed class PanelResumenResponse
{
    public PanelMetricasResponse Metricas { get; init; } = new();

    public IReadOnlyCollection<ActividadRecienteResponse> ActividadReciente { get; init; } =
        Array.Empty<ActividadRecienteResponse>();

    public PanelAlertasResponse Alertas { get; init; } = new();
}

public sealed class PanelMetricasResponse
{
    public int ClientesActivos { get; init; }

    public int CasosActivos { get; init; }

    public int ExpedientesActivos { get; init; }
}

public sealed class ActividadRecienteResponse
{
    public string Tipo { get; init; } = string.Empty;

    public long CasoId { get; init; }

    public long? ExpedienteId { get; init; }

    public string Titulo { get; init; } = string.Empty;

    public string? Referencia { get; init; }

    public DateTime FechaActividad { get; init; }
}

public sealed class PanelAlertasResponse
{
    public bool Disponible { get; init; }

    public int TotalPendientes { get; init; }
}