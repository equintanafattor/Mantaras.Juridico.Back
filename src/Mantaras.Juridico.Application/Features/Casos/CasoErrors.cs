using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.Casos;

public static class CasoErrors
{
    public static readonly Error NoEncontrado = new(
        "Casos.NoEncontrado",
        "El caso solicitado no existe."
    );

    public static readonly Error ClientesNoEncontrados = new(
        "Casos.ClientesNoEncontrados",
        "Uno o más clientes informados no existen o están inactivos."
    );

    public static readonly Error ExpedientesActivos = new(
        "Casos.ExpedientesActivos",
        "No se puede dar de baja el caso porque tiene expedientes activos."
    );
}