using FluentValidation;
using FluentValidation.Results;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.Catalogos.Exceptions;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.TiposBeneficio.Requests;
using Mantaras.Juridico.Application.Features.TiposBeneficio.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.TiposBeneficio.Services;

public class TiposBeneficioService : ITiposBeneficioService
{
    private readonly ITipoBeneficioRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GuardarTipoBeneficioRequest> _guardarValidator;
    private readonly IValidator<BuscarCatalogosRequest> _buscarValidator;

    public TiposBeneficioService(
        ITipoBeneficioRepository repository,
        ICurrentUserService currentUser,
        IValidator<GuardarTipoBeneficioRequest> guardarValidator,
        IValidator<BuscarCatalogosRequest> buscarValidator
    )
    {
        _repository = repository;
        _currentUser = currentUser;
        _guardarValidator = guardarValidator;
        _buscarValidator = buscarValidator;
    }

    public async Task<Result<TipoBeneficioResponse>> CrearAsync(
        GuardarTipoBeneficioRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _guardarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<TipoBeneficioResponse>.Failure(ErrorValidacion(validacion));
        }

        var nombre = NombreCatalogo.Normalizar(request.Nombre);

        if (await _repository.ExisteNombreAsync(nombre, cancellationToken: cancellationToken))
        {
            return Result<TipoBeneficioResponse>.Failure(TipoBeneficioErrors.NombreDuplicado);
        }

        var entidad = new TipoBeneficio
        {
            Nombre = nombre,
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
            return Result<TipoBeneficioResponse>.Failure(TipoBeneficioErrors.NombreDuplicado);
        }

        return Result<TipoBeneficioResponse>.Success(Mapear(entidad));
    }

    public async Task<Result<TipoBeneficioResponse>> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        var entidad = await _repository.ObtenerPorIdAsync(id, cancellationToken);

        if (entidad is null)
        {
            return Result<TipoBeneficioResponse>.Failure(TipoBeneficioErrors.NoEncontrado);
        }

        return Result<TipoBeneficioResponse>.Success(Mapear(entidad));
    }

    public async Task<Result<TipoBeneficioResponse>> ActualizarAsync(
        long id,
        GuardarTipoBeneficioRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _guardarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<TipoBeneficioResponse>.Failure(ErrorValidacion(validacion));
        }

        var entidad = await _repository.ObtenerPorIdAsync(id, cancellationToken);

        if (entidad is null)
        {
            return Result<TipoBeneficioResponse>.Failure(TipoBeneficioErrors.NoEncontrado);
        }

        var nombre = NombreCatalogo.Normalizar(request.Nombre);

        if (await _repository.ExisteNombreAsync(nombre, id, cancellationToken))
        {
            return Result<TipoBeneficioResponse>.Failure(TipoBeneficioErrors.NombreDuplicado);
        }

        if (entidad.Nombre == nombre)
        {
            return Result<TipoBeneficioResponse>.Success(Mapear(entidad));
        }

        entidad.Nombre = nombre;
        entidad.FechaModificacion = DateTime.UtcNow;
        entidad.UsuarioModificacion = _currentUser.Usuario;

        try
        {
            await _repository.GuardarCambiosAsync(cancellationToken);
        }
        catch (NombreCatalogoDuplicadoException)
        {
            return Result<TipoBeneficioResponse>.Failure(TipoBeneficioErrors.NombreDuplicado);
        }

        return Result<TipoBeneficioResponse>.Success(Mapear(entidad));
    }

    public Task<Result<bool>> DarDeBajaAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => CambiarActivoAsync(id, false, cancellationToken);

    public Task<Result<bool>> ReactivarAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => CambiarActivoAsync(id, true, cancellationToken);

    public async Task<Result<PagedResponse<TipoBeneficioResponse>>> BuscarAsync(
        BuscarCatalogosRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _buscarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<PagedResponse<TipoBeneficioResponse>>.Failure(ErrorValidacion(validacion));
        }

        // Secuencial: ambas consultas comparten el mismo DbContext scoped.
        var items = await _repository.BuscarAsync(
            request.Busqueda, request.SoloActivos, request.Page, request.PageSize,
            cancellationToken
        );

        var total = await _repository.ContarAsync(
            request.Busqueda, request.SoloActivos, cancellationToken
        );

        return Result<PagedResponse<TipoBeneficioResponse>>.Success(
            new PagedResponse<TipoBeneficioResponse>
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
            return Result<bool>.Failure(TipoBeneficioErrors.NoEncontrado);
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

    private static Error ErrorValidacion(ValidationResult validacion) =>
        TipoBeneficioErrors.DatosInvalidos(
            string.Join(" ", validacion.Errors.Select(x => x.ErrorMessage).Distinct())
        );

    private static TipoBeneficioResponse Mapear(TipoBeneficio entidad) => new()
    {
        TipoBeneficioId = entidad.TipoBeneficioId,
        Nombre = entidad.Nombre,
        Activo = entidad.Activo,
        FechaCreacion = entidad.FechaCreacion,
        FechaModificacion = entidad.FechaModificacion,
    };
}
