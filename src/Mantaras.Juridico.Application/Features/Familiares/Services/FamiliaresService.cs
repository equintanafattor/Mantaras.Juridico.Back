using FluentValidation;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Clientes;
using Mantaras.Juridico.Application.Features.Familiares.Requests;
using Mantaras.Juridico.Application.Features.Familiares.Responses;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Familiares.Services;

public sealed class FamiliaresService : IFamiliaresService
{
    private readonly IClienteRepository _clientes;
    private readonly IRelacionFamiliarRepository _relaciones;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<VincularFamiliarRequest> _validator;

    public FamiliaresService(
        IClienteRepository clientes,
        IRelacionFamiliarRepository relaciones,
        ICurrentUserService currentUser,
        IValidator<VincularFamiliarRequest> validator
    )
    {
        _clientes = clientes;
        _relaciones = relaciones;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<IReadOnlyCollection<FamiliarResponse>>> ListarAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    )
    {
        var cliente = await _clientes.ObtenerPorIdAsync(
            clienteId,
            cancellationToken
        );

        if (cliente is null)
        {
            return Result<IReadOnlyCollection<FamiliarResponse>>
                .Failure(ClienteErrors.NoEncontrado);
        }

        var relaciones = await _relaciones.ListarPorClienteAsync(
            clienteId,
            cancellationToken
        );

        var familiares = relaciones
            .Select(relacion => Mapear(
                relacion,
                clienteId,
                relacion.ClienteAId == clienteId
                    ? relacion.ClienteB
                    : relacion.ClienteA
            ))
            .OrderBy(x => x.NombreCompleto)
            .ThenBy(x => x.FamiliarId)
            .ToArray();

        return Result<IReadOnlyCollection<FamiliarResponse>>
            .Success(familiares);
    }

    public async Task<Result<FamiliarResponse>> VincularAsync(
        long clienteId,
        VincularFamiliarRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validacion = await _validator.ValidateAsync(
            request,
            cancellationToken
        );

        if (!validacion.IsValid)
        {
            return Result<FamiliarResponse>.Failure(new Error(
                "Familiares.DatosInvalidos",
                string.Join(
                    " ",
                    validacion.Errors
                        .Select(x => x.ErrorMessage)
                        .Distinct()
                )
            ));
        }

        if (clienteId <= 0)
        {
            return Result<FamiliarResponse>.Failure(
                FamiliaresErrors.IdentificadorInvalido
            );
        }

        if (clienteId == request.FamiliarId)
        {
            return Result<FamiliarResponse>.Failure(
                FamiliaresErrors.MismoCliente
            );
        }

        var cliente = await _clientes.ObtenerPorIdAsync(
            clienteId,
            cancellationToken
        );

        if (cliente is null)
        {
            return Result<FamiliarResponse>.Failure(
                ClienteErrors.NoEncontrado
            );
        }

        var familiar = await _clientes.ObtenerPorIdAsync(
            request.FamiliarId,
            cancellationToken
        );

        if (familiar is null)
        {
            return Result<FamiliarResponse>.Failure(
                FamiliaresErrors.FamiliarNoEncontrado
            );
        }

        var clienteAId = Math.Min(clienteId, request.FamiliarId);
        var clienteBId = Math.Max(clienteId, request.FamiliarId);

        var parentescoDeB = clienteId == clienteAId
            ? request.Parentesco
            : InvertirParentesco(request.Parentesco);

        var relacion = await _relaciones.ObtenerPorParejaAsync(
            clienteAId,
            clienteBId,
            cancellationToken
        );

        if (relacion is not null && relacion.Activo)
        {
            if (relacion.ParentescoDeB != parentescoDeB)
            {
                return Result<FamiliarResponse>.Failure(
                    FamiliaresErrors.ParentescoDiferente
                );
            }

            // La relación ya existe: no cambia datos ni auditoría.
            return Result<FamiliarResponse>.Success(
                Mapear(relacion, clienteId, familiar)
            );
        }

        if (!cliente.Activo || !familiar.Activo)
        {
            return Result<FamiliarResponse>.Failure(
                FamiliaresErrors.ClientesInactivos
            );
        }

        var ahora = ObtenerAhoraUtc();

        if (relacion is null)
        {
            relacion = new RelacionFamiliar
            {
                ClienteAId = clienteAId,
                ClienteBId = clienteBId,
                ParentescoDeB = parentescoDeB,
                Activo = true,
                FechaCreacion = ahora,
                UsuarioCreacion = _currentUser.Usuario,
            };

            await _relaciones.AgregarAsync(
                relacion,
                cancellationToken
            );
        }
        else
        {
            // Reactiva la misma fila y conserva la auditoría de alta.
            relacion.ParentescoDeB = parentescoDeB;
            relacion.Activo = true;
            relacion.FechaModificacion = ahora;
            relacion.UsuarioModificacion = _currentUser.Usuario;
        }

        var guardado = await _relaciones.IntentarGuardarCambiosAsync(
            cancellationToken
        );

        if (!guardado)
        {
            return Result<FamiliarResponse>.Failure(
                FamiliaresErrors.ConflictoGuardado
            );
        }

        return Result<FamiliarResponse>.Success(
            Mapear(relacion, clienteId, familiar)
        );
    }

    public async Task<Result<bool>> DesvincularAsync(
        long clienteId,
        long familiarId,
        CancellationToken cancellationToken = default
    )
    {
        if (clienteId <= 0 || familiarId <= 0)
        {
            return Result<bool>.Failure(
                FamiliaresErrors.IdentificadorInvalido
            );
        }

        if (clienteId == familiarId)
        {
            return Result<bool>.Failure(
                FamiliaresErrors.MismoCliente
            );
        }

        var cliente = await _clientes.ObtenerPorIdAsync(
            clienteId,
            cancellationToken
        );

        if (cliente is null)
        {
            return Result<bool>.Failure(ClienteErrors.NoEncontrado);
        }

        var relacion = await _relaciones.ObtenerPorParejaAsync(
            Math.Min(clienteId, familiarId),
            Math.Max(clienteId, familiarId),
            cancellationToken
        );

        if (relacion is null)
        {
            return Result<bool>.Failure(
                FamiliaresErrors.RelacionNoEncontrada
            );
        }

        if (!relacion.Activo)
        {
            return Result<bool>.Success(true);
        }

        relacion.Activo = false;
        relacion.FechaModificacion = ObtenerAhoraUtc();
        relacion.UsuarioModificacion = _currentUser.Usuario;

        var guardado = await _relaciones.IntentarGuardarCambiosAsync(
            cancellationToken
        );

        if (!guardado)
        {
            return Result<bool>.Failure(
                FamiliaresErrors.ConflictoGuardado
            );
        }

        return Result<bool>.Success(true);
    }

    private static FamiliarResponse Mapear(
        RelacionFamiliar relacion,
        long clienteId,
        Cliente familiar
    )
    {
        return new FamiliarResponse
        {
            RelacionFamiliarId = relacion.RelacionFamiliarId,
            FamiliarId = familiar.ClienteId,
            NombreCompleto = $"{familiar.Apellido}, {familiar.Nombre}",
            Dni = familiar.Dni,
            Cuil = familiar.Cuil,
            Activo = familiar.Activo,
            Parentesco = relacion.ClienteAId == clienteId
                ? relacion.ParentescoDeB
                : InvertirParentesco(relacion.ParentescoDeB),
            FechaCreacion = relacion.FechaCreacion,
            UsuarioCreacion = relacion.UsuarioCreacion,
            FechaModificacion = relacion.FechaModificacion,
            UsuarioModificacion = relacion.UsuarioModificacion,
        };
    }

    private static TipoParentesco InvertirParentesco(
        TipoParentesco parentesco
    )
    {
        return parentesco switch
        {
            TipoParentesco.Progenitor => TipoParentesco.Hijo,
            TipoParentesco.Hijo => TipoParentesco.Progenitor,
            TipoParentesco.Hermano => TipoParentesco.Hermano,
            TipoParentesco.Conyuge => TipoParentesco.Conyuge,
            TipoParentesco.Pareja => TipoParentesco.Pareja,
            TipoParentesco.Abuelo => TipoParentesco.Nieto,
            TipoParentesco.Nieto => TipoParentesco.Abuelo,
            TipoParentesco.Tio => TipoParentesco.Sobrino,
            TipoParentesco.Sobrino => TipoParentesco.Tio,
            TipoParentesco.OtroFamiliar => TipoParentesco.OtroFamiliar,
            _ => throw new ArgumentOutOfRangeException(
                nameof(parentesco),
                parentesco,
                "Parentesco no reconocido."
            ),
        };
    }

    private static DateTime ObtenerAhoraUtc()
    {
        var ahora = DateTime.UtcNow;

        // Precisión de microsegundos para coincidir con PostgreSQL.
        return ahora.AddTicks(-(ahora.Ticks % 10));
    }
}