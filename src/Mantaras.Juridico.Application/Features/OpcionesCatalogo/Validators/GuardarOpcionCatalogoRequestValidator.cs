using FluentValidation;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Requests;

namespace Mantaras.Juridico.Application.Features.OpcionesCatalogo.Validators;

public sealed class GuardarOpcionCatalogoRequestValidator
    : AbstractValidator<GuardarOpcionCatalogoRequest>
{
    public GuardarOpcionCatalogoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre es obligatorio.")
            .MaximumLength(200)
            .WithMessage("El nombre no puede superar los 200 caracteres.");
    }
}
