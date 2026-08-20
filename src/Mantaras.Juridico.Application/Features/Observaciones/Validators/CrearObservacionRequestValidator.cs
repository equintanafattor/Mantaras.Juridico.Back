using FluentValidation;
using Mantaras.Juridico.Application.Features.Observaciones.Requests;

namespace Mantaras.Juridico.Application.Features.Observaciones.Validators;

public sealed class CrearObservacionRequestValidator
    : AbstractValidator<CrearObservacionRequest>
{
    public CrearObservacionRequestValidator()
    {
        RuleFor(request => request.Texto)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("La observación es obligatoria.")
            .Must(texto => !string.IsNullOrWhiteSpace(texto))
            .WithMessage("La observación es obligatoria.")
            .MaximumLength(2000)
            .WithMessage(
                "La observación no puede superar los 2000 caracteres."
            );
    }
}