using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Validators;

public sealed class AplicarReglaVencimientoRequestValidator
    : AbstractValidator<AplicarReglaVencimientoRequest>
{
    public AplicarReglaVencimientoRequestValidator()
    {
        RuleFor(x => x.FechaBase)
            .NotEmpty()
            .WithMessage("La fecha base es obligatoria.");

        RuleFor(x => x.OrigenFechaBase)
            .IsInEnum()
            .WithMessage("El origen de la fecha base no es válido.");

        RuleFor(x => x.ClaveIdempotencia)
            .NotEmpty()
            .WithMessage("La clave de idempotencia es obligatoria.")
            .MaximumLength(150)
            .WithMessage("La clave de idempotencia no puede superar los 150 caracteres.");

        RuleForEach(x => x.ClienteIds)
            .GreaterThan(0)
            .WithMessage("Los identificadores de clientes deben ser mayores que cero.");

        RuleForEach(x => x.CasoIds)
            .GreaterThan(0)
            .WithMessage("Los identificadores de expedientes administrativos deben ser mayores que cero.");

        RuleForEach(x => x.ExpedienteIds)
            .GreaterThan(0)
            .WithMessage("Los identificadores de expedientes judiciales deben ser mayores que cero.");

        RuleForEach(x => x.ResponsableIds)
            .GreaterThan(0)
            .WithMessage("Los identificadores de responsables deben ser mayores que cero.");

        RuleFor(x => x)
            .Must(TenerOrigenConsistente)
            .WithMessage(
                "Debe informarse únicamente el identificador correspondiente al origen seleccionado."
            );
    }

    private static bool TenerOrigenConsistente(
        AplicarReglaVencimientoRequest request
    )
    {
        var cantidadIds = new long?[]
        {
            request.EntradaAgendaOrigenId,
            request.CasoOrigenId,
            request.ExpedienteOrigenId,
            request.ObservacionOrigenId,
        }.Count(x => x.HasValue);

        return request.OrigenFechaBase switch
        {
            OrigenFechaBaseAgenda.Manual => cantidadIds == 0,
            OrigenFechaBaseAgenda.EntradaAgenda =>
                cantidadIds == 1 && request.EntradaAgendaOrigenId > 0,
            OrigenFechaBaseAgenda.ExpedienteAdministrativo =>
                cantidadIds == 1 && request.CasoOrigenId > 0,
            OrigenFechaBaseAgenda.ExpedienteJudicial =>
                cantidadIds == 1 && request.ExpedienteOrigenId > 0,
            OrigenFechaBaseAgenda.Movimiento =>
                cantidadIds == 1 && request.ObservacionOrigenId > 0,
            _ => false,
        };
    }
}
