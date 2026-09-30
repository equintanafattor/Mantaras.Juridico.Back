using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Requests;

namespace Mantaras.Juridico.Application.Features.Agenda.Validators;

public sealed class BuscarAgendaRequestValidator
    : AbstractValidator<BuscarAgendaRequest>
{
    public BuscarAgendaRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.Desde.HasValue || !x.Hasta.HasValue || x.Hasta >= x.Desde)
            .WithMessage("La fecha hasta no puede ser anterior a la fecha desde.")
            .OverridePropertyName(nameof(BuscarAgendaRequest.Hasta));

        RuleFor(x => x.TipoEntradaAgendaId)
            .GreaterThan(0)
            .When(x => x.TipoEntradaAgendaId.HasValue)
            .WithMessage("El tipo de entrada informado no es válido.");

        RuleFor(x => x.Estado)
            .IsInEnum()
            .When(x => x.Estado.HasValue)
            .WithMessage("El estado informado no es válido.");

        ValidarId(x => x.ResponsableId, "responsable");
        ValidarId(x => x.ClienteId, "cliente");
        ValidarId(x => x.CasoId, "expediente administrativo");
        ValidarId(x => x.ExpedienteId, "expediente judicial");

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
            .When(x => x.Page >= 1 && x.PageSize is >= 1 and <= 100)
            .WithMessage("La página solicitada supera el rango permitido.");
    }

    private void ValidarId(
        System.Linq.Expressions.Expression<Func<BuscarAgendaRequest, long?>> expression,
        string nombre
    )
    {
        RuleFor(expression)
            .Must(id => !id.HasValue || id.Value > 0)
            .WithMessage($"El {nombre} informado no es válido.");
    }
}
