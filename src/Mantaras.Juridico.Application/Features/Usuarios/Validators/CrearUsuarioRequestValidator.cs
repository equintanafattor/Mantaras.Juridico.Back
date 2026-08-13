using FluentValidation;
using Mantaras.Juridico.Application.Common.Authorization;
using Mantaras.Juridico.Application.Features.Usuarios.Requests;

namespace Mantaras.Juridico.Application.Features.Usuarios.Validators;

public sealed class CrearUsuarioRequestValidator : AbstractValidator<CrearUsuarioRequest>
{
    public CrearUsuarioRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre es obligatorio.")
            .MaximumLength(150)
            .WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress()
            .WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(256)
            .WithMessage("El correo electrónico no puede superar los 256 caracteres.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .MinimumLength(10)
            .WithMessage("La contraseña debe tener al menos 10 caracteres.")
            .MaximumLength(200)
            .WithMessage("La contraseña no puede superar los 200 caracteres.")
            .Matches("[A-Z]")
            .WithMessage("La contraseña debe contener una mayúscula.")
            .Matches("[a-z]")
            .WithMessage("La contraseña debe contener una minúscula.")
            .Matches("[0-9]")
            .WithMessage("La contraseña debe contener un número.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("La contraseña debe contener un carácter especial.");

        RuleFor(x => x.Rol)
            .Must(rol => !string.IsNullOrWhiteSpace(rol) && RolesSistema.Todos.Contains(rol))
            .WithMessage("El rol informado no es válido.");
    }
}
