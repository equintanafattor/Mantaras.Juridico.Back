using FluentValidation;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Validators;

public sealed class ConfirmarImportacionFeriadosRequestValidator
    : AbstractValidator<ConfirmarImportacionFeriadosRequest>
{
    public ConfirmarImportacionFeriadosRequestValidator()
    {
        RuleFor(x => x.Huella)
            .Matches("^[0-9a-f]{64}$")
            .WithMessage("La huella de la vista previa es obligatoria y debe ser la que devolvió la vista previa.");

        RuleFor(x => x.FechasRevisionIncluidas)
            .NotNull()
            .WithMessage("Las fechas a revisar incluidas no pueden ser nulas.")
            .Must(x => x is null || x.Length <= 100)
            .WithMessage("No se pueden incluir más de 100 fechas a revisar.");
    }
}
