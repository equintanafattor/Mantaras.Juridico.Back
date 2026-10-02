using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Requests;

namespace Mantaras.Juridico.Application.Features.Agenda.Validators;

public sealed class GuardarEntradaAgendaRequestValidator
    : AbstractValidator<GuardarEntradaAgendaRequest>
{
    public GuardarEntradaAgendaRequestValidator()
    {
        RuleFor(x => x.TipoEntradaAgendaId)
            .GreaterThan(0)
            .WithMessage("El tipo de entrada es obligatorio.");

        RuleFor(x => x.Titulo)
            .NotEmpty()
            .WithMessage("El título es obligatorio.")
            .MaximumLength(300)
            .WithMessage("El título no puede superar los 300 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(4000)
            .WithMessage("La descripción no puede superar los 4000 caracteres.");

        RuleFor(x => x.Prioridad)
            .IsInEnum()
            .WithMessage("La prioridad informada no es válida.");

        RuleFor(x => x.FechaInicio)
            .NotEqual(default(DateOnly))
            .WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(x => x.FechaFin)
            .Must((request, fechaFin) =>
                !fechaFin.HasValue || fechaFin.Value >= request.FechaInicio
            )
            .WithMessage("La fecha de finalización no puede ser anterior al inicio.");

        RuleFor(x => x)
            .Must(x =>
                !x.FechaFin.HasValue
                || x.FechaFin.Value != x.FechaInicio
                || !x.HoraInicio.HasValue
                || !x.HoraFin.HasValue
                || x.HoraFin.Value >= x.HoraInicio.Value
            )
            .WithMessage("La hora de finalización no puede ser anterior al inicio.")
            .OverridePropertyName(nameof(GuardarEntradaAgendaRequest.HoraFin));

        RuleFor(x => x.HoraFin)
            .Null()
            .When(x => !x.FechaFin.HasValue)
            .WithMessage("Para informar una hora de finalización debe indicar su fecha.");

        RuleFor(x => x.HoraVencimiento)
            .Null()
            .When(x => !x.FechaVencimiento.HasValue)
            .WithMessage("Para informar una hora de vencimiento debe indicar su fecha.");

        RuleFor(x => x.FechaVencimiento)
            .Must((request, fechaVencimiento) =>
                !fechaVencimiento.HasValue
                || fechaVencimiento.Value >= request.FechaInicio
            )
            .WithMessage("La fecha de vencimiento no puede ser anterior al inicio.");

        ValidarIds(x => x.ClienteIds, "clientes");
        ValidarIds(x => x.CasoIds, "expedientes administrativos");
        ValidarIds(x => x.ExpedienteIds, "expedientes judiciales");
        ValidarIds(x => x.ResponsableIds, "responsables");

        RuleFor(x => x.Recurrencia!.Frecuencia).IsInEnum()
            .When(x => x.Recurrencia is not null);
        RuleFor(x => x.Recurrencia!.Intervalo).InclusiveBetween(1, 365)
            .When(x => x.Recurrencia is not null);
        RuleFor(x => x.Recurrencia!.CantidadOcurrencias).InclusiveBetween(2, 100)
            .When(x => x.Recurrencia is not null);
        RuleFor(x => x).Must(x =>
        {
            if (x.Recurrencia is not { } r || r.Intervalo < 1 || r.CantidadOcurrencias is < 2 or > 100)
                return true;
            try
            {
                var pasos = checked((r.CantidadOcurrencias - 1) * r.Intervalo);
                var ultima = r.Frecuencia switch
                {
                    Mantaras.Juridico.Domain.Enums.FrecuenciaRecurrenciaAgenda.Diaria => x.FechaInicio.AddDays(pasos),
                    Mantaras.Juridico.Domain.Enums.FrecuenciaRecurrenciaAgenda.Semanal => x.FechaInicio.AddDays(checked(pasos * 7)),
                    Mantaras.Juridico.Domain.Enums.FrecuenciaRecurrenciaAgenda.Mensual => x.FechaInicio.AddMonths(pasos),
                    Mantaras.Juridico.Domain.Enums.FrecuenciaRecurrenciaAgenda.Anual => x.FechaInicio.AddYears(pasos),
                    _ => x.FechaInicio,
                };
                return ultima.DayNumber - x.FechaInicio.DayNumber <= 3650
                    && (!x.FechaFin.HasValue || x.FechaFin.Value.AddDays(ultima.DayNumber - x.FechaInicio.DayNumber).Year <= 9999)
                    && (!x.FechaVencimiento.HasValue || x.FechaVencimiento.Value.AddDays(ultima.DayNumber - x.FechaInicio.DayNumber).Year <= 9999);
            }
            catch (ArgumentOutOfRangeException) { return false; }
            catch (OverflowException) { return false; }
        }).WithMessage("La recurrencia no puede superar diez años ni el rango de fechas admitido.")
          .OverridePropertyName(nameof(GuardarEntradaAgendaRequest.Recurrencia));
    }

    private void ValidarIds(
        System.Linq.Expressions.Expression<Func<GuardarEntradaAgendaRequest, IReadOnlyCollection<long>>> expression,
        string nombre
    )
    {
        RuleFor(expression)
            .NotNull()
            .Must(ids => ids.Count <= 100)
            .WithMessage($"No se pueden informar más de 100 {nombre}.")
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage($"Los {nombre} informados no son válidos.");
    }
}
