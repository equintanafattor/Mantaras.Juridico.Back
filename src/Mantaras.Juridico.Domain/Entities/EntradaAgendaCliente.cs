namespace Mantaras.Juridico.Domain.Entities;

public sealed class EntradaAgendaCliente
{
    public long EntradaAgendaId { get; set; }

    public long ClienteId { get; set; }

    public EntradaAgenda EntradaAgenda { get; set; } = null!;

    public Cliente Cliente { get; set; } = null!;
}
