using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

// Arma las respuestas de entradas con el mismo criterio en todos los servicios.
// Requiere las relaciones cargadas con sus entidades (cliente, caso, expediente).
internal static class EntradaAgendaMapper
{
    public const string UsuarioNoDisponible = "Usuario no disponible";

    // Relee la entrada sin seguimiento para devolver relaciones con nombre.
    public static async Task<EntradaAgendaResponse?> ObtenerDetalleAsync(
        IAgendaRepository repository,
        long entradaAgendaId,
        CancellationToken cancellationToken
    )
    {
        var entrada = await repository.ObtenerPorIdAsync(
            entradaAgendaId,
            seguimiento: false,
            cancellationToken: cancellationToken
        );

        if (entrada is null)
        {
            return null;
        }

        var nombres = await repository.ObtenerNombresUsuariosAsync(
            entrada.Responsables.Select(x => x.UsuarioId).Distinct().ToArray(),
            cancellationToken
        );

        return MapearDetalle(entrada, nombres);
    }

    public static EntradaAgendaResponse MapearDetalle(
        EntradaAgenda entrada,
        IReadOnlyDictionary<long, string> nombresResponsables
    )
    {
        var condicion = CriteriosAgenda.Calcular(entrada);

        return new EntradaAgendaResponse
        {
            EntradaAgendaId = entrada.EntradaAgendaId,
            RecurrenciaAgendaId = entrada.RecurrenciaAgendaId,
            TipoEntradaAgendaId = entrada.TipoEntradaAgendaId,
            TipoEntradaNombre = entrada.TipoEntrada.Nombre,
            TipoEntradaColor = entrada.TipoEntrada.Color,
            Titulo = entrada.Titulo,
            Descripcion = entrada.Descripcion,
            Estado = entrada.Estado,
            Prioridad = entrada.Prioridad,
            FechaInicio = entrada.FechaInicio,
            HoraInicio = entrada.HoraInicio,
            FechaFin = entrada.FechaFin,
            HoraFin = entrada.HoraFin,
            FechaVencimiento = entrada.FechaVencimiento,
            HoraVencimiento = entrada.HoraVencimiento,
            DiasParaVencimiento = condicion.DiasParaVencimiento,
            EstaVencida = condicion.EstaVencida,
            ProximaAVencer = condicion.ProximaAVencer,
            EstaAtrasada = condicion.EstaAtrasada,
            TransicionesPermitidas = CriteriosAgenda.TransicionesPermitidas(entrada.Estado),
            ZonaHoraria = entrada.ZonaHoraria,
            ClienteIds = entrada.Clientes.Select(x => x.ClienteId).ToArray(),
            CasoIds = entrada.Casos.Select(x => x.CasoId).ToArray(),
            ExpedienteIds = entrada.Expedientes.Select(x => x.ExpedienteId).ToArray(),
            ResponsableIds = entrada.Responsables.Select(x => x.UsuarioId).ToArray(),
            Clientes = MapearClientes(entrada),
            Casos = MapearCasos(entrada),
            Expedientes = MapearExpedientes(entrada),
            Responsables = MapearResponsables(entrada, nombresResponsables),
            FechaCreacion = entrada.FechaCreacion,
            FechaModificacion = entrada.FechaModificacion,
            Activo = entrada.Activo,
        };
    }

    public static EntradaAgendaListadoResponse MapearListado(
        EntradaAgenda entrada,
        IReadOnlyDictionary<long, string> nombresResponsables
    )
    {
        var condicion = CriteriosAgenda.Calcular(entrada);

        return new EntradaAgendaListadoResponse
        {
            EntradaAgendaId = entrada.EntradaAgendaId,
            RecurrenciaAgendaId = entrada.RecurrenciaAgendaId,
            TipoEntradaAgendaId = entrada.TipoEntradaAgendaId,
            TipoEntradaNombre = entrada.TipoEntrada.Nombre,
            TipoEntradaColor = entrada.TipoEntrada.Color,
            Titulo = entrada.Titulo,
            Descripcion = entrada.Descripcion,
            Estado = entrada.Estado,
            Prioridad = entrada.Prioridad,
            FechaInicio = entrada.FechaInicio,
            HoraInicio = entrada.HoraInicio,
            FechaFin = entrada.FechaFin,
            HoraFin = entrada.HoraFin,
            FechaVencimiento = entrada.FechaVencimiento,
            HoraVencimiento = entrada.HoraVencimiento,
            DiasParaVencimiento = condicion.DiasParaVencimiento,
            EstaVencida = condicion.EstaVencida,
            ProximaAVencer = condicion.ProximaAVencer,
            EstaAtrasada = condicion.EstaAtrasada,
            TransicionesPermitidas = CriteriosAgenda.TransicionesPermitidas(entrada.Estado),
            Clientes = MapearClientes(entrada),
            Casos = MapearCasos(entrada),
            Expedientes = MapearExpedientes(entrada),
            Responsables = MapearResponsables(entrada, nombresResponsables),
            Activo = entrada.Activo,
        };
    }

    private static RelacionAgendaResponse[] MapearClientes(EntradaAgenda entrada) =>
        entrada.Clientes
            .OrderBy(x => x.Cliente.Apellido)
            .ThenBy(x => x.Cliente.Nombre)
            .Select(x => new RelacionAgendaResponse
            {
                Id = x.ClienteId,
                Nombre = $"{x.Cliente.Apellido}, {x.Cliente.Nombre}",
                Referencia = x.Cliente.Dni ?? x.Cliente.Cuil,
            })
            .ToArray();

    private static RelacionAgendaResponse[] MapearCasos(EntradaAgenda entrada) =>
        entrada.Casos
            .OrderBy(x => x.Caso.Titulo)
            .Select(x => new RelacionAgendaResponse
            {
                Id = x.CasoId,
                Nombre = x.Caso.Titulo,
                Referencia = x.Caso.NumeroExpedienteAnses ?? x.Caso.NumeroBeneficio,
            })
            .ToArray();

    private static RelacionAgendaResponse[] MapearExpedientes(EntradaAgenda entrada) =>
        entrada.Expedientes
            .OrderBy(x => x.Expediente.Caratula)
            .Select(x => new RelacionAgendaResponse
            {
                Id = x.ExpedienteId,
                Nombre = x.Expediente.Caratula,
                Referencia = x.Expediente.NumeroExpediente,
            })
            .ToArray();

    private static RelacionAgendaResponse[] MapearResponsables(
        EntradaAgenda entrada,
        IReadOnlyDictionary<long, string> nombres
    ) =>
        entrada.Responsables
            .OrderBy(x => nombres.GetValueOrDefault(x.UsuarioId))
            .Select(x => new RelacionAgendaResponse
            {
                Id = x.UsuarioId,
                Nombre = nombres.GetValueOrDefault(x.UsuarioId, UsuarioNoDisponible),
            })
            .ToArray();
}
