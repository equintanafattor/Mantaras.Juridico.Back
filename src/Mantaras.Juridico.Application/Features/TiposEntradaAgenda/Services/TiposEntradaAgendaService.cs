using FluentValidation;
using FluentValidation.Results;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.Catalogos.Exceptions;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Requests;
using Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Services;

public class TiposEntradaAgendaService : ITiposEntradaAgendaService
{
    private readonly ITipoEntradaAgendaRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GuardarTipoEntradaAgendaRequest> _guardarValidator;
    private readonly IValidator<BuscarCatalogosRequest> _buscarValidator;

    public TiposEntradaAgendaService(
        ITipoEntradaAgendaRepository repository,
        ICurrentUserService currentUser,
        IValidator<GuardarTipoEntradaAgendaRequest> guardarValidator,
        IValidator<BuscarCatalogosRequest> buscarValidator
    )
    {
        _repository = repository;
        _currentUser = currentUser;
        _guardarValidator = guardarValidator;
        _buscarValidator = buscarValidator;
    }

    public async Task<Result<TipoEntradaAgendaResponse>> CrearAsync(
        GuardarTipoEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _guardarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<TipoEntradaAgendaResponse>.Failure(ErrorValidacion(validacion));
        }

        var nombre = NombreCatalogo.Normalizar(request.Nombre);

        if (await _repository.ExisteNombreAsync(nombre, cancellationToken: cancellationToken))
        {
            return Result<TipoEntradaAgendaResponse>.Failure(TipoEntradaAgendaErrors.NombreDuplicado);
        }

        var entidad = new TipoEntradaAgenda
        {
            Nombre = nombre,
            Descripcion = TextoOpcional(request.Descripcion),
            Color = TextoOpcional(request.Color)?.ToLowerInvariant(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
        };

        await _repository.AgregarAsync(entidad, cancellationToken);

        try
        {
            await _repository.GuardarCambiosAsync(cancellationToken);
        }
        catch (NombreCatalogoDuplicadoException)
        {
            return Result<TipoEntradaAgendaResponse>.Failure(TipoEntradaAgendaErrors.NombreDuplicado);
        }

        return Result<TipoEntradaAgendaResponse>.Success(Mapear(entidad));
    }

    public async Task<Result<TipoEntradaAgendaResponse>> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        var entidad = await _repository.ObtenerPorIdAsync(id, cancellationToken);

        if (entidad is null)
        {
            return Result<TipoEntradaAgendaResponse>.Failure(TipoEntradaAgendaErrors.NoEncontrado);
        }

        return Result<TipoEntradaAgendaResponse>.Success(Mapear(entidad));
    }

    public async Task<Result<TipoEntradaAgendaResponse>> ActualizarAsync(
        long id,
        GuardarTipoEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _guardarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<TipoEntradaAgendaResponse>.Failure(ErrorValidacion(validacion));
        }

        var entidad = await _repository.ObtenerPorIdAsync(id, cancellationToken);

        if (entidad is null)
        {
            return Result<TipoEntradaAgendaResponse>.Failure(TipoEntradaAgendaErrors.NoEncontrado);
        }

        var nombre = NombreCatalogo.Normalizar(request.Nombre);

        if (await _repository.ExisteNombreAsync(nombre, id, cancellationToken))
        {
            return Result<TipoEntradaAgendaResponse>.Failure(TipoEntradaAgendaErrors.NombreDuplicado);
        }

        var descripcion = TextoOpcional(request.Descripcion);
        var color = TextoOpcional(request.Color)?.ToLowerInvariant();

        if (entidad.Nombre == nombre && entidad.Descripcion == descripcion && entidad.Color == color)
        {
            return Result<TipoEntradaAgendaResponse>.Success(Mapear(entidad));
        }

        entidad.Nombre = nombre;
        entidad.Descripcion = descripcion;
        entidad.Color = color;
        entidad.FechaModificacion = DateTime.UtcNow;
        entidad.UsuarioModificacion = _currentUser.Usuario;

        try
        {
            await _repository.GuardarCambiosAsync(cancellationToken);
        }
        catch (NombreCatalogoDuplicadoException)
        {
            return Result<TipoEntradaAgendaResponse>.Failure(TipoEntradaAgendaErrors.NombreDuplicado);
        }

        return Result<TipoEntradaAgendaResponse>.Success(Mapear(entidad));
    }

    public Task<Result<bool>> DarDeBajaAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => CambiarActivoAsync(id, false, cancellationToken);

    public Task<Result<bool>> ReactivarAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => CambiarActivoAsync(id, true, cancellationToken);

    public async Task<Result<PagedResponse<TipoEntradaAgendaResponse>>> BuscarAsync(
        BuscarCatalogosRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _buscarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<PagedResponse<TipoEntradaAgendaResponse>>.Failure(ErrorValidacion(validacion));
        }

        // Secuencial: ambas consultas comparten el mismo DbContext scoped.
        var items = await _repository.BuscarAsync(
            request.Busqueda, request.SoloActivos, request.Page, request.PageSize,
            cancellationToken
        );

        var total = await _repository.ContarAsync(
            request.Busqueda, request.SoloActivos, cancellationToken
        );

        return Result<PagedResponse<TipoEntradaAgendaResponse>>.Success(
            new PagedResponse<TipoEntradaAgendaResponse>
            {
                Items = items.Select(Mapear).ToArray(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = total,
            }
        );
    }

    private async Task<Result<bool>> CambiarActivoAsync(
        long id,
        bool activo,
        CancellationToken cancellationToken
    )
    {
        var entidad = await _repository.ObtenerPorIdAsync(id, cancellationToken);

        if (entidad is null)
        {
            return Result<bool>.Failure(TipoEntradaAgendaErrors.NoEncontrado);
        }

        if (entidad.Activo == activo)
        {
            return Result<bool>.Success(true);
        }

        entidad.Activo = activo;
        entidad.FechaModificacion = DateTime.UtcNow;
        entidad.UsuarioModificacion = _currentUser.Usuario;

        await _repository.GuardarCambiosAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static string? TextoOpcional(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static Error ErrorValidacion(ValidationResult validacion) =>
        TipoEntradaAgendaErrors.DatosInvalidos(
            string.Join(" ", validacion.Errors.Select(x => x.ErrorMessage).Distinct())
        );

    private static TipoEntradaAgendaResponse Mapear(TipoEntradaAgenda entidad) => new()
    {
        TipoEntradaAgendaId = entidad.TipoEntradaAgendaId,
        Nombre = entidad.Nombre,
        Descripcion = entidad.Descripcion,
        Color = entidad.Color,
        Activo = entidad.Activo,
        FechaCreacion = entidad.FechaCreacion,
        FechaModificacion = entidad.FechaModificacion,
    };
}
