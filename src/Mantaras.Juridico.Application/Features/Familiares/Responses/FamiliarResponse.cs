using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Familiares.Responses;

public sealed class FamiliarResponse
{
    public long RelacionFamiliarId { get; set; }

    public long FamiliarId { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string? Dni { get; set; }

    public string? Cuil { get; set; }

    public TipoParentesco Parentesco { get; set; }

    // Estado del cliente familiar, no de la relación.
    public bool Activo { get; set; }

    // Auditoría de la relación.
    public DateTime FechaCreacion { get; set; }

    public string? UsuarioCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }
}