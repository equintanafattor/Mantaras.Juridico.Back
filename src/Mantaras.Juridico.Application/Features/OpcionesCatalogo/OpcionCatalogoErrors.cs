using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.OpcionesCatalogo;

public static class OpcionCatalogoErrors
{
    public static readonly Error TipoInvalido = new(
        "Catalogos.TipoInvalido",
        "El catálogo solicitado no existe."
    );

    public static readonly Error NoEncontrado = new(
        "Catalogos.OpcionNoEncontrada",
        "La opción de catálogo no existe."
    );

    public static readonly Error NombreDuplicado = new(
        "Catalogos.NombreDuplicado",
        "Ya existe una opción con ese nombre en el catálogo."
    );

    public static Error DatosInvalidos(string mensaje) => new(
        "Catalogos.DatosInvalidos",
        mensaje
    );
}
