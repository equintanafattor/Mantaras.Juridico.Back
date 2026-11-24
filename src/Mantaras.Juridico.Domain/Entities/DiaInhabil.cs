using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class DiaInhabil : AuditableEntity
{
    public long DiaInhabilId { get; set; }

    public DateOnly Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public OrigenDiaInhabil Origen { get; set; } = OrigenDiaInhabil.Manual;

    // Solo importados: código de la fuente y fecha tal como la publicó.
    // FechaFuente permite reconocer el feriado aunque el usuario haya movido Fecha.
    public string? Fuente { get; set; }

    public DateOnly? FechaFuente { get; set; }

    public DateTime? FechaSincronizacion { get; set; }

    // Un importado editado, desactivado o reactivado a mano deja de actualizarse
    // desde la fuente.
    public bool ModificadoManualmente { get; set; }
}
