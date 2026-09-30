using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

internal static class RecordatoriosAgendaFactory
{
    private static readonly TimeOnly HoraPredeterminada = new(9, 0);

    public static void AgregarPredeterminadosDeTipo(
        EntradaAgenda entrada,
        IEnumerable<RecordatorioPredeterminadoTipoAgenda> configuraciones,
        DateTime fechaCreacion,
        string usuario
    )
    {
        Agregar(
            entrada,
            configuraciones
                .Where(x => x.Activo)
                .Select(x => (x.BaseCalculo, x.MinutosAnticipacion)),
            fechaCreacion,
            usuario
        );
    }

    public static void AgregarPredeterminadosDeRegla(
        EntradaAgenda entrada,
        IEnumerable<RecordatorioPredeterminadoReglaVencimiento> configuraciones,
        DateTime fechaCreacion,
        string usuario
    )
    {
        Agregar(
            entrada,
            configuraciones
                .Where(x => x.Activo)
                .Select(x => (x.BaseCalculo, x.MinutosAnticipacion)),
            fechaCreacion,
            usuario
        );
    }

    private static void Agregar(
        EntradaAgenda entrada,
        IEnumerable<(
            BaseCalculoRecordatorioAgenda BaseCalculo,
            int MinutosAnticipacion
        )> configuraciones,
        DateTime fechaCreacion,
        string usuario
    )
    {
        var fechasExistentes = entrada.Recordatorios
            .Where(x => x.Activo)
            .Select(x => x.FechaProgramadaUtc)
            .ToHashSet();

        foreach (var configuracion in configuraciones)
        {
            var fechaBaseLocal = ObtenerFechaBaseLocal(
                entrada,
                configuracion.BaseCalculo
            );

            if (!fechaBaseLocal.HasValue)
            {
                continue;
            }

            var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
                entrada.ZonaHoraria
            );
            var fechaBaseUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(
                    fechaBaseLocal.Value,
                    DateTimeKind.Unspecified
                ),
                zonaHoraria
            );
            var fechaProgramadaUtc = fechaBaseUtc.AddMinutes(
                -configuracion.MinutosAnticipacion
            );

            if (!fechasExistentes.Add(fechaProgramadaUtc))
            {
                continue;
            }

            entrada.Recordatorios.Add(
                new RecordatorioAgenda
                {
                    Canal = CanalRecordatorioAgenda.Interno,
                    FechaProgramadaUtc = fechaProgramadaUtc,
                    Atendido = false,
                    FechaCreacion = fechaCreacion,
                    UsuarioCreacion = usuario,
                    Activo = true,
                }
            );
        }
    }

    private static DateTime? ObtenerFechaBaseLocal(
        EntradaAgenda entrada,
        BaseCalculoRecordatorioAgenda baseCalculo
    )
    {
        return baseCalculo switch
        {
            BaseCalculoRecordatorioAgenda.Inicio => entrada.FechaInicio.ToDateTime(
                entrada.HoraInicio ?? HoraPredeterminada
            ),
            BaseCalculoRecordatorioAgenda.Vencimiento when
                entrada.FechaVencimiento.HasValue =>
                    entrada.FechaVencimiento.Value.ToDateTime(
                        entrada.HoraVencimiento ?? HoraPredeterminada
                    ),
            _ => null,
        };
    }
}
