using Microsoft.AspNetCore.Identity;

namespace Mantaras.Juridico.Infrastructure.Identity;

public sealed class UsuarioIdentity : IdentityUser<long>
{
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
