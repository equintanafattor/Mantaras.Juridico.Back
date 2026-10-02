using Mantaras.Juridico.Application.Common.Results;
namespace Mantaras.Juridico.Application.Features.DiasInhabiles;
public static class DiaInhabilErrors
{
    public static readonly Error NoEncontrado = new("DiasInhabiles.NoEncontrado", "El día inhábil solicitado no existe.");
    public static readonly Error FechaDuplicada = new("DiasInhabiles.FechaDuplicada", "Ya existe un día inhábil para esa fecha. Si está inactivo, reactivá el registro existente.");
    public static Error DatosInvalidos(string mensaje) => new("DiasInhabiles.DatosInvalidos", mensaje);
}
public sealed class FechaDiaInhabilDuplicadaException : Exception
{
    public FechaDiaInhabilDuplicadaException(Exception innerException)
        : base("Ya existe un día inhábil para esa fecha.", innerException) { }
}
