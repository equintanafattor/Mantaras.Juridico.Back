namespace Mantaras.Juridico.Application.Features.OpcionesCatalogo.Responses;

public sealed class OpcionCatalogoResponse
{
    public long OpcionCatalogoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
