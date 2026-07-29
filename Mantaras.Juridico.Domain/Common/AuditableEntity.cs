namespace Mantaras.Juridico.Domain.Common;

public abstract class AuditableEntity
{
    public DateTime FechaCreacion { get; set; }

    public string? UsuarioCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }

    public bool Activo { get; set; } = true;
}