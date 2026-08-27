using FluentValidation;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo.Requests;

namespace Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo.Validators;

public class GuardarTipoExpedienteAdministrativoRequestValidator : AbstractValidator<GuardarTipoExpedienteAdministrativoRequest>
{
    public GuardarTipoExpedienteAdministrativoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("El nombre del tipo de expediente administrativo es obligatorio.")
            .Must(nombre => NombreCatalogo.Normalizar(nombre).Length <= 150)
            .WithMessage("El nombre no puede superar los 150 caracteres después de normalizarlo.");
    }
}
