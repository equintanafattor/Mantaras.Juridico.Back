using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class AgendaGeneracionReglaConfiguration
    : IEntityTypeConfiguration<AgendaGeneracionRegla>
{
    public void Configure(EntityTypeBuilder<AgendaGeneracionRegla> builder)
    {
        builder.ToTable(
            "AgendaGeneracionesReglas",
            tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_AgendaGeneracionesReglas_Origen",
                    "(\"OrigenFechaBase\" = 1 AND num_nonnulls(\"EntradaAgendaOrigenId\", \"CasoOrigenId\", \"ExpedienteOrigenId\", \"ObservacionOrigenId\") = 0) OR "
                    + "(\"OrigenFechaBase\" = 2 AND \"EntradaAgendaOrigenId\" IS NOT NULL AND num_nonnulls(\"CasoOrigenId\", \"ExpedienteOrigenId\", \"ObservacionOrigenId\") = 0) OR "
                    + "(\"OrigenFechaBase\" = 3 AND \"CasoOrigenId\" IS NOT NULL AND num_nonnulls(\"EntradaAgendaOrigenId\", \"ExpedienteOrigenId\", \"ObservacionOrigenId\") = 0) OR "
                    + "(\"OrigenFechaBase\" = 4 AND \"ExpedienteOrigenId\" IS NOT NULL AND num_nonnulls(\"EntradaAgendaOrigenId\", \"CasoOrigenId\", \"ObservacionOrigenId\") = 0) OR "
                    + "(\"OrigenFechaBase\" = 5 AND \"ObservacionOrigenId\" IS NOT NULL AND num_nonnulls(\"EntradaAgendaOrigenId\", \"CasoOrigenId\", \"ExpedienteOrigenId\") = 0)"
                )
        );

        builder.HasKey(x => x.AgendaGeneracionReglaId);
        builder.Property(x => x.AgendaGeneracionReglaId).ValueGeneratedOnAdd();
        builder.Property(x => x.FechaBase).HasColumnType("date").IsRequired();
        builder.Property(x => x.OrigenFechaBase).HasConversion<int>().IsRequired();
        builder.Property(x => x.ClaveIdempotencia).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.EntradaAgendaId).IsUnique();
        builder.HasIndex(x => x.ClaveIdempotencia).IsUnique();
        builder.HasIndex(x => x.ReglaVencimientoId);
        builder.HasIndex(x => x.EntradaAgendaOrigenId);
        builder.HasIndex(x => x.CasoOrigenId);
        builder.HasIndex(x => x.ExpedienteOrigenId);
        builder.HasIndex(x => x.ObservacionOrigenId);

        builder
            .HasOne(x => x.ReglaVencimiento)
            .WithMany(x => x.Generaciones)
            .HasForeignKey(x => x.ReglaVencimientoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.EntradaAgenda)
            .WithOne(x => x.GeneracionRegla)
            .HasForeignKey<AgendaGeneracionRegla>(x => x.EntradaAgendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.EntradaAgendaOrigen)
            .WithMany(x => x.GeneracionesDependientes)
            .HasForeignKey(x => x.EntradaAgendaOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.CasoOrigen)
            .WithMany()
            .HasForeignKey(x => x.CasoOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ExpedienteOrigen)
            .WithMany()
            .HasForeignKey(x => x.ExpedienteOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ObservacionOrigen)
            .WithMany()
            .HasForeignKey(x => x.ObservacionOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<UsuarioIdentity>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioGeneracionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
