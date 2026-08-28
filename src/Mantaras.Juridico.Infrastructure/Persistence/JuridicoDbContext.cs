using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Persistence;

public class JuridicoDbContext : IdentityDbContext<UsuarioIdentity, IdentityRole<long>, long>
{
    public JuridicoDbContext(DbContextOptions<JuridicoDbContext> options)
        : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<RelacionFamiliar> RelacionesFamiliares =>
        Set<RelacionFamiliar>();

    public DbSet<Expediente> Expedientes => Set<Expediente>();

    public DbSet<Caso> Casos => Set<Caso>();

    public DbSet<CasoCliente> CasosClientes => Set<CasoCliente>();

    public DbSet<Observacion> Observaciones => Set<Observacion>();

    public DbSet<TipoBeneficio> TiposBeneficio => Set<TipoBeneficio>();

    public DbSet<TipoExpedienteAdministrativo> TiposExpedienteAdministrativo =>
        Set<TipoExpedienteAdministrativo>();

    public DbSet<HojaResumenCaso> HojasResumenCasos =>
        Set<HojaResumenCaso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JuridicoDbContext).Assembly);
    }
}
