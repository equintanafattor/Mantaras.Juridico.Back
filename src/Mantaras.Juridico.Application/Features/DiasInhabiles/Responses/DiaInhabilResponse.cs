using Mantaras.Juridico.Domain.Enums;
namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;
public sealed class DiaInhabilResponse
{
    public long DiaInhabilId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public OrigenDiaInhabil Origen { get; set; }
    public string? Fuente { get; set; }
    public DateOnly? FechaFuente { get; set; }
    public DateTime? FechaSincronizacion { get; set; }
    public bool ModificadoManualmente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
