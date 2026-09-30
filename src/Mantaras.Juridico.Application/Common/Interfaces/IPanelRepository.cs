using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IPanelRepository
{
    Task<int> ContarClientesActivosAsync(
        CancellationToken cancellationToken = default
    );

    Task<int> ContarCasosActivosAsync(
        CancellationToken cancellationToken = default
    );

    Task<int> ContarExpedientesActivosAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Caso>> ObtenerCasosRecientesAsync(
        int cantidad,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Expediente>> ObtenerExpedientesRecientesAsync(
        int cantidad,
        CancellationToken cancellationToken = default
    );

    Task<PanelAgendaData> ObtenerResumenAgendaAsync(
        DateOnly hoy,
        int cantidadProximos,
        CancellationToken cancellationToken = default
    );
}

public sealed class PanelAgendaData
{
    public int Hoy { get; init; }

    public int Proximos { get; init; }

    public int Vencidos { get; init; }

    public IReadOnlyCollection<PanelAgendaItemData> ElementosProximos { get; init; } =
        Array.Empty<PanelAgendaItemData>();
}

public sealed class PanelAgendaItemData
{
    public long EntradaAgendaId { get; init; }

    public string Titulo { get; init; } = string.Empty;

    public string TipoEntradaNombre { get; init; } = string.Empty;

    public string? TipoEntradaColor { get; init; }

    public EstadoEntradaAgenda Estado { get; init; }

    public PrioridadAgenda Prioridad { get; init; }

    public DateOnly FechaReferencia { get; init; }

    public TimeOnly? HoraReferencia { get; init; }

    public IReadOnlyCollection<PanelAgendaContextoData> Clientes { get; init; } =
        Array.Empty<PanelAgendaContextoData>();

    public IReadOnlyCollection<PanelAgendaContextoData> Casos { get; init; } =
        Array.Empty<PanelAgendaContextoData>();

    public IReadOnlyCollection<PanelAgendaContextoData> Expedientes { get; init; } =
        Array.Empty<PanelAgendaContextoData>();
}

public sealed class PanelAgendaContextoData
{
    public long Id { get; init; }

    public string Nombre { get; init; } = string.Empty;
}
