namespace Mantaras.Juridico.Application.Features.Observaciones.Responses;

public sealed class ObservacionResponse
{
    public long ObservacionId { get; set; }

    public string Texto { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacion { get; set; } = string.Empty;
}