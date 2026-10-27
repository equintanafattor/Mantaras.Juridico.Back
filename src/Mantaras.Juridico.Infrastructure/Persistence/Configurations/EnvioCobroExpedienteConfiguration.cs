using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class EnvioCobroExpedienteConfiguration
    : IEntityTypeConfiguration<EnvioCobroExpediente>
{
    public void Configure(EntityTypeBuilder<EnvioCobroExpediente> builder)
    {
        builder.ToTable("EnviosCobroExpedientes");

        builder.HasKey(x => x.EnvioCobroExpedienteId);
        builder.Property(x => x.EnvioCobroExpedienteId).ValueGeneratedOnAdd();
        builder.Property(x => x.TipoDocumento).HasConversion<int>().IsRequired();
        builder.Property(x => x.FechaEnvio).HasColumnType("date").IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder
            .HasIndex(x => x.ExpedienteId)
            .IsUnique()
            .HasFilter("\"Activo\"");
        builder.HasIndex(x => x.AgendaGeneracionReglaId).IsUnique();

        builder
            .HasOne(x => x.Expediente)
            .WithMany(x => x.EnviosCobro)
            .HasForeignKey(x => x.ExpedienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.AgendaGeneracionRegla)
            .WithOne()
            .HasForeignKey<EnvioCobroExpediente>(x => x.AgendaGeneracionReglaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
