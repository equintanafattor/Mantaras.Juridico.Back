using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.TiposEntradaAgenda;

public static class TipoEntradaAgendaErrors
{
    public static readonly Error NoEncontrado = new(
        "TiposEntradaAgenda.NoEncontrado",
        "El tipo de entrada solicitado no existe."
    );

    public static readonly Error NombreDuplicado = new(
        "TiposEntradaAgenda.NombreDuplicado",
        "Ya existe un tipo de entrada con ese nombre. Si está inactivo, reactivá el registro existente."
    );

    public static Error DatosInvalidos(string mensaje) => new(
        "TiposEntradaAgenda.DatosInvalidos",
        mensaje
    );
}
