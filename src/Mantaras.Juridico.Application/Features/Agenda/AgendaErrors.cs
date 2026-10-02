using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda;

public static class AgendaErrors
{
    public static readonly Error NoEncontrada = new(
        "Agenda.NoEncontrada",
        "La entrada de agenda solicitada no existe."
    );

    public static readonly Error TipoNoEncontradoOInactivo = new(
        "Agenda.TipoNoEncontradoOInactivo",
        "El tipo de entrada informado no existe o se encuentra inactivo."
    );

    public static readonly Error ClienteNoEncontradoOInactivo = new(
        "Agenda.ClienteNoEncontradoOInactivo",
        "Uno o más clientes informados no existen o se encuentran inactivos."
    );

    public static readonly Error CasoNoEncontradoOInactivo = new(
        "Agenda.CasoNoEncontradoOInactivo",
        "Uno o más expedientes administrativos informados no existen o se encuentran inactivos."
    );

    public static readonly Error ExpedienteNoEncontradoOInactivo = new(
        "Agenda.ExpedienteNoEncontradoOInactivo",
        "Uno o más expedientes judiciales informados no existen o se encuentran inactivos."
    );

    public static readonly Error ResponsableNoEncontradoOInactivo = new(
        "Agenda.ResponsableNoEncontradoOInactivo",
        "Uno o más responsables informados no existen o se encuentran inactivos."
    );

    public static readonly Error FechaVencimientoObligatoria = new(
        "Agenda.FechaVencimientoObligatoria",
        "La fecha de vencimiento es obligatoria para registrar un vencimiento manual."
    );

    public static readonly Error ReglaNoEncontrada = new(
        "Agenda.ReglaNoEncontrada",
        "La regla de vencimiento solicitada no existe."
    );

    public static readonly Error NombreReglaDuplicado = new(
        "Agenda.NombreReglaDuplicado",
        "Ya existe una regla de vencimiento con ese nombre."
    );

    public static readonly Error ReglaInactiva = new(
        "Agenda.ReglaInactiva",
        "La regla de vencimiento seleccionada se encuentra inactiva."
    );

    public static readonly Error OrigenFechaBaseNoEncontrado = new(
        "Agenda.OrigenFechaBaseNoEncontrado",
        "El origen seleccionado para la fecha base no existe o se encuentra inactivo."
    );

    public static readonly Error RecordatorioNoEncontrado = new(
        "Agenda.RecordatorioNoEncontrado",
        "El recordatorio solicitado no existe."
    );

    public static readonly Error RecordatorioAtendido = new(
        "Agenda.RecordatorioAtendido",
        "Los recordatorios atendidos se conservan como historial y no se pueden quitar ni reprogramar."
    );
    public static readonly Error RecordatorioDatosInvalidos = new(
        "Agenda.RecordatorioDatosInvalidos",
        "La base debe ser Inicio o Vencimiento y la anticipación debe estar entre 0 y 525600 minutos."
    );
    public static readonly Error RecordatorioFechaFueraDeRango = new(
        "Agenda.RecordatorioFechaFueraDeRango",
        "La fecha programada del recordatorio queda fuera del rango permitido."
    );

    public static readonly Error RecordatorioDuplicado = new(
        "Agenda.RecordatorioDuplicado",
        "Ya existe un recordatorio activo para esa entrada y fecha."
    );

    public static readonly Error FechaBaseRecordatorioNoDisponible = new(
        "Agenda.FechaBaseRecordatorioNoDisponible",
        "La entrada no posee la fecha requerida para calcular el recordatorio."
    );

    public static Error TransicionEstadoInvalida(
        EstadoEntradaAgenda estadoActual,
        EstadoEntradaAgenda estadoSolicitado
    ) => new(
        "Agenda.TransicionEstadoInvalida",
        $"No se puede cambiar una entrada de {estadoActual} a {estadoSolicitado}."
    );
}
