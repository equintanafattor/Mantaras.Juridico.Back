using FluentValidation;
using Mantaras.Juridico.Application.Features.Expedientes.Requests;

namespace Mantaras.Juridico.Application.Features.Expedientes.Validators;

public sealed class CrearExpedienteRequestValidator
    : AbstractValidator<CrearExpedienteRequest>
{
    public CrearExpedienteRequestValidator()
    {
        RuleFor(x => x.CasoId)
            .GreaterThan(0)
            .WithMessage("El caso informado no es válido.");

        RuleFor(x => x.ExpedientePadreId)
            .GreaterThan(0)
            .When(x => x.ExpedientePadreId.HasValue)
            .WithMessage("El expediente padre informado no es válido.");

        RuleFor(x => x.NumeroExpediente)
            .MaximumLength(100)
            .WithMessage("El número de expediente no puede superar los 100 caracteres.");

        RuleFor(x => x.Caratula)
            .NotEmpty()
            .WithMessage("La carátula del expediente es obligatoria.")
            .MaximumLength(1000)
            .WithMessage("La carátula no puede superar los 1000 caracteres.");

        RuleFor(x => x.Juzgado)
            .MaximumLength(500)
            .WithMessage("El juzgado no puede superar los 500 caracteres.");

        RuleFor(x => x.EstadoLegal)
            .MaximumLength(200)
            .WithMessage("El estado legal no puede superar los 200 caracteres.");
    }
}