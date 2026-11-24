using Mantaras.Juridico.Application.Common.Results;
namespace Mantaras.Juridico.Application.Features.DiasInhabiles;
public static class DiaInhabilErrors
{
    public static readonly Error NoEncontrado = new("DiasInhabiles.NoEncontrado", "El día inhábil solicitado no existe.");
    public static readonly Error FechaDuplicada = new("DiasInhabiles.FechaDuplicada", "Ya existe un día inhábil para esa fecha. Si está inactivo, reactivá el registro existente.");
    public static Error DatosInvalidos(string mensaje) => new("DiasInhabiles.DatosInvalidos", mensaje);
    public static readonly Error AnioNoDisponible = new("DiasInhabiles.AnioNoDisponible", "La fuente oficial no publica el calendario de feriados de ese año.");
    public static readonly Error FuenteNoDisponible = new("DiasInhabiles.FuenteNoDisponible", "No se pudo consultar la fuente oficial de feriados. El calendario actual no se modificó; intentá más tarde.");
    public static readonly Error FuenteInvalida = new("DiasInhabiles.FuenteInvalida", "La fuente oficial devolvió datos con un formato inesperado. El calendario actual no se modificó.");
    public static readonly Error VistaPreviaDesactualizada = new("DiasInhabiles.VistaPreviaDesactualizada", "La fuente o el calendario cambiaron desde la vista previa. Volvé a consultarla antes de confirmar.");
    public const string CodigoAnioSinCalendario = "DiasInhabiles.AnioSinCalendario";
}
public sealed class FechaDiaInhabilDuplicadaException : Exception
{
    public FechaDiaInhabilDuplicadaException(Exception innerException)
        : base("Ya existe un día inhábil para esa fecha.", innerException) { }
}
