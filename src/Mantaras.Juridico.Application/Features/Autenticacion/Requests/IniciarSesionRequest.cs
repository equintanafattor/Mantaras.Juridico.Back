namespace Mantaras.Juridico.Application.Features.Autenticacion.Requests;

public sealed class IniciarSesionRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
