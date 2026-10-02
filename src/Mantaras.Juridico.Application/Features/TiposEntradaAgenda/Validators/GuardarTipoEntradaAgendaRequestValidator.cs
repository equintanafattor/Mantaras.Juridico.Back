using FluentValidation;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Requests;

namespace Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Validators;

public class GuardarTipoEntradaAgendaRequestValidator : AbstractValidator<GuardarTipoEntradaAgendaRequest>
{
    public GuardarTipoEntradaAgendaRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("El nombre del tipo de entrada es obligatorio.")
            .Must(nombre => NombreCatalogo.Normalizar(nombre).Length <= 150)
            .WithMessage("El nombre no puede superar los 150 caracteres después de normalizarlo.");
        RuleFor(x => x.Descripcion)
            .Must(texto => string.IsNullOrWhiteSpace(texto) || texto.Trim().Length <= 500)
            .WithMessage("La descripción no puede superar los 500 caracteres.");
        RuleFor(x => x.Color)
            .Must(EsColorValido)
            .WithMessage("Usá un color hexadecimal de seis dígitos (#2563EB) o blue, red, purple, yellow, green, orange, pink o gray.");
    }

    private static bool EsColorValido(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return true;
        var valor = color.Trim().ToLowerInvariant();
        return valor is "blue" or "red" or "purple" or "yellow" or "green" or "orange" or "pink" or "gray"
            || System.Text.RegularExpressions.Regex.IsMatch(valor, "^#[0-9a-f]{6}$");
    }
}
