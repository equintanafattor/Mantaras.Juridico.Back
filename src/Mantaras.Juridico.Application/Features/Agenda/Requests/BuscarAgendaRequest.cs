using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Requests;

public sealed class BuscarAgendaRequest : PagedRequest
{
    public DateOnly? Desde { get; set; }

    public DateOnly? Hasta { get; set; }

    public long? TipoEntradaAgendaId { get; set; }

    public EstadoEntradaAgenda? Estado { get; set; }

    public long? ResponsableId { get; set; }

    public long? ClienteId { get; set; }

    public long? CasoId { get; set; }

    public long? ExpedienteId { get; set; }

    public string? Busqueda { get; set; }

    public bool SoloActivos { get; set; } = true;
}
