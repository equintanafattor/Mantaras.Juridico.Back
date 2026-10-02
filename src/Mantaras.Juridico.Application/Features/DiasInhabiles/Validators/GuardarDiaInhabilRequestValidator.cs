using FluentValidation;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Validators;
public sealed class GuardarDiaInhabilRequestValidator : AbstractValidator<GuardarDiaInhabilRequest>
{
    public GuardarDiaInhabilRequestValidator()
    {
        RuleFor(x => x.Fecha).NotEqual(default(DateOnly)).WithMessage("Ingresá una fecha válida posterior al 01/01/0001.");
        RuleFor(x => x.Descripcion).Cascade(CascadeMode.Stop).NotEmpty()
            .WithMessage("La descripción es obligatoria.")
            .Must(texto => texto.Trim().Length <= 300)
            .WithMessage("La descripción no puede superar los 300 caracteres.");
    }
}
