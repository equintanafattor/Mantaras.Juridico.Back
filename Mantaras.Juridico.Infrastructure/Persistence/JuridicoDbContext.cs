using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Persistence;

public class JuridicoDbContext : DbContext
{
    public JuridicoDbContext(DbContextOptions<JuridicoDbContext> options)
        : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Expediente> Expedientes => Set<Expediente>();

    public DbSet<Caso> Casos { get; set; }
    
    public DbSet<CasoCliente> CasosClientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JuridicoDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
