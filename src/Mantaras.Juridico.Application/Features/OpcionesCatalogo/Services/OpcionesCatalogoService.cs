using FluentValidation;
using FluentValidation.Results;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.Catalogos.Exceptions;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Requests;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.OpcionesCatalogo.Services;

public sealed class OpcionesCatalogoService : IOpcionesCatalogoService
{
    private readonly IOpcionCatalogoRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GuardarOpcionCatalogoRequest> _guardarValidator;
    private readonly IValidator<BuscarCatalogosRequest> _buscarValidator;

    public OpcionesCatalogoService(
        IOpcionCatalogoRepository repository,
        ICurrentUserService currentUser,
        IValidator<GuardarOpcionCatalogoRequest> guardarValidator,
        IValidator<BuscarCatalogosRequest> buscarValidator
    )
    {
        _repository = repository;
        _currentUser = currentUser;
        _guardarValidator = guardarValidator;
        _buscarValidator = buscarValidator;
    }

    public async Task<Result<OpcionCatalogoResponse>> CrearAsync(
        string tipo,
        GuardarOpcionCatalogoRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (!TiposOpcionCatalogo.EsValido(tipo))
            return Result<OpcionCatalogoResponse>.Failure(OpcionCatalogoErrors.TipoInvalido);

        var validacion = await _guardarValidator.ValidateAsync(request, cancellationToken);
        if (!validacion.IsValid)
            return Result<OpcionCatalogoResponse>.Failure(ErrorValidacion(validacion));

        var nombre = NombreCatalogo.Normalizar(request.Nombre);
        if (await _repository.ExisteNombreAsync(tipo, nombre, cancellationToken: cancellationToken))
            return Result<OpcionCatalogoResponse>.Failure(OpcionCatalogoErrors.NombreDuplicado);

        var entidad = new OpcionCatalogo
        {
            Tipo = tipo,
            Nombre = nombre,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
        };

        await _repository.AgregarAsync(entidad, cancellationToken);
        var resultado = await GuardarAsync(entidad, cancellationToken);
        return resultado;
    }

    public async Task<Result<OpcionCatalogoResponse>> ObtenerPorIdAsync(
        string tipo,
        long id,
        CancellationToken cancellationToken = default
    )
    {
        if (!TiposOpcionCatalogo.EsValido(tipo))
            return Result<OpcionCatalogoResponse>.Failure(OpcionCatalogoErrors.TipoInvalido);

        var entidad = await _repository.ObtenerPorIdAsync(tipo, id, cancellationToken);
        return entidad is null
            ? Result<OpcionCatalogoResponse>.Failure(OpcionCatalogoErrors.NoEncontrado)
            : Result<OpcionCatalogoResponse>.Success(Mapear(entidad));
    }

    public async Task<Result<OpcionCatalogoResponse>> ActualizarAsync(
        string tipo,
        long id,
        GuardarOpcionCatalogoRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (!TiposOpcionCatalogo.EsValido(tipo))
            return Result<OpcionCatalogoResponse>.Failure(OpcionCatalogoErrors.TipoInvalido);

        var validacion = await _guardarValidator.ValidateAsync(request, cancellationToken);
        if (!validacion.IsValid)
            return Result<OpcionCatalogoResponse>.Failure(ErrorValidacion(validacion));

        var entidad = await _repository.ObtenerPorIdAsync(tipo, id, cancellationToken);
        if (entidad is null)
            return Result<OpcionCatalogoResponse>.Failure(OpcionCatalogoErrors.NoEncontrado);

        var nombre = NombreCatalogo.Normalizar(request.Nombre);
        if (await _repository.ExisteNombreAsync(tipo, nombre, id, cancellationToken))
            return Result<OpcionCatalogoResponse>.Failure(OpcionCatalogoErrors.NombreDuplicado);

        entidad.Nombre = nombre;
        entidad.FechaModificacion = DateTime.UtcNow;
        entidad.UsuarioModificacion = _currentUser.Usuario;
        return await GuardarAsync(entidad, cancellationToken);
    }

    public async Task<Result<bool>> CambiarEstadoAsync(
        string tipo,
        long id,
        bool activar,
        CancellationToken cancellationToken = default
    )
    {
        if (!TiposOpcionCatalogo.EsValido(tipo))
            return Result<bool>.Failure(OpcionCatalogoErrors.TipoInvalido);

        var entidad = await _repository.ObtenerPorIdAsync(tipo, id, cancellationToken);
        if (entidad is null)
            return Result<bool>.Failure(OpcionCatalogoErrors.NoEncontrado);

        if (entidad.Activo == activar)
            return Result<bool>.Success(true);

        entidad.Activo = activar;
        entidad.FechaModificacion = DateTime.UtcNow;
        entidad.UsuarioModificacion = _currentUser.Usuario;
        await _repository.GuardarCambiosAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<PagedResponse<OpcionCatalogoResponse>>> BuscarAsync(
        string tipo,
        BuscarCatalogosRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (!TiposOpcionCatalogo.EsValido(tipo))
            return Result<PagedResponse<OpcionCatalogoResponse>>.Failure(OpcionCatalogoErrors.TipoInvalido);

        var validacion = await _buscarValidator.ValidateAsync(request, cancellationToken);
        if (!validacion.IsValid)
            return Result<PagedResponse<OpcionCatalogoResponse>>.Failure(ErrorValidacion(validacion));

        var items = await _repository.BuscarAsync(
            tipo, request.Busqueda, request.SoloActivos, request.Page, request.PageSize,
            cancellationToken
        );
        var total = await _repository.ContarAsync(
            tipo, request.Busqueda, request.SoloActivos, cancellationToken
        );

        return Result<PagedResponse<OpcionCatalogoResponse>>.Success(new()
        {
            Items = items.Select(Mapear).ToArray(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = total,
        });
    }

    private async Task<Result<OpcionCatalogoResponse>> GuardarAsync(
        OpcionCatalogo entidad,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _repository.GuardarCambiosAsync(cancellationToken);
            return Result<OpcionCatalogoResponse>.Success(Mapear(entidad));
        }
        catch (NombreCatalogoDuplicadoException)
        {
            return Result<OpcionCatalogoResponse>.Failure(OpcionCatalogoErrors.NombreDuplicado);
        }
    }

    private static Error ErrorValidacion(ValidationResult validacion) =>
        OpcionCatalogoErrors.DatosInvalidos(
            string.Join(" ", validacion.Errors.Select(x => x.ErrorMessage).Distinct())
        );

    private static OpcionCatalogoResponse Mapear(OpcionCatalogo entidad) => new()
    {
        OpcionCatalogoId = entidad.OpcionCatalogoId,
        Nombre = entidad.Nombre,
        Activo = entidad.Activo,
        FechaCreacion = entidad.FechaCreacion,
        FechaModificacion = entidad.FechaModificacion,
    };
}
