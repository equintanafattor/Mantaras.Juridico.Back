using FluentValidation;
using Mantaras.Juridico.Application.Features.Autenticacion.Requests;

namespace Mantaras.Juridico.Application.Features.Autenticacion.Validators;

public sealed class IniciarSesionRequestValidator : AbstractValidator<IniciarSesionRequest>
{
    public IniciarSesionRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress()
            .WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(256)
            .WithMessage("El correo electrónico no puede superar los 256 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .MaximumLength(200)
            .WithMessage("La contraseña no puede superar los 200 caracteres.");
    }
}
