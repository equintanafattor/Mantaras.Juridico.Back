namespace Mantaras.Juridico.Domain.Entities;

public sealed class CasoExpediente
{
    public long CasoId { get; set; }

    public long ExpedienteId { get; set; }

    public Caso Caso { get; set; } = null!;

    public Expediente Expediente { get; set; } = null!;
}
