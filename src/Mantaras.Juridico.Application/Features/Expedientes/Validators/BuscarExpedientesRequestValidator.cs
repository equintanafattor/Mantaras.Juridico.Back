using FluentValidation;
using Mantaras.Juridico.Application.Features.Expedientes.Requests;

namespace Mantaras.Juridico.Application.Features.Expedientes.Validators;

public sealed class BuscarExpedientesRequestValidator
    : AbstractValidator<BuscarExpedientesRequest>
{
    public BuscarExpedientesRequestValidator()
    {
        RuleFor(x => x.JuzgadoId)
            .GreaterThan(0)
            .When(x => x.JuzgadoId.HasValue)
            .WithMessage("El juzgado informado no es válido.");
    }
}
