using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.Juzgados;

public static class JuzgadoErrors
{
    public static readonly Error NoEncontrado = new(
        "Juzgados.NoEncontrado",
        "El juzgado solicitado no existe."
    );

    public static readonly Error NombreDuplicado = new(
        "Juzgados.NombreDuplicado",
        "Ya existe un juzgado con ese nombre. Si está inactivo, reactivá el registro existente."
    );

    public static Error DatosInvalidos(string mensaje) => new(
        "Juzgados.DatosInvalidos",
        mensaje
    );
}
