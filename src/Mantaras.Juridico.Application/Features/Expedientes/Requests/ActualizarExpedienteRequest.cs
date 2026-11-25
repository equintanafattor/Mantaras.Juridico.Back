using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Requests;

public sealed class ActualizarExpedienteRequest
{
    public IReadOnlyCollection<long> CasoIds { get; set; } = Array.Empty<long>();

    public long? ExpedientePadreId { get; set; }

    public TipoExpediente TipoExpediente { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    private long? _juzgadoId;

    // Omitido: conserva el juzgado actual. null: desvincula. Valor: vincula ese juzgado.
    public long? JuzgadoId
    {
        get => _juzgadoId;
        set
        {
            _juzgadoId = value;
            JuzgadoIdInformado = true;
        }
    }

    internal bool JuzgadoIdInformado { get; private set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }

    public string? TipoTramite { get; set; }

    public RegistrarEnvioCobroRequest? EnvioCobro { get; set; }
}
