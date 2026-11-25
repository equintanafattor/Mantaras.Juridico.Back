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

        builder.Property(x => x.EstadoLegal).HasMaxLength(200);

        builder.Property(x => x.TipoTramite).HasMaxLength(200);

        builder.Property(x => x.Activo).HasDefaultValue(true);

        builder.HasIndex(x => x.NumeroExpediente);

        builder.HasIndex(x => x.ExpedientePadreId);

        builder.HasIndex(x => x.JuzgadoId);

        // Restrict: un juzgado con expedientes vinculados solo se da de baja lógicamente.
        builder
            .HasOne(x => x.Juzgado)
            .WithMany(x => x.Expedientes)
            .HasForeignKey(x => x.JuzgadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(x => x.TipoExpediente)
            .HasConversion<int>()
            .HasDefaultValue(TipoExpediente.Principal);

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
