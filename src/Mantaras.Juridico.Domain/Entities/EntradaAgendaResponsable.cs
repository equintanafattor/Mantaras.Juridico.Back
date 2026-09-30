namespace Mantaras.Juridico.Domain.Entities;

public sealed class EntradaAgendaResponsable
{
    public long EntradaAgendaId { get; set; }

    public long UsuarioId { get; set; }

    public EntradaAgenda EntradaAgenda { get; set; } = null!;
}
