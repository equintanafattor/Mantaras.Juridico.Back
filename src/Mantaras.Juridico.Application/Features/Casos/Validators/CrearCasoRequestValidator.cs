using FluentValidation;
using Mantaras.Juridico.Application.Features.Casos.Requests;

namespace Mantaras.Juridico.Application.Features.Casos.Validators;

public sealed class CrearCasoRequestValidator : AbstractValidator<CrearCasoRequest>
{
    public CrearCasoRequestValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .WithMessage("El título del caso es obligatorio.")
            .MaximumLength(300)
            .WithMessage("El título no puede superar los 300 caracteres.");

        RuleFor(x => x.FaseInterna)
            .IsInEnum()
            .WithMessage("La fase interna informada no es válida.");

        RuleFor(x => x.TipoTramite)
            .MaximumLength(200)
            .WithMessage("El tipo de trámite no puede superar los 200 caracteres.");

        RuleFor(x => x.Clientes)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("El caso debe tener al menos un cliente.")
            .Must(TenerClientesUnicos)
            .WithMessage("No se puede informar el mismo cliente más de una vez.")
            .Must(TenerUnSoloPrincipal)
            .WithMessage("El caso debe tener exactamente un cliente principal.");

        RuleForEach(x => x.Clientes).SetValidator(new CasoClienteRequestValidator());
    }

    private static bool TenerClientesUnicos(
        IReadOnlyCollection<CasoClienteRequest>? clientes
    )
    {
        return clientes is null
            || clientes.Select(x => x.ClienteId).Distinct().Count() == clientes.Count;
    }

    private static bool TenerUnSoloPrincipal(
        IReadOnlyCollection<CasoClienteRequest>? clientes
    )
    {
        return clientes is not null
            && clientes.Count(x => x.EsPrincipal) == 1;
    }
}
