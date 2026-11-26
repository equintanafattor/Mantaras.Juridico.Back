namespace Mantaras.Juridico.Application.Features.Expedientes.Responses;

// Cliente vinculado a un judicial de cobro a través de sus casos (administrativos).
public sealed class ClienteExpedienteCobroResponse
{
    public long ClienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    // "Apellido, Nombre", igual que CasoClienteResponse.
    public string NombreCompleto { get; set; } = string.Empty;

    public string? Dni { get; set; }

    public string? Cuil { get; set; }

    public bool Activo { get; set; }

    // Es principal (CasoCliente.EsPrincipal) en al menos uno de los casos del judicial.
    public bool EsPrincipal { get; set; }
}
