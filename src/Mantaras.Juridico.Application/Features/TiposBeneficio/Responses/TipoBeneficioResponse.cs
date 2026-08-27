namespace Mantaras.Juridico.Application.Features.TiposBeneficio.Responses;

public class TipoBeneficioResponse
{
    public long TipoBeneficioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
