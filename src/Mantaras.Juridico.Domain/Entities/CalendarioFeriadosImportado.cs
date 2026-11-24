namespace Mantaras.Juridico.Domain.Entities;

// Un año tiene calendario cargado solo si existe su fila: las fechas manuales
// sueltas no cuentan como cobertura.
public sealed class CalendarioFeriadosImportado
{
    public int Anio { get; set; }

    public string Fuente { get; set; } = string.Empty;

    public string UrlFuente { get; set; } = string.Empty;

    // dateModified publicado por la fuente, si lo informa.
    public DateTime? FechaPublicacionFuente { get; set; }

    public DateTime FechaPrimeraSincronizacion { get; set; }

    public DateTime FechaSincronizacion { get; set; }

    public string? UsuarioSincronizacion { get; set; }

    // Huella de la vista previa confirmada más la selección de revisión:
    // un reintento con la misma clave devuelve el resultado ya aplicado.
    public string UltimaClaveConfirmacion { get; set; } = string.Empty;

    public int Creados { get; set; }

    public int Actualizados { get; set; }

    public int Existentes { get; set; }

    public int Conflictos { get; set; }

    public int RevisionIncluidos { get; set; }
}
