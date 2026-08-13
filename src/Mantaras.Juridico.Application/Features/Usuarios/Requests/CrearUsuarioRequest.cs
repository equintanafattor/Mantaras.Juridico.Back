namespace Mantaras.Juridico.Application.Features.Usuarios.Requests;

public sealed class CrearUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;
}
