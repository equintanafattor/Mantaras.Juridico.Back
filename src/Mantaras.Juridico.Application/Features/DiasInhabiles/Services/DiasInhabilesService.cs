using FluentValidation;
using FluentValidation.Results;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Services;

public class DiasInhabilesService : IDiasInhabilesService
{
    private readonly IDiaInhabilRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GuardarDiaInhabilRequest> _guardarValidator;
    private readonly IValidator<BuscarDiasInhabilesRequest> _buscarValidator;

    public DiasInhabilesService(
        IDiaInhabilRepository repository,
        ICurrentUserService currentUser,
        IValidator<GuardarDiaInhabilRequest> guardarValidator,
        IValidator<BuscarDiasInhabilesRequest> buscarValidator
    )
    {
        _repository = repository;
        _currentUser = currentUser;
        _guardarValidator = guardarValidator;
        _buscarValidator = buscarValidator;
    }

    public async Task<Result<DiaInhabilResponse>> CrearAsync(
        GuardarDiaInhabilRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _guardarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<DiaInhabilResponse>.Failure(ErrorValidacion(validacion));
        }

        var fecha = request.Fecha;
        var descripcion = request.Descripcion.Trim();

        if (await _repository.ExisteFechaAsync(fecha, cancellationToken: cancellationToken))
        {
            return Result<DiaInhabilResponse>.Failure(DiaInhabilErrors.FechaDuplicada);
        }

        var entidad = new DiaInhabil
        {
            Fecha = fecha,
            Descripcion = descripcion,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
        };

        await _repository.AgregarAsync(entidad, cancellationToken);

        try
        {
            await _repository.GuardarCambiosAsync(cancellationToken);
        }
        catch (FechaDiaInhabilDuplicadaException)
        {
            return Result<DiaInhabilResponse>.Failure(DiaInhabilErrors.FechaDuplicada);
        }

        return Result<DiaInhabilResponse>.Success(Mapear(entidad));
    }

    public async Task<Result<DiaInhabilResponse>> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        var entidad = await _repository.ObtenerPorIdAsync(id, cancellationToken);

        if (entidad is null)
        {
            return Result<DiaInhabilResponse>.Failure(DiaInhabilErrors.NoEncontrado);
        }

        return Result<DiaInhabilResponse>.Success(Mapear(entidad));
    }

    public async Task<Result<DiaInhabilResponse>> ActualizarAsync(
        long id,
        GuardarDiaInhabilRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _guardarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<DiaInhabilResponse>.Failure(ErrorValidacion(validacion));
        }

        var entidad = await _repository.ObtenerPorIdAsync(id, cancellationToken);

        if (entidad is null)
        {
            return Result<DiaInhabilResponse>.Failure(DiaInhabilErrors.NoEncontrado);
        }

        var fecha = request.Fecha;
        var descripcion = request.Descripcion.Trim();

        if (await _repository.ExisteFechaAsync(fecha, id, cancellationToken))
        {
            return Result<DiaInhabilResponse>.Failure(DiaInhabilErrors.FechaDuplicada);
        }

        if (entidad.Fecha == fecha && entidad.Descripcion == descripcion)
        {
            return Result<DiaInhabilResponse>.Success(Mapear(entidad));
        }

        entidad.Fecha = fecha;
        entidad.Descripcion = descripcion;
        entidad.FechaModificacion = DateTime.UtcNow;
        entidad.UsuarioModificacion = _currentUser.Usuario;

        try
        {
            await _repository.GuardarCambiosAsync(cancellationToken);
        }
        catch (FechaDiaInhabilDuplicadaException)
        {
            return Result<DiaInhabilResponse>.Failure(DiaInhabilErrors.FechaDuplicada);
        }

        return Result<DiaInhabilResponse>.Success(Mapear(entidad));
    }

    public Task<Result<bool>> DarDeBajaAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => CambiarActivoAsync(id, false, cancellationToken);

    public Task<Result<bool>> ReactivarAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => CambiarActivoAsync(id, true, cancellationToken);

    public async Task<Result<PagedResponse<DiaInhabilResponse>>> BuscarAsync(
        BuscarDiasInhabilesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _buscarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<PagedResponse<DiaInhabilResponse>>.Failure(ErrorValidacion(validacion));
        }

        // Secuencial: ambas consultas comparten el mismo DbContext scoped.
        var items = await _repository.BuscarAsync(
            request.Anio, request.SoloActivos, request.Page, request.PageSize,
            cancellationToken
        );

        var total = await _repository.ContarAsync(
            request.Anio, request.SoloActivos, cancellationToken
        );

        return Result<PagedResponse<DiaInhabilResponse>>.Success(
            new PagedResponse<DiaInhabilResponse>
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
            return Result<bool>.Failure(DiaInhabilErrors.NoEncontrado);
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
        DiaInhabilErrors.DatosInvalidos(
            string.Join(" ", validacion.Errors.Select(x => x.ErrorMessage).Distinct())
        );

    private static DiaInhabilResponse Mapear(DiaInhabil entidad) => new()
    {
        DiaInhabilId = entidad.DiaInhabilId,
        Fecha = entidad.Fecha,
        Descripcion = entidad.Descripcion,
        Activo = entidad.Activo,
        FechaCreacion = entidad.FechaCreacion,
        FechaModificacion = entidad.FechaModificacion,
    };
}
