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
        var nombre = request.Nombre.Trim();
        var apellido = request.Apellido.Trim();
        var dni = NormalizarOpcional(request.Dni);
        var cuil = NormalizarOpcional(request.Cuil);

        if (dni is not null && await _clienteRepository.ExisteDniAsync(dni, cancellationToken))
        {
            return Result<ClienteResponse>.Failure(ClienteErrors.DniDuplicado);
        }

        if (cuil is not null && await _clienteRepository.ExisteCuilAsync(cuil, cancellationToken))
        {
            return Result<ClienteResponse>.Failure(ClienteErrors.CuilDuplicado);
        }

        var cliente = new Cliente
        {
            Nombre = nombre,
            Apellido = apellido,
            Dni = dni,
            Cuil = cuil,
            ClaveSeguridadSocial = NormalizarOpcional(request.ClaveSeguridadSocial),
            FechaNacimiento = request.FechaNacimiento,
            Telefono = NormalizarOpcional(request.Telefono),
            Email = NormalizarOpcional(request.Email),
            Domicilio = NormalizarOpcional(request.Domicilio),
            Localidad = NormalizarOpcional(request.Localidad),
            Provincia = NormalizarOpcional(request.Provincia),
            Observaciones = NormalizarOpcional(request.Observaciones),
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        await _clienteRepository.AgregarAsync(cliente, cancellationToken);

        await _clienteRepository.GuardarCambiosAsync(cancellationToken);

        var response = MapearResponse(cliente);

        return Result<ClienteResponse>.Success(response);
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

        var items = clientes.Select(MapearResponse).ToArray();

        return new PagedResponse<ClienteResponse>
        {
            Items = items,
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
            Observaciones = cliente.Observaciones,
        };
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
