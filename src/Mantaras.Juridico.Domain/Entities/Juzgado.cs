using Mantaras.Juridico.Domain.Common;

namespace Mantaras.Juridico.Domain.Entities;

public class Juzgado : AuditableEntity
{
    public const int LargoMaximoNombre = 500;

    public long JuzgadoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public ICollection<Expediente> Expedientes { get; set; } = new List<Expediente>();
}
