namespace Mantaras.Juridico.Application.Features.Catalogos.Requests;

public class BuscarCatalogosRequest
{
    public string? Busqueda { get; set; }
    public bool SoloActivos { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
