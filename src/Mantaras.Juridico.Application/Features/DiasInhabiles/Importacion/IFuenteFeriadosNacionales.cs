using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Importacion;

public interface IFuenteFeriadosNacionales
{
    string Codigo { get; }

    string Nombre { get; }

    string ConstruirUrl(int anio);

    // Falla con AnioNoDisponible, FuenteNoDisponible o FuenteInvalida. Nunca
    // devuelve una publicación parcial: si un registro no se puede leer, falla toda.
    Task<Result<PublicacionFeriadosNacionales>> ObtenerAsync(
        int anio,
        CancellationToken cancellationToken = default
    );
}

public sealed record PublicacionFeriadosNacionales(
    int Anio,
    string Url,
    DateTime? FechaPublicacion,
    IReadOnlyList<RegistroFuenteFeriado> Registros
);

// Tipo tal como lo publica la fuente: inamovible, trasladable, no_laborable,
// turistico, especial...
public sealed record RegistroFuenteFeriado(DateOnly Fecha, string Nombre, string Tipo);
