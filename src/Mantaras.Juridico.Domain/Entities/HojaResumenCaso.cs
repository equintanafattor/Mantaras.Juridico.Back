namespace Mantaras.Juridico.Domain.Entities;

public sealed class HojaResumenCaso
{
    public long CasoId { get; set; }

    public bool? TieneCalculoPrevio { get; set; }

    public string? HaberInicialReajustadoCaracteristicas { get; set; }

    public decimal? HaberInicialPbu { get; set; }

    public string? HaberInicialObservacion { get; set; }

    public decimal? HaberInicialMonto { get; set; }

    public int? MovilidadActualizacionMes { get; set; }

    public int? MovilidadActualizacionAnio { get; set; }

    public string? MovilidadObservaciones { get; set; }

    public decimal? MovilidadMonto { get; set; }

    public DateOnly? RetroactivoFechaInicio { get; set; }

    public DateOnly? RetroactivoFechaActualizacion { get; set; }

    public string? RetroactivoObservacion { get; set; }

    public decimal? RetroactivoMonto { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string? UsuarioCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }

    public Caso Caso { get; set; } = null!;
}