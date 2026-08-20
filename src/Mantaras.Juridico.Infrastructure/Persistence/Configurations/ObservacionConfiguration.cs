using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class ObservacionConfiguration
    : IEntityTypeConfiguration<Observacion>
{
    public void Configure(EntityTypeBuilder<Observacion> builder)
    {
        builder.ToTable(
            "Observaciones",
            tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_Observaciones_UnPropietario",
                    "num_nonnulls(\"ClienteId\", \"CasoId\", \"ExpedienteId\") = 1"
                )
        );

        builder.HasKey(x => x.ObservacionId);

        builder.Property(x => x.ObservacionId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Texto)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.Property(x => x.UsuarioCreacion)
            .HasMaxLength(100);

        builder.HasIndex(x => new
        {
            x.ClienteId,
            x.FechaCreacion,
        });

        builder.HasIndex(x => new
        {
            x.CasoId,
            x.FechaCreacion,
        });

        builder.HasIndex(x => new
        {
            x.ExpedienteId,
            x.FechaCreacion,
        });

        builder
            .HasOne(x => x.Cliente)
            .WithMany(x => x.HistorialObservaciones)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Caso)
            .WithMany(x => x.HistorialObservaciones)
            .HasForeignKey(x => x.CasoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Expediente)
            .WithMany(x => x.HistorialObservaciones)
            .HasForeignKey(x => x.ExpedienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}