namespace Mantaras.Juridico.Application.Features.Juzgados.Responses;

public class JuzgadoResponse
{
    public long JuzgadoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
