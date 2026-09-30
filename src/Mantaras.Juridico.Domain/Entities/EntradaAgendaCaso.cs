namespace Mantaras.Juridico.Domain.Entities;

public sealed class EntradaAgendaCaso
{
    public long EntradaAgendaId { get; set; }

    public long CasoId { get; set; }

    public EntradaAgenda EntradaAgenda { get; set; } = null!;

    public Caso Caso { get; set; } = null!;
}
