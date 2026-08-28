using FluentValidation;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Casos.Requests;
using Mantaras.Juridico.Application.Features.Casos.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Casos.Services;

public sealed class HojaResumenCasoService : IHojaResumenCasoService
{
    private readonly ICasoRepository _casoRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GuardarHojaResumenCasoRequest> _validator;

    public HojaResumenCasoService(
        ICasoRepository casoRepository,
        ICurrentUserService currentUser,
        IValidator<GuardarHojaResumenCasoRequest> validator
    )
    {
        _casoRepository = casoRepository;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<HojaResumenCasoResponse>> ObtenerAsync(
        long casoId,
        CancellationToken cancellationToken = default
    )
    {
        var caso = await _casoRepository.ObtenerConHojaResumenAsync(
            casoId,
            cancellationToken
        );

        if (caso is null)
        {
            return Result<HojaResumenCasoResponse>.Failure(
                CasoErrors.NoEncontrado
            );
        }

        return Result<HojaResumenCasoResponse>.Success(
            Mapear(casoId, caso.HojaResumen)
        );
    }

    public async Task<Result<HojaResumenCasoResponse>> GuardarAsync(
        long casoId,
        GuardarHojaResumenCasoRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _validator.ValidateAsync(
            request,
            cancellationToken
        );

        if (!validacion.IsValid)
        {
            return Result<HojaResumenCasoResponse>.Failure(
                new Error(
                    "Casos.HojaResumen.DatosInvalidos",
                    string.Join(
                        " ",
                        validacion.Errors.Select(x => x.ErrorMessage)
                    )
                )
            );
        }

        var caso = await _casoRepository.ObtenerConHojaResumenAsync(
            casoId,
            cancellationToken
        );

        if (caso is null)
        {
            return Result<HojaResumenCasoResponse>.Failure(
                CasoErrors.NoEncontrado
            );
        }

        var datos = request with
        {
            HaberInicialReajustadoCaracteristicas =
                Normalizar(request.HaberInicialReajustadoCaracteristicas),
            HaberInicialObservacion =
                Normalizar(request.HaberInicialObservacion),
            MovilidadObservaciones =
                Normalizar(request.MovilidadObservaciones),
            RetroactivoObservacion =
                Normalizar(request.RetroactivoObservacion),
        };

        var hoja = caso.HojaResumen;

        if (hoja is not null && Coincide(hoja, datos))
        {
            return Result<HojaResumenCasoResponse>.Success(
                Mapear(casoId, hoja)
            );
        }

        var esNueva = hoja is null;
        var ahora = DateTime.UtcNow;

        // PostgreSQL almacena estas fechas con precisión de microsegundos.
        ahora = ahora.AddTicks(-(ahora.Ticks % 10));

        if (hoja is null)
        {
            hoja = new HojaResumenCaso
            {
                CasoId = casoId,
                FechaCreacion = ahora,
                UsuarioCreacion = _currentUser.Usuario,
            };
        }
        else
        {
            hoja.FechaModificacion = ahora;
            hoja.UsuarioModificacion = _currentUser.Usuario;
        }

        hoja.TieneCalculoPrevio = datos.TieneCalculoPrevio;

        hoja.HaberInicialReajustadoCaracteristicas =
            datos.HaberInicialReajustadoCaracteristicas;
        hoja.HaberInicialPbu = datos.HaberInicialPbu;
        hoja.HaberInicialObservacion = datos.HaberInicialObservacion;
        hoja.HaberInicialMonto = datos.HaberInicialMonto;

        hoja.MovilidadActualizacionMes = datos.MovilidadActualizacionMes;
        hoja.MovilidadActualizacionAnio = datos.MovilidadActualizacionAnio;
        hoja.MovilidadObservaciones = datos.MovilidadObservaciones;
        hoja.MovilidadMonto = datos.MovilidadMonto;

        hoja.RetroactivoFechaInicio = datos.RetroactivoFechaInicio;
        hoja.RetroactivoFechaActualizacion = datos.RetroactivoFechaActualizacion;
        hoja.RetroactivoObservacion = datos.RetroactivoObservacion;
        hoja.RetroactivoMonto = datos.RetroactivoMonto;

        if (esNueva)
        {
            await _casoRepository.AgregarHojaResumenAsync(
                hoja,
                cancellationToken
            );
        }

        var guardada = await _casoRepository.GuardarHojaResumenAsync(
            cancellationToken
        );

        if (!guardada)
        {
            return Result<HojaResumenCasoResponse>.Failure(
                HojaResumenCasoErrors.CreacionConcurrente
            );
        }

        return Result<HojaResumenCasoResponse>.Success(
            Mapear(casoId, hoja)
        );
    }

    private static bool Coincide(
        HojaResumenCaso hoja,
        GuardarHojaResumenCasoRequest datos
    )
    {
        return hoja.TieneCalculoPrevio == datos.TieneCalculoPrevio
            && hoja.HaberInicialReajustadoCaracteristicas
                == datos.HaberInicialReajustadoCaracteristicas
            && hoja.HaberInicialPbu == datos.HaberInicialPbu
            && hoja.HaberInicialObservacion == datos.HaberInicialObservacion
            && hoja.HaberInicialMonto == datos.HaberInicialMonto
            && hoja.MovilidadActualizacionMes == datos.MovilidadActualizacionMes
            && hoja.MovilidadActualizacionAnio == datos.MovilidadActualizacionAnio
            && hoja.MovilidadObservaciones == datos.MovilidadObservaciones
            && hoja.MovilidadMonto == datos.MovilidadMonto
            && hoja.RetroactivoFechaInicio == datos.RetroactivoFechaInicio
            && hoja.RetroactivoFechaActualizacion == datos.RetroactivoFechaActualizacion
            && hoja.RetroactivoObservacion == datos.RetroactivoObservacion
            && hoja.RetroactivoMonto == datos.RetroactivoMonto;
    }

    private static HojaResumenCasoResponse Mapear(
        long casoId,
        HojaResumenCaso? hoja
    )
    {
        return new HojaResumenCasoResponse
        {
            CasoId = casoId,
            Registrada = hoja is not null,
            TieneCalculoPrevio = hoja?.TieneCalculoPrevio,
            HaberInicialReajustadoCaracteristicas =
                hoja?.HaberInicialReajustadoCaracteristicas,
            HaberInicialPbu = hoja?.HaberInicialPbu,
            HaberInicialObservacion = hoja?.HaberInicialObservacion,
            HaberInicialMonto = hoja?.HaberInicialMonto,
            MovilidadActualizacionMes = hoja?.MovilidadActualizacionMes,
            MovilidadActualizacionAnio = hoja?.MovilidadActualizacionAnio,
            MovilidadObservaciones = hoja?.MovilidadObservaciones,
            MovilidadMonto = hoja?.MovilidadMonto,
            RetroactivoFechaInicio = hoja?.RetroactivoFechaInicio,
            RetroactivoFechaActualizacion = hoja?.RetroactivoFechaActualizacion,
            RetroactivoObservacion = hoja?.RetroactivoObservacion,
            RetroactivoMonto = hoja?.RetroactivoMonto,
            FechaCreacion = hoja?.FechaCreacion,
            UsuarioCreacion = hoja?.UsuarioCreacion,
            FechaModificacion = hoja?.FechaModificacion,
            UsuarioModificacion = hoja?.UsuarioModificacion,
        };
    }

    private static string? Normalizar(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}