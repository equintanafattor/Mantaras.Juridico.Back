using FluentValidation;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.TiposBeneficio.Requests;

namespace Mantaras.Juridico.Application.Features.TiposBeneficio.Validators;

public class GuardarTipoBeneficioRequestValidator : AbstractValidator<GuardarTipoBeneficioRequest>
{
    public GuardarTipoBeneficioRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("El nombre del tipo de beneficio es obligatorio.")
            .Must(nombre => NombreCatalogo.Normalizar(nombre).Length <= 100)
            .WithMessage("El nombre no puede superar los 100 caracteres después de normalizarlo.");
    }
}
