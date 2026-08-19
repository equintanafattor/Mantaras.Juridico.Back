using FluentValidation;
using Mantaras.Juridico.Application.Features.Casos.Requests;

namespace Mantaras.Juridico.Application.Features.Casos.Validators;

public sealed class CrearExpedientePrincipalRequestValidator
    : AbstractValidator<CrearExpedientePrincipalRequest>
{
    public CrearExpedientePrincipalRequestValidator()
    {
        RuleFor(x => x.NumeroExpediente)
            .MaximumLength(100)
            .WithMessage(
                "El número de expediente no puede superar los 100 caracteres."
            );

        RuleFor(x => x.Caratula)
            .NotEmpty()
            .WithMessage("La carátula del expediente es obligatoria.")
            .MaximumLength(1000)
            .WithMessage(
                "La carátula no puede superar los 1000 caracteres."
            );

        RuleFor(x => x.Juzgado)
            .MaximumLength(500)
            .WithMessage("El juzgado no puede superar los 500 caracteres.");

        RuleFor(x => x.EstadoLegal)
            .MaximumLength(200)
            .WithMessage(
                "El estado legal no puede superar los 200 caracteres."
            );
        RuleFor(x => x.Observaciones)
            .MaximumLength(2000)
            .WithMessage(
                "Las observaciones no pueden superar los 2000 caracteres."
            );
    }
}