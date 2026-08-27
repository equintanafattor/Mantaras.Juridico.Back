using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class ActualizarCasoRequest
{
    public string Titulo { get; set; } = string.Empty;

    public FaseCaso FaseInterna { get; set; }

    public string? TipoTramite { get; set; }

    // Compatibilidad con clientes anteriores: omitir conserva; null explícito limpia.
    private string? _numeroExpedienteAnses;

    public string? NumeroExpedienteAnses
    {
        get => _numeroExpedienteAnses;
        set
        {
            _numeroExpedienteAnses = value;
            NumeroExpedienteAnsesInformado = true;
        }
    }

    internal bool NumeroExpedienteAnsesInformado { get; private set; }

    private long? _tipoBeneficioId;

    public long? TipoBeneficioId
    {
        get => _tipoBeneficioId;
        set
        {
            _tipoBeneficioId = value;
            TipoBeneficioIdInformado = true;
        }
    }

    internal bool TipoBeneficioIdInformado { get; private set; }

    private long? _tipoExpedienteAdministrativoId;

    public long? TipoExpedienteAdministrativoId
    {
        get => _tipoExpedienteAdministrativoId;
        set
        {
            _tipoExpedienteAdministrativoId = value;
            TipoExpedienteAdministrativoIdInformado = true;
        }
    }

    internal bool TipoExpedienteAdministrativoIdInformado { get; private set; }

    public IReadOnlyCollection<CasoClienteRequest> Clientes { get; set; } =
        Array.Empty<CasoClienteRequest>();
}
