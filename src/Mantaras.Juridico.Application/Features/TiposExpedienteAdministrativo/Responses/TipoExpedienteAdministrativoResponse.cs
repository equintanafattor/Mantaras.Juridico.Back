namespace Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo.Responses;

public class TipoExpedienteAdministrativoResponse
{
    public long TipoExpedienteAdministrativoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
