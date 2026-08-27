namespace Mantaras.Juridico.Application.Features.TiposBeneficio.Requests;

// El alta y la edición tienen el mismo cuerpo. Activo se cambia por acciones separadas.
public class GuardarTipoBeneficioRequest
{
    public string Nombre { get; set; } = string.Empty;
}
