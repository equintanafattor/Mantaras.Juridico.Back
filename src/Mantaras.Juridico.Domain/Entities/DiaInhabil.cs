using Mantaras.Juridico.Domain.Common;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class DiaInhabil : AuditableEntity
{
    public long DiaInhabilId { get; set; }

    public DateOnly Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;
}
