using FluentValidation;
using Mantaras.Juridico.Application.Features.Expedientes.Requests;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Validators;

public sealed class ActualizarExpedienteRequestValidator
    : AbstractValidator<ActualizarExpedienteRequest>
{
    public ActualizarExpedienteRequestValidator()
    {
        RuleFor(x => x.ExpedientePadreId)
            .GreaterThan(0)
            .When(x => x.ExpedientePadreId.HasValue)
            .WithMessage("El expediente padre informado no es válido.");

        RuleFor(x => x.TipoExpediente)
            .IsInEnum()
            .WithMessage("El tipo de expediente informado no es válido.");

        RuleFor(x => x)
            .Must(x =>
                !Enum.IsDefined(
                    typeof(TipoExpediente),
                    x.TipoExpediente
                )
                || x.TipoExpediente != TipoExpediente.Principal
                || !x.ExpedientePadreId.HasValue
            )
            .WithMessage(
                "Un expediente principal no puede tener expediente padre."
            )
            .OverridePropertyName(
                nameof(ActualizarExpedienteRequest.TipoExpediente)
            );

        RuleFor(x => x)
            .Must(x =>
                !Enum.IsDefined(
                    typeof(TipoExpediente),
                    x.TipoExpediente
                )
                || x.TipoExpediente == TipoExpediente.Principal
                || x.ExpedientePadreId.HasValue
            )
            .WithMessage(
                "Los incidentes, apelaciones y ejecuciones deben tener un expediente padre."
            )
            .OverridePropertyName(
                nameof(ActualizarExpedienteRequest.TipoExpediente)
            );

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

        RuleFor(x => x.Observaciones)
            .MaximumLength(2000)
            .WithMessage(
                "Las observaciones no pueden superar los 2000 caracteres."
            );
    }
}