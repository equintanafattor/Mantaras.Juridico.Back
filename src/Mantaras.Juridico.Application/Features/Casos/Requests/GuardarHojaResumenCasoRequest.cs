namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed record GuardarHojaResumenCasoRequest
{
    public bool? TieneCalculoPrevio { get; init; }

    public string? HaberInicialReajustadoCaracteristicas { get; init; }

    public decimal? HaberInicialPbu { get; init; }

    public string? HaberInicialObservacion { get; init; }

    public decimal? HaberInicialMonto { get; init; }

    public int? MovilidadActualizacionMes { get; init; }

    public int? MovilidadActualizacionAnio { get; init; }

    public string? MovilidadObservaciones { get; init; }

    public decimal? MovilidadMonto { get; init; }

    public DateOnly? RetroactivoFechaInicio { get; init; }

    public DateOnly? RetroactivoFechaActualizacion { get; init; }

    public string? RetroactivoObservacion { get; init; }

    public decimal? RetroactivoMonto { get; init; }
}