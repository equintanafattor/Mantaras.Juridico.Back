using FluentValidation;
using Mantaras.Juridico.Application.Features.Casos.Requests;

namespace Mantaras.Juridico.Application.Features.Casos.Validators;

public sealed class CrearCasoConExpedientePrincipalRequestValidator
    : AbstractValidator<CrearCasoConExpedientePrincipalRequest>
{
    public CrearCasoConExpedientePrincipalRequestValidator()
    {
        RuleFor(x => x.Caso)
            .NotNull()
            .WithMessage("Los datos del caso son obligatorios.")
            .SetValidator(new CrearCasoRequestValidator());

        RuleFor(x => x.Expediente)
            .NotNull()
            .WithMessage("Los datos del expediente son obligatorios.")
            .SetValidator(
                new CrearExpedientePrincipalRequestValidator()
            );
    }
}