using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Responses;

public sealed class CasoClienteResponse
{
    public long ClienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string? Dni { get; set; }

    public string? Cuil { get; set; }

    public TipoParticipacionCliente TipoParticipacion { get; set; }

    public bool EsPrincipal { get; set; }
}
