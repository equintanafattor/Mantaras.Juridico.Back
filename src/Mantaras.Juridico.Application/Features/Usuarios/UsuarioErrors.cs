using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.Usuarios;

public static class UsuarioErrors
{
    public static readonly Error NoEncontrado = new(
        "Usuarios.NoEncontrado",
        "El usuario solicitado no existe."
    );

    public static readonly Error EmailDuplicado = new(
        "Usuarios.EmailDuplicado",
        "Ya existe un usuario registrado con ese correo electrónico."
    );

    public static readonly Error RolInvalido = new(
        "Usuarios.RolInvalido",
        "El rol informado no es válido."
    );

    public static readonly Error NoPuedeDesactivarse = new(
        "Usuarios.NoPuedeDesactivarse",
        "No podés desactivar tu propio usuario."
    );

    public static readonly Error UltimoAdministrador = new(
        "Usuarios.UltimoAdministrador",
        "No se puede desactivar al último administrador activo."
    );
}
