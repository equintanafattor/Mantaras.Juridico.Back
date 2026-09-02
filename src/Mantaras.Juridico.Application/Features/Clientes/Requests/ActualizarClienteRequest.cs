namespace Mantaras.Juridico.Application.Features.Clientes.Requests;

public sealed class ActualizarClienteRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string? Dni { get; set; }

    public string? Cuil { get; set; }

    public string? ClaveSeguridadSocial { get; set; }

    public string? DerivadoPor { get; set; }

    public string? DerivadoPorTelefono { get; set; }

    public string? DerivadoPorEmail { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Domicilio { get; set; }

    public string? Localidad { get; set; }

    public string? Provincia { get; set; }
}
