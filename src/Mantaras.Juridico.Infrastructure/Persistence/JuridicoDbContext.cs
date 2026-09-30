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

    public DbSet<CasoExpediente> CasosExpedientes => Set<CasoExpediente>();

    public DbSet<OpcionCatalogo> OpcionesCatalogo => Set<OpcionCatalogo>();

    public DbSet<Observacion> Observaciones => Set<Observacion>();

    public DbSet<TipoBeneficio> TiposBeneficio => Set<TipoBeneficio>();

    public DbSet<TipoExpedienteAdministrativo> TiposExpedienteAdministrativo =>
        Set<TipoExpedienteAdministrativo>();

    public DbSet<HojaResumenCaso> HojasResumenCasos =>
        Set<HojaResumenCaso>();

    public DbSet<TipoEntradaAgenda> TiposEntradaAgenda =>
        Set<TipoEntradaAgenda>();

    public DbSet<EntradaAgenda> EntradasAgenda => Set<EntradaAgenda>();

    public DbSet<EntradaAgendaCliente> EntradasAgendaClientes =>
        Set<EntradaAgendaCliente>();

    public DbSet<EntradaAgendaCaso> EntradasAgendaCasos =>
        Set<EntradaAgendaCaso>();

    public DbSet<EntradaAgendaExpediente> EntradasAgendaExpedientes =>
        Set<EntradaAgendaExpediente>();

    public DbSet<EntradaAgendaResponsable> EntradasAgendaResponsables =>
        Set<EntradaAgendaResponsable>();

    public DbSet<RecordatorioAgenda> RecordatoriosAgenda =>
        Set<RecordatorioAgenda>();

    public DbSet<RecordatorioPredeterminadoTipoAgenda>
        RecordatoriosPredeterminadosTiposAgenda =>
            Set<RecordatorioPredeterminadoTipoAgenda>();

    public DbSet<RecurrenciaAgenda> RecurrenciasAgenda =>
        Set<RecurrenciaAgenda>();

    public DbSet<ReglaVencimiento> ReglasVencimiento =>
        Set<ReglaVencimiento>();

    public DbSet<RecordatorioPredeterminadoReglaVencimiento>
        RecordatoriosPredeterminadosReglasVencimiento =>
            Set<RecordatorioPredeterminadoReglaVencimiento>();

    public DbSet<AgendaGeneracionRegla> AgendaGeneracionesReglas =>
        Set<AgendaGeneracionRegla>();

    public DbSet<DiaInhabil> DiasInhabiles => Set<DiaInhabil>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JuridicoDbContext).Assembly);
    }
}
