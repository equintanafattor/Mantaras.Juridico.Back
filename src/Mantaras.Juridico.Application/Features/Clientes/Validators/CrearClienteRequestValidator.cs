using FluentValidation;
using Mantaras.Juridico.Application.Features.Clientes.Requests;

namespace Mantaras.Juridico.Application.Features.Clientes.Validators;

public class CrearClienteRequestValidator : AbstractValidator<CrearClienteRequest>
{
    public CrearClienteRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del cliente es obligatorio.")
            .MaximumLength(100)
            .WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Apellido)
            .NotEmpty()
            .WithMessage("El apellido del cliente es obligatorio.")
            .MaximumLength(100)
            .WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(x => x.Dni)
            .MaximumLength(20)
            .WithMessage("El DNI no puede superar los 20 caracteres.");

        RuleFor(x => x.Cuil)
            .MaximumLength(20)
            .WithMessage("El CUIL no puede superar los 20 caracteres.");

        RuleFor(x => x.ClaveSeguridadSocial)
            .MaximumLength(500)
            .WithMessage("La Clave de Seguridad Social no puede superar los 500 caracteres.");

        RuleFor(x => x.Telefono)
            .MaximumLength(50)
            .WithMessage("El teléfono no puede superar los 50 caracteres.");

        RuleFor(x => x.Email)
            .MaximumLength(200)
            .WithMessage("El email no puede superar los 200 caracteres.")
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El email informado no tiene un formato válido.");

        RuleFor(x => x.Domicilio)
            .MaximumLength(300)
            .WithMessage("El domicilio no puede superar los 300 caracteres.");

        RuleFor(x => x.Localidad)
            .MaximumLength(150)
            .WithMessage("La localidad no puede superar los 150 caracteres.");

        RuleFor(x => x.Provincia)
            .MaximumLength(150)
            .WithMessage("La provincia no puede superar los 150 caracteres.");

        RuleFor(x => x.FechaNacimiento)
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.FechaNacimiento.HasValue)
            .WithMessage("La fecha de nacimiento no puede ser posterior a hoy.");

        RuleFor(x => x.Dni)
            .Matches(@"^[0-9.\-\s]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Dni))
            .WithMessage("El DNI solo puede contener números, puntos, guiones y espacios.");

        RuleFor(x => x.Cuil)
            .Matches(@"^[0-9.\-\s]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Cuil))
            .WithMessage("El CUIL solo puede contener números, puntos, guiones y espacios.");
    }
}
