using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Expedientes.Requests;
using Mantaras.Juridico.Application.Features.Expedientes.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Expedientes.Services;

public sealed class ExpedientesService : IExpedientesService
{
    private readonly IExpedienteRepository _expedienteRepository;
    private readonly ICasoRepository _casoRepository;
    private readonly ICurrentUserService _currentUser;

    public ExpedientesService(
        IExpedienteRepository expedienteRepository,
        ICasoRepository casoRepository,
        ICurrentUserService currentUser
    )
    {
        _expedienteRepository = expedienteRepository;
        _casoRepository = casoRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ExpedienteResponse>> CrearAsync(
        CrearExpedienteRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var caso = await _casoRepository.ObtenerPorIdAsync(
            request.CasoId,
            cancellationToken
        );

        if (caso is null || !caso.Activo)
        {
            return Result<ExpedienteResponse>.Failure(
                ExpedienteErrors.CasoNoEncontradoOInactivo
            );
        }

        Expediente? expedientePadre = null;

        if (request.ExpedientePadreId.HasValue)
        {
            expedientePadre = await _expedienteRepository.ObtenerPorIdAsync(
                request.ExpedientePadreId.Value,
                cancellationToken
            );

            if (expedientePadre is null || !expedientePadre.Activo)
            {
                return Result<ExpedienteResponse>.Failure(
                    ExpedienteErrors.PadreNoEncontradoOInactivo
                );
            }

            if (expedientePadre.CasoId != request.CasoId)
            {
                return Result<ExpedienteResponse>.Failure(
                    ExpedienteErrors.PadreDeOtroCaso
                );
            }
        }

        var expediente = new Expediente
        {
            CasoId = caso.CasoId,
            ExpedientePadreId = expedientePadre?.ExpedienteId,
            NumeroExpediente = NormalizarOpcional(request.NumeroExpediente),
            Caratula = request.Caratula.Trim(),
            Juzgado = NormalizarOpcional(request.Juzgado),
            FechaInicio = request.FechaInicio,
            EstadoLegal = NormalizarOpcional(request.EstadoLegal),
            Caso = caso,
            ExpedientePadre = expedientePadre,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        await _expedienteRepository.AgregarAsync(
            expediente,
            cancellationToken
        );

        await _expedienteRepository.GuardarCambiosAsync(
            cancellationToken
        );

        return Result<ExpedienteResponse>.Success(
            MapearResponse(expediente)
        );
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

        if (expediente is null)
        {
            return Result<ExpedienteDetalleResponse>.Failure(
                ExpedienteErrors.NoEncontrado
            );
        }

        return Result<ExpedienteDetalleResponse>.Success(
            MapearDetalleResponse(expediente)
        );
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
            return Result<ExpedienteResponse>.Failure(
                ExpedienteErrors.NoEncontrado
            );
        }

        if (request.ExpedientePadreId == expedienteId)
        {
            return Result<ExpedienteResponse>.Failure(
                ExpedienteErrors.PadreEsMismoExpediente
            );
        }

        Expediente? expedientePadre = null;

        if (request.ExpedientePadreId.HasValue)
        {
            expedientePadre = await _expedienteRepository.ObtenerPorIdAsync(
                request.ExpedientePadreId.Value,
                cancellationToken
            );

            if (expedientePadre is null || !expedientePadre.Activo)
            {
                return Result<ExpedienteResponse>.Failure(
                    ExpedienteErrors.PadreNoEncontradoOInactivo
                );
            }

            if (expedientePadre.CasoId != expediente.CasoId)
            {
                return Result<ExpedienteResponse>.Failure(
                    ExpedienteErrors.PadreDeOtroCaso
                );
            }

            var produceCiclo = await ProduceCicloAsync(
                expedienteId,
                expedientePadre,
                cancellationToken
            );

            if (produceCiclo)
            {
                return Result<ExpedienteResponse>.Failure(
                    ExpedienteErrors.JerarquiaCiclica
                );
            }
        }

        expediente.ExpedientePadreId = expedientePadre?.ExpedienteId;
        expediente.ExpedientePadre = expedientePadre;
        expediente.NumeroExpediente = NormalizarOpcional(
            request.NumeroExpediente
        );
        expediente.Caratula = request.Caratula.Trim();
        expediente.Juzgado = NormalizarOpcional(request.Juzgado);
        expediente.FechaInicio = request.FechaInicio;
        expediente.EstadoLegal = NormalizarOpcional(request.EstadoLegal);
        expediente.FechaModificacion = DateTime.UtcNow;
        expediente.UsuarioModificacion = _currentUser.Usuario;

        await _expedienteRepository.GuardarCambiosAsync(
            cancellationToken
        );

        return Result<ExpedienteResponse>.Success(
            MapearResponse(expediente)
        );
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
            return Result<bool>.Failure(
                ExpedienteErrors.NoEncontrado
            );
        }

        if (!expediente.Activo)
        {
            return Result<bool>.Success(true);
        }

        expediente.Activo = false;
        expediente.FechaModificacion = DateTime.UtcNow;
        expediente.UsuarioModificacion = _currentUser.Usuario;

        await _expedienteRepository.GuardarCambiosAsync(
            cancellationToken
        );

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
            return Result<bool>.Failure(
                ExpedienteErrors.NoEncontrado
            );
        }

        if (expediente.Activo)
        {
            return Result<bool>.Success(true);
        }

        if (!expediente.Caso.Activo)
        {
            return Result<bool>.Failure(
                ExpedienteErrors.CasoNoEncontradoOInactivo
            );
        }

        if (expediente.ExpedientePadreId.HasValue)
        {
            var expedientePadre =
                await _expedienteRepository.ObtenerPorIdAsync(
                    expediente.ExpedientePadreId.Value,
                    cancellationToken
                );

            if (expedientePadre is null || !expedientePadre.Activo)
            {
                return Result<bool>.Failure(
                    ExpedienteErrors.PadreNoEncontradoOInactivo
                );
            }

            if (expedientePadre.CasoId != expediente.CasoId)
            {
                return Result<bool>.Failure(
                    ExpedienteErrors.PadreDeOtroCaso
                );
            }
        }

        expediente.Activo = true;
        expediente.FechaModificacion = DateTime.UtcNow;
        expediente.UsuarioModificacion = _currentUser.Usuario;

        await _expedienteRepository.GuardarCambiosAsync(
            cancellationToken
        );

        return Result<bool>.Success(true);
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
            if (expedienteActual.ExpedienteId == expedienteId)
            {
                return true;
            }

            if (!visitados.Add(expedienteActual.ExpedienteId))
            {
                return true;
            }

            if (!expedienteActual.ExpedientePadreId.HasValue)
            {
                return false;
            }

            expedienteActual =
                await _expedienteRepository.ObtenerPorIdAsync(
                    expedienteActual.ExpedientePadreId.Value,
                    cancellationToken
                );
        }

        return false;
    }

    private static ExpedienteDetalleResponse MapearDetalleResponse(
        Expediente expediente
    )
    {
        return new ExpedienteDetalleResponse
        {
            ExpedienteId = expediente.ExpedienteId,
            CasoId = expediente.CasoId,
            TituloCaso = expediente.Caso.Titulo,
            ExpedientePadreId = expediente.ExpedientePadreId,
            NumeroExpediente = expediente.NumeroExpediente,
            Caratula = expediente.Caratula,
            Juzgado = expediente.Juzgado,
            FechaInicio = expediente.FechaInicio,
            EstadoLegal = expediente.EstadoLegal,
            ExpedientePadre = expediente.ExpedientePadre is null
                ? null
                : MapearRelacionado(expediente.ExpedientePadre),
            ExpedientesDerivados = expediente
                .ExpedientesDerivados
                .OrderBy(x => x.FechaInicio)
                .ThenBy(x => x.Caratula)
                .Select(MapearRelacionado)
                .ToArray(),
            FechaCreacion = expediente.FechaCreacion,
            FechaModificacion = expediente.FechaModificacion,
            Activo = expediente.Activo,
        };
    }

    private static ExpedienteResponse MapearResponse(
        Expediente expediente
    )
    {
        return new ExpedienteResponse
        {
            ExpedienteId = expediente.ExpedienteId,
            CasoId = expediente.CasoId,
            TituloCaso = expediente.Caso.Titulo,
            ExpedientePadreId = expediente.ExpedientePadreId,
            NumeroExpediente = expediente.NumeroExpediente,
            Caratula = expediente.Caratula,
            Juzgado = expediente.Juzgado,
            FechaInicio = expediente.FechaInicio,
            EstadoLegal = expediente.EstadoLegal,
            FechaCreacion = expediente.FechaCreacion,
            FechaModificacion = expediente.FechaModificacion,
            Activo = expediente.Activo,
        };
    }

    private static ExpedienteRelacionadoResponse MapearRelacionado(
        Expediente expediente
    )
    {
        return new ExpedienteRelacionadoResponse
        {
            ExpedienteId = expediente.ExpedienteId,
            NumeroExpediente = expediente.NumeroExpediente,
            Caratula = expediente.Caratula,
            Activo = expediente.Activo,
        };
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}