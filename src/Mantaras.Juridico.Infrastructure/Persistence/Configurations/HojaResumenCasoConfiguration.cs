using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class HojaResumenCasoConfiguration
    : IEntityTypeConfiguration<HojaResumenCaso>
{
    public void Configure(EntityTypeBuilder<HojaResumenCaso> builder)
    {
        builder.ToTable(
            "HojasResumenCasos",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_HojasResumenCasos_PeriodoMovilidad",
                    """
                    (
                        "MovilidadActualizacionMes" IS NULL
                        AND "MovilidadActualizacionAnio" IS NULL
                    )
                    OR
                    (
                        "MovilidadActualizacionMes" IS NOT NULL
                        AND "MovilidadActualizacionAnio" IS NOT NULL
                        AND "MovilidadActualizacionMes" BETWEEN 1 AND 12
                        AND "MovilidadActualizacionAnio" BETWEEN 1 AND 9999
                    )
                    """
                );
            }
        );

        builder.HasKey(x => x.CasoId);

        builder.Property(x => x.CasoId)
            .ValueGeneratedNever();

        builder.Property(x => x.HaberInicialReajustadoCaracteristicas)
            .HasMaxLength(2000);

        builder.Property(x => x.HaberInicialPbu)
            .HasPrecision(18, 2);

        builder.Property(x => x.HaberInicialObservacion)
            .HasMaxLength(2000);

        builder.Property(x => x.HaberInicialMonto)
            .HasPrecision(18, 2);

        builder.Property(x => x.MovilidadObservaciones)
            .HasMaxLength(2000);

        builder.Property(x => x.MovilidadMonto)
            .HasPrecision(18, 2);

        builder.Property(x => x.RetroactivoFechaInicio)
            .HasColumnType("date");

        builder.Property(x => x.RetroactivoFechaActualizacion)
            .HasColumnType("date");

        builder.Property(x => x.RetroactivoObservacion)
            .HasMaxLength(2000);

        builder.Property(x => x.RetroactivoMonto)
            .HasPrecision(18, 2);

        builder.Property(x => x.FechaCreacion)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UsuarioCreacion)
            .HasMaxLength(100);

        builder.Property(x => x.FechaModificacion)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UsuarioModificacion)
            .HasMaxLength(100);

        builder
            .HasOne(x => x.Caso)
            .WithOne(x => x.HojaResumen)
            .HasForeignKey<HojaResumenCaso>(x => x.CasoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}