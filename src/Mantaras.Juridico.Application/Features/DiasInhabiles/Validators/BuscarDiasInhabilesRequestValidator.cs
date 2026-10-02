using FluentValidation;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Validators;

public class BuscarDiasInhabilesRequestValidator : AbstractValidator<BuscarDiasInhabilesRequest>
{
    public BuscarDiasInhabilesRequestValidator()
    {
        RuleFor(x => x.Anio)
            .Must(anio => !anio.HasValue || anio.Value >= 1 && anio.Value <= 9999)
            .WithMessage("El año debe estar entre 1 y 9999.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100.");

        RuleFor(x => x)
            .Must(x => ((long)x.Page - 1) * x.PageSize <= int.MaxValue)
            .When(x => x.Page >= 1 && x.PageSize >= 1 && x.PageSize <= 100)
            .WithMessage("La página solicitada supera el rango permitido.");
    }
}
