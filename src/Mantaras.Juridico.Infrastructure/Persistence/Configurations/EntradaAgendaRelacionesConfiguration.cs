using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class EntradaAgendaClienteConfiguration
    : IEntityTypeConfiguration<EntradaAgendaCliente>
{
    public void Configure(EntityTypeBuilder<EntradaAgendaCliente> builder)
    {
        builder.ToTable("EntradasAgendaClientes");
        builder.HasKey(x => new { x.EntradaAgendaId, x.ClienteId });
        builder.HasIndex(x => x.ClienteId);

        builder
            .HasOne(x => x.EntradaAgenda)
            .WithMany(x => x.Clientes)
            .HasForeignKey(x => x.EntradaAgendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EntradaAgendaCasoConfiguration
    : IEntityTypeConfiguration<EntradaAgendaCaso>
{
    public void Configure(EntityTypeBuilder<EntradaAgendaCaso> builder)
    {
        builder.ToTable("EntradasAgendaCasos");
        builder.HasKey(x => new { x.EntradaAgendaId, x.CasoId });
        builder.HasIndex(x => x.CasoId);

        builder
            .HasOne(x => x.EntradaAgenda)
            .WithMany(x => x.Casos)
            .HasForeignKey(x => x.EntradaAgendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Caso)
            .WithMany()
            .HasForeignKey(x => x.CasoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EntradaAgendaExpedienteConfiguration
    : IEntityTypeConfiguration<EntradaAgendaExpediente>
{
    public void Configure(EntityTypeBuilder<EntradaAgendaExpediente> builder)
    {
        builder.ToTable("EntradasAgendaExpedientes");
        builder.HasKey(x => new { x.EntradaAgendaId, x.ExpedienteId });
        builder.HasIndex(x => x.ExpedienteId);

        builder
            .HasOne(x => x.EntradaAgenda)
            .WithMany(x => x.Expedientes)
            .HasForeignKey(x => x.EntradaAgendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Expediente)
            .WithMany()
            .HasForeignKey(x => x.ExpedienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EntradaAgendaResponsableConfiguration
    : IEntityTypeConfiguration<EntradaAgendaResponsable>
{
    public void Configure(EntityTypeBuilder<EntradaAgendaResponsable> builder)
    {
        builder.ToTable("EntradasAgendaResponsables");
        builder.HasKey(x => new { x.EntradaAgendaId, x.UsuarioId });
        builder.HasIndex(x => x.UsuarioId);

        builder
            .HasOne(x => x.EntradaAgenda)
            .WithMany(x => x.Responsables)
            .HasForeignKey(x => x.EntradaAgendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<UsuarioIdentity>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
