using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.Familiares;

public static class FamiliaresErrors
{
    public static readonly Error IdentificadorInvalido = new(
        "Familiares.IdentificadorInvalido",
        "Los identificadores de los clientes deben ser mayores que cero."
    );

    public static readonly Error MismoCliente = new(
        "Familiares.MismoCliente",
        "No se puede vincular un cliente consigo mismo."
    );

    public static readonly Error FamiliarNoEncontrado = new(
        "Familiares.FamiliarNoEncontrado",
        "El cliente seleccionado como familiar no existe."
    );

    public static readonly Error ClientesInactivos = new(
        "Familiares.ClientesInactivos",
        "Ambos clientes deben estar activos para vincularlos."
    );

    public static readonly Error RelacionNoEncontrada = new(
        "Familiares.RelacionNoEncontrada",
        "No existe una relación familiar entre estos clientes."
    );

    public static readonly Error ParentescoDiferente = new(
        "Familiares.ParentescoDiferente",
        "Los clientes ya están vinculados con otro parentesco. "
            + "Desvinculalos antes de registrar el parentesco correcto."
    );

    public static readonly Error ConflictoGuardado = new(
        "Familiares.ConflictoGuardado",
        "Otra solicitud registró esta relación. "
            + "Actualizá el listado antes de continuar."
    );
}