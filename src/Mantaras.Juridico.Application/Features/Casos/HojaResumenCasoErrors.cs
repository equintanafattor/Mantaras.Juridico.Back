using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.Casos;

public static class HojaResumenCasoErrors
{
    public static readonly Error CreacionConcurrente = new(
        "Casos.HojaResumen.CreacionConcurrente",
        "Otro usuario creó la hoja mientras la estabas guardando. Recargá y revisá sus datos antes de volver a guardar."
    );
}