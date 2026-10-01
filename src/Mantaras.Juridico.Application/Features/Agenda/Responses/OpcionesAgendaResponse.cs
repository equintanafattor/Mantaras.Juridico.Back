namespace Mantaras.Juridico.Application.Features.Agenda.Responses;

public sealed class OpcionAgendaResponse
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class OpcionesAgendaResponse
{
    public IReadOnlyCollection<OpcionAgendaResponse> TiposEntrada { get; set; } = Array.Empty<OpcionAgendaResponse>();
    public IReadOnlyCollection<OpcionAgendaResponse> Responsables { get; set; } = Array.Empty<OpcionAgendaResponse>();
}
