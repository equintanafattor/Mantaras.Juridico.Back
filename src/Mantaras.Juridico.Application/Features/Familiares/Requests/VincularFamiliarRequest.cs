using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Familiares.Requests;

public sealed class VincularFamiliarRequest
{
    public long FamiliarId { get; set; }

    // Parentesco del familiar respecto del cliente de la URL.
    public TipoParentesco Parentesco { get; set; }
}