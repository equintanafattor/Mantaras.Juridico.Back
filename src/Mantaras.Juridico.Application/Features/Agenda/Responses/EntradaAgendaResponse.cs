using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Responses;

public sealed class EntradaAgendaResponse
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

    // Abierta, sin vencimiento y con (FechaFin ?? FechaInicio) anterior a hoy.
    public bool EstaAtrasada { get; set; }

    // Estados a los que se puede pasar desde el actual con PATCH /api/agenda/{id}/estado.
    public IReadOnlyCollection<EstadoEntradaAgenda> TransicionesPermitidas { get; set; } =
        Array.Empty<EstadoEntradaAgenda>();

    public string ZonaHoraria { get; set; } = string.Empty;

    public IReadOnlyCollection<long> ClienteIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> CasoIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> ExpedienteIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> ResponsableIds { get; set; } = Array.Empty<long>();

    // Mismas relaciones con nombre que el listado. Los *Ids se mantienen por compatibilidad.
    public IReadOnlyCollection<RelacionAgendaResponse> Clientes { get; set; } =
        Array.Empty<RelacionAgendaResponse>();

    public IReadOnlyCollection<RelacionAgendaResponse> Casos { get; set; } =
        Array.Empty<RelacionAgendaResponse>();

    public IReadOnlyCollection<RelacionAgendaResponse> Expedientes { get; set; } =
        Array.Empty<RelacionAgendaResponse>();

    public IReadOnlyCollection<RelacionAgendaResponse> Responsables { get; set; } =
        Array.Empty<RelacionAgendaResponse>();

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }
}
