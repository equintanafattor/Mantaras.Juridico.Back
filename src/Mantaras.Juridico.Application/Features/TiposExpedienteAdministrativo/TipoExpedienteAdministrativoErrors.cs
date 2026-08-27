using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo;

public static class TipoExpedienteAdministrativoErrors
{
    public static readonly Error NoEncontrado = new(
        "TiposExpedienteAdministrativo.NoEncontrado",
        "El tipo de expediente administrativo solicitado no existe."
    );

    public static readonly Error NombreDuplicado = new(
        "TiposExpedienteAdministrativo.NombreDuplicado",
        "Ya existe un tipo de expediente administrativo con ese nombre. Si está inactivo, reactivá el registro existente."
    );

    public static Error DatosInvalidos(string mensaje) => new(
        "TiposExpedienteAdministrativo.DatosInvalidos",
        mensaje
    );
}
