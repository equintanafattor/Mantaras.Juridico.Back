using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Requests;

namespace Mantaras.Juridico.Application.Features.Agenda.Validators;

public sealed class CrearRecordatorioAgendaRequestValidator
    : AbstractValidator<CrearRecordatorioAgendaRequest>
{
    public CrearRecordatorioAgendaRequestValidator()
    {
        RuleFor(x => x.BaseCalculo)
            .IsInEnum()
            .WithMessage("La base de cálculo del recordatorio no es válida.");

        RuleFor(x => x.MinutosAnticipacion)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La anticipación no puede ser negativa.")
            .LessThanOrEqualTo(525600)
            .WithMessage("La anticipación no puede superar un año.");
    }
}
