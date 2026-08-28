using FluentValidation;
using Mantaras.Juridico.Application.Features.Familiares.Requests;

namespace Mantaras.Juridico.Application.Features.Familiares.Validators;

public sealed class VincularFamiliarRequestValidator
    : AbstractValidator<VincularFamiliarRequest>
{
    public VincularFamiliarRequestValidator()
    {
        RuleFor(x => x.FamiliarId)
            .GreaterThan(0)
            .WithMessage("Seleccioná un cliente como familiar.");

        RuleFor(x => x.Parentesco)
            .IsInEnum()
            .WithMessage("Seleccioná un parentesco válido.");
    }
}