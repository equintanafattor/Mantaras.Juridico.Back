using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Requests;

namespace Mantaras.Juridico.Application.Features.Agenda.Validators;

public sealed class BuscarRecordatoriosAgendaRequestValidator
    : AbstractValidator<BuscarRecordatoriosAgendaRequest>
{
    public BuscarRecordatoriosAgendaRequestValidator()
    {
        RuleFor(x => x.EntradaAgendaId)
            .GreaterThan(0)
            .When(x => x.EntradaAgendaId.HasValue)
            .WithMessage("El identificador de la entrada debe ser mayor que cero.");

        RuleFor(x => x.Estado)
            .IsInEnum()
            .When(x => x.Estado.HasValue)
            .WithMessage("El estado del recordatorio no es válido.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La página debe ser mayor o igual a uno.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100.");
    }
}
