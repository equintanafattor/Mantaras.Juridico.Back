using Mantaras.Juridico.Application.Features.DiasInhabiles.Importacion;
using Mantaras.Juridico.Infrastructure.Services;

namespace Mantaras.Juridico.Tests.DiasInhabiles;

internal static class Fixtures
{
    public static byte[] Leer(int anio) => File.ReadAllBytes(
        Path.Combine(AppContext.BaseDirectory, "Fixtures", $"holidays-{anio}-es.json")
    );

    public static PublicacionFeriadosNacionales Publicacion(int anio) =>
        FuenteFeriadosArgentinaGobAr.Leer(anio, $"https://fixture/{anio}", Leer(anio))
        ?? throw new InvalidOperationException($"Fixture {anio} inválido.");
}
