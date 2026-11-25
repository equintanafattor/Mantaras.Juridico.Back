using FluentValidation;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.Juzgados.Requests;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Juzgados.Validators;

public class GuardarJuzgadoRequestValidator : AbstractValidator<GuardarJuzgadoRequest>
{
    public GuardarJuzgadoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .Must(nombre => NombreCatalogo.Normalizar(nombre).Length > 0)
            .WithMessage("El nombre del juzgado es obligatorio.")
            .Must(nombre => NombreCatalogo.Normalizar(nombre).Length <= Juzgado.LargoMaximoNombre)
            .WithMessage(
                $"El nombre no puede superar los {Juzgado.LargoMaximoNombre} caracteres después de normalizarlo."
            );
    }
}
