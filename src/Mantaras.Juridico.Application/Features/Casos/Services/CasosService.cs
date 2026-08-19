using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Casos.Requests;
using Mantaras.Juridico.Application.Features.Casos.Responses;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Services;

public sealed class CasosService : ICasosService
{
    private readonly ICasoRepository _casoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICurrentUserService _currentUser;

    public CasosService(
        ICasoRepository casoRepository,
        IClienteRepository clienteRepository,
        ICurrentUserService currentUser
    )
    {
        _casoRepository = casoRepository;
        _clienteRepository = clienteRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<CasoResponse>> CrearAsync(
        CrearCasoRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var clienteIds = request.Clientes.Select(x => x.ClienteId).Distinct().ToArray();

        var clientes = await _clienteRepository.ObtenerActivosPorIdsAsync(
            clienteIds,
            cancellationToken
        );

        if (clientes.Count != clienteIds.Length)
        {
            return Result<CasoResponse>.Failure(CasoErrors.ClientesNoEncontrados);
        }

        var clientesPorId = clientes.ToDictionary(x => x.ClienteId);

        var caso = ConstruirCaso(
            request,
            clientesPorId,
            DateTime.UtcNow
        );

        await _casoRepository.AgregarAsync(caso, cancellationToken);
        await _casoRepository.GuardarCambiosAsync(cancellationToken);

        return Result<CasoResponse>.Success(MapearResponse(caso));
    }

    public async Task<Result<CrearCasoConExpedientePrincipalResponse>>
    CrearConExpedientePrincipalAsync(
        CrearCasoConExpedientePrincipalRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var clienteIds = request
            .Caso.Clientes
            .Select(x => x.ClienteId)
            .Distinct()
            .ToArray();

        var clientes = await _clienteRepository.ObtenerActivosPorIdsAsync(
            clienteIds,
            cancellationToken
        );

        if (clientes.Count != clienteIds.Length)
        {
            return Result<CrearCasoConExpedientePrincipalResponse>.Failure(
                CasoErrors.ClientesNoEncontrados
            );
        }

        var clientesPorId = clientes.ToDictionary(x => x.ClienteId);
        var fechaCreacion = DateTime.UtcNow;

        var caso = ConstruirCaso(
            request.Caso,
            clientesPorId,
            fechaCreacion
        );

        var expediente = new Expediente
        {
            TipoExpediente = TipoExpediente.Principal,
            ExpedientePadreId = null,
            NumeroExpediente = NormalizarOpcional(
                request.Expediente.NumeroExpediente
            ),
            Caratula = request.Expediente.Caratula.Trim(),
            Juzgado = NormalizarOpcional(request.Expediente.Juzgado),
            FechaInicio = request.Expediente.FechaInicio,
            EstadoLegal = NormalizarOpcional(
                request.Expediente.EstadoLegal
            ),
            Observaciones = NormalizarOpcional(
                request.Expediente.Observaciones
            ),
            Caso = caso,
            FechaCreacion = fechaCreacion,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        caso.Expedientes.Add(expediente);

        await _casoRepository.AgregarAsync(
            caso,
            cancellationToken
        );

        await _casoRepository.GuardarCambiosAsync(
            cancellationToken
        );

        return Result<CrearCasoConExpedientePrincipalResponse>.Success(
            new CrearCasoConExpedientePrincipalResponse
            {
                CasoId = caso.CasoId,
                ExpedienteId = expediente.ExpedienteId,
                TituloCaso = caso.Titulo,
                NumeroExpediente = expediente.NumeroExpediente,
                Caratula = expediente.Caratula,
                FechaCreacion = fechaCreacion,
            }
        );
    }

    public async Task<Result<CasoDetalleResponse>> ObtenerPorIdAsync(
        long casoId,
        CancellationToken cancellationToken = default
    )
    {
        var caso = await _casoRepository.ObtenerDetallePorIdAsync(
            casoId,
            cancellationToken
        );

        if (caso is null)
        {
            return Result<CasoDetalleResponse>.Failure(CasoErrors.NoEncontrado);
        }

        return Result<CasoDetalleResponse>.Success(MapearDetalleResponse(caso));
    }

    public async Task<PagedResponse<CasoResponse>> BuscarAsync(
        BuscarCasosRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var casos = await _casoRepository.BuscarAsync(
            request.Busqueda,
            request.FaseInterna,
            request.SoloActivos,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        var totalItems = await _casoRepository.ContarAsync(
            request.Busqueda,
            request.FaseInterna,
            request.SoloActivos,
            cancellationToken
        );

        return new PagedResponse<CasoResponse>
        {
            Items = casos.Select(MapearResponse).ToArray(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
        };
    }

    public async Task<Result<CasoResponse>> ActualizarAsync(
        long casoId,
        ActualizarCasoRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var caso = await _casoRepository.ObtenerPorIdAsync(casoId, cancellationToken);

        if (caso is null)
        {
            return Result<CasoResponse>.Failure(CasoErrors.NoEncontrado);
        }

        var clienteIds = request.Clientes.Select(x => x.ClienteId).Distinct().ToArray();

        var clientes = await _clienteRepository.ObtenerActivosPorIdsAsync(
            clienteIds,
            cancellationToken
        );

        if (clientes.Count != clienteIds.Length)
        {
            return Result<CasoResponse>.Failure(CasoErrors.ClientesNoEncontrados);
        }

        var clientesPorId = clientes.ToDictionary(x => x.ClienteId);

        caso.Titulo = request.Titulo.Trim();
        caso.FaseInterna = request.FaseInterna;
        caso.TipoTramite = NormalizarOpcional(request.TipoTramite);
        caso.Observaciones = NormalizarOpcional(request.Observaciones);
        caso.FechaModificacion = DateTime.UtcNow;
        caso.UsuarioModificacion = _currentUser.Usuario;

        var clientesSolicitados = request.Clientes.ToDictionary(x => x.ClienteId);

        var relacionesEliminadas = caso
            .Clientes.Where(x => !clientesSolicitados.ContainsKey(x.ClienteId))
            .ToArray();

        foreach (var relacion in relacionesEliminadas)
        {
            caso.Clientes.Remove(relacion);
        }

        var relacionesExistentes = caso.Clientes.ToDictionary(x => x.ClienteId);

        foreach (var clienteRequest in request.Clientes)
        {
            if (relacionesExistentes.TryGetValue(clienteRequest.ClienteId, out var relacion))
            {
                relacion.TipoParticipacion = clienteRequest.TipoParticipacion;
                relacion.EsPrincipal = clienteRequest.EsPrincipal;
                relacion.Cliente = clientesPorId[clienteRequest.ClienteId];

                continue;
            }

            var cliente = clientesPorId[clienteRequest.ClienteId];

            caso.Clientes.Add(
                new CasoCliente
                {
                    CasoId = caso.CasoId,
                    ClienteId = cliente.ClienteId,
                    TipoParticipacion = clienteRequest.TipoParticipacion,
                    EsPrincipal = clienteRequest.EsPrincipal,
                    Caso = caso,
                    Cliente = cliente,
                }
            );
        }

        await _casoRepository.GuardarCambiosAsync(cancellationToken);

        return Result<CasoResponse>.Success(MapearResponse(caso));
    }

    public async Task<Result<bool>> DarDeBajaAsync(
        long casoId,
        CancellationToken cancellationToken = default
    )
    {
        var caso = await _casoRepository.ObtenerPorIdAsync(
            casoId,
            cancellationToken
        );

        if (caso is null)
        {
            return Result<bool>.Failure(CasoErrors.NoEncontrado);
        }

        if (!caso.Activo)
        {
            return Result<bool>.Success(true);
        }

        var tieneExpedientesActivos =
        await _casoRepository.TieneExpedientesActivosAsync(
            casoId,
            cancellationToken
        );

        if (tieneExpedientesActivos)
        {
            return Result<bool>.Failure(CasoErrors.ExpedientesActivos);
        }

        caso.Activo = false;
        caso.FechaModificacion = DateTime.UtcNow;
        caso.UsuarioModificacion = _currentUser.Usuario;

        await _casoRepository.GuardarCambiosAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RestaurarAsync(
    long casoId,
    CancellationToken cancellationToken = default
)
    {
        var caso = await _casoRepository.ObtenerPorIdAsync(
            casoId,
            cancellationToken
        );

        if (caso is null)
        {
            return Result<bool>.Failure(CasoErrors.NoEncontrado);
        }

        if (caso.Activo)
        {
            return Result<bool>.Success(true);
        }

        caso.Activo = true;
        caso.FechaModificacion = DateTime.UtcNow;
        caso.UsuarioModificacion = _currentUser.Usuario;

        await _casoRepository.GuardarCambiosAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private Caso ConstruirCaso(
    CrearCasoRequest request,
    IReadOnlyDictionary<long, Cliente> clientesPorId,
    DateTime fechaCreacion
)
    {
        var caso = new Caso
        {
            Titulo = request.Titulo.Trim(),
            FaseInterna = request.FaseInterna,
            TipoTramite = NormalizarOpcional(request.TipoTramite),
            Observaciones = NormalizarOpcional(request.Observaciones),
            FechaCreacion = fechaCreacion,
            UsuarioCreacion = _currentUser.Usuario,
            Activo = true,
        };

        foreach (var clienteRequest in request.Clientes)
        {
            var cliente = clientesPorId[clienteRequest.ClienteId];

            caso.Clientes.Add(
                new CasoCliente
                {
                    ClienteId = cliente.ClienteId,
                    TipoParticipacion =
                        clienteRequest.TipoParticipacion,
                    EsPrincipal = clienteRequest.EsPrincipal,
                    Caso = caso,
                    Cliente = cliente,
                }
            );
        }

        return caso;
    }

    private static CasoDetalleResponse MapearDetalleResponse(Caso caso)
    {
        return new CasoDetalleResponse
        {
            CasoId = caso.CasoId,
            Titulo = caso.Titulo,
            FaseInterna = caso.FaseInterna,
            TipoTramite = caso.TipoTramite,
            Observaciones = caso.Observaciones,
            Clientes = caso
                .Clientes.OrderByDescending(x => x.EsPrincipal)
                .ThenBy(x => x.Cliente.Apellido)
                .ThenBy(x => x.Cliente.Nombre)
                .Select(x => new CasoClienteResponse
                {
                    ClienteId = x.ClienteId,
                    Nombre = x.Cliente.Nombre,
                    Apellido = x.Cliente.Apellido,
                    NombreCompleto = $"{x.Cliente.Apellido}, {x.Cliente.Nombre}",
                    Dni = x.Cliente.Dni,
                    Cuil = x.Cliente.Cuil,
                    TipoParticipacion = x.TipoParticipacion,
                    EsPrincipal = x.EsPrincipal,
                })
                .ToArray(),
            Expedientes = caso
                .Expedientes.OrderBy(x => x.ExpedientePadreId.HasValue)
                .ThenBy(x => x.FechaInicio)
                .ThenBy(x => x.Caratula)
                .Select(x => new ExpedienteCasoDetalleResponse
                {
                    ExpedienteId = x.ExpedienteId,
                    TipoExpediente = x.TipoExpediente,
                    ExpedientePadreId = x.ExpedientePadreId,
                    NumeroExpediente = x.NumeroExpediente,
                    Caratula = x.Caratula,
                    Juzgado = x.Juzgado,
                    FechaInicio = x.FechaInicio,
                    EstadoLegal = x.EstadoLegal,
                    Observaciones = x.Observaciones,
                    Activo = x.Activo,
                })
                .ToArray(),
            FechaCreacion = caso.FechaCreacion,
            FechaModificacion = caso.FechaModificacion,
            Activo = caso.Activo,
        };
    }

    private static CasoResponse MapearResponse(Caso caso)
    {
        return new CasoResponse
        {
            CasoId = caso.CasoId,
            Titulo = caso.Titulo,
            FaseInterna = caso.FaseInterna,
            TipoTramite = caso.TipoTramite,
            Observaciones = caso.Observaciones,
            Clientes = caso
                .Clientes.OrderByDescending(x => x.EsPrincipal)
                .ThenBy(x => x.Cliente.Apellido)
                .ThenBy(x => x.Cliente.Nombre)
                .Select(x => new CasoClienteResponse
                {
                    ClienteId = x.ClienteId,
                    Nombre = x.Cliente.Nombre,
                    Apellido = x.Cliente.Apellido,
                    NombreCompleto = $"{x.Cliente.Apellido}, {x.Cliente.Nombre}",
                    Dni = x.Cliente.Dni,
                    Cuil = x.Cliente.Cuil,
                    TipoParticipacion = x.TipoParticipacion,
                    EsPrincipal = x.EsPrincipal,
                })
                .ToArray(),
            FechaCreacion = caso.FechaCreacion,
            FechaModificacion = caso.FechaModificacion,
            Activo = caso.Activo,
        };
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
