namespace Mantaras.Juridico.Application.Features.Autenticacion.Responses;

public sealed class UsuarioAutenticadoResponse
{
    public long UsuarioId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}
