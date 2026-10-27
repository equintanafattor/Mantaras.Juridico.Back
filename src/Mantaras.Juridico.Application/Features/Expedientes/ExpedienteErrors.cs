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
        "El expediente padre debe compartir al menos un expediente administrativo."
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

    public static readonly Error EnvioCobroYaRegistrado = new(
        "Expedientes.EnvioCobroYaRegistrado",
        "El expediente ya tiene un envío de cobro registrado con otros datos. La corrección, el reenvío y la anulación de envíos todavía no están habilitados."
    );

    public static readonly Error EnvioCobroExpedienteInactivo = new(
        "Expedientes.EnvioCobroExpedienteInactivo",
        "No se puede registrar un envío de cobro en un expediente dado de baja."
    );

    public static readonly Error SeguimientoCobroYaGenerado = new(
        "Expedientes.SeguimientoCobroYaGenerado",
        "Ya existe un seguimiento de cobro generado para ese envío."
    );
}
