using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Requests;

namespace Mantaras.Juridico.Application.Features.Agenda.Validators;

public sealed class CambiarEstadoAgendaRequestValidator
    : AbstractValidator<CambiarEstadoAgendaRequest>
{
    public CambiarEstadoAgendaRequestValidator()
    {
        RuleFor(x => x.Estado)
            .IsInEnum()
            .WithMessage("El estado informado no es válido.");
    }
}
