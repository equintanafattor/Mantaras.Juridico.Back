namespace Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Requests;

// El alta y la edición tienen el mismo cuerpo. Activo se cambia por acciones separadas.
public class GuardarTipoEntradaAgendaRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
}
