using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.Autenticacion;

public static class AutenticacionErrors
{
    public static readonly Error CredencialesInvalidas = new(
        "Autenticacion.CredencialesInvalidas",
        "El correo electrónico o la contraseña son incorrectos."
    );
}
