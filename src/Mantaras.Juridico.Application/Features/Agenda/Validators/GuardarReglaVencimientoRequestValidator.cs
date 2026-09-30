using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Requests;

namespace Mantaras.Juridico.Application.Features.Agenda.Validators;

public sealed class GuardarReglaVencimientoRequestValidator
    : AbstractValidator<GuardarReglaVencimientoRequest>
{
    public GuardarReglaVencimientoRequestValidator()
    {
        RuleFor(x => x.TipoEntradaAgendaId)
            .GreaterThan(0)
            .WithMessage("El tipo de entrada es obligatorio.");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la regla es obligatorio.")
            .MaximumLength(200)
            .WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000)
            .WithMessage("La descripción no puede superar los 1000 caracteres.");

        RuleFor(x => x.CantidadDias)
            .InclusiveBetween(0, 36500)
            .WithMessage("La cantidad de días debe estar entre 0 y 36500.");

        RuleFor(x => x.TipoComputo)
            .IsInEnum()
            .WithMessage("El tipo de cómputo informado no es válido.");

        RuleFor(x => x.SentidoCalculo)
            .IsInEnum()
            .WithMessage("El sentido de cálculo informado no es válido.");

        RuleFor(x => x.PrioridadGenerada)
            .IsInEnum()
            .WithMessage("La prioridad informada no es válida.");
    }
}
