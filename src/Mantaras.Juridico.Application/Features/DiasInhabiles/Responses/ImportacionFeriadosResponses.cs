using Mantaras.Juridico.Application.Features.DiasInhabiles.Importacion;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;

public sealed class FuenteFeriadosResponse
{
    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}

public sealed class VistaPreviaImportacionFeriadosResponse
{
    public int Anio { get; set; }

    public FuenteFeriadosResponse Fuente { get; set; } = null!;

    public DateTime? FechaPublicacionFuente { get; set; }

    public DateTime ConsultadoEn { get; set; }

    public string Huella { get; set; } = string.Empty;

    public bool CalendarioCargado { get; set; }

    public DateTime? UltimaSincronizacion { get; set; }

    public ResumenImportacionFeriadosResponse Resumen { get; set; } = null!;

    // Solo feriados nacionales: es lo que se aplica al confirmar.
    public IReadOnlyCollection<ItemImportacionFeriadoResponse> Items { get; set; } =
        Array.Empty<ItemImportacionFeriadoResponse>();

    // No laborables, turísticos y tipos desconocidos. Solo se importan si se
    // incluyen explícitamente en la confirmación.
    public IReadOnlyCollection<ItemImportacionFeriadoResponse> RequierenRevision { get; set; } =
        Array.Empty<ItemImportacionFeriadoResponse>();

    public IReadOnlyCollection<OmitidoImportacionFeriadoResponse> Omitidos { get; set; } =
        Array.Empty<OmitidoImportacionFeriadoResponse>();

    public IReadOnlyCollection<DiaInhabilResponse> AusentesEnFuente { get; set; } =
        Array.Empty<DiaInhabilResponse>();
}

public sealed class ResumenImportacionFeriadosResponse
{
    // Cantidades sobre los feriados nacionales (Items).
    public int Nuevos { get; set; }

    public int Existentes { get; set; }

    public int Actualizables { get; set; }

    public int Conflictos { get; set; }

    public int RequierenRevision { get; set; }

    public int Omitidos { get; set; }

    public int AusentesEnFuente { get; set; }
}

public sealed class ItemImportacionFeriadoResponse
{
    public DateOnly Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public CategoriaImportacionFeriado Categoria { get; set; }

    public EstadoImportacionFeriado Estado { get; set; }

    public IReadOnlyCollection<RegistroFuenteFeriadoResponse> RegistrosFuente { get; set; } =
        Array.Empty<RegistroFuenteFeriadoResponse>();

    public long? DiaInhabilIdExistente { get; set; }

    public DateOnly? FechaExistente { get; set; }

    public OrigenDiaInhabil? OrigenExistente { get; set; }

    public string? DescripcionExistente { get; set; }

    public bool? ActivoExistente { get; set; }

    public bool? ModificadoManualmenteExistente { get; set; }

    public string? Motivo { get; set; }
}

public sealed class RegistroFuenteFeriadoResponse
{
    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;
}

public sealed class OmitidoImportacionFeriadoResponse
{
    public DateOnly Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string Motivo { get; set; } = string.Empty;
}

public sealed class ConfirmacionImportacionFeriadosResponse
{
    public int Anio { get; set; }

    public FuenteFeriadosResponse Fuente { get; set; } = null!;

    public int Creados { get; set; }

    public int Actualizados { get; set; }

    public int Existentes { get; set; }

    public int Conflictos { get; set; }

    public int RevisionIncluidos { get; set; }

    public DateTime SincronizadoEn { get; set; }

    // true si es un reintento de una confirmación ya aplicada: no hubo cambios
    // nuevos y las cantidades son las de la confirmación original.
    public bool YaAplicada { get; set; }
}

public sealed class EstadoCalendarioFeriadosResponse
{
    public int Anio { get; set; }

    // true solo si el año se importó desde la fuente oficial. Las fechas
    // manuales no alcanzan para considerarlo cargado.
    public bool CalendarioCargado { get; set; }

    public FuenteFeriadosResponse? Fuente { get; set; }

    public DateTime? FechaPublicacionFuente { get; set; }

    public DateTime? PrimeraSincronizacion { get; set; }

    public DateTime? UltimaSincronizacion { get; set; }

    public string? UsuarioSincronizacion { get; set; }

    public int DiasInhabilesActivos { get; set; }

    public int FeriadosImportadosActivos { get; set; }

    public int DiasManualesActivos { get; set; }
}

public sealed class AdvertenciaCalendarioResponse
{
    public string Codigo { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public int Anio { get; set; }

    public static IReadOnlyCollection<AdvertenciaCalendarioResponse> DesdeAnios(
        IEnumerable<int> anios
    ) => anios
        .Order()
        .Select(anio => new AdvertenciaCalendarioResponse
        {
            Codigo = DiaInhabilErrors.CodigoAnioSinCalendario,
            Mensaje = $"El cálculo usa días hábiles de {anio}, pero ese año no tiene calendario de feriados importado. Revisá la fecha calculada.",
            Anio = anio,
        })
        .ToArray();
}
