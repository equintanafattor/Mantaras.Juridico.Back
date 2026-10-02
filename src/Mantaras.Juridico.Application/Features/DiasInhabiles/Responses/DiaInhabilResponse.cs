namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;
public sealed class DiaInhabilResponse
{
    public long DiaInhabilId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
