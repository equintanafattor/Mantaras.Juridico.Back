namespace Mantaras.Juridico.Domain.Entities;

public sealed class EntradaAgendaExpediente
{
    public long EntradaAgendaId { get; set; }

    public long ExpedienteId { get; set; }

    public EntradaAgenda EntradaAgenda { get; set; } = null!;

    public Expediente Expediente { get; set; } = null!;
}
