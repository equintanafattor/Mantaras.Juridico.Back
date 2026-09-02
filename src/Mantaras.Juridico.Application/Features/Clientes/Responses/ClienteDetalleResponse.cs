namespace Mantaras.Juridico.Application.Features.Clientes.Responses;

public sealed class ClienteDetalleResponse
{
    public long ClienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string? Dni { get; set; }

    public string? Cuil { get; set; }

    public string? DerivadoPor { get; set; }

    public string? DerivadoPorTelefono { get; set; }

    public string? DerivadoPorEmail { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Domicilio { get; set; }

    public string? Localidad { get; set; }

    public string? Provincia { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public IReadOnlyCollection<CasoClienteDetalleResponse> Casos { get; set; } =
        Array.Empty<CasoClienteDetalleResponse>();
}
