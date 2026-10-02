using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Responses;

public sealed class EntradaAgendaListadoResponse
{
    public long EntradaAgendaId { get; set; }

    public long? RecurrenciaAgendaId { get; set; }

    public long TipoEntradaAgendaId { get; set; }

    public string TipoEntradaNombre { get; set; } = string.Empty;

    public string? TipoEntradaColor { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public EstadoEntradaAgenda Estado { get; set; }

    public PrioridadAgenda Prioridad { get; set; }

    public DateOnly FechaInicio { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public TimeOnly? HoraVencimiento { get; set; }

    public int? DiasParaVencimiento { get; set; }

    public bool EstaVencida { get; set; }

    public bool ProximaAVencer { get; set; }

    public IReadOnlyCollection<RelacionAgendaResponse> Clientes { get; set; } =
        Array.Empty<RelacionAgendaResponse>();

    public IReadOnlyCollection<RelacionAgendaResponse> Casos { get; set; } =
        Array.Empty<RelacionAgendaResponse>();

    public IReadOnlyCollection<RelacionAgendaResponse> Expedientes { get; set; } =
        Array.Empty<RelacionAgendaResponse>();

    public IReadOnlyCollection<RelacionAgendaResponse> Responsables { get; set; } =
        Array.Empty<RelacionAgendaResponse>();

    public bool Activo { get; set; }
}

public sealed class RelacionAgendaResponse
{
    public long Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Referencia { get; set; }
}
