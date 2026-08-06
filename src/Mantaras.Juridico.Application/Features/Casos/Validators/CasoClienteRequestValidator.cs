using FluentValidation;
using Mantaras.Juridico.Application.Features.Casos.Requests;

namespace Mantaras.Juridico.Application.Features.Casos.Validators;

public sealed class CasoClienteRequestValidator : AbstractValidator<CasoClienteRequest>
{
    public CasoClienteRequestValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .WithMessage("El identificador del cliente debe ser válido.");

        RuleFor(x => x.TipoParticipacion)
            .IsInEnum()
            .WithMessage("El tipo de participación informado no es válido.");
    }
}
