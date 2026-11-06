using System.Linq.Expressions;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda;

// Criterio único de Agenda, panel y seguimiento de cobro.
// - Estado: la etapa de trabajo (Pendiente, EnCurso, Pospuesta, Completada, Cancelada).
// - Condición: se calcula a partir de las fechas y solo aplica a entradas abiertas.
//   * Vencida: tiene FechaVencimiento y es anterior a hoy.
//   * Atrasada: no tiene FechaVencimiento y (FechaFin ?? FechaInicio) es anterior a hoy.
//   Completada y Cancelada nunca están vencidas ni atrasadas.
public static class CriteriosAgenda
{
    public const int DiasProximoVencimiento = 7;

    public static readonly IReadOnlyCollection<EstadoEntradaAgenda> EstadosAbiertos =
        new[]
        {
            EstadoEntradaAgenda.Pendiente,
            EstadoEntradaAgenda.EnCurso,
            EstadoEntradaAgenda.Pospuesta,
        };

    public static bool EstaAbierta(EstadoEntradaAgenda estado) =>
        estado is not (EstadoEntradaAgenda.Completada or EstadoEntradaAgenda.Cancelada);

    public static Expression<Func<EntradaAgenda, bool>> Abierta() =>
        x => x.Estado != EstadoEntradaAgenda.Completada
            && x.Estado != EstadoEntradaAgenda.Cancelada;

    public static Expression<Func<EntradaAgenda, bool>> Vencida(DateOnly hoy) =>
        x => x.Estado != EstadoEntradaAgenda.Completada
            && x.Estado != EstadoEntradaAgenda.Cancelada
            && x.FechaVencimiento.HasValue
            && x.FechaVencimiento.Value < hoy;

    public static Expression<Func<EntradaAgenda, bool>> Atrasada(DateOnly hoy) =>
        x => x.Estado != EstadoEntradaAgenda.Completada
            && x.Estado != EstadoEntradaAgenda.Cancelada
            && !x.FechaVencimiento.HasValue
            && (x.FechaFin ?? x.FechaInicio) < hoy;

    public static DateOnly Hoy(string zonaHoraria = EntradaAgenda.ZonaHorariaPredeterminada)
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(zonaHoraria);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona));
    }

    public static CondicionEntradaAgenda Calcular(EntradaAgenda entrada)
    {
        var hoy = Hoy(entrada.ZonaHoraria);
        var abierta = EstaAbierta(entrada.Estado);
        int? dias = entrada.FechaVencimiento.HasValue
            ? entrada.FechaVencimiento.Value.DayNumber - hoy.DayNumber
            : null;
        var atrasada = abierta
            && !entrada.FechaVencimiento.HasValue
            && (entrada.FechaFin ?? entrada.FechaInicio) < hoy;

        return new CondicionEntradaAgenda(
            dias,
            abierta && dias < 0,
            abierta && dias is >= 0 and <= DiasProximoVencimiento,
            atrasada
        );
    }

    public static IReadOnlyCollection<EstadoEntradaAgenda> TransicionesPermitidas(
        EstadoEntradaAgenda estadoActual
    ) => estadoActual switch
    {
        EstadoEntradaAgenda.Pendiente => new[]
        {
            EstadoEntradaAgenda.EnCurso,
            EstadoEntradaAgenda.Pospuesta,
            EstadoEntradaAgenda.Completada,
            EstadoEntradaAgenda.Cancelada,
        },
        EstadoEntradaAgenda.EnCurso => new[]
        {
            EstadoEntradaAgenda.Pendiente,
            EstadoEntradaAgenda.Pospuesta,
            EstadoEntradaAgenda.Completada,
            EstadoEntradaAgenda.Cancelada,
        },
        EstadoEntradaAgenda.Pospuesta => new[]
        {
            EstadoEntradaAgenda.Pendiente,
            EstadoEntradaAgenda.EnCurso,
            EstadoEntradaAgenda.Completada,
            EstadoEntradaAgenda.Cancelada,
        },
        EstadoEntradaAgenda.Completada or EstadoEntradaAgenda.Cancelada => new[]
        {
            EstadoEntradaAgenda.Pendiente,
        },
        _ => Array.Empty<EstadoEntradaAgenda>(),
    };
}

public sealed record CondicionEntradaAgenda(
    int? DiasParaVencimiento,
    bool EstaVencida,
    bool ProximaAVencer,
    bool EstaAtrasada
);
