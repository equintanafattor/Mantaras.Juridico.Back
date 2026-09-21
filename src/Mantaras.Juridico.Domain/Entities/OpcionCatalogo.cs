using Mantaras.Juridico.Domain.Common;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class OpcionCatalogo : AuditableEntity
{
    public long OpcionCatalogoId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;
}
