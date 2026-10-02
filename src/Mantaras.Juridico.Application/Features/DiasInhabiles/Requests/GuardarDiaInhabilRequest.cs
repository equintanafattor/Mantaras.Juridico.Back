namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
public sealed class GuardarDiaInhabilRequest
{
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
