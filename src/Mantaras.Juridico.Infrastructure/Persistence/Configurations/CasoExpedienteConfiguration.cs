using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class CasoExpedienteConfiguration
    : IEntityTypeConfiguration<CasoExpediente>
{
    public void Configure(EntityTypeBuilder<CasoExpediente> builder)
    {
        builder.ToTable("CasosExpedientes");

        builder.HasKey(x => new { x.CasoId, x.ExpedienteId });

        builder.HasIndex(x => x.ExpedienteId);

        builder
            .HasOne(x => x.Caso)
            .WithMany(x => x.Expedientes)
            .HasForeignKey(x => x.CasoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Expediente)
            .WithMany(x => x.Casos)
            .HasForeignKey(x => x.ExpedienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
