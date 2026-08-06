using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class CasoClienteRequest
{
    public long ClienteId { get; set; }

    public TipoParticipacionCliente TipoParticipacion { get; set; }

    public bool EsPrincipal { get; set; }
}
