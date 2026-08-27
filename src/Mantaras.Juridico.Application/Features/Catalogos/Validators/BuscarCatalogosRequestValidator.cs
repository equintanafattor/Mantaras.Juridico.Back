using FluentValidation;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;

namespace Mantaras.Juridico.Application.Features.Catalogos.Validators;

public class BuscarCatalogosRequestValidator : AbstractValidator<BuscarCatalogosRequest>
{
    public BuscarCatalogosRequestValidator()
    {
        RuleFor(x => x.Busqueda)
            .MaximumLength(150)
            .WithMessage("La búsqueda no puede superar los 150 caracteres.");

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
