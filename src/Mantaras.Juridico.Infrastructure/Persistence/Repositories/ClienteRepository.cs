using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly JuridicoDbContext _dbContext;

    public ClienteRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Cliente?> ObtenerPorIdAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Clientes.FirstOrDefaultAsync(
            x => x.ClienteId == clienteId,
            cancellationToken
        );
    }

    public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        return _dbContext.Clientes.AnyAsync(x => x.Dni == dni, cancellationToken);
    }

    public Task<bool> ExisteCuilAsync(string cuil, CancellationToken cancellationToken = default)
    {
        return _dbContext.Clientes.AnyAsync(x => x.Cuil == cuil, cancellationToken);
    }

    public async Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _dbContext.Clientes.AddAsync(cliente, cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Cliente>> BuscarAsync(
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = ConstruirConsulta(busqueda, soloActivos);

        return await query
            .OrderBy(x => x.Apellido)
            .ThenBy(x => x.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(
        string? busqueda,
        bool soloActivos,
        CancellationToken cancellationToken = default
    )
    {
        var query = ConstruirConsulta(busqueda, soloActivos);

        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<Cliente> ConstruirConsulta(string? busqueda, bool soloActivos)
    {
        var query = _dbContext.Clientes.AsQueryable();

        if (soloActivos)
        {
            query = query.Where(x => x.Activo);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Nombre, $"%{termino}%")
                || EF.Functions.ILike(x.Apellido, $"%{termino}%")
                || x.Dni != null && EF.Functions.ILike(x.Dni, $"%{termino}%")
                || x.Cuil != null && EF.Functions.ILike(x.Cuil, $"%{termino}%")
            );
        }

        return query;
    }
}
