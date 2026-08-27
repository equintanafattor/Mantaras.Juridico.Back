namespace Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo.Requests;

// El alta y la edición tienen el mismo cuerpo. Activo se cambia por acciones separadas.
public class GuardarTipoExpedienteAdministrativoRequest
{
    public string Nombre { get; set; } = string.Empty;
}
