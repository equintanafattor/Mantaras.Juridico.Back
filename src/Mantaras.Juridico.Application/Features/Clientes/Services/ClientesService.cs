using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Clientes.Requests;
using Mantaras.Juridico.Application.Features.Clientes.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Clientes.Services;

public class ClientesService : IClientesService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ICurrentUserService _currentUser;

    public ClientesService(IClienteRepository clienteRepository, ICurrentUserService currentUser)
    {
        _clienteRepository = clienteRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ClienteResponse>> CrearAsync(
        CrearClienteRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var dni = NormalizarDocumento(request.Dni);
        var cuil = NormalizarDocumento(request.Cuil);

        if (
            dni is not null
            && await _clienteRepository.ExisteDniAsync(dni, cancellationToken: cancellationToken)
        )
        {
            return Result<ClienteResponse>.Failure(ClienteErrors.DniDuplicado);
        }

        if (
            cuil is not null
            && await _clienteRepository.ExisteCuilAsync(cuil, cancellationToken: cancellationToken)
        )
        {
            return Result<ClienteResponse>.Failure(ClienteErrors.CuilDuplicado);
        }

        var cliente = new Cliente
        {
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Dni = dni,
            Cuil = cuil,
            ClaveSeguridadSocial = NormalizarOpcional(request.ClaveSeguridadSocial),
            FechaNacimiento = request.FechaNacimiento,
            Telefono = NormalizarOpcional(request.Telefono),
            Email = NormalizarOpcional(request.Email),
            Domicilio = NormalizarOpcional(request.Domicilio),
            Localidad = NormalizarOpcional(request.Localidad),
            Provincia = NormalizarOpcional(request.Provincia),
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        await _clienteRepository.AgregarAsync(cliente, cancellationToken);
        await _clienteRepository.GuardarCambiosAsync(cancellationToken);

        return Result<ClienteResponse>.Success(MapearResponse(cliente));
    }

    public async Task<Result<ClienteDetalleResponse>> ObtenerPorIdAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    )
    {
        var cliente = await _clienteRepository.ObtenerDetallePorIdAsync(
            clienteId,
            cancellationToken
        );

        if (cliente is null)
        {
            return Result<ClienteDetalleResponse>.Failure(ClienteErrors.NoEncontrado);
        }

        return Result<ClienteDetalleResponse>.Success(MapearDetalleResponse(cliente));
    }

    public async Task<Result<ClienteDetalleResponse>> ActualizarAsync(
        long clienteId,
        ActualizarClienteRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);

        if (cliente is null)
        {
            return Result<ClienteDetalleResponse>.Failure(ClienteErrors.NoEncontrado);
        }

        var dni = NormalizarDocumento(request.Dni);
        var cuil = NormalizarDocumento(request.Cuil);

        if (
            dni is not null
            && await _clienteRepository.ExisteDniAsync(dni, clienteId, cancellationToken)
        )
        {
            return Result<ClienteDetalleResponse>.Failure(ClienteErrors.DniDuplicado);
        }

        if (
            cuil is not null
            && await _clienteRepository.ExisteCuilAsync(cuil, clienteId, cancellationToken)
        )
        {
            return Result<ClienteDetalleResponse>.Failure(ClienteErrors.CuilDuplicado);
        }

        cliente.Nombre = request.Nombre.Trim();
        cliente.Apellido = request.Apellido.Trim();
        cliente.Dni = dni;
        cliente.Cuil = cuil;
        var nuevaClaveSeguridadSocial = NormalizarOpcional(request.ClaveSeguridadSocial);

        if (nuevaClaveSeguridadSocial is not null)
        {
            cliente.ClaveSeguridadSocial = nuevaClaveSeguridadSocial;
        }
        cliente.FechaNacimiento = request.FechaNacimiento;
        cliente.Telefono = NormalizarOpcional(request.Telefono);
        cliente.Email = NormalizarOpcional(request.Email);
        cliente.Domicilio = NormalizarOpcional(request.Domicilio);
        cliente.Localidad = NormalizarOpcional(request.Localidad);
        cliente.Provincia = NormalizarOpcional(request.Provincia);
        cliente.FechaModificacion = DateTime.UtcNow;
        cliente.UsuarioModificacion = _currentUser.Usuario;

        await _clienteRepository.GuardarCambiosAsync(cancellationToken);

        var clienteActualizado = await _clienteRepository.ObtenerDetallePorIdAsync(
            clienteId,
            cancellationToken
        );

        return Result<ClienteDetalleResponse>.Success(MapearDetalleResponse(clienteActualizado!));
    }

    public async Task<Result<bool>> DarDeBajaAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    )
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);

        if (cliente is null)
        {
            return Result<bool>.Failure(ClienteErrors.NoEncontrado);
        }

        // La operación es idempotente: dar de baja dos veces no produce error.
        if (!cliente.Activo)
        {
            return Result<bool>.Success(true);
        }

        cliente.Activo = false;
        cliente.FechaModificacion = DateTime.UtcNow;
        cliente.UsuarioModificacion = _currentUser.Usuario;

        await _clienteRepository.GuardarCambiosAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ReactivarAsync(
    long clienteId,
    CancellationToken cancellationToken = default
)
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(
            clienteId,
            cancellationToken
        );

        if (cliente is null)
        {
            return Result<bool>.Failure(ClienteErrors.NoEncontrado);
        }

        // La operación es idempotente: reactivar un cliente activo no produce error.
        if (cliente.Activo)
        {
            return Result<bool>.Success(true);
        }

        cliente.Activo = true;
        cliente.FechaModificacion = DateTime.UtcNow;
        cliente.UsuarioModificacion = _currentUser.Usuario;

        await _clienteRepository.GuardarCambiosAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<PagedResponse<ClienteResponse>> BuscarAsync(
        BuscarClientesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var clientes = await _clienteRepository.BuscarAsync(
            request.Busqueda,
            request.SoloActivos,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        var totalItems = await _clienteRepository.ContarAsync(
            request.Busqueda,
            request.SoloActivos,
            cancellationToken
        );

        return new PagedResponse<ClienteResponse>
        {
            Items = clientes.Select(MapearResponse).ToArray(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
        };
    }

    private static ClienteResponse MapearResponse(Cliente cliente)
    {
        return new ClienteResponse
        {
            ClienteId = cliente.ClienteId,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            NombreCompleto = $"{cliente.Apellido}, {cliente.Nombre}",
            Dni = cliente.Dni,
            Cuil = cliente.Cuil,
            FechaNacimiento = cliente.FechaNacimiento,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            Domicilio = cliente.Domicilio,
            Localidad = cliente.Localidad,
            Provincia = cliente.Provincia,
            FechaCreacion = cliente.FechaCreacion,
            Activo = cliente.Activo,
        };
    }

    private static ClienteDetalleResponse MapearDetalleResponse(Cliente cliente)
    {
        return new ClienteDetalleResponse
        {
            ClienteId = cliente.ClienteId,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            NombreCompleto = $"{cliente.Apellido}, {cliente.Nombre}",
            Dni = cliente.Dni,
            Cuil = cliente.Cuil,
            FechaNacimiento = cliente.FechaNacimiento,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            Domicilio = cliente.Domicilio,
            Localidad = cliente.Localidad,
            Provincia = cliente.Provincia,
            FechaCreacion = cliente.FechaCreacion,
            FechaModificacion = cliente.FechaModificacion,
            Activo = cliente.Activo,
            Casos = cliente
                .Casos.OrderByDescending(casoCliente => casoCliente.Caso.FechaCreacion)
                .Select(casoCliente => new CasoClienteDetalleResponse
                {
                    CasoId = casoCliente.CasoId,
                    Titulo = casoCliente.Caso.Titulo,
                    FaseInterna = casoCliente.Caso.FaseInterna,
                    TipoTramite = casoCliente.Caso.TipoTramite,
                    TipoParticipacion = casoCliente.TipoParticipacion,
                    EsPrincipal = casoCliente.EsPrincipal,
                    Activo = casoCliente.Caso.Activo,
                    Expedientes = casoCliente
                        .Caso.Expedientes.OrderBy(expediente => expediente.FechaInicio)
                        .ThenBy(expediente => expediente.ExpedienteId)
                        .Select(expediente => new ExpedienteClienteDetalleResponse
                        {
                            ExpedienteId = expediente.ExpedienteId,
                            ExpedientePadreId = expediente.ExpedientePadreId,
                            NumeroExpediente = expediente.NumeroExpediente,
                            Caratula = expediente.Caratula,
                            Juzgado = expediente.Juzgado,
                            FechaInicio = expediente.FechaInicio,
                            EstadoLegal = expediente.EstadoLegal,
                            Activo = expediente.Activo,
                        })
                        .ToArray(),
                })
                .ToArray(),
        };
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static string? NormalizarDocumento(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var numeros = new string(valor.Where(char.IsDigit).ToArray());

        return string.IsNullOrEmpty(numeros) ? null : numeros;
    }
}
