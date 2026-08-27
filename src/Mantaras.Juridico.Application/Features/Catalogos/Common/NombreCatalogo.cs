namespace Mantaras.Juridico.Application.Features.Catalogos.Common;

public static class NombreCatalogo
{
    public static string Normalizar(string? nombre)
    {
        var palabras = (nombre ?? string.Empty).Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries
        );

        return string.Join(" ", palabras).ToUpperInvariant();
    }
}
