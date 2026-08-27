using Mantaras.Juridico.Domain.Common;

namespace Mantaras.Juridico.Domain.Entities;

public class TipoBeneficio : AuditableEntity
{
    public long TipoBeneficioId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public ICollection<Caso> Casos { get; set; } = new List<Caso>();
}
