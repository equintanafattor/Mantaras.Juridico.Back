using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Clientes.Responses;

public sealed class CasoClienteDetalleResponse
{
    public long CasoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public FaseCaso FaseInterna { get; set; }

    public string? TipoTramite { get; set; }

    public TipoParticipacionCliente TipoParticipacion { get; set; }

    public bool EsPrincipal { get; set; }

    public bool Activo { get; set; }

    public IReadOnlyCollection<ExpedienteClienteDetalleResponse> Expedientes { get; set; } =
        Array.Empty<ExpedienteClienteDetalleResponse>();
}
