using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Requests;

namespace Mantaras.Juridico.Application.Features.Agenda.Validators;

public sealed class GuardarRecordatoriosPredeterminadosRequestValidator
    : AbstractValidator<GuardarRecordatoriosPredeterminadosRequest>
{
    public GuardarRecordatoriosPredeterminadosRequestValidator()
    {
        RuleFor(x => x.Recordatorios)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("La colección de recordatorios es obligatoria.")
            .Must(x => x.Count <= 10)
            .WithMessage("No pueden configurarse más de 10 recordatorios predeterminados.")
            .Must(x =>
                x.Select(r => new { r.BaseCalculo, r.MinutosAnticipacion })
                    .Distinct()
                    .Count() == x.Count
            )
            .WithMessage("No pueden repetirse recordatorios con la misma configuración.");

        RuleForEach(x => x.Recordatorios)
            .ChildRules(recordatorio =>
            {
                recordatorio.RuleFor(x => x.BaseCalculo)
                    .IsInEnum()
                    .WithMessage("La base de cálculo no es válida.");

                recordatorio.RuleFor(x => x.MinutosAnticipacion)
                    .InclusiveBetween(0, 525600)
                    .WithMessage("La anticipación debe estar entre cero minutos y un año.");
            });
    }
}
