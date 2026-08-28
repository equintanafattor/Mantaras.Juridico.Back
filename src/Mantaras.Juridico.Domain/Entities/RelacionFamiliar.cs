using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class RelacionFamiliar : AuditableEntity
{
    public long RelacionFamiliarId { get; set; }

    public long ClienteAId { get; set; }

    public long ClienteBId { get; set; }

    // Parentesco de ClienteB respecto de ClienteA.
    public TipoParentesco ParentescoDeB { get; set; }

    public Cliente ClienteA { get; set; } = null!;

    public Cliente ClienteB { get; set; } = null!;
}