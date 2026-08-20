using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public class Expediente : AuditableEntity
{
    public long ExpedienteId { get; set; }
    public long CasoId { get; set; }
    public long? ExpedientePadreId { get; set; }
    public string? NumeroExpediente { get; set; }
    public string Caratula { get; set; } = null!;
    public string? Juzgado { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public string? EstadoLegal { get; set; }
    public ICollection<Observacion> HistorialObservaciones { get; set; } =
        new List<Observacion>();
    public Caso Caso { get; set; } = null!;
    public Expediente? ExpedientePadre { get; set; }
    public TipoExpediente TipoExpediente { get; set; }
    public ICollection<Expediente> ExpedientesDerivados { get; set; } = new List<Expediente>();
}
