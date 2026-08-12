using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.Expedientes;

public static class ExpedienteErrors
{
    public static readonly Error NoEncontrado = new(
        "Expedientes.NoEncontrado",
        "El expediente solicitado no existe."
    );

    public static readonly Error CasoNoEncontradoOInactivo = new(
        "Expedientes.CasoNoEncontradoOInactivo",
        "El caso informado no existe o se encuentra inactivo."
    );

    public static readonly Error PadreNoEncontradoOInactivo = new(
        "Expedientes.PadreNoEncontradoOInactivo",
        "El expediente padre no existe o se encuentra inactivo."
    );

    public static readonly Error PadreDeOtroCaso = new(
        "Expedientes.PadreDeOtroCaso",
        "El expediente padre debe pertenecer al mismo caso."
    );

    public static readonly Error PadreEsMismoExpediente = new(
        "Expedientes.PadreEsMismoExpediente",
        "Un expediente no puede ser padre de sí mismo."
    );

    public static readonly Error JerarquiaCiclica = new(
        "Expedientes.JerarquiaCiclica",
        "La relación informada produciría un ciclo en la jerarquía de expedientes."
    );

    public static readonly Error DerivadosActivos = new(
        "Expedientes.DerivadosActivos",
        "No se puede dar de baja el expediente porque tiene expedientes derivados activos."
    );

    public static readonly Error PrincipalDuplicado = new(
        "Expedientes.PrincipalDuplicado",
        "El caso ya tiene un expediente principal."
    );
}