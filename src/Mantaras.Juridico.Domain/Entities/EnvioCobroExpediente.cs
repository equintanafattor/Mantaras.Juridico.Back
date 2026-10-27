using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class EnvioCobroExpediente : AuditableEntity
{
    public long EnvioCobroExpedienteId { get; set; }

    public long ExpedienteId { get; set; }

    public TipoDocumentoCobro TipoDocumento { get; set; }

    public DateOnly FechaEnvio { get; set; }

    public long AgendaGeneracionReglaId { get; set; }

    public Expediente Expediente { get; set; } = null!;

    public AgendaGeneracionRegla AgendaGeneracionRegla { get; set; } = null!;
}
