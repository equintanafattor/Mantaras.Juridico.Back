using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public class Caso : AuditableEntity
{
    public long CasoId { get; set; }

    public string Titulo { get; set; } = null!;

    public FaseCaso FaseInterna { get; set; }

    public string? TipoTramite { get; set; }

    public ICollection<Observacion> HistorialObservaciones { get; set; } =
        new List<Observacion>();

    public ICollection<CasoCliente> Clientes { get; set; } = new List<CasoCliente>();

    public ICollection<Expediente> Expedientes { get; set; } = new List<Expediente>();
}
