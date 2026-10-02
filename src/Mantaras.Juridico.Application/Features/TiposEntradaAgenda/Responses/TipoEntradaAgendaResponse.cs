namespace Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Responses;

public class TipoEntradaAgendaResponse
{
    public long TipoEntradaAgendaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
