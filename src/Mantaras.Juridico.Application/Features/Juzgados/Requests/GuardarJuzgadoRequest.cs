namespace Mantaras.Juridico.Application.Features.Juzgados.Requests;

// El alta y la edición tienen el mismo cuerpo. Activo se cambia por acciones separadas.
public class GuardarJuzgadoRequest
{
    public string Nombre { get; set; } = string.Empty;
}
