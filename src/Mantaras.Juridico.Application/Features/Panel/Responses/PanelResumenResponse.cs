namespace Mantaras.Juridico.Application.Features.Panel.Responses;

using Mantaras.Juridico.Domain.Enums;

public sealed class PanelResumenResponse
{
    public PanelMetricasResponse Metricas { get; init; } = new();

    public IReadOnlyCollection<ActividadRecienteResponse> ActividadReciente { get; init; } =
        Array.Empty<ActividadRecienteResponse>();

    public PanelAlertasResponse Alertas { get; init; } = new();

    public PanelAgendaResponse Agenda { get; init; } = new();
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

public sealed class PanelAgendaResponse
{
    public int Hoy { get; init; }

    public int Proximos { get; init; }

    public int Vencidos { get; init; }

    public IReadOnlyCollection<PanelAgendaItemResponse> ElementosProximos
    {
        get;
        init;
    } = Array.Empty<PanelAgendaItemResponse>();
}

public sealed class PanelAgendaItemResponse
{
    public long EntradaAgendaId { get; init; }

    public string Titulo { get; init; } = string.Empty;

    public string TipoEntradaNombre { get; init; } = string.Empty;

    public string? TipoEntradaColor { get; init; }

    public EstadoEntradaAgenda Estado { get; init; }

    public PrioridadAgenda Prioridad { get; init; }

    public DateOnly FechaReferencia { get; init; }

    public TimeOnly? HoraReferencia { get; init; }

    public bool EsDeHoy { get; init; }

    public IReadOnlyCollection<PanelAgendaContextoResponse> Contextos { get; init; } =
        Array.Empty<PanelAgendaContextoResponse>();
}

public sealed class PanelAgendaContextoResponse
{
    public string Tipo { get; init; } = string.Empty;

    public long Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;
}
