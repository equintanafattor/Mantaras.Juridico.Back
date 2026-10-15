namespace Mantaras.Juridico.Application.Features.Observaciones.Requests;

public sealed class CrearObservacionRequest
{
    public string Texto { get; set; } = string.Empty;

    public DateOnly? FechaMovimiento { get; set; }
}