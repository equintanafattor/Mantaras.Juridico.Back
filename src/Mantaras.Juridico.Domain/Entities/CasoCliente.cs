using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public class CasoCliente
{
    public long CasoId { get; set; }

    public long ClienteId { get; set; }

    public TipoParticipacionCliente TipoParticipacion { get; set; }

    public bool EsPrincipal { get; set; }

    public Caso Caso { get; set; } = null!;

    public Cliente Cliente { get; set; } = null!;
}
