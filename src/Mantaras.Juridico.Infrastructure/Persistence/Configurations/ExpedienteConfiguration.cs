using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public class ExpedienteConfiguration : IEntityTypeConfiguration<Expediente>
{
    public void Configure(EntityTypeBuilder<Expediente> builder)
    {
        builder.ToTable("Expedientes");

        builder.HasKey(x => x.ExpedienteId);

        builder.Property(x => x.NumeroExpediente).HasMaxLength(100);

        builder.Property(x => x.Caratula).HasMaxLength(1000).IsRequired();

        builder.Property(x => x.Juzgado).HasMaxLength(500);

        builder.Property(x => x.EstadoLegal).HasMaxLength(200);

        builder.Property(x => x.Observaciones).HasMaxLength(2000);

        builder.Property(x => x.Activo).HasDefaultValue(true);

        builder.HasIndex(x => x.NumeroExpediente);

        builder.HasIndex(x => x.CasoId);

        builder.HasIndex(x => x.ExpedientePadreId);

        builder
            .HasIndex(
                x => x.CasoId,
                "IX_Expedientes_CasoId_Principal"
            )
            .IsUnique()
            .HasFilter("\"TipoExpediente\" = 1");

        builder
            .Property(x => x.TipoExpediente)
            .HasConversion<int>()
            .HasDefaultValue(TipoExpediente.Principal);

        builder
            .HasOne(x => x.Caso)
            .WithMany(x => x.Expedientes)
            .HasForeignKey(x => x.CasoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ExpedientePadre)
            .WithMany(x => x.ExpedientesDerivados)
            .HasForeignKey(x => x.ExpedientePadreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);

        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);

        builder
            .Property(x => x.FechaInicio)
            .HasColumnType("date");
    }
}
