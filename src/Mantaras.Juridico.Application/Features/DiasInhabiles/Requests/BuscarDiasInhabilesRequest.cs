namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
public sealed class BuscarDiasInhabilesRequest
{
    public int? Anio { get; set; }
    public bool SoloActivos { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
