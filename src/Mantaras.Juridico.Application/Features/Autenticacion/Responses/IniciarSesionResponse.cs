namespace Mantaras.Juridico.Application.Features.Autenticacion.Responses;

public sealed class IniciarSesionResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiraEnUtc { get; set; }

    public UsuarioAutenticadoResponse Usuario { get; set; } = new();
}
