using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class AgendaGeneracionRegla : AuditableEntity
{
    public long AgendaGeneracionReglaId { get; set; }

    public long ReglaVencimientoId { get; set; }

    public long EntradaAgendaId { get; set; }

    public DateOnly FechaBase { get; set; }

    public OrigenFechaBaseAgenda OrigenFechaBase { get; set; }

    public long? EntradaAgendaOrigenId { get; set; }

    public long? CasoOrigenId { get; set; }

    public long? ExpedienteOrigenId { get; set; }

    public long? ObservacionOrigenId { get; set; }

    public long? UsuarioGeneracionId { get; set; }

    public string ClaveIdempotencia { get; set; } = string.Empty;

    public ReglaVencimiento ReglaVencimiento { get; set; } = null!;

    public EntradaAgenda EntradaAgenda { get; set; } = null!;

    public EntradaAgenda? EntradaAgendaOrigen { get; set; }

    public Caso? CasoOrigen { get; set; }

    public Expediente? ExpedienteOrigen { get; set; }

    public Observacion? ObservacionOrigen { get; set; }
}
