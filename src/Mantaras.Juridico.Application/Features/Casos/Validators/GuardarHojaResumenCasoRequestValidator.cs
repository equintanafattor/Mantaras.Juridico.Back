using FluentValidation;
using Mantaras.Juridico.Application.Features.Casos.Requests;

namespace Mantaras.Juridico.Application.Features.Casos.Validators;

public sealed class GuardarHojaResumenCasoRequestValidator
    : AbstractValidator<GuardarHojaResumenCasoRequest>
{
    public GuardarHojaResumenCasoRequestValidator()
    {
        RuleFor(x => x.HaberInicialReajustadoCaracteristicas)
            .MaximumLength(2000)
            .WithMessage(
                "Las características no pueden superar los 2000 caracteres."
            );

        RuleFor(x => x.HaberInicialObservacion)
            .MaximumLength(2000)
            .WithMessage(
                "La observación del haber inicial no puede superar los 2000 caracteres."
            );

        RuleFor(x => x.MovilidadObservaciones)
            .MaximumLength(2000)
            .WithMessage(
                "Las observaciones de movilidad no pueden superar los 2000 caracteres."
            );

        RuleFor(x => x.RetroactivoObservacion)
            .MaximumLength(2000)
            .WithMessage(
                "La observación del retroactivo no puede superar los 2000 caracteres."
            );

        RuleFor(x => x.HaberInicialPbu)
            .Must(EsImporteValido)
            .WithMessage(
                "PBU admite hasta 16 dígitos enteros y 2 decimales."
            );

        RuleFor(x => x.HaberInicialMonto)
            .Must(EsImporteValido)
            .WithMessage(
                "El monto del haber inicial admite hasta 16 dígitos enteros y 2 decimales."
            );

        RuleFor(x => x.MovilidadMonto)
            .Must(EsImporteValido)
            .WithMessage(
                "El monto de movilidad admite hasta 16 dígitos enteros y 2 decimales."
            );

        RuleFor(x => x.RetroactivoMonto)
            .Must(EsImporteValido)
            .WithMessage(
                "El monto del retroactivo admite hasta 16 dígitos enteros y 2 decimales."
            );

        RuleFor(x => x.MovilidadActualizacionMes)
            .Must(mes => mes is null || (mes >= 1 && mes <= 12))
            .WithMessage(
                "El mes de actualización debe estar entre 1 y 12."
            );

        RuleFor(x => x.MovilidadActualizacionAnio)
            .Must(anio => anio is null || (anio >= 1 && anio <= 9999))
            .WithMessage(
                "El año de actualización debe estar entre 1 y 9999."
            );

        RuleFor(x => x.MovilidadActualizacionMes)
            .Must((request, mes) =>
                mes.HasValue == request.MovilidadActualizacionAnio.HasValue
            )
            .WithMessage(
                "Completá tanto el mes como el año de actualización, o dejá ambos vacíos."
            );
    }

    private static bool EsImporteValido(decimal? valor)
    {
        if (!valor.HasValue)
        {
            return true;
        }

        const decimal maximo = 9_999_999_999_999_999.99m;

        return valor.Value >= -maximo
            && valor.Value <= maximo
            && decimal.Round(valor.Value, 2) == valor.Value;
    }
}