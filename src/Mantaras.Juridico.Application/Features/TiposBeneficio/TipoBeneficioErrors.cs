using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.TiposBeneficio;

public static class TipoBeneficioErrors
{
    public static readonly Error NoEncontrado = new(
        "TiposBeneficio.NoEncontrado",
        "El tipo de beneficio solicitado no existe."
    );

    public static readonly Error NombreDuplicado = new(
        "TiposBeneficio.NombreDuplicado",
        "Ya existe un tipo de beneficio con ese nombre. Si está inactivo, reactivá el registro existente."
    );

    public static Error DatosInvalidos(string mensaje) => new(
        "TiposBeneficio.DatosInvalidos",
        mensaje
    );
}
