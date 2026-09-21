using Mantaras.Juridico.Domain.Common;

namespace Mantaras.Juridico.Domain.Entities;

public class Caso : AuditableEntity
{
    public long CasoId { get; set; }

    public string Titulo { get; set; } = null!;

    public string FaseInterna { get; set; } = string.Empty;

    public string? TipoTramite { get; set; }

    public string? NumeroExpedienteAnses { get; set; }

    public string? NumeroBeneficio { get; set; }

    public long? TipoBeneficioId { get; set; }

    public long? TipoExpedienteAdministrativoId { get; set; }

    public TipoBeneficio? TipoBeneficio { get; set; }

    public TipoExpedienteAdministrativo? TipoExpedienteAdministrativo { get; set; }

    public HojaResumenCaso? HojaResumen { get; set; }

    public ICollection<Observacion> HistorialObservaciones { get; set; } =
        new List<Observacion>();

    public ICollection<CasoCliente> Clientes { get; set; } = new List<CasoCliente>();

    public ICollection<CasoExpediente> Expedientes { get; set; } =
        new List<CasoExpediente>();
}
