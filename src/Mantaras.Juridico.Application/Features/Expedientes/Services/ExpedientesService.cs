using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Services;
using Mantaras.Juridico.Application.Features.Expedientes.Requests;
using Mantaras.Juridico.Application.Features.Expedientes.Responses;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Services;

public sealed class ExpedientesService : IExpedientesService
{
    private readonly IExpedienteRepository _expedienteRepository;
    private readonly ICasoRepository _casoRepository;
    private readonly IReglasVencimientoService _reglasVencimientoService;
    private readonly ICurrentUserService _currentUser;

    public ExpedientesService(
        IExpedienteRepository expedienteRepository,
        ICasoRepository casoRepository,
        IReglasVencimientoService reglasVencimientoService,
        ICurrentUserService currentUser
    )
    {
        _expedienteRepository = expedienteRepository;
        _casoRepository = casoRepository;
        _reglasVencimientoService = reglasVencimientoService;
        _currentUser = currentUser;
    }

    public async Task<Result<ExpedienteResponse>> CrearAsync(
        CrearExpedienteRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var casoIds = request.CasoIds.Distinct().ToArray();
        var casos = await _casoRepository.ObtenerActivosPorIdsAsync(
            casoIds,
            cancellationToken
        );

        if (casos.Count != casoIds.Length)
        {
            return Result<ExpedienteResponse>.Failure(
                ExpedienteErrors.CasoNoEncontradoOInactivo
            );
        }

        if (
            request.TipoExpediente == TipoExpediente.Principal
            && await _expedienteRepository.ExistePrincipalAsync(
                casoIds,
                cancellationToken: cancellationToken
            )
        )
        {
            return Result<ExpedienteResponse>.Failure(
                ExpedienteErrors.PrincipalDuplicado
            );
        }

        var padreResult = await ResolverPadreAsync(
            request.ExpedientePadreId,
            null,
            cancellationToken
        );

        if (padreResult.Error is { } error)
        {
            return Result<ExpedienteResponse>.Failure(error);
        }

        if (
            padreResult.Padre is not null
            && !padreResult.Padre.Casos.Any(x => casoIds.Contains(x.CasoId))
        )
        {
            return Result<ExpedienteResponse>.Failure(ExpedienteErrors.PadreDeOtroCaso);
        }

        var expediente = new Expediente
        {
            ExpedientePadreId = padreResult.Padre?.ExpedienteId,
            TipoExpediente = request.TipoExpediente,
            NumeroExpediente = NormalizarOpcional(request.NumeroExpediente),
            Caratula = request.Caratula.Trim(),
            Juzgado = NormalizarOpcional(request.Juzgado),
            FechaInicio = request.FechaInicio,
            EstadoLegal = NormalizarOpcional(request.EstadoLegal),
            TipoTramite = NormalizarOpcional(request.TipoTramite),
            ExpedientePadre = padreResult.Padre,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        foreach (var caso in casos)
        {
            expediente.Casos.Add(
                new CasoExpediente
                {
                    CasoId = caso.CasoId,
                    Caso = caso,
                    Expediente = expediente,
                }
            );
        }

        await _expedienteRepository.AgregarAsync(expediente, cancellationToken);
        await _expedienteRepository.GuardarCambiosAsync(cancellationToken);

        return Result<ExpedienteResponse>.Success(MapearResponse(expediente));
    }

    public async Task<Result<ExpedienteDetalleResponse>> ObtenerPorIdAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    )
    {
        var expediente = await _expedienteRepository.ObtenerDetallePorIdAsync(
            expedienteId,
            cancellationToken
        );

        return expediente is null
            ? Result<ExpedienteDetalleResponse>.Failure(ExpedienteErrors.NoEncontrado)
            : Result<ExpedienteDetalleResponse>.Success(MapearDetalleResponse(expediente));
    }

    public async Task<PagedResponse<ExpedienteResponse>> BuscarAsync(
        BuscarExpedientesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var expedientes = await _expedienteRepository.BuscarAsync(
            request.CasoId,
            request.Busqueda,
            request.SoloActivos,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        var totalItems = await _expedienteRepository.ContarAsync(
            request.CasoId,
            request.Busqueda,
            request.SoloActivos,
            cancellationToken
        );

        return new PagedResponse<ExpedienteResponse>
        {
            Items = expedientes.Select(MapearResponse).ToArray(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
        };
    }

    public async Task<Result<ExpedienteResponse>> ActualizarAsync(
        long expedienteId,
        ActualizarExpedienteRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var expediente = await _expedienteRepository.ObtenerPorIdAsync(
            expedienteId,
            cancellationToken
        );

        if (expediente is null)
        {
            return Result<ExpedienteResponse>.Failure(ExpedienteErrors.NoEncontrado);
        }

        var casoIds = request.CasoIds.Distinct().ToArray();
        var casos = await _casoRepository.ObtenerActivosPorIdsAsync(
            casoIds,
            cancellationToken
        );

        if (casos.Count != casoIds.Length)
        {
            return Result<ExpedienteResponse>.Failure(
                ExpedienteErrors.CasoNoEncontradoOInactivo
            );
        }

        if (
            request.TipoExpediente == TipoExpediente.Principal
            && await _expedienteRepository.ExistePrincipalAsync(
                casoIds,
                expedienteId,
                cancellationToken
            )
        )
        {
            return Result<ExpedienteResponse>.Failure(
                ExpedienteErrors.PrincipalDuplicado
            );
        }

        if (request.ExpedientePadreId == expedienteId)
        {
            return Result<ExpedienteResponse>.Failure(
                ExpedienteErrors.PadreEsMismoExpediente
            );
        }

        var padreResult = await ResolverPadreAsync(
            request.ExpedientePadreId,
            expedienteId,
            cancellationToken
        );

        if (padreResult.Error is { } error)
        {
            return Result<ExpedienteResponse>.Failure(error);
        }

        if (
            padreResult.Padre is not null
            && !padreResult.Padre.Casos.Any(x => casoIds.Contains(x.CasoId))
        )
        {
            return Result<ExpedienteResponse>.Failure(ExpedienteErrors.PadreDeOtroCaso);
        }

        expediente.ExpedientePadreId = padreResult.Padre?.ExpedienteId;
        expediente.ExpedientePadre = padreResult.Padre;
        expediente.TipoExpediente = request.TipoExpediente;
        expediente.NumeroExpediente = NormalizarOpcional(request.NumeroExpediente);
        expediente.Caratula = request.Caratula.Trim();
        expediente.Juzgado = NormalizarOpcional(request.Juzgado);
        expediente.FechaInicio = request.FechaInicio;
        expediente.EstadoLegal = NormalizarOpcional(request.EstadoLegal);
        expediente.TipoTramite = NormalizarOpcional(request.TipoTramite);
        expediente.FechaModificacion = DateTime.UtcNow;
        expediente.UsuarioModificacion = _currentUser.Usuario;

        var solicitados = casos.ToDictionary(x => x.CasoId);
        var eliminados = expediente.Casos
            .Where(x => !solicitados.ContainsKey(x.CasoId))
            .ToArray();

        foreach (var relacion in eliminados)
        {
            expediente.Casos.Remove(relacion);
        }

        var existentes = expediente.Casos.Select(x => x.CasoId).ToHashSet();

        foreach (var caso in casos.Where(x => !existentes.Contains(x.CasoId)))
        {
            expediente.Casos.Add(
                new CasoExpediente
                {
                    CasoId = caso.CasoId,
                    ExpedienteId = expediente.ExpedienteId,
                    Caso = caso,
                    Expediente = expediente,
                }
            );
        }

        foreach (var relacion in expediente.Casos)
        {
            relacion.Caso = solicitados[relacion.CasoId];
        }

        var errorEnvioCobro = await RegistrarEnvioCobroAsync(
            expediente,
            request.EnvioCobro,
            cancellationToken
        );

        if (errorEnvioCobro is not null)
        {
            return Result<ExpedienteResponse>.Failure(errorEnvioCobro);
        }

        await _expedienteRepository.GuardarCambiosAsync(cancellationToken);

        return Result<ExpedienteResponse>.Success(MapearResponse(expediente));
    }

    public async Task<PagedResponse<ExpedienteCobroResponse>> BuscarCobroAsync(
        BuscarExpedientesCobroRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var fechaActual = ObtenerFechaActual();
        var expedientes = await _expedienteRepository.BuscarCobroAsync(
            request.Busqueda,
            request.SoloActivos,
            request.SeguimientoVencido,
            fechaActual,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        var totalItems = await _expedienteRepository.ContarCobroAsync(
            request.Busqueda,
            request.SoloActivos,
            request.SeguimientoVencido,
            fechaActual,
            cancellationToken
        );

        return new PagedResponse<ExpedienteCobroResponse>
        {
            Items = expedientes
                .Select(expediente => new ExpedienteCobroResponse
                {
                    ExpedienteId = expediente.ExpedienteId,
                    Casos = MapearCasos(expediente),
                    TipoExpediente = expediente.TipoExpediente,
                    NumeroExpediente = expediente.NumeroExpediente,
                    Caratula = expediente.Caratula,
                    Juzgado = expediente.Juzgado,
                    EstadoLegal = expediente.EstadoLegal,
                    Activo = expediente.Activo,
                    EnvioCobro = MapearEnvioCobro(expediente, fechaActual)!,
                })
                .ToArray(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
        };
    }

    public async Task<Result<bool>> DarDeBajaAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    )
    {
        var expediente = await _expedienteRepository.ObtenerPorIdAsync(
            expedienteId,
            cancellationToken
        );

        if (expediente is null)
        {
            return Result<bool>.Failure(ExpedienteErrors.NoEncontrado);
        }

        if (!expediente.Activo)
        {
            return Result<bool>.Success(true);
        }

        if (
            await _expedienteRepository.TieneDerivadosActivosAsync(
                expedienteId,
                cancellationToken
            )
        )
        {
            return Result<bool>.Failure(ExpedienteErrors.DerivadosActivos);
        }

        expediente.Activo = false;
        expediente.FechaModificacion = DateTime.UtcNow;
        expediente.UsuarioModificacion = _currentUser.Usuario;

        await _expedienteRepository.GuardarCambiosAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RestaurarAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    )
    {
        var expediente = await _expedienteRepository.ObtenerPorIdAsync(
            expedienteId,
            cancellationToken
        );

        if (expediente is null)
        {
            return Result<bool>.Failure(ExpedienteErrors.NoEncontrado);
        }

        if (expediente.Activo)
        {
            return Result<bool>.Success(true);
        }

        if (expediente.Casos.Count == 0 || expediente.Casos.Any(x => !x.Caso.Activo))
        {
            return Result<bool>.Failure(
                ExpedienteErrors.CasoNoEncontradoOInactivo
            );
        }

        if (expediente.ExpedientePadreId.HasValue)
        {
            var padre = await _expedienteRepository.ObtenerPorIdAsync(
                expediente.ExpedientePadreId.Value,
                cancellationToken
            );

            if (padre is null || !padre.Activo)
            {
                return Result<bool>.Failure(
                    ExpedienteErrors.PadreNoEncontradoOInactivo
                );
            }

            var casoIds = expediente.Casos.Select(x => x.CasoId).ToHashSet();
            if (!padre.Casos.Any(x => casoIds.Contains(x.CasoId)))
            {
                return Result<bool>.Failure(ExpedienteErrors.PadreDeOtroCaso);
            }

        }

        expediente.Activo = true;
        expediente.FechaModificacion = DateTime.UtcNow;
        expediente.UsuarioModificacion = _currentUser.Usuario;

        await _expedienteRepository.GuardarCambiosAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private async Task<Error?> RegistrarEnvioCobroAsync(
        Expediente expediente,
        RegistrarEnvioCobroRequest? solicitud,
        CancellationToken cancellationToken
    )
    {
        if (solicitud is null)
        {
            return null;
        }

        var envioActivo = expediente.EnviosCobro.SingleOrDefault(x => x.Activo);

        if (envioActivo is not null)
        {
            return envioActivo.TipoDocumento == solicitud.TipoDocumento
                && envioActivo.FechaEnvio == solicitud.FechaEnvio
                    ? null
                    : ExpedienteErrors.EnvioCobroYaRegistrado;
        }

        if (!expediente.Activo)
        {
            return ExpedienteErrors.EnvioCobroExpedienteInactivo;
        }

        var generacionResult = await _reglasVencimientoService
            .PrepararAplicacionPorCodigoAsync(
                ReglaVencimiento.CodigoSeguimientoCobroJudicial,
                new AplicarReglaVencimientoRequest
                {
                    FechaBase = solicitud.FechaEnvio,
                    OrigenFechaBase = OrigenFechaBaseAgenda.ExpedienteJudicial,
                    ExpedienteOrigenId = expediente.ExpedienteId,
                    ClaveIdempotencia = ConstruirClaveSeguimientoCobro(
                        expediente.ExpedienteId,
                        solicitud
                    ),
                    CasoIds = expediente.Casos.Select(x => x.CasoId).ToArray(),
                },
                cancellationToken
            );

        if (generacionResult.IsFailure)
        {
            return generacionResult.Errors.First();
        }

        var generacion = generacionResult.Value!;

        // Una generación ya persistida con la misma clave pertenece a un envío anterior.
        if (generacion.AgendaGeneracionReglaId != 0)
        {
            return ExpedienteErrors.SeguimientoCobroYaGenerado;
        }

        expediente.EnviosCobro.Add(
            new EnvioCobroExpediente
            {
                ExpedienteId = expediente.ExpedienteId,
                Expediente = expediente,
                TipoDocumento = solicitud.TipoDocumento,
                FechaEnvio = solicitud.FechaEnvio,
                AgendaGeneracionRegla = generacion,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = _currentUser.Usuario,
                Activo = true,
            }
        );

        return null;
    }

    private static string ConstruirClaveSeguimientoCobro(
        long expedienteId,
        RegistrarEnvioCobroRequest solicitud
    ) =>
        $"cobro-judicial:{expedienteId}:{(int)solicitud.TipoDocumento}:{solicitud.FechaEnvio:yyyy-MM-dd}";

    private static DateOnly ObtenerFechaActual()
    {
        return DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById(
                    EntradaAgenda.ZonaHorariaPredeterminada
                )
            )
        );
    }

    private static EnvioCobroResponse? MapearEnvioCobro(
        Expediente expediente,
        DateOnly fechaActual
    )
    {
        var envio = expediente.EnviosCobro.SingleOrDefault(x => x.Activo);

        if (envio is null)
        {
            return null;
        }

        var generacion = envio.AgendaGeneracionRegla;
        var fechaSeguimiento = generacion.EntradaAgenda.FechaVencimiento!.Value;
        var diasParaSeguimiento = fechaSeguimiento.DayNumber - fechaActual.DayNumber;

        return new EnvioCobroResponse
        {
            EnvioCobroExpedienteId = envio.EnvioCobroExpedienteId,
            TipoDocumento = envio.TipoDocumento,
            FechaEnvio = envio.FechaEnvio,
            AgendaGeneracionReglaId = generacion.AgendaGeneracionReglaId,
            ReglaVencimientoId = generacion.ReglaVencimientoId,
            EntradaAgendaId = generacion.EntradaAgendaId,
            FechaSeguimiento = fechaSeguimiento,
            EstadoSeguimiento = generacion.EntradaAgenda.Estado,
            DiasParaSeguimiento = diasParaSeguimiento,
            SeguimientoVencido = diasParaSeguimiento < 0,
            FechaRegistro = envio.FechaCreacion,
            UsuarioRegistro = envio.UsuarioCreacion,
        };
    }

    private async Task<(Expediente? Padre, Error? Error)> ResolverPadreAsync(
        long? expedientePadreId,
        long? expedienteId,
        CancellationToken cancellationToken
    )
    {
        if (!expedientePadreId.HasValue)
        {
            return (null, null);
        }

        var padre = await _expedienteRepository.ObtenerPorIdAsync(
            expedientePadreId.Value,
            cancellationToken
        );

        if (padre is null || !padre.Activo)
        {
            return (null, ExpedienteErrors.PadreNoEncontradoOInactivo);
        }

        if (
            expedienteId.HasValue
            && await ProduceCicloAsync(expedienteId.Value, padre, cancellationToken)
        )
        {
            return (null, ExpedienteErrors.JerarquiaCiclica);
        }

        return (padre, null);
    }

    private async Task<bool> ProduceCicloAsync(
        long expedienteId,
        Expediente expedientePadrePropuesto,
        CancellationToken cancellationToken
    )
    {
        var visitados = new HashSet<long>();
        Expediente? expedienteActual = expedientePadrePropuesto;

        while (expedienteActual is not null)
        {
            if (
                expedienteActual.ExpedienteId == expedienteId
                || !visitados.Add(expedienteActual.ExpedienteId)
            )
            {
                return true;
            }

            if (!expedienteActual.ExpedientePadreId.HasValue)
            {
                return false;
            }

            expedienteActual = await _expedienteRepository.ObtenerPorIdAsync(
                expedienteActual.ExpedientePadreId.Value,
                cancellationToken
            );
        }

        return false;
    }

    private static ExpedienteDetalleResponse MapearDetalleResponse(Expediente expediente)
    {
        return new ExpedienteDetalleResponse
        {
            ExpedienteId = expediente.ExpedienteId,
            Casos = MapearCasos(expediente),
            ExpedientePadreId = expediente.ExpedientePadreId,
            TipoExpediente = expediente.TipoExpediente,
            NumeroExpediente = expediente.NumeroExpediente,
            Caratula = expediente.Caratula,
            Juzgado = expediente.Juzgado,
            FechaInicio = expediente.FechaInicio,
            EstadoLegal = expediente.EstadoLegal,
            TipoTramite = expediente.TipoTramite,
            EnvioCobro = MapearEnvioCobro(expediente, ObtenerFechaActual()),
            ExpedientePadre = expediente.ExpedientePadre is null
                ? null
                : MapearRelacionado(expediente.ExpedientePadre),
            ExpedientesDerivados = expediente.ExpedientesDerivados
                .OrderBy(x => x.FechaInicio)
                .ThenBy(x => x.Caratula)
                .Select(MapearRelacionado)
                .ToArray(),
            FechaCreacion = expediente.FechaCreacion,
            FechaModificacion = expediente.FechaModificacion,
            Activo = expediente.Activo,
        };
    }

    private static ExpedienteResponse MapearResponse(Expediente expediente)
    {
        return new ExpedienteResponse
        {
            ExpedienteId = expediente.ExpedienteId,
            Casos = MapearCasos(expediente),
            ExpedientePadreId = expediente.ExpedientePadreId,
            TipoExpediente = expediente.TipoExpediente,
            NumeroExpediente = expediente.NumeroExpediente,
            Caratula = expediente.Caratula,
            Juzgado = expediente.Juzgado,
            FechaInicio = expediente.FechaInicio,
            EstadoLegal = expediente.EstadoLegal,
            TipoTramite = expediente.TipoTramite,
            EnvioCobro = MapearEnvioCobro(expediente, ObtenerFechaActual()),
            FechaCreacion = expediente.FechaCreacion,
            FechaModificacion = expediente.FechaModificacion,
            Activo = expediente.Activo,
        };
    }

    private static IReadOnlyCollection<CasoExpedienteResponse> MapearCasos(
        Expediente expediente
    )
    {
        return expediente.Casos
            .OrderBy(x => x.Caso.Titulo)
            .ThenBy(x => x.CasoId)
            .Select(x => new CasoExpedienteResponse
            {
                CasoId = x.CasoId,
                Titulo = x.Caso.Titulo,
                NumeroExpedienteAnses = x.Caso.NumeroExpedienteAnses,
                NumeroBeneficio = x.Caso.NumeroBeneficio,
                TipoBeneficioId = x.Caso.TipoBeneficioId,
                TipoBeneficioNombre = x.Caso.TipoBeneficio?.Nombre,
                TipoBeneficioActivo = x.Caso.TipoBeneficio?.Activo,
                Activo = x.Caso.Activo,
            })
            .ToArray();
    }

    private static ExpedienteRelacionadoResponse MapearRelacionado(
        Expediente expediente
    )
    {
        return new ExpedienteRelacionadoResponse
        {
            ExpedienteId = expediente.ExpedienteId,
            TipoExpediente = expediente.TipoExpediente,
            NumeroExpediente = expediente.NumeroExpediente,
            Caratula = expediente.Caratula,
            Activo = expediente.Activo,
        };
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
